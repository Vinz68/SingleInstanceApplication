namespace SingleInstanceApp_using_UDP;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

class Program
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
            ApplicationGuid = "0dc5b292-1f3d-4a58-dbdb-ab1fa9215e33",   // Our application GUID
            CheckEntireLan = true // Set to true to check the entire LAN, false for localhost only
        };

        using (var singleInstanceWrapper = new SingleInstanceAppWrapper(options, logger))
        {
            if (!await singleInstanceWrapper.IsApplicationFirstInstanceAsync())
            {
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