using System.Collections.Generic;
using Brutal.ImGuiApi;
using KSA;
using ModMenu;
using StarMap.API;

namespace NavHud;

[StarMapMod]
public class NavHudModEntryPoint {
    private static readonly NavHudSettings Settings = new();
    private static readonly NavHudRenderer Renderer = new(Settings);

    [ModMenuEntry("NavHud")]
    public static void DrawMenu() {
        if(ImGui.MenuItem("Off", "", Settings.Mode == NavMode.Off)) {
            Settings.Mode = NavMode.Off;
        }

        if(ImGui.MenuItem("Equatorial", "", Settings.Mode == NavMode.Basic)) {
            Settings.Mode = NavMode.Basic;
        }

        if(ImGui.MenuItem("Az / Alt", "", Settings.Mode == NavMode.AzAlt)) {
            Settings.Mode = NavMode.AzAlt;
        }

        ImGui.Separator();

        ImGui.Checkbox("Ignore Zoom", ref Settings.IgnoreZoom);

        if(!Settings.IgnoreZoom) {
            ImGui.SliderFloat(
                "Sphere Size",
                ref Settings.FixedSphereSize,
                0f,
                200.0f
            );
        }

        ImGui.Separator();

        ImGui.Checkbox("Show Grid Lines", ref Settings.ShowGridLines);
        ImGui.Checkbox("Show Attitude Marker", ref Settings.ShowAttitudeMarker);
    }

    [StarMapAfterGui]
    public void OnAfterUi(double dt) {
        Renderer.Draw(dt);
    }
}