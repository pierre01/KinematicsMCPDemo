using System.Globalization;
using KinematicsDemo.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KinematicsDemoTests.Views;

[TestClass]
public class InvertDoubleConverterTests
{
    private readonly InvertDoubleConverter _converter = new();

    [TestMethod]
    public void Convert_InvertsRenderCoordinate()
    {
        var renderedOffset = _converter.Convert(150d, typeof(double), null!, CultureInfo.InvariantCulture);

        Assert.AreEqual(-150d, renderedOffset);
    }

    [TestMethod]
    public void ConvertBack_RestoresPhysicalCoordinate()
    {
        var physicalCoordinate = _converter.ConvertBack(-150d, typeof(double), null!, CultureInfo.InvariantCulture);

        Assert.AreEqual(150d, physicalCoordinate);
    }
}
