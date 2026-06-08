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

        DrawWireSphere(
            draw_list: drawList,
            center: center,
            radius: radius,
            segments: settings.Segments,
            rings: settings.Rings,
            color: settings.Color,
            forward: frame.forward,
            right: frame.right,
            up: frame.up
        );
    }

    private void DrawWireSphere(
    ImDrawListPtr draw_list,
    float3 center,
    float radius,
    int segments,
    int rings,
    float4 color,
    float3 forward,
    float3 right,
    float3 up
) {
        if(radius <= 0.0f) return;
        if(segments < 4) return;
        if(rings < 2) return;

        // Latitude / declination rings.
        // zAxis is the pole axis.
        // xAxis/yAxis define the equatorial plane.
        for(int r = 1; r < rings; r++) {
            float theta = MathF.PI * r / rings;

            float z = MathF.Cos(theta) * radius;
            float ringRadius = MathF.Sin(theta) * radius;

            for(int i = 0; i < segments; i++) {
                float a0 = 2.0f * MathF.PI * i / segments;
                float a1 = 2.0f * MathF.PI * (i + 1) / segments;

                float3 p0 =
                    center +
                    forward * (MathF.Cos(a0) * ringRadius) +
                    right * (MathF.Sin(a0) * ringRadius) +
                    up * z;

                float3 p1 =
                    center +
                    forward * (MathF.Cos(a1) * ringRadius) +
                    right * (MathF.Sin(a1) * ringRadius) +
                    up * z;

                lines.Line(draw_list, p0, p1, color);
            }
        }

        // Longitude / right ascension arcs.
        int meridianStep = Math.Max(1, segments / 16);

        for(int s = 0; s < segments; s += meridianStep) {
            float phi = 2.0f * MathF.PI * s / segments;

            for(int r = 0; r < rings; r++) {
                float theta0 = MathF.PI * r / rings;
                float theta1 = MathF.PI * (r + 1) / rings;

                float3 p0 =
                    center +
                    forward * (MathF.Sin(theta0) * MathF.Cos(phi) * radius) +
                    right * (MathF.Sin(theta0) * MathF.Sin(phi) * radius) +
                    up * (MathF.Cos(theta0) * radius);

                float3 p1 =
                    center +
                    forward * (MathF.Sin(theta1) * MathF.Cos(phi) * radius) +
                    right * (MathF.Sin(theta1) * MathF.Sin(phi) * radius) +
                    up * (MathF.Cos(theta1) * radius);

                lines.Line(draw_list, p0, p1, color);
            }
        }
    }
}