using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Daqifi.Desktop.Bpd;

namespace Daqifi.Desktop.ViewModels;

/// <summary>
/// Backs a single named BPD measurement readout tile. Holds the static
/// definition (label/unit/format) plus the latest live value, and exposes a
/// pre-formatted display string for binding.
/// </summary>
public partial class BpdMeasurementViewModel : ObservableObject
{
    private readonly BpdMeasurementDefinition _definition;

    /// <summary>Creates a readout for the given measurement definition.</summary>
    public BpdMeasurementViewModel(BpdMeasurementDefinition definition)
    {
        _definition = definition;
    }

    /// <summary>Stable identifier for the measurement.</summary>
    public BpdMeasurementKey Key => _definition.Key;

    /// <summary>Human-readable label shown on the tile.</summary>
    public string Label => _definition.Label;

    /// <summary>Engineering unit suffix (e.g. "°C", "V").</summary>
    public string Unit => _definition.Unit;

    /// <summary>
    /// Latest value, or <c>null</c> when no reading is available (device not
    /// streaming this measurement). Setting it refreshes <see cref="DisplayValue"/>
    /// and <see cref="HasValue"/>.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayValue))]
    [NotifyPropertyChangedFor(nameof(HasValue))]
    private double? _value;

    /// <summary>True when a live value is present.</summary>
    public bool HasValue => Value.HasValue;

    /// <summary>
    /// The value formatted with the definition's numeric format, or an em dash
    /// placeholder when no reading is available. The unit is shown separately.
    /// </summary>
    public string DisplayValue => Value.HasValue
        ? Value.Value.ToString(_definition.Format, CultureInfo.InvariantCulture)
        : "—";
}
