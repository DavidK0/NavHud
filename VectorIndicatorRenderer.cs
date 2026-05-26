using Brutal.Numerics;
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
            NavHudFrame frame,
            float3 directionEgo,
            IndicatorSettings settings
        ) {
            if(!settings.Enabled) return;

            float3 dir = HudMath.Normalize(directionEgo);
            if(HudMath.IsZero(dir)) return;

            float3 position = frame.CenterEgo + dir * frame.Radius;

            symbols.Draw(
                settings.Symbol,
                position,
                dir,
                frame.Radius,
                settings.Color
            );
        }
    }
}
