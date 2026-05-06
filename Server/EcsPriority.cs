namespace Server;

public enum EcsPriority
{
    Low = -1000,
    UpdateCollisionWorld = -501,
    UpdateCollisions = -502,
    Default = 0,
    High = 1000,
}