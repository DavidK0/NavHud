using Brutal.Numerics;

namespace NavHud {
    public enum HudSymbol {
        Cross,
        X,
        Diamond,
        NotchedBar,
        Prograde,
        Retrograde,
        Normal,
        Antinormal,
        RadialIn,
        RadialOut,
        Target,
        Antitarget,
        Maneuver,
    }

    public sealed class HudSymbolRenderer {
        private readonly IHudLineRenderer lines;

        public HudSymbolRenderer(IHudLineRenderer lines) {
            this.lines = lines;
        }

        public void Draw(
            HudSymbol symbol,
            float3 position,
            float3 normal,
            float radius,
            float4 color
        ) {
            float size = radius * 0.035f;

            GetBillboardBasis(normal, out float3 right, out float3 up);

            switch(symbol) {
                case HudSymbol.Cross:
                    DrawCross(position, right, up, size, color);
                    break;

                case HudSymbol.X:
                    DrawX(position, right, up, size, color);
                    break;

                case HudSymbol.Diamond:
                    DrawDiamond(position, right, up, size, color);
                    break;

                case HudSymbol.NotchedBar:
                    DrawNotchedBar(position, right, up, size, color);
                    break;

                case HudSymbol.Prograde:
                    DrawPrograde(position, right, up, size, color);
                    break;

                case HudSymbol.Retrograde:
                    DrawRetrograde(position, right, up, size, color);
                    break;

                case HudSymbol.Normal:
                    DrawNormal(position, right, up, size, color);
                    break;

                case HudSymbol.Antinormal:
                    DrawAntinormal(position, right, up, size, color);
                    break;

                case HudSymbol.RadialIn:
                    DrawRadialIn(position, right, up, size, color);
                    break;

                case HudSymbol.RadialOut:
                    DrawRadialOut(position, right, up, size, color);
                    break;

                case HudSymbol.Target:
                    DrawTarget(position, right, up, size, color);
                    break;

                case HudSymbol.Antitarget:
                    DrawAntitarget(position, right, up, size, color);
                    break;

                case HudSymbol.Maneuver:
                    DrawManeuver(position, right, up, size, color);
                    break;
            }
        }

        private void DrawCross(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            lines.Line(center - right * size, center + right * size, color);
            lines.Line(center - up * size, center + up * size, color);
        }

        private void DrawX(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            float3 a = right + up;
            float3 b = right - up;

            lines.Line(center - a * size, center + a * size, color);
            lines.Line(center - b * size, center + b * size, color);
        }

        private void DrawDiamond(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            float3 top = center + up * size;
            float3 rightPoint = center + right * size;
            float3 bottom = center - up * size;
            float3 leftPoint = center - right * size;

            lines.Line(top, rightPoint, color);
            lines.Line(rightPoint, bottom, color);
            lines.Line(bottom, leftPoint, color);
            lines.Line(leftPoint, top, color);
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

        private void DrawNotchedBar(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            float barHalfWidth = size * 0.55f;
            float notchHalfWidth = size * 0.15f;
            float notchDepth = size * 0.75f;

            right = HudMath.Normalize(right); 
            up = HudMath.Normalize(up); 
            float3 leftEnd = center - right * barHalfWidth; 
            float3 rightEnd = center + right * barHalfWidth;
            float3 notchLeft = center - right * notchHalfWidth;
            float3 notchRight = center + right * notchHalfWidth; 
            float3 notchTip = center + up * notchDepth;
            lines.Line(leftEnd, notchLeft, color);
            lines.Line(notchRight, rightEnd, color); 
            lines.Line(notchLeft, notchTip, color); 
            lines.Line(notchTip, notchRight, color);
        }

        private void DrawPrograde(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            float r = size * 0.82f;

            DrawCircle(center, right, up, r, color);
            DrawDot(center, right, up, size * 0.11f, color);

            lines.Line(center + up * r, center + up * size * 1.55f, color);
            lines.Line(center - right * r, center - right * size * 1.55f, color);
            lines.Line(center + right * r, center + right * size * 1.55f, color);
        }

        private void DrawRetrograde(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            float r = size * 0.82f;

            DrawCircle(center, right, up, r, color);
            DrawXInside(center, right, up, size * 0.58f, color);

            lines.Line(center + up * r, center + up * size * 1.55f, color);

            float3 downLeft = HudMath.Normalize(-right - up * 0.65f);
            float3 downRight = HudMath.Normalize(right - up * 0.65f);

            lines.Line(center + downLeft * r, center + downLeft * size * 1.55f, color);
            lines.Line(center + downRight * r, center + downRight * size * 1.55f, color);
        }

        private void DrawNormal(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            float h = size * 1.15f;
            float w = size * 1.05f;

            float3 top = center + up * h;
            float3 left = center - up * h * 0.75f - right * w;
            float3 rightPoint = center - up * h * 0.75f + right * w;

            lines.Line(top, rightPoint, color);
            lines.Line(rightPoint, left, color);
            lines.Line(left, top, color);

            DrawDot(center - up * size * 0.18f, right, up, size * 0.10f, color);
        }

        private void DrawAntinormal(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            float h = size * 1.15f;
            float w = size * 1.05f;

            float3 bottom = center - up * h;
            float3 left = center + up * h * 0.75f - right * w;
            float3 rightPoint = center + up * h * 0.75f + right * w;

            lines.Line(bottom, rightPoint, color);
            lines.Line(rightPoint, left, color);
            lines.Line(left, bottom, color);

            DrawDot(center + up * size * 0.18f, right, up, size * 0.10f, color);

            lines.Line(center + up * h * 0.75f, center + up * size * 1.55f, color);

            float3 downLeft = HudMath.Normalize(-right - up * 0.60f);
            float3 downRight = HudMath.Normalize(right - up * 0.60f);

            lines.Line(center + downLeft * size * 0.95f, center + downLeft * size * 1.55f, color);
            lines.Line(center + downRight * size * 0.95f, center + downRight * size * 1.55f, color);
        }

        private void DrawRadialIn(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            float r = size * 0.86f;

            DrawCircle(center, right, up, r, color);

            DrawInwardCornerTick(center, right, up, size, 1.0f, 1.0f, color);
            DrawInwardCornerTick(center, right, up, size, -1.0f, 1.0f, color);
            DrawInwardCornerTick(center, right, up, size, 1.0f, -1.0f, color);
            DrawInwardCornerTick(center, right, up, size, -1.0f, -1.0f, color);
        }

        private void DrawRadialOut(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            float r = size * 0.82f;

            DrawCircle(center, right, up, r, color);
            DrawDot(center, right, up, size * 0.10f, color);

            DrawOutwardCornerTick(center, right, up, size, 1.0f, 1.0f, color);
            DrawOutwardCornerTick(center, right, up, size, -1.0f, 1.0f, color);
            DrawOutwardCornerTick(center, right, up, size, 1.0f, -1.0f, color);
            DrawOutwardCornerTick(center, right, up, size, -1.0f, -1.0f, color);
        }

        private void DrawTarget(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            float r = size * 0.82f;

            DrawCircle(center, right, up, r, color);
            DrawDot(center, right, up, size * 0.10f, color);

            lines.Line(center - right * size * 1.45f, center - right * r, color);
            lines.Line(center + right * r, center + right * size * 1.45f, color);
            lines.Line(center - up * size * 1.45f, center - up * r, color);
            lines.Line(center + up * r, center + up * size * 1.45f, color);
        }

        private void DrawAntitarget(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            float r = size * 0.82f;

            DrawCircle(center, right, up, r, color);
            DrawXInside(center, right, up, size * 0.58f, color);

            float3 a = HudMath.Normalize(right + up);
            float3 b = HudMath.Normalize(right - up);

            lines.Line(center + a * r, center + a * size * 1.45f, color);
            lines.Line(center - a * r, center - a * size * 1.45f, color);
            lines.Line(center + b * r, center + b * size * 1.45f, color);
            lines.Line(center - b * r, center - b * size * 1.45f, color);
        }

        private void DrawManeuver(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            float inner = size * 0.36f;
            float outer = size * 1.05f;

            DrawCircle(center, right, up, inner, color);

            lines.Line(center + up * inner, center + up * outer, color);
            lines.Line(center - up * inner, center - up * outer, color);
            lines.Line(center + right * inner, center + right * outer, color);
            lines.Line(center - right * inner, center - right * outer, color);

            float3 a = HudMath.Normalize(right + up);
            float3 b = HudMath.Normalize(right - up);

            lines.Line(center + a * inner, center + a * outer, color);
            lines.Line(center - a * inner, center - a * outer, color);
            lines.Line(center + b * inner, center + b * outer, color);
            lines.Line(center - b * inner, center - b * outer, color);
        }
        private void DrawCircle(
            float3 center,
            float3 right,
            float3 up,
            float radius,
            float4 color,
            int segments = 40
        ) {
            float step = MathF.PI * 2.0f / segments;

            float3 previous = center + right * radius;

            for(int i = 1; i <= segments; i++) {
                float a = step * i;

                float3 current =
                    center +
                    right * (MathF.Cos(a) * radius) +
                    up * (MathF.Sin(a) * radius);

                lines.Line(previous, current, color);
                previous = current;
            }
        }

        private void DrawDot(
            float3 center,
            float3 right,
            float3 up,
            float radius,
            float4 color
        ) {
            DrawCircle(center, right, up, radius, color, 12);
        }

        private void DrawXInside(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float4 color
        ) {
            float3 a = HudMath.Normalize(right + up);
            float3 b = HudMath.Normalize(right - up);

            lines.Line(center - a * size, center + a * size, color);
            lines.Line(center - b * size, center + b * size, color);
        }

        private void DrawInwardCornerTick(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float sx,
            float sy,
            float4 color
        ) {
            float3 cornerDir = HudMath.Normalize(right * sx + up * sy);

            float3 outer = center + cornerDir * size * 0.78f;
            float3 inner = center + cornerDir * size * 0.43f;

            lines.Line(outer, inner, color);
        }

        private void DrawOutwardCornerTick(
            float3 center,
            float3 right,
            float3 up,
            float size,
            float sx,
            float sy,
            float4 color
        ) {
            float3 cornerDir = HudMath.Normalize(right * sx + up * sy);

            float3 inner = center + cornerDir * size * 0.82f;
            float3 outer = center + cornerDir * size * 1.28f;

            lines.Line(inner, outer, color);
        }
    }
}