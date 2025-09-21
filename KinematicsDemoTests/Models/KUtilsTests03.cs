using KinematicsDemo.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace KinematicsDemoTests.Models;

[TestClass]
public class KUtilsTests03
{

    [TestMethod]
    [DataRow(10, 20, false)]
    [DataRow(20, 10, true)]
    [DataRow(175, -172, false)]
    [DataRow(-175, 172, true)]
    [DataRow(-10, 10, false)]
    [DataRow(5, -170, true)]
    [DataRow(5, 175, false)]
    [DataRow(156, -176, false)]
    public void IsDirectionBetweenTwoAnglesClockwise(double angleA, double angleB, bool expected)
    {
        //Act
        bool actual = KUtils.IsDirectionBetweenTwoAnglesClockwise(angleA, angleB);

        //Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(77, 126)]
    [DataRow(126, 168)]
    [DataRow(168, -150)]
    [DataRow(-150, -110)]
    [DataRow(-110, -39)]
    public void IsDirectionBetweenTwoAnglesClockwise02(double angleA, double angleB)
    {
        // Arrange

        bool result = KUtils.IsDirectionBetweenTwoAnglesClockwise(angleA, angleB);

        //Assert
        Assert.IsFalse(result, "Expected the result to be false.");
    }

    [TestMethod]
    public void GetAnglesInBetweenTwoAngles_WithLoop_WithStepsEqualTo10_ReturnsCorrectResult()
    {
        // arrange
        double angle1 = KUtils.NormalizeAngle(183);
        double angle2 = -138;
        int steps = 10;

        // act
        List<double> result = KUtils.GetAnglesInBetweenTwoAngles(angle1, angle2, steps);

        // assert
        Assert.AreEqual(10, result.Count);
        Assert.AreEqual(-173.1, result.First());
    }

    [TestMethod]
    public void GetAnglesInBetweenTwoAngles_WithLoop2_WithStepsEqualTo10_ReturnsCorrectResult()
    {
        // arrange
        double angle1 = 156;
        double angle2 = KUtils.NormalizeAngle(183);
        int steps = 10;

        // act
        List<double> result = KUtils.GetAnglesInBetweenTwoAngles(angle1, angle2, steps);

        // assert
        Assert.AreEqual(10, result.Count);
        Assert.AreEqual(158.7, result.First());
    }
    [TestMethod]
    public void GetAnglesInBetweenTwoAngles_WithStepsEqualToTwo02_ReturnsCorrectResult()
    {
        // arrange
        double angle1 = 90;
        double angle2 = 180;
        int steps = 2;

        // act
        List<double> result = KUtils.GetAnglesInBetweenTwoAngles(angle1, angle2, steps);

        // assert
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(135, result.First());
    }

    [TestMethod]
    public void GetAnglesInBetweenTwoAngles_WithStepsEqualToTwo_ReturnsCorrectResult()
    {
        //TODO: This test is failing. Fix it.
        // arrange
        double angle1 = 0;
        double angle2 = 180;
        int steps = 2;

        // act
        List<double> result = KUtils.GetAnglesInBetweenTwoAngles(angle1, angle2, steps);

        // assert
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(90, result[0]);
        Assert.AreEqual(180, result[1]);
    }

    [TestMethod]
    public void GetAnglesInBetweenTwoAngles_WithStepsEqualToThree_ReturnsCorrectResult()
    {
        // arrange
        double angle1 = 0;
        double angle2 = 180;
        int steps = 3;

        // act
        List<double> result = KUtils.GetAnglesInBetweenTwoAngles(angle1, angle2, steps);

        // assert
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(60, result[0]);
        Assert.AreEqual(120, result[1]);
    }

    [TestMethod]
    public void GetAnglesInBetweenTwoAngles_WithSameInputAngles_ReturnsEmptyList()
    {
        // arrange
        double angle1 = 180;
        double angle2 = 180;
        int steps = 5;

        // act
        List<double> result = KUtils.GetAnglesInBetweenTwoAngles(angle1, angle2, steps);

        // assert
        Assert.AreEqual(5, result.Count);
        Assert.AreEqual(180, result[0]);
        Assert.AreEqual(180, result[1]);
        Assert.AreEqual(180, result[2]);
        Assert.AreEqual(180, result[3]);
        Assert.AreEqual(180, result[4]);
    }

    [TestMethod]
    public void GetAnglesInBetweenTwoAngles_WithNegativeInputAngles_ReturnsCorrectResult()
    {
        // arrange
        double angle1 = -90;
        double angle2 = 90;
        int steps = 4;

        // act
        List<double> result = KUtils.GetAnglesInBetweenTwoAngles(angle1, angle2, steps);

        // assert
        Assert.AreEqual(4, result.Count);
        Assert.AreEqual(-45, result[0]);
        Assert.AreEqual(-0, result[1]);
        Assert.AreEqual(45, result[2]);
        Assert.AreEqual(90, result[3]);
    }

    [TestMethod]
    public void GetAnglesInBetweenTwoAngles_WithLargerFirstAngle_ReturnsCorrectResult()
    {
        // arrange
        double angle1 = KUtils.NormalizeAngle(220); ;
        double angle2 = 120;
        int steps = 5;

        // act
        List<double> result = KUtils.GetAnglesInBetweenTwoAngles(angle1, angle2, steps);

        // assert
        Assert.AreEqual(5, result.Count);
        Assert.AreEqual(-160, result[0]);
        Assert.AreEqual(180, result[1]);
        Assert.AreEqual(160, result[2]);
        Assert.AreEqual(140, result[3]);
        Assert.AreEqual(120, result[4]);
    }
}