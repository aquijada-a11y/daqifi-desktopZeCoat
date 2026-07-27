using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Daqifi.Desktop.Bpd;
using Daqifi.Desktop.Device;
using Daqifi.Desktop.Logger;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace Daqifi.Desktop.ViewModels;

/// <summary>
/// Backs the BPD diagnostics pane: the eight named process measurements and the
/// BPD workflow status indicator (sleeping / connected / armed / running /
/// run complete / error).
/// </summary>
/// <remarks>
/// <para><b>Status</b> is driven by the device's status field. Every stream
/// frame decodes a <c>DeviceStatus</c> integer into a <see cref="DeviceMessage"/>;
/// this pane subscribes to <see cref="LoggingManager.DeviceMessageReceived"/>
/// (which fires whether or not a session is recording) and maps that integer
/// through <see cref="BpdStatusMapper"/>.</para>
/// <para><b>Measurements</b> are scaffolded: the eight tiles and their formatting
/// are wired, but the C5 firmware does not yet stream these named values. Until
/// it does, enable <see cref="IsSimulationEnabled"/> to demo the pane with
/// synthetic data. To wire real values later, call
/// <see cref="ApplyMeasurement"/> from the device decode path (or replace the
/// simulation block) — nothing else needs to change.</para>
/// <para>The value refresh mirrors the Channels pane: a UI-thread timer applies
/// the latest device status and (when simulating) generates values, so the hot
/// streaming thread never touches the UI.</para>
/// </remarks>
public partial class BpdPaneViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer _refreshTimer;
    private readonly IDeviceMessageSource _messageSource;
    private readonly Random _random = new();
    private readonly object _statusLock = new();
    private int? _latestDeviceStatus;
    private int _simTick;
    private bool _disposed;

    /// <summary>The eight named BPD measurement tiles, in catalog order.</summary>
    public ObservableCollection<BpdMeasurementViewModel> Measurements { get; } = [];

    /// <summary>The current BPD workflow status.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusCaption))]
    [NotifyPropertyChangedFor(nameof(StatusBrush))]
    private BpdStatus _currentStatus = BpdStatus.Sleeping;

    /// <summary>
    /// When true, the pane generates synthetic measurement values and cycles the
    /// status, for demonstrating the UI before firmware streams real data.
    /// </summary>
    [ObservableProperty]
    private bool _isSimulationEnabled;

    /// <summary>Short caption for the current status (e.g. "Run Complete").</summary>
    public string StatusCaption => BpdStatusMapper.GetCaption(CurrentStatus);

    /// <summary>Design-token brush for the status dot, resolved defensively.</summary>
    public Brush StatusBrush => ResolveBrush(BpdStatusMapper.GetBrushKey(CurrentStatus));

    /// <summary>
    /// Creates the pane view-model and begins listening for status frames.
    /// </summary>
    /// <param name="messageSource">
    /// Source of device status frames. Defaults to <see cref="LoggingManager.Instance"/>
    /// in production; tests supply a fake to avoid the application DI container.
    /// </param>
    public BpdPaneViewModel(IDeviceMessageSource? messageSource = null)
    {
        _messageSource = messageSource ?? LoggingManager.Instance;

        foreach (var definition in BpdMeasurementCatalog.All)
        {
            Measurements.Add(new BpdMeasurementViewModel(definition));
        }

        _messageSource.DeviceMessageReceived += OnDeviceMessageReceived;

        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _refreshTimer.Tick += OnRefreshTick;
        _refreshTimer.Start();
    }

    /// <summary>
    /// Records the latest device status off the streaming thread. The value is
    /// applied to the UI on the next refresh tick so we never mutate UI state
    /// from a background thread.
    /// </summary>
    private void OnDeviceMessageReceived(object? sender, DeviceMessage message)
    {
        lock (_statusLock)
        {
            _latestDeviceStatus = message.DeviceStatus;
        }
    }

    private void OnRefreshTick(object? sender, EventArgs e) => RefreshOnce();

    /// <summary>
    /// Applies one refresh: generates simulated data when simulation is on,
    /// otherwise maps the latest device status frame to <see cref="CurrentStatus"/>.
    /// Extracted from the timer tick so it can be exercised deterministically.
    /// </summary>
    internal void RefreshOnce()
    {
        if (_disposed) return;

        if (IsSimulationEnabled)
        {
            StepSimulation();
            return;
        }

        int? raw;
        lock (_statusLock)
        {
            raw = _latestDeviceStatus;
        }

        CurrentStatus = raw.HasValue
            ? BpdStatusMapper.FromDeviceStatus(raw.Value)
            : BpdStatus.Sleeping;
    }

    /// <summary>
    /// Applies a live value to the tile for <paramref name="key"/>. This is the
    /// seam for wiring real firmware data: call it from the device decode path
    /// once the C5 streams these named measurements. Passing <c>null</c> clears
    /// the reading back to the placeholder.
    /// </summary>
    public void ApplyMeasurement(BpdMeasurementKey key, double? value)
    {
        foreach (var measurement in Measurements)
        {
            if (measurement.Key == key)
            {
                measurement.Value = value;
                return;
            }
        }
    }

    #region Simulation (demo only)

    // Per-measurement (baseline, amplitude) used to generate plausible synthetic
    // readings. Demo only — delete alongside StepSimulation once real values are wired.
    private static (double Baseline, double Amplitude) SimProfile(BpdMeasurementKey key) => key switch
    {
        BpdMeasurementKey.GasTemperature          => (32, 6),
        BpdMeasurementKey.GasPressure             => (0.005, 0.002),
        BpdMeasurementKey.Battery1Temperature     => (30, 4),
        BpdMeasurementKey.Battery2Temperature     => (31, 4),
        BpdMeasurementKey.CircuitBoardTemperature => (42, 8),
        BpdMeasurementKey.BatteryVoltage          => (12.2, 0.4),
        BpdMeasurementKey.FilamentVoltage         => (6.5, 1.5),
        BpdMeasurementKey.FilamentCurrent         => (8.0, 2.0),
        _                                          => (0, 1),
    };

    private void StepSimulation()
    {
        foreach (var measurement in Measurements)
        {
            var (baseline, amplitude) = SimProfile(measurement.Key);
            // Smooth wander around the baseline.
            var noise = (_random.NextDouble() - 0.5) * 2 * amplitude;
            measurement.Value = baseline + noise;
        }

        // Cycle the workflow status roughly every 4 seconds (200 ms tick x 20).
        _simTick++;
        if (_simTick % 20 == 0)
        {
            var states = Enum.GetValues<BpdStatus>();
            CurrentStatus = states[(_simTick / 20) % states.Length];
        }
    }

    #endregion

    private static Brush ResolveBrush(string key)
    {
        if (Application.Current?.TryFindResource(key) is Brush brush)
        {
            return brush;
        }

        // Fallback for design-time / unit-test contexts with no resource dictionary.
        var gray = new SolidColorBrush(Color.FromRgb(0x5A, 0x5E, 0x66));
        gray.Freeze();
        return gray;
    }

    /// <summary>Stops the timer and detaches the status subscription.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _refreshTimer.Stop();
        _refreshTimer.Tick -= OnRefreshTick;
        _messageSource.DeviceMessageReceived -= OnDeviceMessageReceived;

        GC.SuppressFinalize(this);
    }
}
