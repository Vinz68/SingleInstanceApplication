namespace SingleInstanceApp_using_UDP;

public class SingleInstanceAppOptions
{
    /// <summary>
    /// Gets or sets the unique GUID for the application.
    /// Each application should have a unique GUID.
    /// </summary>
    public string ApplicationGuid { get; set; } = "1aee6cb7-ef42-4d78-97ba-ac8ae744c4a5";

    /// <summary>
    /// Gets or sets the name of the application (optional).
    /// If not set, the word "Application" is used in log messages.
    /// </summary>
    public string ApplicationName { get; set; } = "Application";

    /// <summary>
    /// Gets or sets whether to check for other instances on the entire LAN.
    /// If true, uses IPAddress.Broadcast; if false, uses IPAddress.Loopback.
    /// Default: false (check only on local computer).
    /// </summary>
    public bool CheckEntireLan { get; set; } = false;

    /// <summary>
    /// Gets or sets the timeout (in milliseconds) for receiving UDP responses.
    /// Default: 1000 milliseconds.
    /// </summary>
    public int ReceiveTimeout { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the UDP port to use for communication.
    /// Default: 56253.
    /// Note the UDP port +1 is also used for sending/receiving UDP messages.
    /// </summary>
    public int UdpPort { get; set; } = 56_565;
}

