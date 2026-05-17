namespace Shared.SharedSystemRealisation;

public enum EcsPriority
{
    Low = -1000,
    UpdateCollisions = -502,
    UpdateCollisionWorld = UpdateCollisions + 1,
    Default = 0,
    StateUpdater = 50,
    AfterTargetScanner = TargetScanner - 1,
    TargetScanner = 100,
    High = 1000,
}