using Brutal.Numerics;

namespace NavHud;

public sealed class NavHudSettings {
    public bool RendezvousTrackEnabled = true;

    public NavFrame GridFrame = NavFrame.Auto;
    public NavFrame VelocityFrame = NavFrame.Auto;

    public GridSettings Grid = new();
    public bool VelocityEnabled = true;

    public SymbolsSettings Symbols = new();

    public bool IgnoreZoom = true;
    public float FixedSphereSize = 10.0f;
    public float ZoomScale = 100.0f;

    public float RendezvousTrackTimeStep = 10;
    public float RendezvousTrackMaxTime = 10000;

    public NavHudSettings Clone() {
        return new NavHudSettings {
            RendezvousTrackEnabled = RendezvousTrackEnabled,

            GridFrame = GridFrame,
            VelocityFrame = VelocityFrame,

            Grid = Grid.Clone(),
            VelocityEnabled = VelocityEnabled,

            Symbols = Symbols.Clone(),

            IgnoreZoom = IgnoreZoom,
            FixedSphereSize = FixedSphereSize,
            ZoomScale = ZoomScale,
        };
    }
}

public sealed class GridSettings {
    public bool Enabled = true;
    public int Segments = 64;
    public int Rings = 12;
    public float4 Color = float4.One;

    public GridSettings Clone() {
        return new GridSettings {
            Enabled = Enabled,
            Segments = Segments,
            Rings = Rings,
            Color = Color
        };
    }
}

public sealed class SymbolsSettings {
    public float Size = .02f;
    public float LineThickness = 5f;

    public SymbolsSettings Clone() {
        return new SymbolsSettings {
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
        
        if(string.IsNullOrEmpty(saveId)) {
            _current = new NavHudSettings();
            return;
        }
        
        _store.Load();
        _current = _store.GetCurrent(saveId).Clone();
    }

    public static void SaveForSave(string saveId) {
        EnsureInitialized();

        if(string.IsNullOrEmpty(saveId))
            return;

        _store.Set(saveId, _current.Clone());
        _store.Save(saveId);
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

        s.RendezvousTrackEnabled = block.GetBool("rendezvous_track_enabled", s.RendezvousTrackEnabled);

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

        s.VelocityEnabled = block.GetBool(
            "velocity_enabled",
            s.VelocityEnabled);

        s.Symbols.Size = block.GetFloat(
            "symbol_size",
            s.Symbols.Size);
        s.Symbols.LineThickness = block.GetFloat(
            "symbol_line_thickness",
            s.Symbols.LineThickness);

        s.IgnoreZoom = block.GetBool("ignore_zoom", s.IgnoreZoom);
        s.FixedSphereSize = block.GetFloat("fixed_sphere_size", s.FixedSphereSize);
        s.ZoomScale = block.GetFloat("zoom_scale", s.ZoomScale);

        s.RendezvousTrackTimeStep = block.GetFloat(
            "rendezvous_track_time_step",
            s.RendezvousTrackTimeStep);
        s.RendezvousTrackMaxTime = block.GetFloat(
            "rendezvous_track_max_time",
            s.RendezvousTrackMaxTime);

        return s;
    }

    public static void Write(
        SettingsBlockWriter writer,
        string saveId,
        NavHudSettings s) {

        writer.BeginSettingsBlock(saveId);

        writer.Write("rendezvous_track_enabled", s.RendezvousTrackEnabled);

        writer.Write("grid_frame", s.GridFrame);
        writer.Write("velocity_frame", s.VelocityFrame);

        writer.Write("ignore_zoom", s.IgnoreZoom);
        writer.Write("fixed_sphere_size", s.FixedSphereSize);
        writer.Write("zoom_scale", s.ZoomScale);

        writer.Write("grid_enabled", s.Grid.Enabled);
        writer.Write("velocity_enabled", s.VelocityEnabled);

        writer.Write("symbol_size", s.Symbols.Size);
        writer.Write("symbol_line_thickness", s.Symbols.LineThickness);

        writer.Write("grid_segments", s.Grid.Segments);
        writer.Write("grid_rings", s.Grid.Rings);
        writer.Write("grid_color", s.Grid.Color);

        writer.Write("rendezvous_track_time_step", s.RendezvousTrackTimeStep);
        writer.Write("rendezvous_track_max_time", s.RendezvousTrackMaxTime);

        writer.EndBlock();
    }
}
