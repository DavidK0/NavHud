using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace NavHud;

public sealed class GridRenderer {
    private readonly IHudLineRenderer lines;

    public GridRenderer(IHudLineRenderer lines) {
        this.lines = lines;
    }

    private void DrawAzAltWireSphere(
        ImDrawListPtr draw_list,
        float3 center,
        float radius,
        int azSegments,
        int altRings,
        float4 color,
        float3 east,
        float3 north,
        float3 up
    ) {
        if(radius <= 0.0f) return;
        if(azSegments < 4) return;
        if(altRings < 2) return;

        // Altitude rings: constant altitude, sweeping azimuth.
        for(int r = 1; r < altRings; r++) {
            float alt = -MathF.PI / 2.0f + MathF.PI * r / altRings;

            for(int i = 0; i < azSegments; i++) {
                float az0 = 2.0f * MathF.PI * i / azSegments;
                float az1 = 2.0f * MathF.PI * (i + 1) / azSegments;

                float3 p0 = center + AzAltToEgoOffset(
                    az0,
                    alt,
                    radius,
                    east,
                    north,
                    up
                );

                float3 p1 = center + AzAltToEgoOffset(
                    az1,
                    alt,
                    radius,
                    east,
                    north,
                    up
                );

                lines.Line(draw_list, p0, p1, color);
            }
        }

        // Azimuth arcs: constant azimuth, sweeping altitude.
        int azimuthStep = Math.Max(1, azSegments / 16);

        for(int s = 0; s < azSegments; s += azimuthStep) {
            float az = 2.0f * MathF.PI * s / azSegments;

            for(int r = 0; r < altRings; r++) {
                float alt0 = -MathF.PI / 2.0f + MathF.PI * r / altRings;
                float alt1 = -MathF.PI / 2.0f + MathF.PI * (r + 1) / altRings;

                float3 p0 = center + AzAltToEgoOffset(
                    az,
                    alt0,
                    radius,
                    east,
                    north,
                    up
                );

                float3 p1 = center + AzAltToEgoOffset(
                    az,
                    alt1,
                    radius,
                    east,
                    north,
                    up
                );

                lines.Line(draw_list, p0, p1, color);
            }
        }
    }

    private static float3 AzAltToEgoOffset(
        float az,
        float alt,
        float radius,
        float3 east,
        float3 north,
        float3 up
    ) {
        return
            east * (MathF.Cos(alt) * MathF.Sin(az) * radius) +
            north * (MathF.Cos(alt) * MathF.Cos(az) * radius) +
            up * (MathF.Sin(alt) * radius);
    }

    public void DrawEnu(ImDrawListPtr draw_list, NavHudFrame frame, GridSettings settings) {
        if(frame.EastEgo is not { } east) return;
        if(frame.NorthEgo is not { } north) return;
        if(frame.UpEgo is not { } up) return;

        DrawAzAltWireSphere(
            draw_list: draw_list,
            center: frame.CenterEgo,
            radius: frame.Radius,
            azSegments: settings.Segments,
            altRings: settings.Rings,
            color: settings.Color,
            east: east,
            north: north,
            up: up
        );
    }

    public void DrawLvlh(ImDrawListPtr draw_list, NavHudFrame frame, GridSettings settings) {
        if(frame.EastEgo is not { } xAxis) return;
        if(frame.NorthEgo is not { } yAxis) return;
        if(frame.UpEgo is not { } zAxis) return;

        DrawBasisWireSphere(
            draw_list: draw_list,
            center: frame.CenterEgo,
            radius: frame.Radius,
            segments: settings.Segments,
            rings: settings.Rings,
            color: settings.Color,
            xAxis: xAxis,
            yAxis: yAxis,
            zAxis: zAxis
        );
    }

    public void DrawEquatorial(ImDrawListPtr draw_list, NavHudFrame frame, GridSettings settings) {
        if(frame.EastEgo is not { } xAxis) return;
        if(frame.NorthEgo is not { } yAxis) return;
        if(frame.UpEgo is not { } zAxis) return;

        DrawBasisWireSphere(
            draw_list: draw_list,
            center: frame.CenterEgo,
            radius: frame.Radius,
            segments: settings.Segments,
            rings: settings.Rings,
            color: settings.Color,
            xAxis: xAxis,
            yAxis: yAxis,
            zAxis: zAxis
        );
    }

    public void DrawVlf(ImDrawListPtr draw_list, NavHudFrame frame, GridSettings settings) {
        if(frame.EastEgo is not { } velocityAxis) return;
        if(frame.NorthEgo is not { } rightAxis) return;
        if(frame.UpEgo is not { } upAxis) return;

        DrawBasisWireSphere(
            draw_list: draw_list,
            center: frame.CenterEgo,
            radius: frame.Radius,
            segments: settings.Segments,
            rings: settings.Rings,
            color: settings.Color,
            xAxis: velocityAxis,
            yAxis: rightAxis,
            zAxis: upAxis
        );
    }

    public void DrawBurn(ImDrawListPtr draw_list, NavHudFrame frame, GridSettings settings) {
        if(frame.BodyForwardEgo is not { } forwardAxis) return;
        if(frame.BodyRightEgo is not { } rightAxis) return;
        if(frame.BodyUpEgo is not { } upAxis) return;

        DrawBasisWireSphere(
            draw_list: draw_list,
            center: frame.CenterEgo,
            radius: frame.Radius,
            segments: settings.Segments,
            rings: settings.Rings,
            color: settings.Color,
            xAxis: forwardAxis,
            yAxis: rightAxis,
            zAxis: upAxis
        );
    }

    public void DrawTgt(ImDrawListPtr draw_list, NavHudFrame frame, GridSettings settings) {
        if(frame.BodyForwardEgo is not { } forwardAxis) return;
        if(frame.BodyRightEgo is not { } rightAxis) return;
        if(frame.BodyUpEgo is not { } upAxis) return;

        DrawBasisWireSphere(
            draw_list: draw_list,
            center: frame.CenterEgo,
            radius: frame.Radius,
            segments: settings.Segments,
            rings: settings.Rings,
            color: settings.Color,
            xAxis: forwardAxis,
            yAxis: rightAxis,
            zAxis: upAxis
        );
    }

    public void DrawTvel(ImDrawListPtr draw_list, NavHudFrame frame, GridSettings settings) {
        if(frame.BodyForwardEgo is not { } forwardAxis) return;
        if(frame.BodyRightEgo is not { } rightAxis) return;
        if(frame.BodyUpEgo is not { } upAxis) return;

        DrawBasisWireSphere(
            draw_list: draw_list,
            center: frame.CenterEgo,
            radius: frame.Radius,
            segments: settings.Segments,
            rings: settings.Rings,
            color: settings.Color,
            xAxis: forwardAxis,
            yAxis: rightAxis,
            zAxis: upAxis
        );
    }

    public void DrawDock(ImDrawListPtr draw_list, NavHudFrame frame, GridSettings settings) {
        if(frame.BodyForwardEgo is not { } forwardAxis) return;
        if(frame.BodyRightEgo is not { } rightAxis) return;
        if(frame.BodyUpEgo is not { } upAxis) return;

        DrawBasisWireSphere(
            draw_list: draw_list,
            center: frame.CenterEgo,
            radius: frame.Radius,
            segments: settings.Segments,
            rings: settings.Rings,
            color: settings.Color,
            xAxis: forwardAxis,
            yAxis: rightAxis,
            zAxis: upAxis
        );
    }

    private void DrawBasisWireSphere(
    ImDrawListPtr draw_list,
    float3 center,
    float radius,
    int segments,
    int rings,
    float4 color,
    float3 xAxis,
    float3 yAxis,
    float3 zAxis
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
                    xAxis * (MathF.Cos(a0) * ringRadius) +
                    yAxis * (MathF.Sin(a0) * ringRadius) +
                    zAxis * z;

                float3 p1 =
                    center +
                    xAxis * (MathF.Cos(a1) * ringRadius) +
                    yAxis * (MathF.Sin(a1) * ringRadius) +
                    zAxis * z;

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
                    xAxis * (MathF.Sin(theta0) * MathF.Cos(phi) * radius) +
                    yAxis * (MathF.Sin(theta0) * MathF.Sin(phi) * radius) +
                    zAxis * (MathF.Cos(theta0) * radius);

                float3 p1 =
                    center +
                    xAxis * (MathF.Sin(theta1) * MathF.Cos(phi) * radius) +
                    yAxis * (MathF.Sin(theta1) * MathF.Sin(phi) * radius) +
                    zAxis * (MathF.Cos(theta1) * radius);

                lines.Line(draw_list, p0, p1, color);
            }
        }
    }
}