using KinematicsDemo.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Windows;

namespace KinematicsDemoTests.Models
{
    [TestClass]
    public class KUtilsTests02
    {
        [TestMethod]
        public void NormalizeAngle_ExpectNormalizedAngleInRangeMinus180To180()
        {
            // Arrange
            double angle = 400;

            var expected = 40;

            // Act
            var result = KUtils.NormalizeAngle2(angle);

            // Assert        
            Assert.AreEqual(expected, result);
        }
        

        [TestMethod]
        [DataRow(400, 40)]
        [DataRow(-190, 170)]
        [DataRow(270, 270)]
        [DataRow(-10, 350)]
        [DataRow(0, 0)]
        [DataRow(720, 0)]
        [DataRow(1444, 4)]
        [DataRow(360, 0)]
        public void NormalizeAngle_ExpectNormalizedAngle(double angle, double expected)
        {

            // Act
            var result = KUtils.NormalizeAngle2(angle);

            // Assert        
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizeAngle_ExpectNormalizedAngleNegative()
        {
            // Arrange
            double angle = -190;

            var expected = 170;

            // Act
            var result = KUtils.NormalizeAngle(angle);

            // Assert        
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizeAngle_ExpectNormalizedAnglePositive()
        {
            // Arrange
            double angle = 270;

            var expected = -90;

            // Act
            var result = KUtils.NormalizeAngle(angle);

            // Assert        
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizeAngle_ExpectNormalizedAnglePositive02()
        {
            // Arrange
            double angle = 10;

            var expected = 10;

            // Act
            var result = KUtils.NormalizeAngle(angle);

            // Assert        
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizeAngle_ExpectNormalizedAngleNegative02()
        {
            // Arrange
            double angle = 350;

            var expected = -10;

            // Act
            var result = KUtils.NormalizeAngle(angle);

            // Assert        
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizeAngle_ExpectNormalizedAngleNegative04()
        {
            // Arrange
            double angle = -179;

            var expected = -179;

            // Act
            var result = KUtils.NormalizeAngle(angle);

            // Assert        
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizeAngle_ExpectNormalizedAngleNegative05()
        {
            // Arrange
            double angle = -181;

            var expected = 179;

            // Act
            var result = KUtils.NormalizeAngle(angle);

            // Assert        
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizeAngle_ExpectNormalizedAngleNegative06()
        {
            // Arrange
            double angle = -180;

            var expected = 180;

            // Act
            var result = KUtils.NormalizeAngle(angle);

            // Assert        
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizeAngle_ExpectNormalizedAngleNegative07()
        {
            // Arrange
            double angle = -360;

            var expected = 0;

            // Act
            var result = KUtils.NormalizeAngle(angle);

            // Assert        
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizeAngle_ExpectNormalizedAngleNegative08()
        {
            // Arrange
            double angle = 360;

            var expected = 0;

            // Act
            var result = KUtils.NormalizeAngle(angle);

            // Assert        
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizeAngle_ExpectNormalizedAngleNegative03()
        {
            // Arrange
            double angle = 1070;

            var expected = -10;

            // Act
            var result = KUtils.NormalizeAngle(angle);

            // Assert        
            Assert.AreEqual(expected, result);
        }


    }
}