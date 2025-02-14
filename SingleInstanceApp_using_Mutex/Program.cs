namespace SingleInstanceApplication;

using Microsoft.Extensions.Logging;
using System;

internal class Program
{

    /// <summary>
    /// This example demonstrates how to prevent more than one instance of an application from running at the same time on a computer.
    /// Simple example with all source code in a single file "SingleInstanceAppWrapper.cs".
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
        // Replace the string with your application's unique GUID
        // you can use any GUID generator to create one; like: https://guidgenerator.com/
        Guid appGuid = new Guid("ebcb505f-011b-49b7-dbdb-d987a07ebdc8");

        // Create a logger (you can use any logging framework that implements ILogger)
        ILogger logger = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(); // Add console logging
        }).CreateLogger<Program>();

        // Create an instance of the SingleInstanceAppWrapper
        var singleInstanceWrapper = new SingleInstanceAppWrapper(appGuid);

        // Check if the application is already running
        if (!singleInstanceWrapper.IsApplicationFirstInstance())
        {
            logger.LogInformation("Closing this instance because another instance is already running.");
            return;
        }

        // Application is not running, proceed with the main logic
        logger.LogInformation("Application started successfully.");
        logger.LogInformation("Application is running...");

        logger.LogInformation("Press any key to exit.");
        Console.ReadLine();

        // clean up
        singleInstanceWrapper.Dispose();
    }
}