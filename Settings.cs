using Brutal.Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace NavHud {
    public sealed class NavHudSettings {
        public NavMode Mode = NavMode.Basic;

        public bool IgnoreZoom = true;
        public float FixedSphereSize = 10.0f;
        public float ZoomScale = 100.0f;

        public bool ShowGridLines = true;
        public bool ShowAttitudeMarker = true;

        public IndicatorSettings Prograde = new(HudSymbol.Prograde, new float4(1.0f, 1.0f, 0.0f, 1.0f)); // Yellow
        public IndicatorSettings Retrograde = new(HudSymbol.Retrograde, new float4(1.0f, 1.0f, 0.0f, 1.0f)); // Yellow
        public IndicatorSettings Normal = new(HudSymbol.Normal, new float4(1.0f, 0.0f, 1.0f, 1.0f)); // Magenta
        public IndicatorSettings Antinormal = new(HudSymbol.Antinormal, new float4(1.0f, 0.0f, 1.0f, 1.0f)); // Magenta
        public IndicatorSettings RadialIn = new(HudSymbol.RadialIn, new float4(0.0f, 1.0f, 1.0f, 1.0f)); // Cyan
        public IndicatorSettings RadialOut = new(HudSymbol.RadialOut, new float4(0.0f, 1.0f, 1.0f, 1.0f)); // Cyan
        public IndicatorSettings Target = new(HudSymbol.Target, new float4(1.0f, 1.0f, 0.0f, 1.0f)); // Cyan
        public IndicatorSettings Antitarget = new(HudSymbol.Antitarget, new float4(1.0f, 1.0f, 0.0f, 1.0f)); // Cyan
        public IndicatorSettings DockingAlignment = new(HudSymbol.Target, new float4(0.0f, 0.0f, 1.0f, 1.0f)); // Blue
        public IndicatorSettings Maneuver = new(HudSymbol.Target, new float4(0.0f, 0.0f, 1.0f, 1.0f)); // Blue

        public GridSettings Grid = new();

    }

    public sealed class IndicatorSettings {
        public bool Enabled = true;
        public float4 Color = float4.One;
        public HudSymbol Symbol;

        public IndicatorSettings() : this(HudSymbol.Cross, float4.One) {}

        public IndicatorSettings(HudSymbol symbol, float4 color) {
            Symbol = symbol;
            Color = color;
        }
    }

    public enum NavMode {
        Off,
        Basic,
        AzAlt
    }

    public sealed class GridSettings {
        public int Segments = 64;
        public int Rings = 12;
        public float4 Color = float4.One;
    }
}
