using KinematicsDemo.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;

namespace KinematicsDemoTests.Models;

[TestClass()]
public class KUtilsTests
{

    double ZeroDegInRad = 0;
    double ZeroDegInRadV2 = Math.PI;
    double TenDegInRad = Math.PI / 18;
    double ThirtyDegInRad = Math.PI / 6;
    double FortyFiveDegInRad = Math.PI / 4;
    double OneEightyDegInRad = Math.PI;
    double ThreeSixtyDegInRad = Math.PI * 2;

    double pi = Math.PI;
    double pi2 = Math.PI / 2;
    const int Precision = 6;

    public KUtilsTests()
    {

    }

    [TestMethod()]
    public void GetDistanceBetweenTwoPointsTest()
    {
        var dist = KUtils.GetDistanceBetweenTwoPoints(new Point(3, 2), new Point(7, 8));
        Assert.IsTrue(7.21 == Math.Round(dist, 2));
        dist = KUtils.GetDistanceBetweenTwoPoints(new Point(4, -2), new Point(-10, 3));
        Assert.IsTrue(15 == Math.Round(dist, 0));

        dist = KUtils.GetDistanceBetweenTwoPoints(new Point(0, -0), new Point(0, 4));
        Assert.IsTrue(4 == dist);
        dist = KUtils.GetDistanceBetweenTwoPoints(new Point(0, 4), new Point(3, 0));
        Assert.IsTrue(5 == dist);
        dist = KUtils.GetDistanceBetweenTwoPoints(new Point(0, 0), new Point(6, 0));
        Assert.IsTrue(6 == dist);

    }

    [TestMethod()]
    public void GetPointsInBetweenTwoPointsTest()
    {
        Point A = new Point(0, 0);
        Point B = new Point(0, 5);
        var pts = KUtils.GetPointsInBetweenTwoPoints(A, B, 5);
        Assert.IsTrue(pts.Count == 5);
    }

    [TestMethod()]
    public void ToDegreeTest()
    {
        var res = KUtils.RadianToDegree(2 * pi);
        Assert.IsTrue(res == 0 || res == 360);
        res = KUtils.RadianToDegree(0);
        Assert.IsTrue(res == 0 || res == 360);

        res = KUtils.RadianToDegree(11 * pi / 6);
        Assert.IsTrue(res == -30);

        res = KUtils.RadianToDegree(3 * pi / 2);
        Assert.IsTrue(res == -90d);

        res = KUtils.RadianToDegree(3 * pi / 2);
        Assert.IsTrue(res == -90d);

        res = KUtils.RadianToDegree(4 * pi / 3);
        res = Math.Round(res, Precision);
        Assert.IsTrue(res == -120D, $"{res} != 120");

        res = KUtils.RadianToDegree(FortyFiveDegInRad);
        res = Math.Round(res, Precision);
        Assert.IsTrue(res == 45, $"{res} != 45");

        res = KUtils.RadianToDegree(pi);
        Assert.IsTrue(res == 180);

        res = KUtils.RadianToDegree(pi);
        Assert.IsTrue(res == 180);

    }

    [TestMethod()]
    public void ToRadianTest()
    {
        var res = KUtils.DegreeToRadian(0);
        Assert.IsTrue(res == 0 || res == Math.PI);

        res = KUtils.DegreeToRadian(90);
        var rVal = Math.Round(res, Precision);
        Assert.IsTrue(rVal == Math.Round(pi2, Precision)); // Test with rounded value to avoid rounding errors
        Assert.IsTrue(res == pi2); // Test with real value  

        res = KUtils.DegreeToRadian(180);
        rVal = Math.Round(res, Precision);
        Assert.IsTrue(rVal == Math.Round(pi, Precision)); // Test with rounded value to avoid rounding errors
        Assert.IsTrue(res == pi); // Test with real value

        res = KUtils.DegreeToRadian(-180);
        rVal = Math.Round(res, Precision);
        Assert.IsTrue(rVal == Math.Round(pi, Precision)); // Test with rounded value to avoid rounding errors
        Assert.IsTrue(res == pi); // Test with real value

        res = KUtils.DegreeToRadian(240);
        rVal = Math.Round(res, Precision);
        Assert.IsTrue(rVal == Math.Round(4 * pi / 3, Precision)); // Test with rounded value to avoid rounding errors
        Assert.IsTrue(res == 4 * pi / 3); // Test with real value

        res = KUtils.DegreeToRadian(-120); // Note to self: this is not a typo, it's a test for negative values
        rVal = Math.Round(res, Precision);
        Assert.IsTrue(rVal == Math.Round(4 * pi / 3, Precision)); // Test with rounded value to avoid rounding errors
        Assert.IsFalse(res == 4D * pi / 3D); // Test with real value is not precise enough and comparison fails

    }

    [TestMethod()]
    public void GetClosestAngleBetweenTwoAnglesTest()
    {

        var res = KUtils.GetClosestAngleBetweenTwoAngles(ZeroDegInRad, -5, 5);
        Assert.IsTrue(res == 0 || res == Math.PI);

        res = KUtils.GetClosestAngleBetweenTwoAngles(ZeroDegInRadV2, -5, 5);
        res = KUtils.RadianToDegree(res);
        Assert.IsTrue( res == 5 );

        res = KUtils.GetClosestAngleBetweenTwoAngles(TenDegInRad, -5, 5);
        res = KUtils.RadianToDegree(res);
        Assert.IsTrue(res == 5);

        res = KUtils.GetClosestAngleBetweenTwoAngles(ThirtyDegInRad, -5, 45);
        res = KUtils.RadianToDegree(res);
        var rVal = Math.Round(res, Precision);
        Assert.IsTrue(rVal == 30);

        res = KUtils.GetClosestAngleBetweenTwoAngles(ThirtyDegInRad, 40, 90);
        res = KUtils.RadianToDegree(res);
        rVal = Math.Round(res, Precision);
        Assert.IsTrue(rVal == 40);

        res = KUtils.GetClosestAngleBetweenTwoAngles(ThirtyDegInRad, 5, 10);
        res = KUtils.RadianToDegree(res);
        rVal = Math.Round(res, Precision);
        Assert.IsTrue(rVal == 10);

        res = KUtils.GetClosestAngleBetweenTwoAngles(OneEightyDegInRad, -93, 93);
        res = KUtils.RadianToDegree(res);
        rVal = Math.Round(res, Precision);
        Assert.AreEqual(93, rVal);

        res = KUtils.GetClosestAngleBetweenTwoAngles(OneEightyDegInRad, -168, 168);
        res = KUtils.RadianToDegree(res);
        rVal = Math.Round(res, Precision);
        Assert.AreEqual(168, rVal);

        res = KUtils.GetClosestAngleBetweenTwoAngles(OneEightyDegInRad, -960, 960);
        res = KUtils.RadianToDegree(res);
        rVal = Math.Round(res, Precision);
        Assert.IsTrue(rVal == 180);

        res = KUtils.GetClosestAngleBetweenTwoAngles(ThreeSixtyDegInRad, -960, 960);
        res = KUtils.RadianToDegree(res);
        rVal = Math.Round(res, Precision);
        Assert.AreEqual(0, rVal);

    }

    [TestMethod]
    public void GetClosestAngleBetweenTwoAngles_MinGreaterThanMax_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => KUtils.GetClosestAngleBetweenTwoAngles(0, 5, -5));
    }
}
