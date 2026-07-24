using SkiaSharp;

namespace KinematicsDemo.Styles
{
    /// <summary>
    /// Colors used in the robot drawing
    /// </summary>
    public static class SkiaColors
    {
        /// <summary>
        /// Joint Paint
        /// </summary>
        public static SKPaint JointPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse("d69d85"),
            StrokeWidth = 2,
        };

        public static SKPaint UpperArmPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse("2e6796"),
            StrokeWidth = 8,
        };

        public static SKPaint ForearmPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse("3b2e58"),
            StrokeWidth = 4,
        };

        public static SKPaint EffectorPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse("970000"),
            StrokeWidth = 2,
        };

        public static SKPaint JointPaint1 = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse("64d69d85"),
            StrokeWidth = 2,
        };

        public static SKPaint UpperArmPaint1 = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse("642e6796"),
            StrokeWidth = 8,
        };

        public static SKPaint ForearmPaint1 = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse("643b2e58"),
            StrokeWidth = 4,
        };

        public static SKPaint PathPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse("643b2e58"),
            StrokeWidth = 1,
        };

        public static SKPaint EffectorPaint1 = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse("64970000"),
            StrokeWidth = 2,
        };

        public static SKPaint MousePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse("cb0000"),
            StrokeWidth = 2,
        };

        public static SKPaint MouseRecordingPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse("0000cb"),
            StrokeWidth = 2,
        };
    }
}
