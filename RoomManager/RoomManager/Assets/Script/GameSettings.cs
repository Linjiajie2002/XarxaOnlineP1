using System.Collections.Generic;

public static class GameSettings
{
    public static string ServerIP { get; set; }
    public static int ServerPort { get; set; }
    public static bool IsServer { get; set; }
    public static string PlayerName { get; set; } = "Player";

    // 用于存储活跃的服务器端口，确保不重复
    private static HashSet<int> activePorts = new HashSet<int>();

    public static bool TryRegisterPort(int port)
    {
        if (activePorts.Contains(port))
            return false;

        activePorts.Add(port);
        return true;
    }

    public static void UnregisterPort(int port)
    {
        activePorts.Remove(port);
    }
}