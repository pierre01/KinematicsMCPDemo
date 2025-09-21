using KinematicsDemo.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;

namespace KinematicsDemoTests.Models;

public class KUtilsTest04
{
    [TestClass]
    public class ClosestPointOnCircumferenceTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ClosestPointOnCircumference_RadiusLessThanZero_ThrowsArgumentException()
        {
            // Arrange
            Point pointA = new Point(2, 0);
            Point center = new Point(0, 0);
            double radius = -1;

            // Act
            KUtils.ClosestPointOnCircumference(pointA, center, radius);

            // Assert is handled by ExpectedException
        }

    [TestMethod]
    [DataRow(0, 0, 0, 0, 0)]
    [DataRow(0, 11, 0, 0, 10)]
    [DataRow(0, 9, 0, 0, 10)]
    [DataRow(5, 12, 5, 10, 10)]
    [DataRow(5, 5, 0, 0, 5)]
    [DataRow(1, 3, 4, 2, 1)]
    [DataRow(-3, 2, 1, 3, 5)]
    public void ClosestPointOnCircumferenceTest(double x1,double y1,double x2,double y2,  double radius)
    {    
        Point pointA = new Point(x1,y1); 
        Point center =  new Point(x2,y2); 
        try
        {
            Point closestPoint = KUtils.ClosestPointOnCircumference(pointA, center, radius);

            double distance = Math.Round( KUtils.GetDistanceBetweenTwoPoints(closestPoint, center),5);

            Assert.AreEqual(radius, distance);
        }
        catch (ArgumentException ex)
        {
            Assert.Fail(ex.Message);
        }
    }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ClosestPointOnCircumference_PointAEqualsCenter_ThrowsArgumentException()
        {
            // Arrange
            Point pointA = new Point(0, 0);
            Point center = new Point(0, 0);
            double radius = 5;

            // Act
            KUtils.ClosestPointOnCircumference(pointA, center, radius);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public void ClosestPointOnCircumference_CenterAtOriginAndRadius10PointOutsideCircumference_ReturnsPointOnCircumference()
        {
            // Arrange
            Point pointA = new Point(0, 11);
            Point center = new Point(0, 0);
            double radius = 10;

            // Act
            Point result = KUtils.ClosestPointOnCircumference(pointA, center, radius);

            // Assert
            Assert.AreEqual(new Point(0, 10), result);
        }

        [TestMethod]
        public void ClosestPointOnCircumference_CenterAtOriginAndRadius10PointInsideCircumference_ReturnsPointOnCircumference()
        {
            // Arrange
            Point pointA = new Point(0, 9);
            Point center = new Point(0, 0);
            double radius = 10;

            // Act
            Point result = KUtils.ClosestPointOnCircumference(pointA, center, radius);

            // Assert
            Assert.AreEqual(new Point(0, 10), result);
        }

        [TestMethod]
        public void ClosestPointOnCircumference_CenterAtOriginAndRadius10PointOnCircumference_ReturnsSamePoint()
        {
            // Arrange
            Point pointA = new Point(0, 10);
            Point center = new Point(0, 0);
            double radius = 10;

            // Act
            Point result = KUtils.ClosestPointOnCircumference(pointA, center, radius);

            // Assert
            Assert.AreEqual(pointA, result);
        }

        [TestMethod]
        public void ClosestPointOnCircumference_CenterAtZeroPointAt5_ReturnsCorrectClosestPointOnCircumference()
        {
            // Arrange
            Point pointA = new Point(5, 5);
            Point center = new Point(0, 0);
            double radius = 5;

            // Act
            Point result = KUtils.ClosestPointOnCircumference(pointA, center, radius);

            // perfect 45% angle so x and y are equal
            // Assert
            Assert.AreEqual(result.X, result.Y);

            // Same as above but with a different point inside the circle

            // Perfect 45% angle but inside the circle so the results should be the same as above
            Point pointB = new Point(2, 2);

            Point result2 = KUtils.ClosestPointOnCircumference(pointB, center, radius);

            Assert.AreEqual(result, result2);
        }

        [TestMethod]
        public void ClosestPointOnCircumference_PointAndCenterOnSameAxisAndRadius10_ReturnsCorrectClosestPointOnCircumference()
        {
            // Arrange
            Point pointA = new Point(5, 12);
            Point center = new Point(5, 10);
            double radius = 10;

            // Act
            Point result = KUtils.ClosestPointOnCircumference(pointA, center, radius);

            // Assert
            Assert.AreEqual(new Point(5, 20), result);
        }
    }

}
