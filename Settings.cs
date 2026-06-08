using Brutal.Numerics;
using System.Globalization;
using System.Text;
using Brutal.Logging;

namespace NavHud;

public sealed class NavHudSettings {
    public bool Enabled = true;

    public NavFrame GridFrame = NavFrame.Auto;
    public NavFrame VelocityFrame = NavFrame.Auto;

    public GridSettings Grid = new();
    public VelocitySettings Velocity = new();

    public bool IgnoreZoom = true;
    public float FixedSphereSize = 10.0f;
    public float ZoomScale = 100.0f;

    public float SymbolSize = 0.03f;
    public float SymbolLineThickness = 2.0f;

    public IndicatorSettings Attitude = new(HudSymbol.Attitude, new float4(1.0f, 1.0f, 1.0f, 1.0f)); // White
    public IndicatorSettings Antiattitude = new(HudSymbol.Antiattitude, new float4(1.0f, 1.0f, 1.0f, 1.0f)); // White
    public IndicatorSettings Prograde = new(HudSymbol.Prograde, new float4(1.0f, 1.0f, 0.0f, 1.0f)); // Yellow
    public IndicatorSettings Retrograde = new(HudSymbol.Retrograde, new float4(1.0f, 1.0f, 0.0f, 1.0f)); // Yellow
    public IndicatorSettings Normal = new(HudSymbol.Normal, new float4(1.0f, 0.0f, 1.0f, 1.0f)); // Magenta
    public IndicatorSettings Antinormal = new(HudSymbol.Antinormal, new float4(1.0f, 0.0f, 1.0f, 1.0f)); // Magenta
    public IndicatorSettings RadialIn = new(HudSymbol.RadialIn, new float4(0.0f, 1.0f, 1.0f, 1.0f)); // Cyan
    public IndicatorSettings RadialOut = new(HudSymbol.RadialOut, new float4(0.0f, 1.0f, 1.0f, 1.0f)); // Cyan
    public IndicatorSettings Target = new(HudSymbol.Target, new float4(0.0f, 1.0f, 1.0f, 1.0f)); // Cyan
    public IndicatorSettings Antitarget = new(HudSymbol.Antitarget, new float4(0.0f, 1.0f, 1.0f, 1.0f)); // Cyan
    public IndicatorSettings DockingAlignment = new(HudSymbol.DockingAlignment, new float4(0.0f, 0.0f, 1.0f, 1.0f)); // Blue
    public IndicatorSettings Maneuver = new(HudSymbol.Maneuver, new float4(0.0f, 0.0f, 1.0f, 1.0f)); // Blue

    public NavHudSettings Clone() {
        return new NavHudSettings {
            Enabled = Enabled,

            GridFrame = GridFrame,
            VelocityFrame = VelocityFrame,

            Grid = Grid.Clone(),
            Velocity = Velocity.Clone(),

            IgnoreZoom = IgnoreZoom,
            FixedSphereSize = FixedSphereSize,
            ZoomScale = ZoomScale,

            Prograde = Prograde.Clone(),
            Retrograde = Retrograde.Clone(),
            Normal = Normal.Clone(),
            Antinormal = Antinormal.Clone(),
            RadialIn = RadialIn.Clone(),
            RadialOut = RadialOut.Clone(),
            Target = Target.Clone(),
            Antitarget = Antitarget.Clone(),
            DockingAlignment = DockingAlignment.Clone(),
            Maneuver = Maneuver.Clone()
        };
    }
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

    public IndicatorSettings Clone() {
        return new IndicatorSettings {
            Enabled = Enabled,
            Color = Color,
            Symbol = Symbol
        };
    }
}

public enum NavFrame {
    Auto,

    Cce,
    Cci,
    Enu,
    Lvlh,

    SurfVel,
    Vlh,
    Burn,
    TVel,

    Tgt,
    Dock
}

public sealed class GridSettings {
    public bool Enabled = true;
    public int Segments = 64;
    public int Rings = 12;
    public float4 Color = float4.One;

    public GridSettings Clone() {
        return new GridSettings {
            Segments = Segments,
            Rings = Rings,
            Color = Color
        };
    }
}

public sealed class VelocitySettings {
    public bool Enabled = true;
    public float Size = .01f;
    public float LineThickness = 1f;

    public VelocitySettings Clone() {
        return new VelocitySettings {
            Enabled = Enabled,
            Size = Size,
            LineThickness = LineThickness
        };
    }
}

internal static class NavHudSettingsStore {
    private static SaveScopedSettingsStore<NavHudSettings>? _store;
    private static NavHudSettings _current = new();

    public static NavHudSettings Current {
        get {
            EnsureInitialized();
            return _current;
        }
    }

    public static void LoadForSave(string saveId) {
        EnsureInitialized();

        _store.Load();

        // Important: clone so UI does not mutate the persisted block directly.
        _current = _store.GetCurrent(saveId).Clone();
    }

    public static void SaveForSave(string saveId) {
        EnsureInitialized();

        if(string.IsNullOrEmpty(saveId))
            return;

        // Important: clone so future UI edits do not mutate the saved block directly.
        _store.Set(saveId, _current.Clone());
        _store.Save();
    }

    public static void SetCurrentFromDefaults() {
        _current = new NavHudSettings();
    }

    private static void EnsureInitialized() {
        if(_store == null)
            throw new InvalidOperationException("NavHudSettingsStore.Init() must be called before use.");
    }

    public static void Init() {
        string userDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        string savesDir = Path.Combine(
            userDocs,
            "My Games",
            "Kitten Space Agency",
            "saves");

        _store = new SaveScopedSettingsStore<NavHudSettings>(
            savesDir,
            "NavHud_settings.toml",
            () => new NavHudSettings(),
            NavHudSettingsToml.Read,
            NavHudSettingsToml.Write);
    }

    public static void Load() {
        EnsureInitialized();
        _store.Load();
    }

    public static void Save() {
        EnsureInitialized();
        _store.Save();
    }
}

internal static class NavHudSettingsToml {
    public static NavHudSettings Read(SettingsBlock block) {
        var s = new NavHudSettings();

        s.Enabled = block.GetBool("enabled", s.Enabled);

        if(block.TryEnum("grid_frame", out NavFrame gridFrame))
            s.GridFrame = gridFrame;
        if(block.TryEnum("velocity_frame", out NavFrame velocityFrame))
            s.VelocityFrame = velocityFrame;

        s.Grid.Enabled = block.GetBool(
            "grid_enabled",
            s.Grid.Enabled);
        s.Grid.Segments = block.GetInt(
            "grid_segments",
            s.Grid.Segments);
        s.Grid.Rings = block.GetInt(
            "grid_rings",
            s.Grid.Rings);
        s.Grid.Color = block.GetFloat4(
            "grid_color",
            s.Grid.Color);

        s.Velocity.Enabled = block.GetBool(
            "velocity_enabled",
            s.Velocity.Enabled);
        s.Velocity.Size = block.GetFloat(
            "velocity_size",
            s.Velocity.Size);
        s.Velocity.LineThickness = block.GetFloat(
            "velocity_line_thickness",
            s.Velocity.LineThickness);

        s.IgnoreZoom = block.GetBool("ignore_zoom", s.IgnoreZoom);
        s.FixedSphereSize = block.GetFloat("fixed_sphere_size", s.FixedSphereSize);
        s.ZoomScale = block.GetFloat("zoom_scale", s.ZoomScale);

        ReadIndicator(block, "prograde", s.Prograde);
        ReadIndicator(block, "retrograde", s.Retrograde);
        ReadIndicator(block, "normal", s.Normal);
        ReadIndicator(block, "antinormal", s.Antinormal);
        ReadIndicator(block, "radial_in", s.RadialIn);
        ReadIndicator(block, "radial_out", s.RadialOut);
        ReadIndicator(block, "target", s.Target);
        ReadIndicator(block, "antitarget", s.Antitarget);
        ReadIndicator(block, "docking_alignment", s.DockingAlignment);
        ReadIndicator(block, "maneuver", s.Maneuver);

        return s;
    }

    public static void Write(
        SettingsBlockWriter writer,
        string saveId,
        NavHudSettings s) {

        writer.Write("enabled", s.Enabled);

        writer.BeginSettingsBlock(saveId);

        writer.Write("grid_frame", s.GridFrame);
        writer.Write("velocity_frame", s.VelocityFrame);

        writer.Write("ignore_zoom", s.IgnoreZoom);
        writer.Write("fixed_sphere_size", s.FixedSphereSize);
        writer.Write("zoom_scale", s.ZoomScale);

        writer.Write("show_grid_lines", s.Grid.Enabled);
        writer.Write("show_velocity", s.Velocity.Enabled);

        writer.Write("symbol_size", s.SymbolSize);
        writer.Write("symbol_line_thickness", s.SymbolLineThickness);

        WriteIndicator(writer, "prograde", s.Prograde);
        WriteIndicator(writer, "retrograde", s.Retrograde);
        WriteIndicator(writer, "normal", s.Normal);
        WriteIndicator(writer, "antinormal", s.Antinormal);
        WriteIndicator(writer, "radial_in", s.RadialIn);
        WriteIndicator(writer, "radial_out", s.RadialOut);
        WriteIndicator(writer, "target", s.Target);
        WriteIndicator(writer, "antitarget", s.Antitarget);
        WriteIndicator(writer, "docking_alignment", s.DockingAlignment);
        WriteIndicator(writer, "maneuver", s.Maneuver);

        writer.Write("grid_segments", s.Grid.Segments);
        writer.Write("grid_rings", s.Grid.Rings);
        writer.Write("grid_color", s.Grid.Color);

        writer.EndBlock();
    }

    private static void ReadIndicator(
        SettingsBlock block,
        string prefix,
        IndicatorSettings s) {

        s.Enabled = block.GetBool(
            $"{prefix}_enabled",
            s.Enabled);

        if(block.TryEnum($"{prefix}_symbol", out HudSymbol symbol))
            s.Symbol = symbol;

        s.Color = block.GetFloat4(
            $"{prefix}_color",
            s.Color);
    }

    private static void WriteIndicator(
        SettingsBlockWriter writer,
        string prefix,
        IndicatorSettings s) {

        writer.Write($"{prefix}_enabled", s.Enabled);
        writer.Write($"{prefix}_symbol", s.Symbol);
        writer.Write($"{prefix}_color", s.Color);
    }
}