namespace SingleInstanceApp_using_UDP;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


/// <summary>
/// This class will handle the logic for checking if the application is already running on this computer or on the LAN
/// 
/// UDP messages are used to check system-wide or lan-wide if application is already active and prevents a 2nd instance.
/// The application's unique GUID is used in the UDP message, ensuring that the check is unique to your application.
/// The GUID and other options can be passed to the constructor "options" variable.
/// </summary>
public class SingleInstanceAppWrapper : IDisposable
{
    private const int UdpPort = 56_253;
    private readonly SingleInstanceAppOptions _options;
    private readonly ILogger _logger;
    private readonly UdpClient _udpClient;
    private CancellationTokenSource _cts;

    public SingleInstanceAppWrapper(SingleInstanceAppOptions options, ILogger logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Set the ReuseAddress option to allow multiple instances to bind to the same address and port
        _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, UdpPort));
        _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);

        _cts = new CancellationTokenSource();
    }

    public async Task<bool> IsApplicationFirstInstanceAsync()
    {
        // Determine the IP address to use based on the CheckEntireLan option
        IPAddress ipAddress = _options.CheckEntireLan ? IPAddress.Broadcast : IPAddress.Loopback;

        // Send ApplicationAliveRequest
        string requestMessage = $"ApplicationAliveRequest:{_options.ApplicationGuid}";
        byte[] requestBytes = Encoding.UTF8.GetBytes(requestMessage);
        await _udpClient.SendAsync(requestBytes, requestBytes.Length, new IPEndPoint(ipAddress, UdpPort));

        // Wait for a response
        _udpClient.Client.ReceiveTimeout = _options.ReceiveTimeout;
        try
        {
            UdpReceiveResult result = await _udpClient.ReceiveAsync();
            string responseMessage = Encoding.UTF8.GetString(result.Buffer);

            if (responseMessage == $"ApplicationAliveResponse:{_options.ApplicationGuid}")
            {
                _logger.LogInformation("{ApplicationName} is already running. Closing this instance...", _options.ApplicationName);
                return false; // Another instance is running
            }
        }
        catch (SocketException)
        {
            // Timeout occurred, no other instance is running
            return true;
        }

        return true;
    }

    public void StartListeningForApplicationAliveRequests()
    {
        _ = ListenForApplicationAliveRequests(_cts.Token);
    }

    private async Task ListenForApplicationAliveRequests(CancellationToken stopToken)
    {
        while (!stopToken.IsCancellationRequested)
        {
            try
            {
                // Listen for incoming UDP messages
                UdpReceiveResult result = await _udpClient.ReceiveAsync(stopToken);
                string message = Encoding.UTF8.GetString(result.Buffer);

                if (message.StartsWith("ApplicationAliveRequest:"))
                {
                    string receivedGuid = message.Substring("ApplicationAliveRequest:".Length);

                    // Check if the GUID matches
                    if (receivedGuid == _options.ApplicationGuid)
                    {
                        // Send ApplicationAliveResponse
                        string responseMessage = $"ApplicationAliveResponse:{receivedGuid}";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                        await _udpClient.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // The task was canceled, exit the loop
                _logger.LogInformation("UDP listener stopped for {ApplicationName}.", _options.ApplicationName);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while listening for UDP messages in {ApplicationName}.",
                    string.IsNullOrEmpty(_options.ApplicationName) ? "Application" : _options.ApplicationName);
            }
        }
    }

    public void StopListeningForApplicationAliveRequests()
    {
        _cts?.Cancel();
    }

    public void Dispose()
    {
        StopListeningForApplicationAliveRequests();

        _udpClient?.Close();
        _cts?.Dispose();
    }
}
