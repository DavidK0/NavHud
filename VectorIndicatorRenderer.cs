using Brutal.ImGuiApi;
using Brutal.Numerics;
using KSA;
using NavHud;
using System;
using System.Collections.Generic;
using System.Text;

namespace NavHud {
    public sealed class VectorIndicatorRenderer {
        private readonly HudSymbolRenderer symbols;

        public VectorIndicatorRenderer(HudSymbolRenderer symbols) {
            this.symbols = symbols;
        }

        public void DrawMarker(
            ImDrawListPtr draw_list,
            NavHudFrame frame,
            float3 directionEgo,
            IndicatorSettings settings,
            float symbolSize,
            float thickness,
            float rollRadians=0f
        ) {
            if(!settings.Enabled) return;

            float3 dir = HudMath.Normalize(directionEgo);
            if(HudMath.IsZero(dir)) return;

            float3 position = frame.CenterEgo + dir * frame.Radius;

            symbols.Draw(
                draw_list,
                settings.Symbol,
                position,
                dir,
                frame.Radius,
                settings.Color,
                symbolSize,
                thickness,
                rollRadians
            );
        }
    }
}
