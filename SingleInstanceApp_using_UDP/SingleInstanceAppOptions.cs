using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleInstanceApp_using_UDP;


public class SingleInstanceAppOptions
{
    /// <summary>
    /// Gets or sets the timeout (in milliseconds) for receiving UDP responses.
    /// Default: 1000 milliseconds.
    /// </summary>
    public int ReceiveTimeout { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the unique GUID for the application.
    /// Default: "ebcb505f-011b-49b7-dbdb-d987a07ebdc8".
    /// </summary>
    public string ApplicationGuid { get; set; } = "ebcb505f-011b-49b7-dbdb-d987a07ebdc8";

    /// <summary>
    /// Gets or sets the name of the application (optional).
    /// If not set, the word "Application" is used in log messages.
    /// </summary>
    public string ApplicationName { get; set; } = "Application";

    /// <summary>
    /// Gets or sets whether to check for other instances on the entire LAN.
    /// If true, uses IPAddress.Broadcast; if false, uses IPAddress.Loopback.
    /// Default: false (check only on localhost).
    /// </summary>
    public bool CheckEntireLan { get; set; } = false;
}
