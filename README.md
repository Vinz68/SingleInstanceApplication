# SingleInstanceApplication

This repo contains 2 examples of single instance applications in C#.
It is plafom independent and can be run on Windows, Linux and MacOS.


## SingleInstanceApp_using_Mutex
This example project demonstrates how to prevent more than one instance of an application from running at the same time on a computer.
It is using a Mutex to create a named system-wide lock.


## SingleInstanceApp_using_UDP
This example project demonstrates how to prevent more than one instance of an application 
from running at the same time on the local host (this computer), or on the entire LAN.

This example uses UDP messages to check if the application is already running.
The default listen UDP port is 56253, but you can change it in the options.
It uses also UDP port +1 for sending and receiving UDP messages.
These ports must be open in the firewall, when running on the LAN.
       
Most SingleInstanceAppOptions are optional, but you should set the ApplicationGuid to a unique GUID.


/// This class will handle the logic for checking if the application is already running on this computer or on the LAN
/// 
/// UDP messages are used to check system-wide or lan-wide if application is already active and prevents a 2nd instance.
/// The application's unique GUID is used in the UDP message, ensuring that the check is unique to your application.
/// The GUID and other options can be passed to the constructor "options" variable.
