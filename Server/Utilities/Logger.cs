using System.Runtime.CompilerServices;
using Hypercube.Utilities.Constants;
using Hypercube.Utilities.Debugging.Logger;

namespace Server.Utilities;

public class MyLogger : ConsoleLogger
{
    private readonly Type _type;

    public MyLogger(object target)
    {
        _type = target.GetType();
    }

    public override void Log(LogLevel level, string message)
    {
        if (level < this.LogLevel)
            return;
        
        var interpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 4);
        interpolatedStringHandler.AppendFormatted(GetColor(level));
        interpolatedStringHandler.AppendLiteral("[");
        interpolatedStringHandler.AppendFormatted(level);
        interpolatedStringHandler.AppendLiteral("] ");
        interpolatedStringHandler.AppendLiteral("[");
        interpolatedStringHandler.AppendFormatted(_type.Name);
        interpolatedStringHandler.AppendLiteral("] ");
        interpolatedStringHandler.AppendFormatted(message);
        interpolatedStringHandler.AppendFormatted(Ansi.Reset);
        Echo(interpolatedStringHandler.ToStringAndClear());
    }
}