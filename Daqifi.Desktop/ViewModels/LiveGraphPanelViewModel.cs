using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Brush = System.Windows.Media.Brush;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Daqifi.Desktop.Channel;
using Daqifi.Desktop.Logger;
using OxyPlot;

namespace Daqifi.Desktop.ViewModels;

/// <summary>
/// One channel's on/off toggle for a single live graph. Visibility is tracked per graph
/// (see <see cref="LiveGraphPanelViewModel"/>), so the same channel can be shown on one
/// graph and hidden on another.
/// </summary>
public partial class LiveGraphChannelToggleViewModel : ObservableObject
{
    /// <summary>Creates a toggle for <paramref name="channel"/> seeded to <paramref name="isVisible"/>.</summary>
    public LiveGraphChannelToggleViewModel(IChannel channel, bool isVisible)
    {
        Channel = channel;
        _isVisible = isVisible;
    }

    /// <summary>The underlying channel this toggle controls.</summary>
    public IChannel Channel { get; }

    /// <summary>Channel name shown on the chip.</summary>
    public string Name => Channel.Name;

    /// <summary>Owning device serial number (disambiguates same-named channels across devices).</summary>
    public string DeviceSerialNo => Channel.DeviceSerialNo;

    /// <summary>The channel's plot color, used for the chip's swatch.</summary>
    public Brush ColorBrush => Channel.ChannelColorBrush;

    /// <summary>Whether this channel is shown on this graph. Per-graph state.</summary>
    [ObservableProperty]
    private bool _isVisible;
}

/// <summary>
/// Backs one panel of the live-graph grid: its own OxyPlot model (via a dedicated
/// <see cref="PlotLogger"/>), a per-graph list of channel toggles, and the plot's
/// zoom/save commands. Each panel is fully independent — toggling a channel here does not
/// affect the other graphs.
/// </summary>
public partial class LiveGraphPanelViewModel : ObservableObject, IDisposable
{
    private readonly ObservableCollection<IChannel> _channelSource;
    private bool _disposed;

    /// <summary>
    /// Creates a panel bound to <paramref name="plotter"/>, drawing its toggle list from the
    /// shared <paramref name="channelSource"/> (the app's active input channels).
    /// </summary>
    public LiveGraphPanelViewModel(int index, PlotLogger plotter, ObservableCollection<IChannel> channelSource)
    {
        Index = index;
        Plotter = plotter;
        _channelSource = channelSource;

        _channelSource.CollectionChanged += OnChannelSourceChanged;
        RebuildChannels();
    }

    /// <summary>Zero-based position of this graph in the grid.</summary>
    public int Index { get; }

    /// <summary>Display title, e.g. "Graph 1".</summary>
    public string Title => $"Graph {Index + 1}";

    /// <summary>This graph's plot engine. Exposes the PlotModel and the zoom/save commands.</summary>
    public PlotLogger Plotter { get; }

    /// <summary>The OxyPlot model rendered by this panel's PlotView.</summary>
    public PlotModel PlotModel => Plotter.PlotModel;

    /// <summary>Channels available to toggle on this graph, each with its own per-graph visibility.</summary>
    public ObservableCollection<LiveGraphChannelToggleViewModel> Channels { get; } = [];

    /// <summary>Toggles one channel's visibility on THIS graph only.</summary>
    [RelayCommand]
    private void ToggleChannel(LiveGraphChannelToggleViewModel toggle)
    {
        if (toggle == null) { return; }

        toggle.IsVisible = !toggle.IsVisible;
        Plotter.SetChannelVisibility(toggle.DeviceSerialNo, toggle.Name, toggle.IsVisible);
    }

    private void OnChannelSourceChanged(object sender, NotifyCollectionChangedEventArgs e) => RebuildChannels();

    /// <summary>
    /// Rebuilds the toggle list from the shared active-channel source, preserving this graph's
    /// existing per-channel visibility (kept on the <see cref="PlotLogger"/>) across rebuilds.
    /// The source collection is cleared and re-added when the subscribed-channel set changes.
    /// </summary>
    private void RebuildChannels()
    {
        Channels.Clear();
        foreach (var channel in _channelSource)
        {
            var visible = Plotter.IsChannelVisible(channel.DeviceSerialNo, channel.Name);
            Channels.Add(new LiveGraphChannelToggleViewModel(channel, visible));
        }
    }

    /// <summary>Detaches the channel-source subscription.</summary>
    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;

        _channelSource.CollectionChanged -= OnChannelSourceChanged;
        GC.SuppressFinalize(this);
    }
}
