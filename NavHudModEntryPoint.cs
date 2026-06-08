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

    private static NavHudRenderer? Renderer;

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

        ImGui.Checkbox("Enabled", ref settings.Enabled);

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

        ImGui.Checkbox("Show Grid Lines", ref settings.Grid.Enabled);

        if(ImGui.BeginMenu("Grid Orientation")) {

            if(ImGui.MenuItem("Auto", "", settings.GridFrame == NavFrame.Auto)) {
                settings.GridFrame = NavFrame.Auto;
            }

            if(ImGui.MenuItem("Star", "", settings.GridFrame == NavFrame.Cce)) {
                settings.GridFrame = NavFrame.Cce;
            }

            if(ImGui.MenuItem("Equatorial", "", settings.GridFrame == NavFrame.Cci)) {
                settings.GridFrame = NavFrame.Cci;
            }

            if(ImGui.MenuItem("Surface", "", settings.GridFrame == NavFrame.Enu)) {
                settings.GridFrame = NavFrame.Enu;
            }

            if(ImGui.MenuItem("Orbit", "", settings.GridFrame == NavFrame.Lvlh)) {
                settings.GridFrame = NavFrame.Lvlh;
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();


        if(ImGui.BeginMenu("Velocity Mode")) {

            if(ImGui.MenuItem("Auto", "", settings.VelocityFrame == NavFrame.Auto)) {
                settings.VelocityFrame = NavFrame.Auto;
            }

            if(ImGui.MenuItem("Surface", "", settings.VelocityFrame == NavFrame.SurfVel)) {
                settings.VelocityFrame = NavFrame.SurfVel;
            }

            if(ImGui.MenuItem("Orbit", "", settings.VelocityFrame == NavFrame.Vlh)) {
                settings.VelocityFrame = NavFrame.Vlh;
            }

            if(Program.ControlledVehicle != null && Program.ControlledVehicle.Target != null) {
                if(ImGui.MenuItem("Target", "", settings.VelocityFrame == NavFrame.TVel)) {
                    settings.VelocityFrame = NavFrame.TVel;
                }
            }

            ImGui.EndMenu();
        }


        // Add a checkbox for showing symbols.
        // If symbols are shown, add drop down menu which has: slider for symbol line thickness, and checkbox for showing arrows to symbols
        if(ImGui.BeginMenu("Symbols")) {
            //ImGui.Checkbox("Show Symbols", ref settings.ShowSymbols);
            ImGui.SliderFloat(
                "Symbol Line Thickness",
                ref settings.Velocity.LineThickness,
                1f,
                15.0f
            );
            ImGui.SliderFloat(
                "Symbol Size",
                ref settings.Velocity.Size,
                .01f,
                .3f
            );
            //ImGui.Checkbox("Show Arrows To Symbols", ref settings.ShowArrowsToSymbols);
            ImGui.EndMenu();
        }
    }

    [StarMapAfterGui]
    public static void OnAfterUi(double dt) {
        if(Renderer == null)
            return;

        // If the vehicle doesn't have a target and the current mode is a target-relative mode, change the mode to auto
        if(NavHudSettingsStore.Current.GridFrame is NavFrame.Tgt or NavFrame.TVel or NavFrame.Dock) {
            IOrbiter? target = Program.ControlledVehicle?.Target;
            if(target == null) {
                NavHudSettingsStore.Current.GridFrame = NavFrame.Auto;
            }
        }

        Renderer.Draw(dt, NavHudSettingsStore.Current);
    }
}
