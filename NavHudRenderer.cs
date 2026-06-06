
using Brutal.ImGuiApi;
using Brutal.Numerics;
using KSA;

namespace NavHud;

public unsafe sealed class NavHudRenderer {
    private readonly NavHudSettings settings;
    private readonly GridRenderer gridRenderer;
    private readonly ImDrawLineRenderer lines;
    private readonly List<IHudIndicator> indicators;

    public NavHudRenderer() {
        //IHudLineRenderer lines = new GizmoHudLineRenderer();
        lines = new ImDrawLineRenderer();


        var symbolRenderer = new HudSymbolRenderer(lines);
        var vectorRenderer = new VectorIndicatorRenderer(symbolRenderer);

        gridRenderer = new GridRenderer(lines);


        IHudIndicator attitdueIndicatorRenderer = new AttitudeIndicatorRenderer(vectorRenderer);
        IHudIndicator antiattitudeIndicatorRenderer = new AntiattitudeIndicatorRenderer(vectorRenderer);
        IHudIndicator progradeIndicatorRenderer = new ProgradeIndicatorRenderer(vectorRenderer);
        IHudIndicator retrogradeIndicatorRenderer = new RetrogradeIndicatorRenderer(vectorRenderer);
        IHudIndicator normalIndicatorRenderer = new NormalIndicatorRenderer(vectorRenderer);
        IHudIndicator antinormalIndicatorRenderer = new AntinormalIndicatorRenderer(vectorRenderer);
        IHudIndicator radialInIndicatorRenderer = new RadialInIndicatorRenderer(vectorRenderer);
        IHudIndicator radialOutIndicatorRenderer = new RadialOutIndicatorRenderer(vectorRenderer);
        IHudIndicator targetIndicatorRenderer = new TargetIndicatorRenderer(vectorRenderer);

        indicators = new List<IHudIndicator> {
            attitdueIndicatorRenderer,
            antiattitudeIndicatorRenderer,
            progradeIndicatorRenderer,
            retrogradeIndicatorRenderer,
            normalIndicatorRenderer,
            antinormalIndicatorRenderer,
            radialInIndicatorRenderer,
            radialOutIndicatorRenderer,
            targetIndicatorRenderer
            // docking, etc. later.
        };
    }

    public void Draw(double dt, NavHudSettings settings) {
        if(!settings.Enabled) return;

        Vehicle vehicle = Program.ControlledVehicle;
        Camera camera = Program.GetMainCamera();

        if(!NavHudFrameBuilder.TryCreate(vehicle, camera, settings, out NavHudFrame frame)) {
            return;
        }

        // CREATE WINDOW
        ImGuiViewport* viewport = ImGui.GetMainViewport();
        float2 center = new float2(viewport->Pos.X + viewport->Size.X * 0.5f, viewport->Pos.Y + viewport->Size.Y * 0.5f);
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
        ImGui.Begin("HUDFullscreenWindow", flags);
        ImDrawListPtr draw_list = ImGui.GetWindowDrawList();

        DrawGrid(draw_list, frame, settings);

        foreach(IHudIndicator indicator in indicators) {
            if(indicator.IsEnabled(settings)) {
                indicator.Draw(draw_list, frame, settings);
            }
        }

        ImGui.End();
    }

    private void DrawGrid(ImDrawListPtr draw_list, NavHudFrame frame, NavHudSettings settings) {
        if(!settings.ShowGridLines) return;
        gridRenderer.DrawGrid(draw_list, frame, settings.Grid);
    }
}