namespace Shared.Attributes;

public class PriorityAttribute : Attribute
{
    public int Priority { get; private set; } = 0;

    public PriorityAttribute(int priority)
    {
        Priority = priority;
    }
    
    public PriorityAttribute(object priority) 
    {
        if (priority is Enum)
        {
            Priority = Convert.ToInt32(priority);
        }
    }
}