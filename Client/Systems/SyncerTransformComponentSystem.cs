using System.Diagnostics;
using Client.Components;
using Hypercube.Core.Ecs;
using Hypercube.Core.Systems.Transform;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Shared.Components;

namespace Client.Systems;

public class SyncerTransformComponentSystem : EntitySystem
{
    private Query _query = null!;
    private static readonly Stopwatch _gameTimer = Stopwatch.StartNew();

    public override void Initialize()
    {
        _query = GetQuery().WithAll<Transform, TransformComponent, InterpolationComponent>().Build();
    }
    
    public override void Update(float deltaTime)
    {
        deltaTime = 1 / 60f;
        // 1. Константы (подстрой под свой сервер)
        const float serverTickRate = 0.05f; // Сервер шлет пакеты каждые 50мс
        const float bufferTime = 0.1f;      // Задержка интерполяции (2 тика)

        float currentTime = (float)_gameTimer.Elapsed.TotalSeconds;
        float renderTime = currentTime - bufferTime;

        _query.With<Transform, InterpolationComponent, TransformComponent>((entity, ref sharedTransform, ref interp, ref transform) =>
        {
            // 2. Логика приема (OnServerUpdate внутри Update)
            if (sharedTransform.Position != interp.LastPosition)
            {
                interp.LastPosition = sharedTransform.Position;
                // Добавляем в очередь: Время получения + Позиция
                interp.Snapshots.Enqueue((currentTime, sharedTransform.Position));
                
                // Чистим старье
                while (interp.Snapshots.Count > 10) interp.Snapshots.Dequeue();
            }

            if (interp.Snapshots.Count < 2) return;

            // 3. Ищем два снимка, между которыми лежит renderTime
            var list = interp.Snapshots.ToList(); // Для удобства поиска
            (float Time, Vector2 Pos) from = list[0];
            (float Time, Vector2 Pos) to = list[^1];
            bool found = false;

            for (int i = 0; i < list.Count - 1; i++)
            {
                if (renderTime >= list[i].Time && renderTime <= list[i+1].Time)
                {
                    from = list[i];
                    to = list[i+1];
                    found = true;
                    break;
                }
            }

            // 4. САМЫЙ ВАЖНЫЙ МОМЕНТ: Расчет позиции
            if (found)
            {
                // Мы находимся внутри "истории". Считаем процент прохода t
                float t = (renderTime - from.Time) / (to.Time - from.Time);
                t = Math.Clamp(t, 0f, 1f);
                // Линейно двигаемся. Это даст 100% плавность, если t меняется плавно.
                Vector2 interpPos = Vector2.Lerp(from.Pos, to.Pos, t);
                transform.LocalPosition = new Vector3(interpPos.X, interpPos.Y, transform.LocalPosition.Z);
            }
            else if (renderTime > to.Time)
            {
                // Мы "вылетели" в будущее (пакет опоздал). 
                // Чтобы не было фриза, плавно тянемся к последней известной точке.
                Vector2 velocity = (to.Pos - from.Pos) / (to.Time - from.Time);
                // Экстраполяция: продолжаем движение с той же скоростью (опционально)
                // Но пока просто мягко дотянем:
                var nextPos = Vector2.Lerp(transform.LocalPosition.Xy, to.Pos, deltaTime * 20f);
                //transform.LocalPosition = new Vector3(nextPos.X, nextPos.Y, transform.LocalPosition.Z);
            }
        });
    }

    // Измени сигнатуру, чтобы принимать время снаружи
    public void OnServerUpdate(Entity entity, Vector2 pos, float time) 
    {
        ref var interp = ref GetComponent<InterpolationComponent>(entity);
    
        // Если пакет пришел "из прошлого" (UDP такой UDP), игнорируем его
        if (interp.Snapshots.Count > 0)
        {
            // Ищем самое свежее время в очереди
            var lastTime = 0f;
            foreach(var s in interp.Snapshots) if(s.Time > lastTime) lastTime = s.Time;
        
            if (time <= lastTime) return; 
        }

        interp.Snapshots.Enqueue((time, pos));
    
        if (interp.Snapshots.Count > 10) 
            interp.Snapshots.Dequeue();
    }
}