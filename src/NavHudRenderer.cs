
using Brutal.ImGuiApi;
using Brutal.Numerics;
using KSA;

namespace NavHud;

public unsafe sealed class NavHudRenderer {
    private readonly ImDrawLineRenderer lines;

    private readonly GridRenderer gridRenderer;
    private readonly VelocityRenderer velocityRenderer;
    private readonly RendezvousTrackRenderer rendezvousTrackRenderer;
    private readonly ConstellationsRenderer constellationsRenderer;

    private readonly AttitudeIndicatorRenderer attitudeRenderer;
    private readonly TargetIndicatorRenderer targetRenderer;
    private readonly DockIndicatorRenderer dockRenderer;
    private readonly BurnIndicatorRenderer burnRenderer;

    public NavHudRenderer() {
        lines = new ImDrawLineRenderer();
        HudSymbolRenderer symbolRenderer = new HudSymbolRenderer(lines);

        gridRenderer = new GridRenderer(lines);
        velocityRenderer = new VelocityRenderer(symbolRenderer);
        rendezvousTrackRenderer = new RendezvousTrackRenderer(lines);
        constellationsRenderer = new ConstellationsRenderer(lines);

        attitudeRenderer = new AttitudeIndicatorRenderer(symbolRenderer);
        targetRenderer = new TargetIndicatorRenderer(symbolRenderer);
        dockRenderer = new DockIndicatorRenderer(symbolRenderer);
        burnRenderer = new BurnIndicatorRenderer(symbolRenderer);
    }

    public void Draw(double dt, NavHudSettings? settings) {
        if(settings == null) return;

        Vehicle vehicle = Program.ControlledVehicle;
        Camera camera = Program.GetMainCamera();

        if(vehicle == null || camera == null) {
            return;
        }

        IParentBody parentBody = vehicle.Orbit.Parent;

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

        DrawGrid(draw_list, vehicle, camera, parentBody, centerf, radius, settings);
        DrawVelocity(draw_list, vehicle, camera, parentBody, centerf, radius, settings);
        DrawRendezvousTrack(draw_list, vehicle, camera, settings);
        DrawConstellations(draw_list, vehicle, camera, parentBody, centerf, radius, settings);

        ImGui.End();
    }

    private void DrawGrid(ImDrawListPtr draw_list, Vehicle vehicle, Camera camera, IParentBody parentBody, float3 center, float radius, NavHudSettings settings) {
        if(!settings.Grid.Enabled) return;

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
        if(NavReferenceBasisBuilder.TryCreate(vehicle, camera, parentBody, settings, resolvedGridFrame, out Basis gridFrame)) {
            gridRenderer.DrawGrid(draw_list, gridFrame, center, radius, settings.Grid);
        }

    }
    private void DrawVelocity(ImDrawListPtr draw_list, Vehicle vehicle, Camera camera, IParentBody parentBody, float3 center, float radius, NavHudSettings settings) {
        if(!settings.VelocityEnabled) return;

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
        if(NavReferenceBasisBuilder.TryCreate(vehicle, camera, parentBody, settings, resolvedVelocityFrame, out Basis velFrame)) {
            velocityRenderer.Draw(draw_list, velFrame, center, radius, settings.Symbols);
        }


        if(NavReferenceBasisBuilder.TryCreate(vehicle, camera, parentBody, settings, NavFrame.Attitude, out Basis attitudeFrame)) {
            attitudeRenderer.Draw(draw_list, attitudeFrame, center, radius, settings.Symbols);
        }
        if(NavReferenceBasisBuilder.TryCreate(vehicle, camera, parentBody, settings, NavFrame.Tgt, out Basis tgtFrame)) {
            targetRenderer.Draw(draw_list, tgtFrame, center, radius, settings.Symbols);
        }
        if(NavReferenceBasisBuilder.TryCreate(vehicle, camera, parentBody, settings, NavFrame.Dock, out Basis dockFrame)) {
            dockRenderer.Draw(draw_list, dockFrame, center, radius, settings.Symbols);
        }
        if(NavReferenceBasisBuilder.TryCreate(vehicle, camera, parentBody, settings, NavFrame.Burn, out Basis burnFrame)) {
            burnRenderer.Draw(draw_list, burnFrame, center, radius, settings.Symbols);
        }
    }
    private void DrawRendezvousTrack(ImDrawListPtr draw_list, Vehicle vehicle, Camera camera, NavHudSettings settings) {
        if(
            !settings.RendezvousTrackEnabled ||
            vehicle.Target == null ||
            vehicle.Target is not Vehicle) return;

        rendezvousTrackRenderer.Draw(draw_list, camera, vehicle, (Vehicle)vehicle.Target, (double)settings.RendezvousTrackMaxTime);
    }

    private void DrawConstellations(ImDrawListPtr draw_list, Vehicle vehicle, Camera camera, IParentBody parentBody, float3 center, float radius, NavHudSettings settings) {
        if(!settings.DrawConstellations) return;
        constellationsRenderer.Draw(draw_list, camera, center);
    }
}
