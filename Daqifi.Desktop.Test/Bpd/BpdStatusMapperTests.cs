using Daqifi.Desktop.Bpd;

namespace Daqifi.Desktop.Test.Bpd;

[TestClass]
public class BpdStatusMapperTests
{
    [DataTestMethod]
    [DataRow(0, BpdStatus.Sleeping)]
    [DataRow(1, BpdStatus.Connected)]
    [DataRow(2, BpdStatus.Armed)]
    [DataRow(3, BpdStatus.Running)]
    [DataRow(4, BpdStatus.RunComplete)]
    [DataRow(5, BpdStatus.Error)]
    public void FromDeviceStatus_KnownCode_ReturnsMappedStatus(int code, BpdStatus expected)
    {
        // Act
        var result = BpdStatusMapper.FromDeviceStatus(code);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow(-1)]
    [DataRow(6)]
    [DataRow(999)]
    public void FromDeviceStatus_UnknownCode_ReturnsDefault(int code)
    {
        // Act
        var result = BpdStatusMapper.FromDeviceStatus(code);

        // Assert
        Assert.AreEqual(BpdStatusMapper.DefaultStatus, result);
    }

    [TestMethod]
    public void CodeToStatus_CoversEveryStatusValue()
    {
        // Arrange
        var mapped = BpdStatusMapper.CodeToStatus.Values.ToHashSet();

        // Assert — every BpdStatus is reachable from some device code.
        foreach (var status in Enum.GetValues<BpdStatus>())
        {
            Assert.IsTrue(mapped.Contains(status), $"No device code maps to {status}");
        }
    }

    [TestMethod]
    public void GetCaption_ReturnsNonEmptyForEveryStatus()
    {
        foreach (var status in Enum.GetValues<BpdStatus>())
        {
            var caption = BpdStatusMapper.GetCaption(status);
            Assert.IsFalse(string.IsNullOrWhiteSpace(caption), $"Empty caption for {status}");
        }
    }

    [TestMethod]
    public void GetCaption_RunComplete_IsHumanReadable()
    {
        Assert.AreEqual("Run Complete", BpdStatusMapper.GetCaption(BpdStatus.RunComplete));
    }

    [TestMethod]
    public void GetBrushKey_ReturnsKnownDesignTokenForEveryStatus()
    {
        // These keys must exist in Resources/DesignTokens.xaml.
        var validKeys = new HashSet<string>
        {
            "TextTertiary", "Accent", "StatusAmber", "StatusGreen", "StatusRed",
        };

        foreach (var status in Enum.GetValues<BpdStatus>())
        {
            var key = BpdStatusMapper.GetBrushKey(status);
            Assert.IsTrue(validKeys.Contains(key), $"Unexpected brush key '{key}' for {status}");
        }
    }

    [TestMethod]
    public void GetBrushKey_Error_IsRed()
    {
        Assert.AreEqual("StatusRed", BpdStatusMapper.GetBrushKey(BpdStatus.Error));
    }
}
