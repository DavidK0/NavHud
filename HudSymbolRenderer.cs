using Brutal.ImGuiApi;
using Brutal.Numerics;
using KSA;
using NavHud;

namespace NavHud;
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
        float3 vehiclePos,
        float radius,
        float symbolSize,
        float thickness,
        float4 color,
        float3 forward,
        float3 right,
        float3 up
    ) {
        float3 symbolCenter = vehiclePos + forward * radius;

        symbolSize = radius * symbolSize;

        switch(symbol) {
            case HudSymbol.Cross:
                DrawCross(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
                break;

            case HudSymbol.X:
                DrawX(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
                break;

            case HudSymbol.Diamond:
                DrawDiamond(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
                break;

            case HudSymbol.Attitude:
                DrawAttitude(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
                break;

            case HudSymbol.Antiattitude:
                DrawAntiattitude(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
                break;

            case HudSymbol.Prograde:
                DrawPrograde(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
                break;

            case HudSymbol.Retrograde:
                DrawRetrograde(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
                break;

            case HudSymbol.Normal:
                DrawNormal(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
                break;

            case HudSymbol.Antinormal:
                DrawAntinormal(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
                break;

            case HudSymbol.RadialIn:
                DrawRadialIn(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
                break;

            case HudSymbol.RadialOut:
                DrawRadialOut(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
                break;

            case HudSymbol.Target:
                DrawTarget(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
                break;

            case HudSymbol.Antitarget:
                DrawAntitarget(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
                break;

            case HudSymbol.Maneuver:
                DrawManeuver(draw_list, symbolCenter, right, up, symbolSize, color, thickness);
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
        float thickness
    ) {
        float barHalfWidth = size * 0.55f;
        float notchHalfWidth = size * 0.15f;
        float notchDepth = size * 0.75f;

        float3 leftEnd = center - right * barHalfWidth;
        float3 rightEnd = center + right * barHalfWidth;

        float3 notchLeft = center - right * notchHalfWidth;
        float3 notchRight = center + right * notchHalfWidth;
        float3 notchTip = center + up * notchDepth;

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
        float thickness
    ) {
        float barHalfWidth = size * 0.55f;
        float notchHalfWidth = size * 0.15f;
        float notchDepth = size * 0.75f;

        float3 leftEnd = center - right * barHalfWidth;
        float3 rightEnd = center + right * barHalfWidth;

        float3 notchLeft = center - right * notchHalfWidth;
        float3 notchRight = center + right * notchHalfWidth;

        float3 leftNotchTip = notchLeft - up * notchDepth;
        float3 rightNotchTip = notchRight - up * notchDepth;

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

        float3 downLeft = VectorMath.Normalize(-right - up * 0.65f);
        float3 downRight = VectorMath.Normalize(right - up * 0.65f);

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

        float3 downLeft = VectorMath.Normalize(-right - up * 0.60f);
        float3 downRight = VectorMath.Normalize(right - up * 0.60f);

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

        float3 a = VectorMath.Normalize(right + up);
        float3 b = VectorMath.Normalize(right - up);

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

        float3 a = VectorMath.Normalize(right + up);
        float3 b = VectorMath.Normalize(right - up);

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
        float3 a = VectorMath.Normalize(right + up);
        float3 b = VectorMath.Normalize(right - up);

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
        float3 cornerDir = VectorMath.Normalize(right * sx + up * sy);

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
        float3 cornerDir = VectorMath.Normalize(right * sx + up * sy);

        float3 inner = center + cornerDir * size * 0.82f;
        float3 outer = center + cornerDir * size * 1.28f;

        lines.Line(draw_list, inner, outer, color, thickness);
    }
}

public interface IHudIndicatorRenderer {
    void Draw(
        ImDrawListPtr drawList,
        Basis veloFrame,
        float3 center,
        float radius,
        SymbolsSettings settings
    );
}

public class VelocityRenderer : IHudIndicatorRenderer {
    private readonly HudSymbolRenderer symbols;
    public VelocityRenderer(HudSymbolRenderer symbols) {
        this.symbols = symbols;
    }
    public void Draw(
        ImDrawListPtr drawList,
        Basis veloFrame,
        float3 center,
        float radius,
        SymbolsSettings settings
    ) {
        float3 forward = veloFrame.forward;
        float3 up = veloFrame.up;
        float3 right = veloFrame.right;

        float4 green = new float4(0.0f, 1.0f, 0.0f, 1.0f);
        float4 cyan = new float4(0.0f, 1.0f, 1.0f, 1.0f);
        float4 magenta = new float4(1.0f, 0.0f, 1.0f, 1.0f);

        symbols.Draw(drawList, HudSymbol.Prograde, center, radius, settings.Size, settings.LineThickness, green, forward, right, up);
        symbols.Draw(drawList, HudSymbol.Retrograde, center, radius, settings.Size, settings.LineThickness, green, -forward, -right, up);
        symbols.Draw(drawList, HudSymbol.Normal, center, radius, settings.Size, settings.LineThickness, magenta, right, -forward, up);
        symbols.Draw(drawList, HudSymbol.Antinormal, center, radius, settings.Size, settings.LineThickness, magenta, -right, forward, up);
        symbols.Draw(drawList, HudSymbol.RadialOut, center, radius, settings.Size, settings.LineThickness, cyan, up, right, -forward);
        symbols.Draw(drawList, HudSymbol.RadialIn, center, radius, settings.Size, settings.LineThickness, cyan, -up, right, forward);
    }
}

public class AttitudeIndicatorRenderer : IHudIndicatorRenderer {
    private readonly HudSymbolRenderer symbols;
    public AttitudeIndicatorRenderer(HudSymbolRenderer symbols) {
        this.symbols = symbols;
    }
    public void Draw(
        ImDrawListPtr drawList,
        Basis veloFrame,
        float3 center,
        float radius,
        SymbolsSettings settings
    ) {
        float3 forward = veloFrame.forward;
        float3 up = veloFrame.up;
        float3 right = veloFrame.right;

        float4 pale_yellow = new float4(1f, 1f, .5f, 1f);

        symbols.Draw(drawList, HudSymbol.Attitude, center, radius, settings.Size, settings.LineThickness, pale_yellow, forward, right, up);
        symbols.Draw(drawList, HudSymbol.Antiattitude, center, radius, settings.Size, settings.LineThickness, pale_yellow, -forward, -right, up);
    }
}

public class TargetIndicatorRenderer : IHudIndicatorRenderer {
    private readonly HudSymbolRenderer symbols;
    public TargetIndicatorRenderer(HudSymbolRenderer symbols) {
        this.symbols = symbols;
    }
    public void Draw(
        ImDrawListPtr drawList,
        Basis veloFrame,
        float3 center,
        float radius,
        SymbolsSettings settings
    ) {
        float3 forward = veloFrame.forward;
        float3 up = veloFrame.up;
        float3 right = veloFrame.right;

        float4 magenta = new float4(1.0f, 0.0f, 1.0f, 1.0f);

        symbols.Draw(drawList, HudSymbol.Target, center, radius, settings.Size, settings.LineThickness, magenta, forward, right, up);
        symbols.Draw(drawList, HudSymbol.Antitarget, center, radius, settings.Size, settings.LineThickness, magenta, -forward, -right, up);
    }
}

public class DockIndicatorRenderer : IHudIndicatorRenderer {
    private readonly HudSymbolRenderer symbols;
    public DockIndicatorRenderer(HudSymbolRenderer symbols) {
        this.symbols = symbols;
    }
    public void Draw(
        ImDrawListPtr drawList,
        Basis veloFrame,
        float3 center,
        float radius,
        SymbolsSettings settings
    ) {
        float3 forward = veloFrame.forward;
        float3 up = veloFrame.up;
        float3 right = veloFrame.right;

        float4 red = new float4(1.0f, 0.0f, 0.0f, 1.0f);

        symbols.Draw(drawList, HudSymbol.Cross, center, radius, settings.Size, settings.LineThickness, red, forward, right, up);
    }
}

public class BurnIndicatorRenderer : IHudIndicatorRenderer {
    private readonly HudSymbolRenderer symbols;
    public BurnIndicatorRenderer(HudSymbolRenderer symbols) {
        this.symbols = symbols;
    }
    public void Draw(
        ImDrawListPtr drawList,
        Basis veloFrame,
        float3 center,
        float radius,
        SymbolsSettings settings
    ) {
        float3 forward = veloFrame.forward;
        float3 up = veloFrame.up;
        float3 right = veloFrame.right;

        float4 blue = new float4(0.0f, 0.0f, 1.0f, 1.0f);

        symbols.Draw(drawList, HudSymbol.Diamond, center, radius, settings.Size, settings.LineThickness, blue, forward, right, up);

    }
}