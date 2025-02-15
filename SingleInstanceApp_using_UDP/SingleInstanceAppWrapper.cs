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
    private readonly SingleInstanceAppOptions _options;
    private readonly ILogger _logger;
    private UdpClient? _udpClientToReceiveRequests;
    private readonly UdpClient _udpClientToSendRequests;
    private CancellationTokenSource _cts;

    public SingleInstanceAppWrapper(ILogger logger,SingleInstanceAppOptions? options = null)
    {
        _logger = logger;
        _options = options ?? new SingleInstanceAppOptions();

        _udpClientToSendRequests = new UdpClient(_options.UdpPort + 1);
        _udpClientToSendRequests.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _udpClientToSendRequests.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);

        _cts = new CancellationTokenSource();
    }

    /// <summary>
    /// Checks if the application is the first instance running on the local computer or on the entire LAN.
    /// </summary>
    /// <returns>true when application is first instance, false when application is already running</returns>
    public async Task<bool> IsApplicationFirstInstanceAsync()
    {
        // Determine the IP address to use based on the CheckEntireLan option
        IPAddress ipAddress = _options.CheckEntireLan ? IPAddress.Broadcast : IPAddress.Loopback;

        // Send ApplicationAliveRequest
        string requestMessage = $"ApplicationAliveRequest:{_options.ApplicationGuid}";
        byte[] requestBytes = Encoding.UTF8.GetBytes(requestMessage);
        await _udpClientToSendRequests.SendAsync(requestBytes, requestBytes.Length, new IPEndPoint(ipAddress, _options.UdpPort));

        try
        {
            using CancellationTokenSource cts = new(_options.ReceiveTimeout);
            UdpReceiveResult result = await _udpClientToSendRequests.ReceiveAsync(cts.Token);
            string responseMessage = Encoding.UTF8.GetString(result.Buffer);

            if (responseMessage == $"ApplicationAliveResponse:{_options.ApplicationGuid}")
                return false; // Another instance is running
        }
        catch (OperationCanceledException)
        {
            // Timeout occurred via cancellation, indicating no other instance is running
            return true;
        }
        catch (SocketException)
        {
            // Other socket errors; defaulting to true
            return true;
        }
        finally
        {
            _udpClientToSendRequests.Close();
        }

        return true;
    }

    public void StartListeningForApplicationAliveRequests()
    {
        // Set the ReuseAddress option to allow multiple instances to bind to the same address and port
        _udpClientToReceiveRequests = new UdpClient(new IPEndPoint(IPAddress.Any, _options.UdpPort));
        _udpClientToReceiveRequests.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _udpClientToReceiveRequests.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);

        _ = ListenForApplicationAliveRequests(_cts.Token);
    }

    private async Task ListenForApplicationAliveRequests(CancellationToken stopToken)
    {
        while (!stopToken.IsCancellationRequested)
        {
            try
            {
                // Listen for incoming UDP messages
                UdpReceiveResult result = await _udpClientToReceiveRequests!.ReceiveAsync(stopToken);
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
                        await _udpClientToReceiveRequests!.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint);
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

        _udpClientToSendRequests?.Close();
        _udpClientToReceiveRequests?.Close();
        _cts?.Dispose();
    }
}
