using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace NavHud;

public sealed class GridRenderer {
    private readonly IHudLineRenderer lines;

    public GridRenderer(IHudLineRenderer lines) {
        this.lines = lines;
    }

    public void DrawGrid(
        ImDrawListPtr drawList,
        Basis frame,
        float3 center,
        float radius,
        GridSettings settings) {

        if(radius <= 0.0f) return;
        if(settings.Segments < 4) return;
        if(settings.Rings < 2) return;

        // Latitude / declination rings.
        // zAxis is the pole axis.
        // xAxis/yAxis define the equatorial plane.
        for(int r = 1; r < settings.Rings; r++) {
            float theta = MathF.PI * r / settings.Rings;

            float z = MathF.Cos(theta) * radius;
            float ringRadius = MathF.Sin(theta) * radius;

            for(int i = 0; i < settings.Segments; i++) {
                float a0 = 2.0f * MathF.PI * i / settings.Segments;
                float a1 = 2.0f * MathF.PI * (i + 1) / settings.Segments;

                float3 p0 =
                    center +
                    frame.forward * (MathF.Cos(a0) * ringRadius) +
                    frame.right * (MathF.Sin(a0) * ringRadius) +
                    frame.up * z;

                float3 p1 =
                    center +
                    frame.forward * (MathF.Cos(a1) * ringRadius) +
                    frame.right * (MathF.Sin(a1) * ringRadius) +
                    frame.up * z;

                lines.Line(drawList, p0, p1, settings.Color);
            }
        }

        // Longitude / right ascension arcs.
        int meridianStep = Math.Max(1, settings.Segments / 16);

        for(int s = 0; s < settings.Segments; s += meridianStep) {
            float phi = 2.0f * MathF.PI * s / settings.Segments;

            for(int r = 0; r < settings.Rings; r++) {
                float theta0 = MathF.PI * r / settings.Rings;
                float theta1 = MathF.PI * (r + 1) / settings.Rings;

                float3 p0 =
                    center +
                    frame.forward * (MathF.Sin(theta0) * MathF.Cos(phi) * radius) +
                    frame.right * (MathF.Sin(theta0) * MathF.Sin(phi) * radius) +
                    frame.up * (MathF.Cos(theta0) * radius);

                float3 p1 =
                    center +
                    frame.forward * (MathF.Sin(theta1) * MathF.Cos(phi) * radius) +
                    frame.right * (MathF.Sin(theta1) * MathF.Sin(phi) * radius) +
                    frame.up * (MathF.Cos(theta1) * radius);

                lines.Line(drawList, p0, p1, settings.Color);
            }
        }
    }
}