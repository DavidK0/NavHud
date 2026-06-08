using Brutal.Numerics;
using KSA;

namespace NavHud;

public static class NavHudBasisBuilder {
    public static bool TryCreate(
        Vehicle? vehicle,
        Camera? camera,
        IParentBody parentBody,
        NavHudSettings settings,
        NavFrame resolvedGridFrame,
        out Basis frame
    ) {
        frame = null!;

        if(vehicle == null || camera == null) return false;

        SetBasisForMode(
            vehicle,
            camera,
            parentBody,
            resolvedGridFrame,
            out float3? forwardEgo,
            out float3? rightEgo,
            out float3? upEgo
        );

        if( forwardEgo == null || rightEgo == null || upEgo == null)
            return false;

        frame = new Basis {
            forward = forwardEgo.Value,
            right = rightEgo.Value,
            up = upEgo.Value
        };

        return true;
    }

    private static void SetBasisForMode(
        Vehicle vehicle,
        Camera camera,
        IParentBody parentBody,
        NavFrame mode,
        out float3? forwardEgo,
        out float3? rightEgo,
        out float3? upEgo
    ) {
        forwardEgo = null;
        rightEgo = null;
        upEgo = null;

        switch(mode) {
            case NavFrame.Cce:
                if(EgoTransform.TryEclDirectionToEgo(vehicle, camera, parentBody, new double3(1, 0, 0), out float3 x)) forwardEgo = x;
                if(EgoTransform.TryEclDirectionToEgo(vehicle, camera, parentBody, new double3(0, 1, 0), out float3 y)) rightEgo = y;
                if(EgoTransform.TryEclDirectionToEgo(vehicle, camera, parentBody, new double3(0, 0, 1), out float3 z)) upEgo = z;
                break;

            case NavFrame.Cci:
                if(EgoTransform.TryCciDirectionToEgo(vehicle, camera, parentBody, new double3(1, 0, 0), out float3 xCci)) forwardEgo = xCci;
                if(EgoTransform.TryCciDirectionToEgo(vehicle, camera, parentBody, new double3(0, 1, 0), out float3 yCci)) rightEgo = yCci;
                if(EgoTransform.TryCciDirectionToEgo(vehicle, camera, parentBody, new double3(0, 0, 1), out float3 zCci)) upEgo = zCci;
                break;

            case NavFrame.Enu:
                if(HudBasis.TryGetLocalHorizontalBasisEgo(
                    vehicle,
                    camera,
                    parentBody,
                    out float3 east,
                    out float3 north,
                    out float3 up
                )) {
                    forwardEgo = east;
                    rightEgo = north;
                    upEgo = up;
                }
                break;

            case NavFrame.Lvlh:
                if(HudBasis.TryGetLvlhBasisEgo(vehicle, camera, parentBody, out float3 forward, out float3 right, out float3 down)) {
                    forwardEgo = forward;
                    rightEgo = right;
                    upEgo = down;
                }
                break;

            case NavFrame.SurfVel:
                if(HudBasis.TryGetSurfaceVelocityBasisEgo(
                    vehicle,
                    camera,
                    parentBody,
                    out float3 surfVel,
                    out float3 surfVelRight,
                    out float3 surfVelUp
                )) {
                    forwardEgo = surfVel;
                    rightEgo = surfVelRight;
                    upEgo = surfVelUp;
                }
                break;

            case NavFrame.Vlh:
                if(HudBasis.TryGetVlhBasisEgo(
                    vehicle,
                    camera,
                    parentBody,
                    out float3 velocity,
                    out float3 normal,
                    out float3 radialOut
                )) {
                    forwardEgo = velocity;
                    rightEgo = normal;
                    upEgo = radialOut;
                }
                break;

            case NavFrame.TVel:
                if(HudBasis.TryGetTargetVelocityBasisEgo(
                    vehicle,
                    camera,
                    parentBody,
                    out float3 tvel,
                    out float3 tvelRight,
                    out float3 tvelUp
                )) {
                    forwardEgo = tvel;
                    rightEgo = tvelRight;
                    upEgo = tvelUp;
                }
                break;

            case NavFrame.Attitude:
                if(HudBasis.TryGetAttitudeBasisEgo(
                    vehicle,
                    camera,
                    parentBody,
                    out float3 attitude,
                    out float3 attitudeRight,
                    out float3 attitudeUp
                )) {
                    forwardEgo = attitude;
                    rightEgo = attitudeRight;
                    upEgo = attitudeUp;
                }
                break;

            case NavFrame.Tgt:
                if(HudBasis.TryGetTargetBasisEgo(
                    vehicle,
                    camera,
                    parentBody,
                    out float3 target,
                    out float3 targetRight,
                    out float3 targetUp
                )) {
                    forwardEgo = target;
                    rightEgo = targetRight;
                    upEgo = targetUp;
                }
                break;

            case NavFrame.Dock:
                if(HudBasis.TryGetDockBasisEgo(
                    vehicle,
                    camera,
                    parentBody,
                    out float3 dockForward,
                    out float3 dockRight,
                    out float3 dockUp
                )) {
                    forwardEgo = dockForward;
                    rightEgo = dockRight;
                    upEgo = dockUp;
                }
                break;

            case NavFrame.Burn:
                if(HudBasis.TryGetBurnBasisEgo(
                    vehicle,
                    camera,
                    parentBody,
                    out float3 burn,
                    out float3 burnRight,
                    out float3 burnUp
                )) {
                    forwardEgo = burn;
                    rightEgo = burnRight;
                    upEgo = burnUp;
                }
                break;
        }
    }
}