using System;
using System.Numerics;
using System.Windows;
using KinematicsDemo.ViewModels;
using SkiaSharp;

namespace KinematicsDemo.Views;

/// <summary>Perspective Skia renderer for the rail-mounted robot.</summary>
internal sealed class RobotPerspectiveRenderer
{
    private Vector3 camera, forward, right, up;
    private float focal, cx, cy;

    public void Draw(SKCanvas c, SKImageInfo info, RobotArmViewModel vm)
    {
        c.ResetMatrix();
        c.Clear(new SKColor(245, 248, 250));
        SetCamera(info, vm);
        float min = (float)(vm.RailPositionRange?.Min ?? -500);
        float max = (float)(vm.RailPositionRange?.Max ?? 500);
        if (max - min < 1) { min -= 400; max += 400; }
        float bx = (float)vm.ArmRailPosition;
        float z = 150 + (float)vm.ArmHeightPosition;
        float mast = 280 + (float)Math.Max(400, vm.MastPositionRange.ZeroMax);

        Grid(c, min, max);
        Box(c, new((min + max) / 2, 0, 24), new(max - min + 160, 90, 48), new(120, 134, 143));
        Box(c, new(bx, 0, 65), new(150, 130, 45), new(166, 178, 186));
        Box(c, new(bx, 0, 70 + mast / 2), new(105, 105, mast), new(190, 198, 204));
        Box(c, new(bx - 55, 0, z), new(28, 128, 105), new(69, 82, 91));

        Point o = vm.RobotArmOriginPosition;
        Vector3 shoulder = World(vm.UpperArmSegment.PointA, o, bx, z);
        Vector3 elbow = World(vm.UpperArmSegment.PointB, o, bx, z);
        Vector3 wrist = World(vm.ForearmSegment.PointB, o, bx, z);
        Vector3 grip = World(vm.EffectorSegment.PointB, o, bx, z);
        Link(c, shoulder, elbow, 58, new(78, 98, 112)); Joint(c, shoulder, 43);
        Link(c, elbow, wrist, 46, new(111, 134, 150)); Joint(c, elbow, 34);
        Link(c, wrist, grip, 29, new(154, 181, 199)); Joint(c, wrist, 25);
        Gripper(c, wrist, grip, vm.IsEffectorGripped);

        using var path = Paint(new(245, 148, 0, 190), 3);
        Vector3? previous = null;
        foreach (var p in vm.RecordedMetaPoints.Points) { var current = World(p.MousePoint, o, bx, z); if (previous.HasValue) Line(c, previous.Value, current, path); previous = current; }
        Target(c, World(vm.MousePoint, o, bx, z));
        using var font = new SKFont(SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold), 15);
        using var text = new SKPaint { Color = new(52, 64, 72), IsAntialias = true };
        c.DrawText("PERSPECTIVE ROBOT VIEW", 22, 32, SKTextAlign.Left, font, text);
        using var small = new SKFont(SKTypeface.FromFamilyName("Segoe UI"), 13);
        c.DrawText($"Rail {vm.ArmRailPosition:0.0} mm     Height {vm.ArmHeightPosition:0.0} mm", 22, 55, SKTextAlign.Left, small, text);
        c.DrawText("Click the arm plane to set the target", 22, info.Height - 22, SKTextAlign.Left, small, text);
    }

    public Point ScreenToRobotPlane(
        Point p,
        double viewWidth,
        double viewHeight,
        double dpiScaleX,
        double dpiScaleY,
        RobotArmViewModel vm)
    {
        // WPF reports the pointer and ActualWidth/Height in device-independent
        // units. SKElement renders in physical pixels when IgnorePixelScaling is
        // false, so apply the same per-monitor DPI transform to both.
        float surfaceWidth = (float)(viewWidth * dpiScaleX);
        float surfaceHeight = (float)(viewHeight * dpiScaleY);
        SetCamera(surfaceWidth, surfaceHeight, vm);
        float surfaceX = (float)(p.X * dpiScaleX);
        float surfaceY = (float)(p.Y * dpiScaleY);
        Vector3 ray = Vector3.Normalize(
            forward
            + (((surfaceX - cx) / focal) * right)
            - (((surfaceY - cy) / focal) * up));
        float planeZ = 150 + (float)vm.ArmHeightPosition;
        if (Math.Abs(ray.Z) < .0001f) return vm.MousePoint;
        Vector3 hit = camera + ray * ((planeZ - camera.Z) / ray.Z);
        Point o = vm.RobotArmOriginPosition;
        // The legacy kinematics plane uses screen coordinates (positive Y is down).
        // The 3D world uses a conventional horizontal Y axis, so the Y component
        // must be reflected when crossing the boundary in either direction.
        return new(o.X + hit.X - vm.ArmRailPosition, o.Y - hit.Y);
    }

    private void SetCamera(SKImageInfo i, RobotArmViewModel vm)
    {
        SetCamera(i.Width, i.Height, vm);
    }

    private void SetCamera(float width, float height, RobotArmViewModel vm)
    {
        float span = (float)Math.Max(850, vm.RailPositionRange?.ZeroMax ?? 1000);
        float center = (float)((vm.RailPositionRange?.Min + vm.RailPositionRange?.Max) / 2 ?? 0);
        Vector3 target = new(center, 0, 270);
        camera = target + new Vector3(span * .95f, -span * 1.25f, span * .78f);
        forward = Vector3.Normalize(target - camera);
        right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitZ));
        up = Vector3.Normalize(Vector3.Cross(right, forward));
        focal = Math.Min(width, height) * 1.15f;
        cx = width * .43f;
        cy = height * .54f;
    }

    private static Vector3 World(Point p, Point o, float bx, float z) => new(bx + (float)(p.X - o.X), -(float)(p.Y - o.Y), z);
    private SKPoint Project(Vector3 p, out float depth) { Vector3 d = p - camera; depth = Vector3.Dot(d, forward); float q = focal / Math.Max(1, depth); return new(cx + Vector3.Dot(d, right) * q, cy - Vector3.Dot(d, up) * q); }
    private static SKPaint Paint(SKColor color, float width) => new() { Color = color, StrokeWidth = width, StrokeCap = SKStrokeCap.Round, IsAntialias = true };
    private void Line(SKCanvas c, Vector3 a, Vector3 b, SKPaint paint) { SKPoint p1 = Project(a, out _), p2 = Project(b, out _); c.DrawLine(p1, p2, paint); }

    private void Grid(SKCanvas c, float min, float max)
    {
        using var p = Paint(new(214, 222, 227), 1);
        for (float x = min - 200; x <= max + 200; x += 100) Line(c, new(x, -500, 0), new(x, 500, 0), p);
        for (float y = -500; y <= 500; y += 100) Line(c, new(min - 200, y, 0), new(max + 200, y, 0), p);
    }

    private void Box(SKCanvas c, Vector3 center, Vector3 size, SKColor color)
    {
        Vector3 h = size / 2;
        Vector3[] v = { center+new Vector3(-h.X,-h.Y,-h.Z),center+new Vector3(h.X,-h.Y,-h.Z),center+new Vector3(h.X,h.Y,-h.Z),center+new Vector3(-h.X,h.Y,-h.Z),center+new Vector3(-h.X,-h.Y,h.Z),center+new Vector3(h.X,-h.Y,h.Z),center+new Vector3(h.X,h.Y,h.Z),center+new Vector3(-h.X,h.Y,h.Z) };
        int[][] faces = { new[]{0,1,2,3},new[]{0,4,5,1},new[]{1,5,6,2},new[]{2,6,7,3},new[]{3,7,4,0},new[]{4,7,6,5} };
        foreach (var f in faces) { using var path = new SKPath(); SKPoint p = Project(v[f[0]], out _); path.MoveTo(p); for(int n=1;n<4;n++){p=Project(v[f[n]],out _);path.LineTo(p);} path.Close(); using var fill=new SKPaint{Color=color,IsAntialias=true}; using var edge=new SKPaint{Color=new(48,58,64,130),Style=SKPaintStyle.Stroke,StrokeWidth=1,IsAntialias=true}; c.DrawPath(path,fill);c.DrawPath(path,edge); }
    }

    private void Link(SKCanvas c, Vector3 a, Vector3 b, float width, SKColor color) { SKPoint p1=Project(a,out float d1),p2=Project(b,out float d2); float w=width*focal/Math.Max(1,(d1+d2)/2); using var shadow=Paint(new(30,36,40,80),w+7);c.DrawLine(p1.X+4,p1.Y+6,p2.X+4,p2.Y+6,shadow);using var body=Paint(color,w);c.DrawLine(p1,p2,body); }
    private void Joint(SKCanvas c, Vector3 v, float radius) { SKPoint p=Project(v,out float d);float r=radius*focal/Math.Max(1,d);using var fill=new SKPaint{Color=new(65,80,90),IsAntialias=true};c.DrawCircle(p,r,fill);using var shine=new SKPaint{Color=new(225,235,240,130),Style=SKPaintStyle.Stroke,StrokeWidth=3,IsAntialias=true};c.DrawArc(new(p.X-r*.65f,p.Y-r*.65f,p.X+r*.65f,p.Y+r*.65f),205,105,false,shine); }
    private void Gripper(SKCanvas c, Vector3 wrist, Vector3 grip, bool closed) { Vector3 d=Vector3.Normalize(grip-wrist),side=Vector3.Normalize(Vector3.Cross(d,Vector3.UnitZ));float s=closed?14:31;using var p=Paint(new(45,54,59),5);Line(c,grip-side*s,grip+side*s,p);Line(c,grip-side*s,grip-side*s+d*38,p);Line(c,grip+side*s,grip+side*s+d*38,p); }
    private void Target(SKCanvas c, Vector3 v) { SKPoint p=Project(v,out _);using var q=new SKPaint{Color=new(239,81,63),StrokeWidth=2.5f,Style=SKPaintStyle.Stroke,IsAntialias=true};c.DrawCircle(p,8,q);c.DrawLine(p.X-12,p.Y,p.X+12,p.Y,q);c.DrawLine(p.X,p.Y-12,p.X,p.Y+12,q); }
}
