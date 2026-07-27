using Daqifi.Desktop.Device;

namespace Daqifi.Desktop.Logger;

/// <summary>
/// Source of device-level status frames, raised for every frame decoded off the
/// stream regardless of whether a logging session is active. Abstracted from
/// <see cref="LoggingManager"/> so consumers (e.g. the BPD pane) can be unit
/// tested without the application's DI container.
/// </summary>
public interface IDeviceMessageSource
{
    /// <summary>Raised on each decoded device status frame.</summary>
    event EventHandler<DeviceMessage>? DeviceMessageReceived;
}
