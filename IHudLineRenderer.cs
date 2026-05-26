using Brutal.Numerics;
using KSA;
using System;
using System.Collections.Generic;
using System.Text;

namespace NavHud {
    public interface IHudLineRenderer {
        void Line(float3 a, float3 b, float4 color);
    }

    public sealed class GizmoHudLineRenderer : IHudLineRenderer {
        public void Line(float3 a, float3 b, float4 color) {
            Program.GizmosRenderer.DrawLine(a, b, color);
        }
    }
}
