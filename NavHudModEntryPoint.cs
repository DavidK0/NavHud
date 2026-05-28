using Brutal.ImGuiApi;
using HarmonyLib;
using KSA;
using ModMenu;
using StarMap.API;
using System.Collections.Generic;

namespace NavHud;

[StarMapMod]
public class NavHudModEntryPoint {
    private static Harmony? _harmony;

    private static NavHudRenderer Renderer;

    [StarMapAllModsLoaded]
    public static void OnFullyLoaded() {
        NavHudSettingsStore.Init();
        NavHudSettingsStore.Load();

        _harmony = new Harmony("dejvid.navhud");
        Renderer = new NavHudRenderer();

        SaveLoadObserver.ApplyPatches(_harmony);
    }

    [ModMenuEntry("NavHud")]
    public static void DrawMenu() {
        NavHudSettings settings = NavHudSettingsStore.Current;

        if(ImGui.MenuItem("Off", "", settings.Mode == NavMode.Off)) {
            settings.Mode = NavMode.Off;
        }

        if(ImGui.MenuItem("Equatorial", "", settings.Mode == NavMode.Equatorial)) {
            settings.Mode = NavMode.Equatorial;
        }

        if(ImGui.MenuItem("Az / Alt", "", settings.Mode == NavMode.AzAlt)) {
            settings.Mode = NavMode.AzAlt;
        }

        ImGui.Separator();

        ImGui.Checkbox("Ignore Zoom", ref settings.IgnoreZoom);

        if(!settings.IgnoreZoom) {
            ImGui.SliderFloat(
                "Sphere Size",
                ref settings.FixedSphereSize,
                0f,
                200.0f
            );
        }

        ImGui.Separator();

        ImGui.Checkbox("Show Grid Lines", ref settings.ShowGridLines);
        ImGui.Checkbox("Show Attitude Marker", ref settings.ShowAttitudeMarker);
    }

    [StarMapAfterGui]
    public static void OnAfterUi(double dt) {
        Renderer.Draw(dt, NavHudSettingsStore.Current);
    }
}