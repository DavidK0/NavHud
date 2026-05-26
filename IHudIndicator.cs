using Brutal.Numerics;
using KSA;
using System;
using System.Collections.Generic;
using System.Text;

namespace NavHud {
    public interface IHudIndicator {
        bool IsEnabled(NavHudSettings settings);
        void Draw(NavHudFrame frame, NavHudSettings settings);
    }

    public sealed class AttitudeIndicatorRenderer : IHudIndicator {
        private readonly IHudLineRenderer lines;

        public AttitudeIndicatorRenderer(IHudLineRenderer lines) {
            this.lines = lines;
        }

        public bool IsEnabled(NavHudSettings settings) {
            return settings.ShowAttitudeMarker;
        }

        public void Draw(NavHudFrame frame, NavHudSettings settings) {
            if(frame.BodyForwardEgo is not { } forward) return;
            if(frame.BodyRightEgo is not { } right) return;
            if(frame.BodyUpEgo is not { } up) return;

            float3 nose = frame.CenterEgo + forward * frame.Radius;

            float barHalfWidth = frame.Radius * 0.055f;
            float notchHalfWidth = frame.Radius * 0.018f;
            float notchDepth = frame.Radius * 0.028f;

            float3 leftEnd = nose - right * barHalfWidth;
            float3 rightEnd = nose + right * barHalfWidth;

            float3 notchLeft = nose - right * notchHalfWidth;
            float3 notchRight = nose + right * notchHalfWidth;
            float3 notchTip = nose + up * notchDepth;

            float4 color = float4.One;

            lines.Line(leftEnd, notchLeft, color);
            lines.Line(notchRight, rightEnd, color);
            lines.Line(notchLeft, notchTip, color);
            lines.Line(notchTip, notchRight, color);
        }
    }
    public sealed class OrbitalVectorIndicatorRenderer : IHudIndicator {
        private readonly VectorIndicatorRenderer vectors;

        public OrbitalVectorIndicatorRenderer(VectorIndicatorRenderer vectors) {
            this.vectors = vectors;
        }

        public bool IsEnabled(NavHudSettings settings) {
            return
                settings.Prograde.Enabled ||
                settings.Retrograde.Enabled ||
                settings.Normal.Enabled ||
                settings.Antinormal.Enabled ||
                settings.RadialOut.Enabled ||
                settings.RadialIn.Enabled;
        }

        public void Draw(NavHudFrame frame, NavHudSettings settings) {
            Vehicle vehicle = frame.Vehicle;

            double3 positionCci = vehicle.GetPositionCci();
            double3 velocityCci = vehicle.GetVelocityCci();

            if(HudMath.IsZero(positionCci)) return;
            if(HudMath.IsZero(velocityCci)) return;

            double3 normalCci = HudMath.Cross(positionCci, velocityCci);
            if(HudMath.IsZero(normalCci)) return;

            DrawDirection(frame, velocityCci, settings.Prograde);
            DrawDirection(frame, -velocityCci, settings.Retrograde);

            DrawDirection(frame, normalCci, settings.Normal);
            DrawDirection(frame, -normalCci, settings.Antinormal);

            DrawDirection(frame, positionCci, settings.RadialOut);
            DrawDirection(frame, -positionCci, settings.RadialIn);
        }

        private void DrawDirection(
            NavHudFrame frame,
            double3 directionCci,
            IndicatorSettings settings
        ) {
            if(!settings.Enabled) return;

            float3 directionEgo = HudMath.CciDirectionToEgo(
                frame.Vehicle,
                frame.Camera,
                frame.Celestial,
                directionCci
            );

            vectors.DrawMarker(frame, directionEgo, settings);
        }
    }
}
