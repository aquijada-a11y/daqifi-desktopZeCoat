using System.Collections.Generic;

namespace Daqifi.Desktop.Bpd;

/// <summary>
/// Identifies each named BPD process measurement requested for the ZeCoat
/// workflow. The enum ordering is the canonical display order on the pane.
/// </summary>
public enum BpdMeasurementKey
{
    GasTemperature,
    GasPressure,
    Battery1Temperature,
    Battery2Temperature,
    CircuitBoardTemperature,
    BatteryVoltage,
    FilamentVoltage,
    FilamentCurrent,
}

/// <summary>
/// Static, display-level description of a single BPD measurement: its label,
/// engineering unit, and numeric format. Deliberately holds no live value —
/// live values live on the <see cref="ViewModels.BpdMeasurementViewModel"/>.
/// </summary>
/// <param name="Key">Stable identifier for the measurement.</param>
/// <param name="Label">Human-readable label shown on the tile.</param>
/// <param name="Unit">Engineering unit suffix (e.g. "°C", "V", "A", "Torr").</param>
/// <param name="Format">Standard .NET numeric format string for the value (e.g. "F1").</param>
public sealed record BpdMeasurementDefinition(
    BpdMeasurementKey Key,
    string Label,
    string Unit,
    string Format);

/// <summary>
/// The canonical catalog of BPD measurements requested for the ZeCoat BPD
/// workflow. Units are provisional and should be confirmed against the C5
/// firmware / sensor calibration once finalized.
/// </summary>
public static class BpdMeasurementCatalog
{
    /// <summary>
    /// The eight named measurements, in canonical display order.
    /// </summary>
    public static IReadOnlyList<BpdMeasurementDefinition> All { get; } =
    [
        new(BpdMeasurementKey.GasTemperature,          "Gas Temp",         "°C",   "F1"),
        new(BpdMeasurementKey.GasPressure,             "Gas Pressure",     "Torr", "F3"),
        new(BpdMeasurementKey.Battery1Temperature,     "Battery 1 Temp",   "°C",   "F1"),
        new(BpdMeasurementKey.Battery2Temperature,     "Battery 2 Temp",   "°C",   "F1"),
        new(BpdMeasurementKey.CircuitBoardTemperature, "Board Temp",       "°C",   "F1"),
        new(BpdMeasurementKey.BatteryVoltage,          "Battery Voltage",  "V",    "F2"),
        new(BpdMeasurementKey.FilamentVoltage,         "Filament Voltage", "V",    "F2"),
        new(BpdMeasurementKey.FilamentCurrent,         "Filament Current", "A",    "F2"),
    ];
}
