
using Brutal.ImGuiApi;
using Brutal.Logging;
using Brutal.Numerics;
using KSA;

namespace NavHud;

public enum NavFrame {
    Auto,

    Cce,
    Cci,
    Enu,
    Lvlh,

    SurfVel,
    Vlh,
    TVel,

    Attitude,
    Tgt,
    Burn,
    Dock
}

public unsafe sealed class NavHudRenderer {
    private readonly ImDrawLineRenderer lines;

    private readonly GridRenderer gridRenderer;
    private readonly VelocityRenderer velocityRenderer;

    private readonly AttitudeIndicatorRenderer attitudeRenderer;
    private readonly TargetIndicatorRenderer targetRenderer;
    private readonly DockIndicatorRenderer dockRenderer;
    private readonly BurnIndicatorRenderer burnRenderer;

    public NavHudRenderer() {
        lines = new ImDrawLineRenderer();
        var symbolRenderer = new HudSymbolRenderer(lines);
        gridRenderer = new GridRenderer(lines);
        velocityRenderer = new VelocityRenderer(symbolRenderer);

        attitudeRenderer = new AttitudeIndicatorRenderer(symbolRenderer);
        targetRenderer = new TargetIndicatorRenderer(symbolRenderer);
        dockRenderer = new DockIndicatorRenderer(symbolRenderer);
        burnRenderer = new BurnIndicatorRenderer(symbolRenderer);
    }

    public void Draw(double dt, NavHudSettings? settings) {
        if(settings == null) return;
        if(!settings.Enabled) return;

        Vehicle vehicle = Program.ControlledVehicle;
        Camera camera = Program.GetMainCamera();

        if(vehicle == null || camera == null) {
            return;
        }

        IParentBody parentBody = (IParentBody)vehicle.Orbit.Parent;

        // CREATE WINDOW
        ImGuiViewport* viewport = ImGui.GetMainViewport();
        if(viewport == null)
            return;
        float2 window_size = viewport->Size;
        ImGui.SetNextWindowPos(viewport->Pos);
        ImGui.SetNextWindowSize(window_size);
        ImGui.SetNextWindowViewport(viewport->ID);
        ImGuiWindowFlags flags =
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoFocusOnAppearing |
            ImGuiWindowFlags.NoInputs |
            ImGuiWindowFlags.NoNavFocus;
        if(!ImGui.Begin("HUDFullscreenWindow", flags)) {
            ImGui.End();
            return;
        }
        ImDrawListPtr draw_list = ImGui.GetWindowDrawList();

        EgoTransform.TryVehicleToEgo(vehicle, camera, parentBody, out double3 center);

        float3 centerf = new float3((float)center.X, (float)center.Y, (float)center.Z);
        float radius = settings.IgnoreZoom
            ? VectorMath.Length(centerf) * settings.ZoomScale
            : settings.FixedSphereSize;

        NavFrame resolvedGridFrame;
        if(settings.GridFrame == NavFrame.Auto) {
            switch(vehicle.VehicleRegion) {
                case VehicleRegion.Surface:
                    resolvedGridFrame = NavFrame.Enu;
                    break;
                case VehicleRegion.LowOrbit:
                    resolvedGridFrame = NavFrame.Lvlh;
                    break;
                case VehicleRegion.HighOrbit:
                    resolvedGridFrame = NavFrame.Cce;
                    break;
                default:
                    resolvedGridFrame = NavFrame.Cce;
                    break;
            }
        } else {
            resolvedGridFrame = settings.GridFrame;
        }
        if(settings.Grid.Enabled) {
            if(NavReferenceFrameBuilderBuilder.TryCreate(vehicle, camera, parentBody, settings, resolvedGridFrame, out Basis gridFrame)) {
                gridRenderer.DrawGrid(draw_list, gridFrame, centerf, radius, settings.Grid);
            }
        }

        if(settings.VelocityFrame == NavFrame.TVel && vehicle.Target == null) {
            settings.VelocityFrame = NavFrame.Auto;
        }
        NavFrame resolvedVelocityFrame;
        if(settings.VelocityFrame == NavFrame.Auto) {
            if(vehicle.Target != null) {
                resolvedVelocityFrame = NavFrame.TVel;
            } else if(vehicle.VehicleRegion == VehicleRegion.Surface) {
                resolvedVelocityFrame = NavFrame.SurfVel;
            } else {
                resolvedVelocityFrame = NavFrame.Vlh;
            }
        } else {
            resolvedVelocityFrame = settings.VelocityFrame;
        }

        // Prograde, retrograde, etc. velocity indicators
        if(settings.VelocityEnabled) {
            if(NavReferenceFrameBuilderBuilder.TryCreate(vehicle, camera, parentBody, settings, resolvedVelocityFrame, out Basis velFrame)) {
                velocityRenderer.Draw(draw_list, velFrame, centerf, radius, settings.Symbols);
            }
        }

        if(NavReferenceFrameBuilderBuilder.TryCreate(vehicle, camera, parentBody, settings, NavFrame.Attitude, out Basis attitudeFrame)) {
            attitudeRenderer.Draw(draw_list, attitudeFrame, centerf, radius, settings.Symbols);
        }
        if(NavReferenceFrameBuilderBuilder.TryCreate(vehicle, camera, parentBody, settings, NavFrame.Tgt, out Basis tgtFrame)) {
            targetRenderer.Draw(draw_list, tgtFrame, centerf, radius, settings.Symbols);
        }
        if(NavReferenceFrameBuilderBuilder.TryCreate(vehicle, camera, parentBody, settings, NavFrame.Dock, out Basis dockFrame)) {
            dockRenderer.Draw(draw_list, dockFrame, centerf, radius, settings.Symbols);
        }
        if(NavReferenceFrameBuilderBuilder.TryCreate(vehicle, camera, parentBody, settings, NavFrame.Burn, out Basis burnFrame)) {
            burnRenderer.Draw(draw_list, burnFrame, centerf, radius, settings.Symbols);
        }


        ImGui.End();
    }
}
