using Daqifi.Desktop.Bpd;
using Daqifi.Desktop.Device;
using Daqifi.Desktop.Logger;
using Daqifi.Desktop.ViewModels;

namespace Daqifi.Desktop.Test.ViewModels;

[TestClass]
public class BpdPaneViewModelTests
{
    /// <summary>Test double that lets a test raise device status frames on demand.</summary>
    private sealed class FakeDeviceMessageSource : IDeviceMessageSource
    {
        public event EventHandler<DeviceMessage>? DeviceMessageReceived;

        public void Raise(int deviceStatus) =>
            DeviceMessageReceived?.Invoke(this, new DeviceMessage { DeviceStatus = deviceStatus });

        public bool HasSubscribers => DeviceMessageReceived != null;
    }

    private static BpdPaneViewModel MakeViewModel(IDeviceMessageSource source) => new(source);

    [TestMethod]
    public void Constructor_CreatesAllEightMeasurementTiles()
    {
        using var vm = MakeViewModel(new FakeDeviceMessageSource());

        Assert.AreEqual(8, vm.Measurements.Count);
        CollectionAssert.AreEquivalent(
            Enum.GetValues<BpdMeasurementKey>(),
            vm.Measurements.Select(m => m.Key).ToArray());
    }

    [TestMethod]
    public void StatusCaption_TracksCurrentStatus()
    {
        using var vm = MakeViewModel(new FakeDeviceMessageSource());

        vm.CurrentStatus = BpdStatus.RunComplete;

        Assert.AreEqual("Run Complete", vm.StatusCaption);
    }

    [TestMethod]
    public void ApplyMeasurement_SetsValueOnMatchingTile()
    {
        using var vm = MakeViewModel(new FakeDeviceMessageSource());

        vm.ApplyMeasurement(BpdMeasurementKey.FilamentCurrent, 7.5);

        var tile = vm.Measurements.Single(m => m.Key == BpdMeasurementKey.FilamentCurrent);
        Assert.AreEqual(7.5, tile.Value);
        // Other tiles remain unset.
        Assert.IsFalse(vm.Measurements.Single(m => m.Key == BpdMeasurementKey.GasTemperature).HasValue);
    }

    [TestMethod]
    public void ApplyMeasurement_Null_ClearsValue()
    {
        using var vm = MakeViewModel(new FakeDeviceMessageSource());
        vm.ApplyMeasurement(BpdMeasurementKey.BatteryVoltage, 12.1);

        vm.ApplyMeasurement(BpdMeasurementKey.BatteryVoltage, null);

        Assert.IsFalse(vm.Measurements.Single(m => m.Key == BpdMeasurementKey.BatteryVoltage).HasValue);
    }

    [TestMethod]
    public void RefreshOnce_NoDeviceStatusYet_ReportsSleeping()
    {
        using var vm = MakeViewModel(new FakeDeviceMessageSource());

        vm.RefreshOnce();

        Assert.AreEqual(BpdStatus.Sleeping, vm.CurrentStatus);
    }

    [TestMethod]
    public void RefreshOnce_AfterDeviceStatusFrame_MapsStatus()
    {
        var source = new FakeDeviceMessageSource();
        using var vm = MakeViewModel(source);

        // Code 3 -> Running per the provisional mapping.
        source.Raise(deviceStatus: 3);
        vm.RefreshOnce();

        Assert.AreEqual(BpdStatus.Running, vm.CurrentStatus);
    }

    [TestMethod]
    public void RefreshOnce_WhenSimulating_PopulatesMeasurementValues()
    {
        using var vm = new BpdPaneViewModel(new FakeDeviceMessageSource()) { IsSimulationEnabled = true };

        vm.RefreshOnce();

        Assert.IsTrue(vm.Measurements.All(m => m.HasValue),
            "Simulation should populate every measurement tile.");
    }

    [TestMethod]
    public void Dispose_UnsubscribesFromDeviceMessages()
    {
        var source = new FakeDeviceMessageSource();
        var vm = MakeViewModel(source);
        Assert.IsTrue(source.HasSubscribers);

        vm.Dispose();

        Assert.IsFalse(source.HasSubscribers, "Dispose should detach the device-message subscription.");

        // A further frame after disposal must not change state on a manual refresh.
        source.Raise(deviceStatus: 5);
        vm.RefreshOnce(); // guarded by _disposed -> no-op
        Assert.AreEqual(BpdStatus.Sleeping, vm.CurrentStatus);
    }
}
