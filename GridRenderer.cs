using System;
using Brutal.Numerics;

namespace NavHud {
    public sealed class GridRenderer {
        private readonly IHudLineRenderer lines;

        public GridRenderer(IHudLineRenderer lines) {
            this.lines = lines;
        }

        public void DrawEquatorial(NavHudFrame frame, GridSettings settings) {
            DrawWireSphere(
                center: frame.CenterEgo,
                radius: frame.Radius,
                segments: settings.Segments,
                rings: settings.Rings,
                color: settings.Color
            );
        }

        public void DrawAzAlt(NavHudFrame frame, GridSettings settings) {
            if(frame.EastEgo is not { } east) return;
            if(frame.NorthEgo is not { } north) return;
            if(frame.UpEgo is not { } up) return;

            DrawAzAltWireSphere(
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

        private void DrawWireSphere(
            float3 center,
            float radius,
            int segments,
            int rings,
            float4 color
        ) {
            if(radius <= 0.0f) return;
            if(segments < 4) return;
            if(rings < 2) return;

            // Declination / latitude rings.
            for(int r = 1; r < rings; r++) {
                float theta = MathF.PI * r / rings;
                float y = MathF.Cos(theta) * radius;
                float ringRadius = MathF.Sin(theta) * radius;

                for(int i = 0; i < segments; i++) {
                    float a0 = 2.0f * MathF.PI * i / segments;
                    float a1 = 2.0f * MathF.PI * (i + 1) / segments;

                    float3 p0 = center + new float3(
                        MathF.Cos(a0) * ringRadius,
                        y,
                        MathF.Sin(a0) * ringRadius
                    );

                    float3 p1 = center + new float3(
                        MathF.Cos(a1) * ringRadius,
                        y,
                        MathF.Sin(a1) * ringRadius
                    );

                    lines.Line(p0, p1, color);
                }
            }

            // Right ascension / longitude arcs.
            int meridianStep = Math.Max(1, segments / 16);

            for(int s = 0; s < segments; s += meridianStep) {
                float phi = 2.0f * MathF.PI * s / segments;

                for(int r = 0; r < rings; r++) {
                    float theta0 = MathF.PI * r / rings;
                    float theta1 = MathF.PI * (r + 1) / rings;

                    float3 p0 = center + new float3(
                        MathF.Sin(theta0) * MathF.Cos(phi) * radius,
                        MathF.Cos(theta0) * radius,
                        MathF.Sin(theta0) * MathF.Sin(phi) * radius
                    );

                    float3 p1 = center + new float3(
                        MathF.Sin(theta1) * MathF.Cos(phi) * radius,
                        MathF.Cos(theta1) * radius,
                        MathF.Sin(theta1) * MathF.Sin(phi) * radius
                    );

                    lines.Line(p0, p1, color);
                }
            }
        }

        private void DrawAzAltWireSphere(
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

                    lines.Line(p0, p1, color);
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

                    lines.Line(p0, p1, color);
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
    }
}