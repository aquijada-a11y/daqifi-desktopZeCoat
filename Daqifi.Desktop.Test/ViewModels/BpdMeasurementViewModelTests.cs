using Daqifi.Desktop.Bpd;
using Daqifi.Desktop.ViewModels;

namespace Daqifi.Desktop.Test.ViewModels;

[TestClass]
public class BpdMeasurementViewModelTests
{
    private static BpdMeasurementViewModel MakeViewModel(string format = "F2", string unit = "V") =>
        new(new BpdMeasurementDefinition(BpdMeasurementKey.FilamentVoltage, "Filament Voltage", unit, format));

    [TestMethod]
    public void DisplayValue_NoValue_ShowsPlaceholder()
    {
        var vm = MakeViewModel();

        Assert.IsFalse(vm.HasValue);
        Assert.AreEqual("—", vm.DisplayValue);
    }

    [TestMethod]
    public void DisplayValue_WithValue_FormatsUsingDefinitionFormat()
    {
        var vm = MakeViewModel(format: "F2");

        vm.Value = 6.512345;

        Assert.IsTrue(vm.HasValue);
        Assert.AreEqual("6.51", vm.DisplayValue);
    }

    [TestMethod]
    public void DisplayValue_UsesInvariantCulture()
    {
        var original = System.Threading.Thread.CurrentThread.CurrentCulture;
        try
        {
            // A culture that uses ',' as the decimal separator must not leak into the display.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            var vm = MakeViewModel(format: "F1");

            vm.Value = 3.5;

            Assert.AreEqual("3.5", vm.DisplayValue);
        }
        finally
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = original;
        }
    }

    [TestMethod]
    public void Value_SetToNull_RevertsToPlaceholder()
    {
        var vm = MakeViewModel();
        vm.Value = 1.0;

        vm.Value = null;

        Assert.IsFalse(vm.HasValue);
        Assert.AreEqual("—", vm.DisplayValue);
    }

    [TestMethod]
    public void SettingValue_RaisesChangeNotificationsForDependentProperties()
    {
        var vm = MakeViewModel();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Value = 2.0;

        CollectionAssert.Contains(changed, nameof(BpdMeasurementViewModel.DisplayValue));
        CollectionAssert.Contains(changed, nameof(BpdMeasurementViewModel.HasValue));
    }

    [TestMethod]
    public void LabelAndUnit_PassThroughFromDefinition()
    {
        var vm = MakeViewModel(unit: "°C");

        Assert.AreEqual("Filament Voltage", vm.Label);
        Assert.AreEqual("°C", vm.Unit);
    }
}
