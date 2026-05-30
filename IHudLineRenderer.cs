using Brutal.ImGuiApi;
using Brutal.Numerics;
using KSA;
using System;
using System.Collections.Generic;
using System.Text;

namespace NavHud;
public interface IHudLineRenderer {
    void Line(ImDrawListPtr draw_list, float3 a, float3 b, float4 color, float thickness = 2.0f);
}

public sealed class GizmoHudLineRenderer : IHudLineRenderer {
    public void Line(ImDrawListPtr draw_list, float3 a, float3 b, float4 color, float thickness = 2.0f) {
        Program.GizmosRenderer.DrawLine(a, b, color);
    }
}

public sealed class ImDrawLineRenderer : IHudLineRenderer {
    public void Line(ImDrawListPtr draw_list, float3 a, float3 b, float4 color, float thickness = 2.0f) {
        Camera camera = Program.GetCamera();

        float2 aScreen = camera.EgoToScreen(a);
        float2 bScreen = camera.EgoToScreen(b);


        if(float.IsNaN(aScreen.X) || float.IsNaN(bScreen.X))
            return;

        byte red = (byte)(color.X * 255);
        byte green = (byte)(color.Y * 255);
        byte blue = (byte)(color.Z * 255);

        ImDrawListExtensions.AddLine(
            draw_list,
            aScreen,
            bScreen,
            new ImColor8(red, green, blue ),
            thickness);
    }
}
