
using Brutal.ImGuiApi;
using Brutal.Numerics;
using KSA;
using System.Globalization;

namespace NavHud;

internal class ConstellationsRenderer {
    private readonly ImDrawLineRenderer lineRenderer;
    private readonly float4 white = new float4(1, 1, 1, 1);
    private readonly List<Segment> segments = new();
    private const double SkyRadius = 1000.0;

    public ConstellationsRenderer(ImDrawLineRenderer lineRenderer) {
        this.lineRenderer = lineRenderer;

        string userDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string lines_in_20_path = Path.Combine(
            userDocs,
            "My Games",
            "Kitten Space Agency",
            "mods",
            "NavHud",
            "lines_in_20.txt");

        LoadConstellationLines(lines_in_20_path);
    }

    public void Draw(ImDrawListPtr draw_list, Camera camera, float3 center) {
        double3 centerD = new double3(center.X, center.Y, center.Z);

        foreach(Segment segment in segments) {
            double3 mirroredA = MirrorForGameSkybox(segment.A);
            double3 mirroredB = MirrorForGameSkybox(segment.B);

            double3 correctedA = RotateConstellationToGameSky(mirroredA);
            double3 correctedB = RotateConstellationToGameSky(mirroredB);

            double3 a = centerD + correctedA * SkyRadius;
            double3 b = centerD + correctedB * SkyRadius;

            lineRenderer.Line(draw_list, a, b, white);
        }
    }

    private void LoadConstellationLines(string path) {
        if(!File.Exists(path)) {
            return;
        }

        string? previousKey = null;
        double3 previousPoint = default;
        bool hasPreviousPoint = false;

        foreach(string rawLine in File.ReadLines(path)) {
            string line = rawLine.Trim();

            if(line.Length == 0) {
                continue;
            }

            string[] parts = line.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries);

            if(parts.Length < 3) {
                continue;
            }

            double raHours = double.Parse(parts[0], CultureInfo.InvariantCulture);
            double decDegrees = double.Parse(parts[1], CultureInfo.InvariantCulture);
            string key = parts[2];

            double3 point = RaDecToDirection(raHours, decDegrees);

            if(hasPreviousPoint && key == previousKey) {
                segments.Add(new Segment(previousPoint, point));
            }

            previousPoint = point;
            previousKey = key;
            hasPreviousPoint = true;
        }
    }

    private static double3 RaDecToDirection(double raHours, double decDegrees) {
        double offsetHours = -6;
        double ra = (raHours + offsetHours) * Math.PI / 12.0;
        double dec = decDegrees * Math.PI / 180.0;

        double cosDec = Math.Cos(dec);

        // J2000 equatorial direction
        double3 eq = new double3(
            Math.Cos(ra) * cosDec,
            Math.Sin(ra) * cosDec,
            Math.Sin(dec)
        );

        return eq;
    }

    private readonly struct Segment {
        public readonly double3 A;
        public readonly double3 B;

        public Segment(double3 a, double3 b) {
            A = a;
            B = b;
        }
    }

    public static float GameSkyRollDegrees = 90f;

    private static double3 MirrorForGameSkybox(double3 v) {
        return new double3(-v.X, v.Y, v.Z);
    }

    private static double3 RotateConstellationToGameSky(double3 v) {
        double3 from = new double3(0, 0, 1);
        double3 to = new double3(0, 1, 0);
        double3 aligned = VectorMath.RotateFromTo(v, from, to);
        return VectorMath.RotateAroundAxis(aligned, to, Math.PI / 2d);
    }
}