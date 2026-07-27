namespace Daqifi.Desktop.Bpd;

/// <summary>
/// The BPD (Batch Physical Deposition) workflow states surfaced on the BPD
/// diagnostics pane. These are process-level states specific to the ZeCoat
/// BPD workflow — they are broader than the desktop app's transport-level
/// <see cref="DAQiFiConnectionStatus"/> and are intended to be driven by a
/// status field reported by the C5 firmware.
/// </summary>
/// <remarks>
/// The mapping from the raw device status integer to these values lives in
/// <see cref="BpdStatusMapper"/>. The integer codes there are provisional and
/// must be reconciled with the C5 firmware once its status field is finalized.
/// </remarks>
public enum BpdStatus
{
    /// <summary>No run in progress; the device is idle / low-power.</summary>
    Sleeping,

    /// <summary>Link to the device is established but no run is staged.</summary>
    Connected,

    /// <summary>A run is staged and the device is ready to start.</summary>
    Armed,

    /// <summary>A deposition run is actively executing.</summary>
    Running,

    /// <summary>The most recent run finished successfully.</summary>
    RunComplete,

    /// <summary>The device or run reported a fault condition.</summary>
    Error,
}
