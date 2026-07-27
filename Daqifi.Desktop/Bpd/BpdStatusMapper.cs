using System.Collections.Generic;

namespace Daqifi.Desktop.Bpd;

/// <summary>
/// Translates the raw device status integer reported by the C5 firmware into a
/// process-level <see cref="BpdStatus"/>, and provides display metadata
/// (caption + design-token brush key) for each status.
/// </summary>
/// <remarks>
/// <para>
/// The desktop app already decodes a <c>DeviceStatus</c> integer off every
/// stream frame (see <c>DeviceMessage.DeviceStatus</c>, populated in
/// <c>AbstractStreamingDevice.ProcessStreamMessage</c>). This mapper is the
/// single, editable seam that turns that integer into a BPD workflow state.
/// </para>
/// <para>
/// The integer codes in <see cref="CodeToStatus"/> are <b>provisional</b>: the
/// v0.1-beta C5 firmware only streams basic voltage/DIO and does not yet define
/// these codes. When Chris finalizes the firmware status field, update this one
/// table (and its tests) to match — no other code needs to change.
/// </para>
/// </remarks>
public static class BpdStatusMapper
{
    /// <summary>Status used when the device reports a code we do not recognize.</summary>
    public const BpdStatus DefaultStatus = BpdStatus.Connected;

    /// <summary>
    /// Provisional mapping of the firmware <c>DeviceStatus</c> integer to a
    /// <see cref="BpdStatus"/>. Reconcile with the C5 firmware once its status
    /// field is finalized. Unknown codes fall back to <see cref="DefaultStatus"/>.
    /// </summary>
    public static IReadOnlyDictionary<int, BpdStatus> CodeToStatus { get; } =
        new Dictionary<int, BpdStatus>
        {
            [0] = BpdStatus.Sleeping,
            [1] = BpdStatus.Connected,
            [2] = BpdStatus.Armed,
            [3] = BpdStatus.Running,
            [4] = BpdStatus.RunComplete,
            [5] = BpdStatus.Error,
        };

    /// <summary>
    /// Maps a raw firmware status code to a <see cref="BpdStatus"/>, returning
    /// <see cref="DefaultStatus"/> for any unrecognized code.
    /// </summary>
    public static BpdStatus FromDeviceStatus(int rawStatus) =>
        CodeToStatus.TryGetValue(rawStatus, out var status) ? status : DefaultStatus;

    /// <summary>Short caption shown next to the status indicator.</summary>
    public static string GetCaption(BpdStatus status) => status switch
    {
        BpdStatus.Sleeping => "Sleeping",
        BpdStatus.Connected => "Connected",
        BpdStatus.Armed => "Armed",
        BpdStatus.Running => "Running",
        BpdStatus.RunComplete => "Run Complete",
        BpdStatus.Error => "Error",
        _ => "Unknown",
    };

    /// <summary>
    /// Design-token brush key (see <c>Resources/DesignTokens.xaml</c>) for the
    /// status dot/pill. Grouped by semantic meaning: idle/neutral, active,
    /// success, and fault.
    /// </summary>
    public static string GetBrushKey(BpdStatus status) => status switch
    {
        BpdStatus.Sleeping => "TextTertiary",
        BpdStatus.Connected => "Accent",
        BpdStatus.Armed => "StatusAmber",
        BpdStatus.Running => "Accent",
        BpdStatus.RunComplete => "StatusGreen",
        BpdStatus.Error => "StatusRed",
        _ => "TextTertiary",
    };
}
