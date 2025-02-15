# SingleInstanceApplication

This repository contains 2 different ways of single instance applications in C#.

It is platform independent and can be run on Windows, Linux and MacOS.

The usage is simple in both examples:
```csharp   
    var singleInstanceWrapper = new SingleInstanceAppWrapper()

    // Check if the application is already running
    if (!singleInstanceWrapper.IsApplicationFirstInstance())
    {
        Console.WriteLine("Application is already running.");
        return;
    }
```

The examples are using the [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting) library for the host builder to use ILogger for logging output, but this is not required. 
You can use the examples without the Microsoft.Extensions.Hosting library by replacing the ILogger with Console.WriteLine.


## SingleInstanceApp_using_Mutex
This example project demonstrates how to prevent more than one instance of an application from running at the same time on a computer.
It is using a [Mutex](https://learn.microsoft.com/en-us/dotnet/api/system.threading.mutex?view=net-9.0) to create a named system-wide lock.


## SingleInstanceApp_using_UDP
This example project demonstrates how to prevent more than one instance of an application 
from running at the same time on the local host (this computer), or on the entire LAN.

This example uses [UDP messages](https://learn.microsoft.com/en-us/dotnet/framework/network-programming/using-udp-services) to check if the application is already running.
The default listen UDP port is 56253, but you can change it in the options.
It uses also UDP port +1 for sending and receiving UDP messages.
These ports must be open in the firewall, when running on the LAN.
       
Most SingleInstanceAppOptions are optional, but you should set the ApplicationGuid to a unique GUID.


