using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace KinematicsDemo.Models;

/// <summary>
/// Simple Kinematics utility functions.
/// </summary>
public static class KUtils
{
    /// <summary>
    /// The last step in the previous list
    /// </summary>
    private static double _lastStepInList;

    /// <summary>
    /// Calculate the distance between two points
    /// </summary>
    /// <param name="pointA">Point A</param>
    /// <param name="pointB">Point B</param>
    /// <returns>Distance</returns>
    public static double GetDistanceBetweenTwoPoints(Point pointA, Point pointB)
    {
        return Math.Sqrt(Math.Pow(pointA.X - pointB.X, 2) + Math.Pow(pointA.Y - pointB.Y, 2));
    }

    /// <summary>
    /// Create a list of (steps) Points between two Points in a straight line
    /// </summary>
    /// <param name="A">Point 1</param>
    /// <param name="B">Point 2</param>
    /// <param name="steps">How Many x Steps in between</param>
    /// <returns>List of x Steps in between A and B...</returns>
    public static List<Point> GetPointsInBetweenTwoPoints(Point A, Point B, int steps)
    {
        var points = new List<Point>();
        double xStep = (B.X - A.X) / steps;
        double yStep = (B.Y - A.Y) / steps;
        for (int i = 0; i < steps; i++)
        {
            points.Add(new Point(A.X + (xStep * i), A.Y + (yStep * i)));
        }

        return points;
    }

    /// <summary>
    /// Check if the shortest angle between 
    /// two normalized angles ([-180,180]) 
    /// is clockwise or not
    /// </summary>
    /// <param name="angleA">Start Angle</param>
    /// <param name="angleB">End Angle</param>
    /// <returns>true if clockwise</returns>
    public static bool IsDirectionBetweenTwoAnglesClockwise(double angleA, double angleB)
    {

        if (angleA > 0 && angleB > 0)
        {
            return angleB - angleA > 0 ? false : true;
        }

        if (angleA < 0 && angleB < 0)
        {
            return angleB - angleA > 0 ? false : true;
        }

        if (Math.Sign(angleA) != Math.Sign(angleB))
        {
            var angleSum = Math.Abs(angleA) + Math.Abs(angleB);
            if (angleSum > 180)
            {
                return angleA > angleB ? false : true;
            }
            else
            {
                return angleA > angleB ? true : false;
            }
        }

        return true;
    }

    /// <summary>
    /// Check if the shortest angle between 
    /// two normalized angles ([0,360]) 
    /// is clockwise or not
    /// </summary>
    /// <param name="startAngle">Start Angle</param>
    /// <param name="endAngle">End Angle</param>
    /// <returns>true if clockwise</returns>
    public static bool IsDirectionBetweenTwoAnglesClockwise2(double startAngle, double endAngle)
    {

        if (startAngle > 0 && endAngle > 0)
        {
            return endAngle - startAngle > 0 ? false : true;
        }

        if (startAngle < 0 && endAngle < 0)
        {
            return endAngle - startAngle > 0 ? false : true;
        }

        if (Math.Sign(startAngle) != Math.Sign(endAngle))
        {
            var angleSum = Math.Abs(startAngle) + Math.Abs(endAngle);
            if (angleSum > 180)
            {
                return startAngle > endAngle ? false : true;
            }
            else
            {
                return startAngle > endAngle ? true : false;
            }
        }

        return true;
    }


    // calculate if the direction between two points on a circle is clockwise or not
    public static bool IsDirectionBetweenTwoPointsOnCircleClockwise(Point A, Point B, Point center)
    {
        var angleA = NormalizeAngle(GetAngleBetweenTwoPoints(A, center));
        var angleB = NormalizeAngle(GetAngleBetweenTwoPoints(B, center));

        return IsDirectionBetweenTwoAnglesClockwise(angleA, angleB);
    }

    /// <summary>
    /// calculate the angle between two points :
    /// a and center. The return value of the function is the calculated angle in degrees. 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="center"></param>
    /// <returns>Angle in degree</returns>
    private static double GetAngleBetweenTwoPoints(Point a, Point center)
    {
        return Math.Atan2(a.Y - center.Y, a.X - center.X) * (180 / Math.PI);
    }


    /// <summary>
    /// Create a list of x angleSteps between two angles ]angle1,angle2]
    /// </summary>
    /// <param name="angle1">Normalized angle start</param>
    /// <param name="angle2">Normalized angle end</param>
    /// 
    /// <param name="steps">how many steps between Angle 1 and Angle 2</param>
    /// <remarks>A normalized angle is an angle between [-180,180]</remarks>
    /// <returns>List of steps</returns>
    public static List<double> GetAnglesInBetweenTwoAngles(double angle1, double angle2, int steps)
    {
        //_lastStepInList
        //angle1 = KUtils.NormalizeAngle(angle1);
        //angle2 = KUtils.NormalizeAngle(angle2);
        var atoBAngle = KUtils.GetAngleInBetweenAandB(angle1, angle2);
        double sign = IsDirectionBetweenTwoAnglesClockwise(angle1, angle2) ? -1 : 1;
        Debug.WriteLine($"-----------  angle1:{angle1} angle2:{angle2} Sign:{sign}");
        //Starts at the last position from the latst steps
        var angleSteps = new List<double>();
        double stepDistance = (atoBAngle) / steps;

        stepDistance = Math.Abs(stepDistance);
        _lastStepInList = angle1; // Smooth the distance between the steps
        for (int i = 0; i < steps; i++)
        {
            var expectedAngle = _lastStepInList + (stepDistance * sign);
            var newAngle = KUtils.NormalizeAngle(expectedAngle);

            angleSteps.Add(newAngle);
            _lastStepInList = angleSteps.Last();
        }
        return angleSteps;
    }

    /// <summary>
    ///  Calculate the angle at A of a triangle A,B,C 
    ///  Knowing the lengths of the 3 sides of the triangle 
    /// </summary>
    /// <param name="lengthOfAtoC">Distance from A to C</param>
    /// <param name="lengthOfAtoB">Distance from A to B</param>
    /// <param name="lengthOfBtoC">Distance from B to C</param>
    /// <returns>Angle in Radians at Point A</returns>
    public static double CalculateAngle(double lengthOfAtoC, double lengthOfAtoB, double lengthOfBtoC)
    {
        double cosTheta = (Math.Pow(lengthOfAtoC, 2) + Math.Pow(lengthOfAtoB, 2) - Math.Pow(lengthOfBtoC, 2)) / (2 * lengthOfAtoC * lengthOfAtoB);
        double angleAtPointA = Math.Acos(cosTheta);
        return angleAtPointA;
    }

    /// <summary>
    /// Convert angleInDegree to radian
    /// </summary>
    /// <param name="degree">Angle in degree to convert</param>
    /// <returns>angle in radian</returns>
    public static double DegreeToRadian(double degree)
    {
        // Convert the angleInDegree value to radians
        double radian = degree * (Math.PI / 180.0D);

        // Check if the radian value is less than 0
        if (radian < 0.0)
        {
            // Add 2π radians to get the equivalent positive radian value
            radian += 2 * Math.PI;
        }

        return radian;
    }

    /// <summary>
    /// Returns the closest angle (in Degree) between two angles (in Degree) from a given radian angle
    /// </summary>
    /// <param name="angleInRadian">Angle in radian</param>
    /// <param name="angleMin">Normalized Min range of angle in Degree</param>
    /// <param name="angleMax">Normalized Max range of angle in Degree</param>
    /// <remarks>A normalized angle is an angle between [-180,180]</remarks>
    /// <returns>closest angle (in Radian) to the range entered 
    /// If the angle is in the range, it will return the angle
    /// if not, it will return the closest angle to the range
    /// </returns>
    public static double GetClosestAngleBetweenTwoAngles(double angleInRadian, double angleMin, double angleMax)
    {
        if (angleMin > angleMax) { throw new ArgumentException("angleMin must be smaller than angleMax"); }

        // convert radian to Degree
        double angleInDegree = angleInRadian * 180 / Math.PI;

        // calculate the closest angle in Radian within the range -angleMin to +angleMax
        if (angleInDegree > angleMax)
        {
            return DegreeToRadian(angleMax);
        }
        else if (angleInDegree < angleMin)
        {
            return DegreeToRadian(angleMin);
        }

        return DegreeToRadian(angleInDegree);
    }

    /// <summary>
    /// Returns the closest angle (in Degree) between two angles (in Degree) from a given radian angle
    /// </summary>
    /// <param name="angleInRadian">Angle in radian</param>
    /// <param name="angleMin">Normalized Min range of angle in Degree</param>
    /// <param name="angleMax">Normalized Max range of angle in Degree</param>
    /// <remarks>A normalized angle is an angle between [-180,180]</remarks>
    /// <returns>closest angle (in Radian) to the range entered 
    /// If the angle is in the range, it will return the angle
    /// if not, it will return the closest angle to the range
    /// </returns>
    public static double GetClosestAngleBetweenTwoAngles2(double angleInRadian, double angleMin, double angleMax)
    {
        if (angleMin > angleMax) { throw new ArgumentException("angleMin must be smaller than angleMax"); }

        // convert radian to Degree
        double angleInDegree = angleInRadian * 180 / Math.PI;

        // calculate the closest angle in Radian within the range -angleMin to +angleMax
        if (angleInDegree > angleMax)
        {
            return DegreeToRadian(angleMax);
        }
        else if (angleInDegree < angleMin)
        {
            return DegreeToRadian(angleMin);
        }

        return DegreeToRadian(angleInDegree);
    }

    /// <summary>
    /// Get the closest angle to go from startAngle to endAngle in degree
    /// </summary>
    /// <param name="angleA"></param>
    /// <param name="angleB"></param>
    /// <returns>Get angle in between A and B in degree</returns>
    public static double GetAngleInBetweenAandB(double angleA, double angleB)
    {
        //if(startAngle > endAngle)
        //{
        //    var temp = startAngle;
        //    startAngle = endAngle;
        //    endAngle = temp;
        //}
        // this is where things fuck up 

        // Normalize both angles to the range [-180, 180]
        angleA = NormalizeAngle(angleA);
        angleB = NormalizeAngle(angleB);

        // Calculate the difference between the angles
        double angleDiff = angleB - angleA;

        // Adjust the difference to the range [-180, 180]
        if (angleDiff > 180)
        {
            angleDiff -= 360;
        }
        else if (angleDiff < -180)
        {
            angleDiff += 360;
        }

        return angleDiff;

        // double angleIncrement = shortestAngle / steps;
    }

    // TODO: Normalize angle between 0 to 360 degree
    /// <summary>
    ///  Takes in an angle in degrees as a parameter and returns the angle normalized to the range [-180, 180]
    /// </summary>
    /// <param name="angle">Angle to normalize</param>
    /// <returns></returns>
    public static double NormalizeAngle(double angle)
    {
        // Normalize the angle to the range [-180, 180]
        angle %= 360;
        if (angle <= -180)
        {
            angle += 360;
        }
        else if (angle > 180)
        {
            angle -= 360;
        }

        return angle;
    }

    /// <summary>
    /// Normalize an angle between 0 and 360
    /// </summary>
    /// <param name="angleDegrees">angle to normalize</param>
    /// <returns>normalized angle</returns>
    public static double NormalizeAngle2(double angleDegrees)
    {
        // Ensure the angle is positive
        while (angleDegrees < 0)
        {
            angleDegrees += 360.0;
        }

        // Reduce the angle to the range 0-360 degrees
        angleDegrees %= 360.0;

        return angleDegrees;
    }

    // TODO: Normalize angle between 0 to 360 degree
    /// <summary>
    /// Translate a value in radian to a value in angleInDegree between 0 to 360
    /// and 0 to -180
    /// </summary>
    /// <param name="radian">angle in Radian</param>
    /// <returns>Degree Value</returns>
    public static double RadianToDegree(double radian)
    {
        // Convert the radian value to degrees
        double degree = radian * (180.0 / Math.PI);

        // Check if the angleInDegree value is greater than 180
        if (degree > 180.0)
        {
            // Subtract 360 degrees to get the equivalent negative angleInDegree value
            degree -= 360.0;
        }

        // Check if the angleInDegree value is less than -180
        if (degree < -180.0)
        {
            // Add 360 degrees to get the equivalent positive angleInDegree value
            degree += 360.0;
        }

        return degree;
    }

    /// <summary>
    /// Calculates the closest point located on a circle circumference to a given point.
    /// </summary>
    /// <example>When robot effector grip is locked, it is the center of a circle
    /// whose radius is the length of the arm.</example>
    /// <param name="pointA">point to target</param>
    /// <param name="center">Center of the Circle</param>
    /// <param name="radius">radius of the circle</param>
    /// <returns>a point on the circle closest to pointA</returns>
    /// <exception cref="ArgumentException">If the radius is less than 0</exception>
    /// <exception cref="ArgumentException">If the point to search is the same as the center</exception>
    public static Point ClosestPointOnCircumference(Point pointA, Point center, double radius)
    {
        #region Check for Exceptions
        // Check if the radius is less than 0
        if (radius < 0)
        {
            throw new ArgumentException("Radius cannot be less than 0", nameof(radius));
        }

        if (pointA == center && radius == 0)
        {
            return pointA;
        }

        // Check if the point to search is the same as the center of the circle
        if (pointA == center)
        {
            throw new ArgumentException("Point to search cannot be the same as the center", nameof(pointA));
        }
        #endregion

        // Determine the vector from center to pointA
        Vector vectorAC = pointA - center;

        // Determine the distance between center and pointA
        double distanceAC = vectorAC.Length;

        // Get the direction vector from center to pointA
        Vector directionAC = vectorAC / distanceAC;

        // Determine the closest point on the circumference of the circle to pointA.
        Point closestPoint = center + radius * directionAC;

        return closestPoint;
    }


    /// <summary>
    /// Calculates the intersection between a plane and a circle in 3D space. The method takes in parameters x, y, and z. Inside the method, there are hardcoded values for the circle's center x0=1.0, y0=2.0, z0=3.0, and the circle's radius r=4.0. 
    /// The values angle1=1.0, angle2=2.0, C=3.0, and D=4.0 are the coefficients of the plane's equation.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns>tuple in 3d space</returns>
    public static (double x, double y, double z) DiscoverIntersection(double x, double y, double z)
    {
        double x0 = 1.0;  // center of the circle
        double y0 = 2.0;
        double z0 = 3.0;
        double r = 4.0;   // radius of the circle
        double A = 1.0;   // coefficients of the plane equation
        double B = 2.0;
        double C = 3.0;
        double D = 4.0;

        // Calculate the coefficients of the quadratic equation in x and y
        double a = A * A + B * B + C * C;
        double b = 2 * (A * x0 + B * y0 + C * z0 - A * C * x - B * C * y - C * D);
        double c = x0 * x0 + y0 * y0 + z0 * z0 - 2 * z0 * D + D * D - r * r * C * C + B * B * y * y + C * C * z * z - 2 * B * y0 * y - 2 * C * z0 * z;

        // Calculate the discriminant of the quadratic equation
        double discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            // No real solutions, the circle and the plane do not intersect
            Console.WriteLine("No intersection");
        }
        else if (discriminant == 0)
        {
            // One solution, the circle is tangent to the plane at this point
            x = -b / (2 * a);
            y = (-A * x - D) / B;
            z = (-A * x - B * y - D) / C;
            Console.WriteLine($"Intersection at ({x}, {y}, {z})");
        }
        else
        {
            // Two solutions, choose the one that is closest to the center of the circle
            double x1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
            double y1 = (-A * x1 - D) / B;
            double z1 = (-A * x1 - B * y1 - D) / C;

            double x2 = (-b - Math.Sqrt(discriminant)) / (2 * a);
            double y2 = (-A * x2 - D) / B;
            double z2 = (-A * x2 - B * y2 - D) / C;

            double distance1 = Math.Sqrt(Math.Pow(x1 - x0, 2) + Math.Pow(y1 - y0, 2) + Math.Pow(z1 - z0, 2));
            double distance2 = Math.Sqrt(Math.Pow(x2 - x0, 2) + Math.Pow(y2 - y0, 2) + Math.Pow(z2 - z0, 2));

            if (distance1 < distance2)
            {
                Console.WriteLine($"Intersection at ({x1}, {y1}, {z1})");
            }
            else
            {
                Console.WriteLine($"Intersection at ({x2}, {y2}, {z2})");
            }
        }

        return (x, y, z);
    }

    /// <summary>
    /// Check if a point is inside a circle
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="circleCenterB"></param>
    /// <param name="circleRadius"></param>
    /// <returns></returns>
    public static bool IsPointInsideCircle(Point pointA, Point circleCenterB, double circleRadius)
    {
        // Calculate the distance between the center of the circle and the point
        double distance = GetDistanceBetweenTwoPoints(pointA, circleCenterB);

        // Check if the distance is less than or equal to the radius
        if (distance <= circleRadius)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


}