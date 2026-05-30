using Brutal.ImGuiApi;
using Brutal.Numerics;
using KSA;
using NavHud;

namespace NavHud {
    public enum HudSymbol {
        Cross,
        X,
        Diamond,
        Attitude,
        Antiattitude,
        Prograde,
        Retrograde,
        Normal,
        Antinormal,
        RadialIn,
        RadialOut,
        Target,
        Antitarget,
        DockingAlignment,
        Maneuver,
    }

    public sealed class HudSymbolRenderer {
        private readonly IHudLineRenderer lines;

        public HudSymbolRenderer(IHudLineRenderer lines) {
            this.lines = lines;
        }

        public void Draw(
            ImDrawListPtr draw_list,
            HudSymbol symbol,
            float3 position,
            float3 normal,
            float radius,
            float4 color,
            float symbolSize,
            float thickness,
            float rollRadians
        ) {
            symbolSize = radius * symbolSize;

            GetBillboardBasis(normal, out float3 right, out float3 up);

            switch(symbol) {
                case HudSymbol.Cross:
                    DrawCross(draw_list, position, right, up, symbolSize, color, thickness);
                    break;

                case HudSymbol.X:
                    DrawX(draw_list, position, right, up, symbolSize, color, thickness);
                    break;

                case HudSymbol.Diamond:
                    DrawDiamond(draw_list, position, right, up, symbolSize, color, thickness);
                    break;

                case HudSymbol.Attitude:
                    DrawAttitude(draw_list, position, right, up, symbolSize, color, thickness, rollRadians);
                    break;

                case HudSymbol.Antiattitude:
                    DrawAntiattitude(draw_list, position, right, up, symbolSize, color, thickness, rollRadians);
                    break;

                case HudSymbol.Prograde:
                    DrawPrograde(draw_list, position, right, up, symbolSize, color, thickness);
                    break;

                case HudSymbol.Retrograde:
                    DrawRetrograde(draw_list, position, right, up, symbolSize, color, thickness);
                    break;

                case HudSymbol.Normal:
                    DrawNormal(draw_list, position, right, up, symbolSize, color, thickness);
                    break;

                case HudSymbol.Antinormal:
                    DrawAntinormal(draw_list, position, right, up, symbolSize, color, thickness);
                    break;

                case HudSymbol.RadialIn:
                    DrawRadialIn(draw_list, position, right, up, symbolSize, color, thickness);
                    break;

                case HudSymbol.RadialOut:
                    DrawRadialOut(draw_list, position, right, up, symbolSize, color, thickness);
                    break;

                case HudSymbol.Target:
                    DrawTarget(draw_list, position, right, up, symbolSize, color, thickness);
                    break;

                case HudSymbol.Antitarget:
                    DrawAntitarget(draw_list, position, right, up, symbolSize, color, thickness);
                    break;

                case HudSymbol.Maneuver:
                    DrawManeuver(draw_list, position, right, up, symbolSize, color, thickness);
                    break;
            }
        }

        private void DrawCross(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness
        ) {
            lines.Line(draw_list, center - right * size, center + right * size, color, thickness);
            lines.Line(draw_list, center - up * size, center + up * size, color, thickness);
        }

        private void DrawX(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness
        ) {
            float3 a = right + up;
            float3 b = right - up;

            lines.Line(draw_list, center - a * size, center + a * size, color, thickness);
            lines.Line(draw_list, center - b * size, center + b * size, color, thickness);
        }

        private void DrawDiamond(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness
        ) {
            float3 top = center + up * size;
            float3 rightPoint = center + right * size;
            float3 bottom = center - up * size;
            float3 leftPoint = center - right * size;

            lines.Line(draw_list, top, rightPoint, color, thickness);
            lines.Line(draw_list, rightPoint, bottom, color, thickness);
            lines.Line(draw_list, bottom, leftPoint, color, thickness);
            lines.Line(draw_list, leftPoint, top, color, thickness);
        }

        private static void GetBillboardBasis(
            float3 normal,
            out float3 right,
            out float3 up
        ) {
            float3 n = HudMath.Normalize(normal);

            // Pick a stable fallback axis that is not parallel to n.
            float3 worldUp = MathF.Abs(n.Y) < 0.9f
                ? new float3(0.0f, 1.0f, 0.0f)
                : new float3(1.0f, 0.0f, 0.0f);

            right = HudMath.Normalize(Cross(worldUp, n));
            up = HudMath.Normalize(Cross(n, right));
        }

        private static float3 Cross(float3 a, float3 b) {
            return new float3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        private void DrawAttitude(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness,
            float rollRadians
        ) {
            float barHalfWidth = size * 0.55f;
            float notchHalfWidth = size * 0.15f;
            float notchDepth = size * 0.75f;

            right = HudMath.Normalize(right);
            up = HudMath.Normalize(up);

            float c = MathF.Cos(rollRadians);
            float s = MathF.Sin(rollRadians);

            // Rotate the local 2D basis by roll.
            // Flip the sign of s if the roll direction is backwards in your HUD.
            float3 rolledRight = right * c + up * s;
            float3 rolledUp = up * c - right * s;

            float3 leftEnd = center - rolledRight * barHalfWidth;
            float3 rightEnd = center + rolledRight * barHalfWidth;

            float3 notchLeft = center - rolledRight * notchHalfWidth;
            float3 notchRight = center + rolledRight * notchHalfWidth;
            float3 notchTip = center + rolledUp * notchDepth;

            lines.Line(draw_list, leftEnd, notchLeft, color, thickness);
            lines.Line(draw_list, notchRight, rightEnd, color, thickness);
            lines.Line(draw_list, notchLeft, notchTip, color, thickness);
            lines.Line(draw_list, notchTip, notchRight, color, thickness);
        }

        private void DrawAntiattitude(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness,
            float rollRadians
        ) {
            float barHalfWidth = size * 0.55f;
            float notchHalfWidth = size * 0.15f;
            float notchDepth = size * 0.75f;

            right = HudMath.Normalize(right);
            up = HudMath.Normalize(up);

            float c = MathF.Cos(rollRadians);
            float s = MathF.Sin(rollRadians);

            // Rotate basis by roll.
            // Flip sign of s if rotation direction is inverted.
            float3 rolledRight = right * c + up * s;
            float3 rolledUp = up * c - right * s;

            float3 leftEnd = center - rolledRight * barHalfWidth;
            float3 rightEnd = center + rolledRight * barHalfWidth;

            float3 notchLeft = center - rolledRight * notchHalfWidth;
            float3 notchRight = center + rolledRight * notchHalfWidth;

            float3 leftNotchTip = notchLeft - rolledUp * notchDepth;
            float3 rightNotchTip = notchRight - rolledUp * notchDepth;

            lines.Line(draw_list, leftEnd, leftNotchTip, color, thickness);
            lines.Line(draw_list, leftNotchTip, notchLeft, color, thickness);

            lines.Line(draw_list, notchRight, rightNotchTip, color, thickness);
            lines.Line(draw_list, rightNotchTip, rightEnd, color, thickness);
        }

        private void DrawPrograde(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness
        ) {
            float r = size * 0.82f;

            DrawCircle(draw_list, center, right, up, r, color, 12, thickness);
            DrawDot(draw_list, center, right, up, size * 0.11f, color, thickness);

            lines.Line(draw_list, center + up * r, center + up * size * 1.55f, color, thickness);
            lines.Line(draw_list, center - right * r, center - right * size * 1.55f, color, thickness);
            lines.Line(draw_list, center + right * r, center + right * size * 1.55f, color, thickness);
        }

        private void DrawRetrograde(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness
        ) {
            float r = size * 0.82f;

            DrawCircle(draw_list, center, right, up, r, color, 12, thickness);
            DrawXInside(draw_list, center, right, up, size * 0.58f, color, thickness);

            lines.Line(draw_list, center + up * r, center + up * size * 1.55f, color, thickness);

            float3 downLeft = HudMath.Normalize(-right - up * 0.65f);
            float3 downRight = HudMath.Normalize(right - up * 0.65f);

            lines.Line(draw_list, center + downLeft * r, center + downLeft * size * 1.55f, color, thickness);
            lines.Line(draw_list, center + downRight * r, center + downRight * size * 1.55f, color, thickness);
        }

        private void DrawNormal(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness
        ) {
            float h = size * 1.15f;
            float w = size * 1.05f;

            float3 top = center + up * h;
            float3 left = center - up * h * 0.75f - right * w;
            float3 rightPoint = center - up * h * 0.75f + right * w;

            lines.Line(draw_list, top, rightPoint, color, thickness);
            lines.Line(draw_list, rightPoint, left, color, thickness);
            lines.Line(draw_list, left, top, color, thickness);

            DrawDot(draw_list, center - up * size * 0.18f, right, up, size * 0.10f, color, thickness);
        }

        private void DrawAntinormal(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness
        ) {
            float h = size * 1.15f;
            float w = size * 1.05f;

            float3 bottom = center - up * h;
            float3 left = center + up * h * 0.75f - right * w;
            float3 rightPoint = center + up * h * 0.75f + right * w;

            lines.Line(draw_list, bottom, rightPoint, color, thickness);
            lines.Line(draw_list, rightPoint, left, color, thickness);
            lines.Line(draw_list, left, bottom, color, thickness);

            DrawDot(draw_list, center + up * size * 0.18f, right, up, size * 0.10f, color, thickness);

            lines.Line(draw_list, center + up * h * 0.75f, center + up * size * 1.55f, color, thickness);

            float3 downLeft = HudMath.Normalize(-right - up * 0.60f);
            float3 downRight = HudMath.Normalize(right - up * 0.60f);

            lines.Line(draw_list, center + downLeft * size * 0.95f, center + downLeft * size * 1.55f, color, thickness);
            lines.Line(draw_list, center + downRight * size * 0.95f, center + downRight * size * 1.55f, color, thickness);
        }

        private void DrawRadialIn(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness
        ) {
            float r = size * 0.86f;

            DrawCircle(draw_list, center, right, up, r, color, 12, thickness);

            DrawInwardCornerTick(draw_list, center, right, up, size, 1.0f, 1.0f, color, thickness);
            DrawInwardCornerTick(draw_list, center, right, up, size, -1.0f, 1.0f, color, thickness);
            DrawInwardCornerTick(draw_list, center, right, up, size, 1.0f, -1.0f, color, thickness);
            DrawInwardCornerTick(draw_list, center, right, up, size, -1.0f, -1.0f, color, thickness);
        }

        private void DrawRadialOut(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness
        ) {
            float r = size * 0.82f;

            DrawCircle(draw_list, center, right, up, r, color, 12, thickness);
            DrawDot(draw_list, center, right, up, size * 0.10f, color, thickness);

            DrawOutwardCornerTick(draw_list, center, right, up, size, 1.0f, 1.0f, color, thickness);
            DrawOutwardCornerTick(draw_list, center, right, up, size, -1.0f, 1.0f, color, thickness);
            DrawOutwardCornerTick(draw_list, center, right, up, size, 1.0f, -1.0f, color, thickness);
            DrawOutwardCornerTick(draw_list, center, right, up, size, -1.0f, -1.0f, color, thickness);
        }

        private void DrawTarget(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness
        ) {
            float r = size * 0.82f;

            DrawCircle(draw_list, center, right, up, r, color, 12, thickness);
            DrawDot(draw_list, center, right, up, size * 0.10f, color, thickness);

            lines.Line(draw_list, center - right * size * 1.45f, center - right * r, color, thickness);
            lines.Line(draw_list, center + right * r, center + right * size * 1.45f, color, thickness);
            lines.Line(draw_list, center - up * size * 1.45f, center - up * r, color, thickness);
            lines.Line(draw_list, center + up * r, center + up * size * 1.45f, color, thickness);
        }

        private void DrawAntitarget(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness
        ) {
            float r = size * 0.82f;

            DrawCircle(draw_list, center, right, up, r, color, 12, thickness);
            DrawXInside(draw_list, center, right, up, size * 0.58f, color, thickness);

            float3 a = HudMath.Normalize(right + up);
            float3 b = HudMath.Normalize(right - up);

            lines.Line(draw_list, center + a * r, center + a * size * 1.45f, color, thickness);
            lines.Line(draw_list, center - a * r, center - a * size * 1.45f, color, thickness);
            lines.Line(draw_list, center + b * r, center + b * size * 1.45f, color, thickness);
            lines.Line(draw_list, center - b * r, center - b * size * 1.45f, color, thickness);
        }

        private void DrawManeuver(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness
        ) {
            float inner = size * 0.36f;
            float outer = size * 1.05f;

            DrawCircle(draw_list, center, right, up, inner, color, 40, thickness);

            lines.Line(draw_list, center + up * inner, center + up * outer, color, thickness);
            lines.Line(draw_list, center - up * inner, center - up * outer, color, thickness);
            lines.Line(draw_list, center + right * inner, center + right * outer, color, thickness);
            lines.Line(draw_list, center - right * inner, center - right * outer, color, thickness);

            float3 a = HudMath.Normalize(right + up);
            float3 b = HudMath.Normalize(right - up);

            lines.Line(draw_list, center + a * inner, center + a * outer, color, thickness);
            lines.Line(draw_list, center - a * inner, center - a * outer, color, thickness);
            lines.Line(draw_list, center + b * inner, center + b * outer, color, thickness);
            lines.Line(draw_list, center - b * inner, center - b * outer, color, thickness);
        }
        private void DrawCircle(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float radius,
            float4 color,
            int segments = 40,
            float thickness = 1.0f
        ) {
            float step = MathF.PI * 2.0f / segments;

            float3 previous = center + right * radius;

            for(int i = 1; i <= segments; i++) {
                float a = step * i;

                float3 current =
                    center +
                    right * (MathF.Cos(a) * radius) +
                    up * (MathF.Sin(a) * radius);

                lines.Line(draw_list, previous, current, color, thickness);
                previous = current;
            }
        }

        private void DrawDot(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float radius,
            float4 color,
            float thickness
        ) {
            DrawCircle(draw_list, center, right, up, radius, color, 12, thickness);
        }

        private void DrawXInside(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color,
            float thickness
        ) {
            float3 a = HudMath.Normalize(right + up);
            float3 b = HudMath.Normalize(right - up);

            lines.Line(draw_list, center - a * size, center + a * size, color, thickness);
            lines.Line(draw_list, center - b * size, center + b * size, color, thickness);
        }

        private void DrawInwardCornerTick(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float sx,
            float sy,
            float4 color,
            float thickness
        ) {
            float3 cornerDir = HudMath.Normalize(right * sx + up * sy);

            float3 outer = center + cornerDir * size * 0.78f;
            float3 inner = center + cornerDir * size * 0.43f;

            lines.Line(draw_list, outer, inner, color, thickness);
        }

        private void DrawOutwardCornerTick(
            ImDrawListPtr draw_list,
            float3 center,
            float3 right,
            float3 up,
            float size,
            float sx,
            float sy,
            float4 color,
            float thickness
        ) {
            float3 cornerDir = HudMath.Normalize(right * sx + up * sy);

            float3 inner = center + cornerDir * size * 0.82f;
            float3 outer = center + cornerDir * size * 1.28f;

            lines.Line(draw_list, inner, outer, color, thickness);
        }
    }
}

public interface IHudIndicator {
    bool IsEnabled(NavHudSettings settings);
    void Draw(ImDrawListPtr draw_list, NavHudFrame frame, NavHudSettings settings);
}

public sealed class AttitudeIndicatorRenderer : IHudIndicator {
    private readonly VectorIndicatorRenderer vectorRenderer;

    public AttitudeIndicatorRenderer(VectorIndicatorRenderer vectorRenderer) {
        this.vectorRenderer = vectorRenderer;
    }

    public bool IsEnabled(NavHudSettings settings) {
        return settings.Attitude.Enabled;
    }

    public void Draw(ImDrawListPtr draw_list, NavHudFrame frame, NavHudSettings settings) {
        if(!settings.Attitude.Enabled) return;
        if(frame.BodyForwardEgo is not { } forward) return;
        if(frame.Vehicle is not { } vehicle) return;

        float rollRadians = (float)HudMath.GetSurfaceAttitude(vehicle).X;

        vectorRenderer.DrawMarker(
            draw_list,
            frame,
            forward,
            settings.Attitude,
            settings.SymbolSize,
            settings.SymbolLineThickness,
            rollRadians
        );
    }
}

public sealed class AntiattitudeIndicatorRenderer : IHudIndicator {
    private readonly VectorIndicatorRenderer vectorRenderer;

    public AntiattitudeIndicatorRenderer(VectorIndicatorRenderer vectorRenderer) {
        this.vectorRenderer = vectorRenderer;
    }

    public bool IsEnabled(NavHudSettings settings) {
        return settings.Antiattitude.Enabled;
    }

    public void Draw(ImDrawListPtr draw_list, NavHudFrame frame, NavHudSettings settings) {
        if(!settings.Antiattitude.Enabled) return;
        if(frame.BodyForwardEgo is not { } forward) return;
        if(frame.Vehicle is not { } vehicle) return;

        float rollRadians = (float)HudMath.GetSurfaceAttitude(vehicle).X;

        vectorRenderer.DrawMarker(
            draw_list,
            frame,
            -forward,
            settings.Antiattitude,
            settings.SymbolSize,
            settings.SymbolLineThickness,
            rollRadians
        );
    }
}


public sealed class  ProgradeIndicatorRenderer : IHudIndicator {
    private readonly VectorIndicatorRenderer vectorRenderer;

    public ProgradeIndicatorRenderer(VectorIndicatorRenderer vectorRenderer) {
        this.vectorRenderer = vectorRenderer;
    }

    public bool IsEnabled(NavHudSettings settings) {
        return settings.Prograde.Enabled;
    }

    public void Draw(ImDrawListPtr draw_list, NavHudFrame frame, NavHudSettings settings) {
        if(!settings.Prograde.Enabled) return;

        float3 direction = HudMath.CciDirectionToEgo(
            frame.Vehicle,
            frame.Camera,
            frame.Celestial,
            frame.Vehicle.GetVelocityCci()
        );

        vectorRenderer.DrawMarker(draw_list, frame, direction, settings.Prograde, settings.SymbolSize, settings.SymbolLineThickness);
    }
}

public sealed class RetrogradeIndicatorRenderer : IHudIndicator {
    private readonly VectorIndicatorRenderer vectorRenderer;

    public RetrogradeIndicatorRenderer(VectorIndicatorRenderer vectorRenderer) {
        this.vectorRenderer = vectorRenderer;
    }

    public bool IsEnabled(NavHudSettings settings) {
        return settings.Retrograde.Enabled;
    }

    public void Draw(ImDrawListPtr draw_list, NavHudFrame frame, NavHudSettings settings) {
        if(!settings.Retrograde.Enabled) return;

        float3 direction = HudMath.CciDirectionToEgo(
            frame.Vehicle,
            frame.Camera,
            frame.Celestial,
            -frame.Vehicle.GetVelocityCci()
        );

        vectorRenderer.DrawMarker(draw_list, frame, direction, settings.Retrograde, settings.SymbolSize, settings.SymbolLineThickness);
    }
}


public sealed class NormalIndicatorRenderer : IHudIndicator {
    private readonly VectorIndicatorRenderer vectorRenderer;

    public NormalIndicatorRenderer(VectorIndicatorRenderer vectorRenderer) {
        this.vectorRenderer = vectorRenderer;
    }

    public bool IsEnabled(NavHudSettings settings) {
        return settings.Normal.Enabled;
    }

    public void Draw(ImDrawListPtr draw_list, NavHudFrame frame, NavHudSettings settings) {
        if(!settings.Normal.Enabled) return;

        Vehicle vehicle = frame.Vehicle;
        double3 positionCci = vehicle.GetPositionCci();
        double3 velocityCci = vehicle.GetVelocityCci();
        double3 normalCci = HudMath.Cross(positionCci, velocityCci);


        float3 direction = HudMath.CciDirectionToEgo(
            frame.Vehicle,
            frame.Camera,
            frame.Celestial,
            normalCci
        );

        vectorRenderer.DrawMarker(draw_list, frame, direction, settings.Normal, settings.SymbolSize, settings.SymbolLineThickness);
    }
}

public sealed class AntinormalIndicatorRenderer : IHudIndicator {
    private readonly VectorIndicatorRenderer vectorRenderer;

    public AntinormalIndicatorRenderer(VectorIndicatorRenderer vectorRenderer) {
        this.vectorRenderer = vectorRenderer;
    }

    public bool IsEnabled(NavHudSettings settings) {
        return settings.Antinormal.Enabled;
    }

    public void Draw(ImDrawListPtr draw_list, NavHudFrame frame, NavHudSettings settings) {
        if(!settings.Antinormal.Enabled) return;

        Vehicle vehicle = frame.Vehicle;
        double3 positionCci = vehicle.GetPositionCci();
        double3 velocityCci = vehicle.GetVelocityCci();
        double3 normalCci = HudMath.Cross(positionCci, velocityCci);


        float3 direction = HudMath.CciDirectionToEgo(
            frame.Vehicle,
            frame.Camera,
            frame.Celestial,
            -normalCci
        );

        vectorRenderer.DrawMarker(draw_list, frame, direction, settings.Antinormal, settings.SymbolSize, settings.SymbolLineThickness);
    }
}


public sealed class RadialInIndicatorRenderer : IHudIndicator {
    private readonly VectorIndicatorRenderer vectorRenderer;

    public RadialInIndicatorRenderer(VectorIndicatorRenderer vectorRenderer) {
        this.vectorRenderer = vectorRenderer;
    }

    public bool IsEnabled(NavHudSettings settings) {
        return settings.RadialIn.Enabled;
    }

    public void Draw(ImDrawListPtr draw_list, NavHudFrame frame, NavHudSettings settings) {
        if(!settings.RadialIn.Enabled) return;

        Vehicle vehicle = frame.Vehicle;
        double3 positionCci = vehicle.GetPositionCci();
        double3 velocityCci = vehicle.GetVelocityCci();
        double3 normalCci = HudMath.Cross(positionCci, velocityCci);
        double3 radialInCci = HudMath.Cross(normalCci, velocityCci);


        float3 direction = HudMath.CciDirectionToEgo(
            frame.Vehicle,
            frame.Camera,
            frame.Celestial,
            radialInCci
        );

        vectorRenderer.DrawMarker(draw_list, frame, direction, settings.RadialIn, settings.SymbolSize, settings.SymbolLineThickness);
    }
}


public sealed class RadialOutIndicatorRenderer : IHudIndicator {
    private readonly VectorIndicatorRenderer vectorRenderer;

    public RadialOutIndicatorRenderer(VectorIndicatorRenderer vectorRenderer) {
        this.vectorRenderer = vectorRenderer;
    }

    public bool IsEnabled(NavHudSettings settings) {
        return settings.RadialOut.Enabled;
    }

    public void Draw(ImDrawListPtr draw_list, NavHudFrame frame, NavHudSettings settings) {
        if(!settings.RadialOut.Enabled) return;

        Vehicle vehicle = frame.Vehicle;
        double3 positionCci = vehicle.GetPositionCci();
        double3 velocityCci = vehicle.GetVelocityCci();
        double3 normalCci = HudMath.Cross(positionCci, velocityCci);
        double3 radialInCci = HudMath.Cross(normalCci, velocityCci);


        float3 direction = HudMath.CciDirectionToEgo(
            frame.Vehicle,
            frame.Camera,
            frame.Celestial,
            -radialInCci
        );

        vectorRenderer.DrawMarker(draw_list, frame, direction, settings.RadialOut, settings.SymbolSize, settings.SymbolLineThickness);
    }
}

public sealed class TargetIndicatorRenderer : IHudIndicator {
    private readonly VectorIndicatorRenderer vectorRenderer;

    public TargetIndicatorRenderer(VectorIndicatorRenderer vectorRenderer) {
        this.vectorRenderer = vectorRenderer;
    }

    public bool IsEnabled(NavHudSettings settings) {
        return settings.Target.Enabled;
    }

    public void Draw(ImDrawListPtr draw_list, NavHudFrame frame, NavHudSettings settings) {
        if(!settings.Target.Enabled) return;

        Vehicle vehicle = frame.Vehicle;
        IOrbiter? target = vehicle.Target;
        if(target == null) return;
        double3 fromPositionCci = vehicle.GetPositionCci();
        double3 toPositionCci = target.GetPositionCci();
        double3 directionCci = toPositionCci - fromPositionCci;

        float3 direction = HudMath.CciDirectionToEgo(
            frame.Vehicle,
            frame.Camera,
            frame.Celestial,
            directionCci
        );

        vectorRenderer.DrawMarker(draw_list, frame, direction, settings.Target, settings.SymbolSize, settings.SymbolLineThickness);
    }
}

