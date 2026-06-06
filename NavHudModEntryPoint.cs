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

        if(ImGui.Checkbox("Enabled", ref settings.Enabled)) {
            settings.Enabled = !settings.Enabled;
        }

        if(ImGui.BeginMenu("Grid Orientation")) {

            if(ImGui.MenuItem("Auto", "", settings.Mode == NavMode.Auto)) {
                settings.Mode = NavMode.Auto;
            }

            if(ImGui.MenuItem("Star", "", settings.Mode == NavMode.Cce)) {
                settings.Mode = NavMode.Cce;
            }

            if(ImGui.MenuItem("Surf", "", settings.Mode == NavMode.EnuBody)) {
                settings.Mode = NavMode.EnuBody;
            }

            if(ImGui.MenuItem("Orbit", "", settings.Mode == NavMode.Lvlh)) {
                settings.Mode = NavMode.Lvlh;
            }

            if(ImGui.MenuItem("OVel", "", settings.Mode == NavMode.VlfBody)) {
                settings.Mode = NavMode.VlfBody;
            }

            if(ImGui.MenuItem("Burn", "", settings.Mode == NavMode.BurnBody)) {
                settings.Mode = NavMode.BurnBody;
            }

            // Only show target-relative options if the vehicle has a target
            IOrbiter? target = Program.ControlledVehicle.Target;

            if(target != null) {
                // Direction-to-target frame
                if(ImGui.MenuItem("TGT", "", settings.Mode == NavMode.Tgt)) {
                    settings.Mode = NavMode.Tgt;
                }

                // Relative target velocity frame
                if(ImGui.MenuItem("TVel", "", settings.Mode == NavMode.TVel)) {
                    settings.Mode = NavMode.TVel;
                }
            }

            // I'm leaving this out until the base game adds docking
            // Docking/target body frame
            //if(ImGui.MenuItem("Dock", "", settings.Mode == NavMode.Dock)) {
            //    settings.Mode = NavMode.Dock;
            //}

            ImGui.EndMenu();
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

        ImGui.Checkbox("Show Grid Lines", ref settings.ShowGridLines);

        // Add a checkbox for showing symbols.
        // If symbols are shown, add drop down menu which has: slider for symbol line thickness, and checkbox for showing arrows to symbols
        if(ImGui.BeginMenu("Symbols")) {
            //ImGui.Checkbox("Show Symbols", ref settings.ShowSymbols);
            ImGui.SliderFloat(
                "Symbol Line Thickness",
                ref settings.SymbolLineThickness,
                1f,
                15.0f
            );
            ImGui.SliderFloat(
                "Symbol Size",
                ref settings.SymbolSize,
                .01f,
                .3f
            );
            //ImGui.Checkbox("Show Arrows To Symbols", ref settings.ShowArrowsToSymbols);
            ImGui.EndMenu();
        }
    }

    [StarMapAfterGui]
    public static void OnAfterUi(double dt) {
        // If the vehicle doesn't have a target and the current mode is a target-relative mode, change the mode to auto
        if(NavHudSettingsStore.Current.Mode is NavMode.Tgt or NavMode.TVel or NavMode.Dock) {
            IOrbiter? target = Program.ControlledVehicle.Target;
            if(target == null) {
                NavHudSettingsStore.Current.Mode = NavMode.Auto;
            }
        }

        Renderer.Draw(dt, NavHudSettingsStore.Current);
    }
}