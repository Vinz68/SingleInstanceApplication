
namespace SingleInstanceApplication;

using System;
using System.Threading;

/// <summary>
/// This class will handle the logic for checking if the application is already running on this computer.
/// 
/// A Mutes is used to create a named system-wide lock. The name of the Mutex is derived from 
/// the application's unique GUID, ensuring that the check is unique to your application.
/// </summary>
public class SingleInstanceAppWrapper : IDisposable
{
    private readonly Guid _appGuid;     // Unique GUID for the application
    private Mutex? _mutex;              // Hold the mMtex for app lifetime

    public SingleInstanceAppWrapper(Guid appGuid)
    {
        _appGuid = appGuid;
    }

    public bool IsApplicationFirstInstance()
    {
        _mutex = new Mutex(true, $"Global\\{_appGuid}", out var createdNew);
        return createdNew;
    }

    public void Dispose()
    {
        _mutex?.Dispose();
        GC.SuppressFinalize(this);
    }
}
