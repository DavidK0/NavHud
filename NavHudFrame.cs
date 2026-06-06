using Brutal.Numerics;
using KSA;
using Brutal.Logging;

namespace NavHud;

public sealed class NavHudFrame {
    public required Vehicle Vehicle { get; init; }
    public required Camera Camera { get; init; }
    public required Celestial Celestial { get; init; }

    public required float3 CenterEgo { get; init; }
    public required float Radius { get; init; }

    public float3? EastEgo { get; init; }
    public float3? NorthEgo { get; init; }
    public float3? UpEgo { get; init; }

    public float3? BodyForwardEgo { get; init; }
    public float3? BodyRightEgo { get; init; }
    public float3? BodyUpEgo { get; init; }

    public bool HasLocalHorizontalBasis =>
        EastEgo.HasValue && NorthEgo.HasValue && UpEgo.HasValue;
}

public static class NavHudFrameBuilder {
    public static bool TryCreate(
        Vehicle? vehicle,
        Camera? camera,
        NavHudSettings settings,
        out NavHudFrame frame
    ) {
        frame = null!;

        if(vehicle == null || camera == null) return false;
        if(vehicle.Orbit.Parent is not Celestial celestial) return false;

        float3 center = HudMath.VehicleToEgoFloat(vehicle, camera);
        float radius = settings.IgnoreZoom
            ? HudMath.Length(center) * settings.ZoomScale
            : settings.FixedSphereSize;

        NavMode resolvedMode = settings.Mode == NavMode.Auto
            ? NavMode.EnuBody
            : settings.Mode;

        SetBasisForMode(
            vehicle,
            camera,
            celestial,
            resolvedMode,
            out float3? xEgo,
            out float3? yEgo,
            out float3? zEgo
        );

        frame = new NavHudFrame {
            Vehicle = vehicle,
            Camera = camera,
            Celestial = celestial,
            CenterEgo = center,
            Radius = radius,

            EastEgo = xEgo,
            NorthEgo = yEgo,
            UpEgo = zEgo,

            BodyForwardEgo = HudMath.BodyDirectionToEgo(vehicle, camera, celestial, new double3(1, 0, 0)),
            BodyRightEgo = HudMath.BodyDirectionToEgo(vehicle, camera, celestial, new double3(0, 1, 0)),
            BodyUpEgo = HudMath.BodyDirectionToEgo(vehicle, camera, celestial, new double3(0, 0, 1))
        };

        return true;
    }

    private static void SetBasisForMode(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        NavMode mode,
        out float3? xEgo,
        out float3? yEgo,
        out float3? zEgo
    ) {
        xEgo = null;
        yEgo = null;
        zEgo = null;

        switch(mode) {
            case NavMode.EnuBody:
                if(HudMath.TryGetLocalHorizontalBasisEgo(
                    vehicle,
                    camera,
                    celestial,
                    out float3 east,
                    out float3 north,
                    out float3 up
                )) {
                    xEgo = east;
                    yEgo = north;
                    zEgo = up;
                }
                break;

            case NavMode.Cce:
                xEgo = HudMath.EclDirectionToEgo(vehicle, camera, celestial, new double3(1, 0, 0));
                yEgo = HudMath.EclDirectionToEgo(vehicle, camera, celestial, new double3(0, 1, 0));
                zEgo = HudMath.EclDirectionToEgo(vehicle, camera, celestial, new double3(0, 0, 1));
                break;

            case NavMode.Lvlh:
                if(HudMath.TryGetLvlhBasisEgo(vehicle, camera, celestial, out float3 forward, out float3 right, out float3 down)) {
                    xEgo = forward;
                    yEgo = right;
                    zEgo = down;
                }
                break;

            case NavMode.VlfBody:


                if(HudMath.TryGetVlfBasisEgo(vehicle, camera, celestial, out float3 velocity, out float3 rightVlf, out float3 upVlf)) {
                    xEgo = velocity;
                    yEgo = rightVlf;
                    zEgo = upVlf;
                }
                break;

            case NavMode.BurnBody:
                if(HudMath.TryGetBurnBasisEgo(vehicle, camera, celestial, out float3 burn, out float3 burnRight, out float3 burnUp)) {
                    xEgo = burn;
                    yEgo = burnRight;
                    zEgo = burnUp;
                }
                break;

            case NavMode.Tgt:
                if(HudMath.TryGetTargetBasisEgo(vehicle, camera, celestial, out float3 target, out float3 targetRight, out float3 targetUp)) {
                    xEgo = target;
                    yEgo = targetRight;
                    zEgo = targetUp;
                }
                break;

            case NavMode.TVel:
                if(HudMath.TryGetTargetVelocityBasisEgo(vehicle, camera, celestial, out float3 tvel, out float3 tvelRight, out float3 tvelUp)) {
                    xEgo = tvel;
                    yEgo = tvelRight;
                    zEgo = tvelUp;
                }
                break;

            case NavMode.Dock:
                if(HudMath.TryGetDockBasisEgo(vehicle, camera, celestial, out float3 dockForward, out float3 dockRight, out float3 dockUp)) {
                    xEgo = dockForward;
                    yEgo = dockRight;
                    zEgo = dockUp;
                }
                break;
        }
    }
}