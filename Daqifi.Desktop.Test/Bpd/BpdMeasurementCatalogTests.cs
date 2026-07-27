using Daqifi.Desktop.Bpd;

namespace Daqifi.Desktop.Test.Bpd;

[TestClass]
public class BpdMeasurementCatalogTests
{
    [TestMethod]
    public void All_ContainsTheEightRequestedMeasurements()
    {
        Assert.AreEqual(8, BpdMeasurementCatalog.All.Count);
    }

    [TestMethod]
    public void All_KeysAreUnique()
    {
        var keys = BpdMeasurementCatalog.All.Select(m => m.Key).ToList();
        Assert.AreEqual(keys.Count, keys.Distinct().Count(), "Duplicate measurement keys found");
    }

    [TestMethod]
    public void All_CoversEveryMeasurementKey()
    {
        var present = BpdMeasurementCatalog.All.Select(m => m.Key).ToHashSet();
        foreach (var key in Enum.GetValues<BpdMeasurementKey>())
        {
            Assert.IsTrue(present.Contains(key), $"Catalog missing {key}");
        }
    }

    [TestMethod]
    public void All_EveryEntryHasLabelUnitAndFormat()
    {
        foreach (var definition in BpdMeasurementCatalog.All)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(definition.Label), $"{definition.Key} has no label");
            Assert.IsFalse(string.IsNullOrWhiteSpace(definition.Unit), $"{definition.Key} has no unit");
            Assert.IsFalse(string.IsNullOrWhiteSpace(definition.Format), $"{definition.Key} has no format");
        }
    }

    [TestMethod]
    public void All_FilamentCurrent_IsAmps()
    {
        var filamentCurrent = BpdMeasurementCatalog.All.Single(m => m.Key == BpdMeasurementKey.FilamentCurrent);
        Assert.AreEqual("A", filamentCurrent.Unit);
    }
}
