namespace EcsServer.Components;

public struct Dirty
{
    public HashSet<int> ComponentIds = [];

    public Dirty()
    {
    }
}