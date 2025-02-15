namespace SingleInstanceApp_using_UDP;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

/// <summary>
/// This example demonstrates how to prevent more than one instance of an application 
/// from running at the same time on the local host (this computer), or on the entire LAN.
/// 
/// Note: This example uses UDP messages to check if the application is already running.
///       The default listen UDP port is 56253, but you can change it in the options.
///       It uses also thus UDP port +1 for sending/receiving UDP messages.
///       These ports must be open in the firewall.
///       
/// This example contains all source code.
/// 
/// Most SingleInstanceAppOptions are optional, but you should set the ApplicationGuid to a unique GUID.
/// 
/// </summary>

internal class Program
{
    static async Task Main(string[] args)
    {
        // Create a logger (you can use any logging framework that implements ILogger)
        ILogger logger = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(); // Add console logging
        }).CreateLogger<Program>();

        // Configure options
         var options = new SingleInstanceAppOptions
        {
            ApplicationName = "SingleInstanceApp_using_UDP",            // Optional application name
            ReceiveTimeout = 2000,                                      // Override default timeout of expected UDP response
            ApplicationGuid = "1aee6cb7-ef42-4d78-97ba-ac8ae744c4a5",   // Our application GUID - you should set the ApplicationGuid to a new unique GUID.
            CheckEntireLan = false // Set to true to check the entire LAN, false for localhost only
        };

        using (var singleInstanceWrapper = new SingleInstanceAppWrapper(logger, options))
        {
            if (!await singleInstanceWrapper.IsApplicationFirstInstanceAsync())
            {
                logger.LogInformation("{ApplicationName} is already running. Closing this instance...", options.ApplicationName);
                return; // Exit if another instance is running
            }

            // Application is not running, proceed with the main logic
            logger.LogInformation("{ApplicationName} started successfully.",
                string.IsNullOrEmpty(options.ApplicationName) ? "Application" : options.ApplicationName);

            // Start listening for ApplicationAliveRequest messages
            singleInstanceWrapper.StartListeningForApplicationAliveRequests();

            Console.WriteLine($"{options.ApplicationName} is running... Press Enter to exit.");
            Console.ReadLine();
        }
    }
}