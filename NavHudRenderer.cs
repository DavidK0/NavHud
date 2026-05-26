
using KSA;

namespace NavHud;

public sealed class NavHudRenderer {
    private readonly NavHudSettings settings;
    private readonly GridRenderer gridRenderer;
    private readonly AttitudeIndicatorRenderer attitudeRenderer;
    private readonly List<IHudIndicator> indicators;

    public NavHudRenderer(NavHudSettings settings) {
        this.settings = settings;

        IHudLineRenderer lines = new GizmoHudLineRenderer();

        var symbolRenderer = new HudSymbolRenderer(lines);
        var vectorRenderer = new VectorIndicatorRenderer(symbolRenderer);

        gridRenderer = new GridRenderer(lines);
        attitudeRenderer = new AttitudeIndicatorRenderer(lines);

        var orbitalVectorIndicators = new OrbitalVectorIndicatorRenderer(vectorRenderer);

        indicators = new List<IHudIndicator> {
        attitudeRenderer,
        orbitalVectorIndicators,
        // target, docking, etc. later.
    };
    }

    public void Draw(double dt) {
        if(settings.Mode == NavMode.Off) return;

        Vehicle vehicle = Program.ControlledVehicle;
        Camera camera = Program.GetMainCamera();

        if(!NavHudFrameBuilder.TryCreate(vehicle, camera, settings, out NavHudFrame frame)) {
            return;
        }

        DrawGrid(frame);

        foreach(IHudIndicator indicator in indicators) {
            if(indicator.IsEnabled(settings)) {
                indicator.Draw(frame, settings);
            }
        }
    }

    private void DrawGrid(NavHudFrame frame) {
        if(!settings.ShowGridLines) return;

        switch(settings.Mode) {
            case NavMode.Basic:
                gridRenderer.DrawEquatorial(frame, settings.Grid);
                break;

            case NavMode.AzAlt:
                gridRenderer.DrawAzAlt(frame, settings.Grid);
                break;
        }
    }
}