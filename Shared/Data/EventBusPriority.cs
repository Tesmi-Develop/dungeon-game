namespace Shared.Data;

public enum EventBusPriority : int
{
    Critical = 4000,
    High = 3000,
    AboveNormal = 2500,
    Default = 2000,
    BelowNormal = 1500,
    Low = 1000,
    Lowest = 0,
    NoPriority = -1,
}