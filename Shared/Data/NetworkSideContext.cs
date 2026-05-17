namespace Shared.Data;

public static class NetworkSideContext
{
    public static NetworkSide NetworkSide;
    
    public static bool IsServer => NetworkSide == NetworkSide.Server;
    public static bool IsClient => NetworkSide == NetworkSide.Client;
}