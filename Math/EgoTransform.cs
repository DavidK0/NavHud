
using Brutal.Numerics;
using KSA;

namespace NavHud;

public static class EgoTransform {
    public static bool TryVehicleToEgo(
        Vehicle vehicle,
        Camera camera,
        IParentBody parentBody,
        out double3 ego
    ) {
        ego = new double3(0.0, 0.0, 0.0);
        if(vehicle == null) return false;
        if(camera == null) return false;
        if(parentBody == null) return false;
        double3 vehicleCci = vehicle.GetPositionCci();
        double3 vehicleCce = parentBody.GetCci2Cce() * vehicleCci;
        double3 vehicleEcl = parentBody.GetPositionEcl() + vehicleCce;
        ego = camera.EclToEgo(vehicleEcl);
        return true;
    }

    public static bool TryBodyDirectionToEgo(
        Vehicle vehicle,
        Camera camera,
        IParentBody parentBody,
        double3 bodyDirection,
        out float3 egoDirection
    ) {
        egoDirection = new float3(0.0f, 0.0f, 0.0f);
        if(vehicle == null) return false;
        if(camera == null) return false;
        if(parentBody == null) return false;
        double3 vehicleCci = vehicle.GetPositionCci();
        double3 dirCci = vehicle.GetBody2Cci() * bodyDirection;
        double3 dirCce = parentBody.GetCci2Cce() * dirCci;
        double3 vehicleCce = parentBody.GetCci2Cce() * vehicleCci;
        double3 vehicleEcl = parentBody.GetPositionEcl() + vehicleCce;
        double3 centerEgo = camera.EclToEgo(vehicleEcl);
        double3 pointEgo = camera.EclToEgo(vehicleEcl + dirCce);
        double3 dirEgo = pointEgo - centerEgo;
        egoDirection = VectorMath.NormalizeToFloat(dirEgo);
        return !VectorMath.IsZero(egoDirection);
    }

    public static bool TryCciDirectionToEgo(
        Vehicle vehicle,
        Camera camera,
        IParentBody parentBody,
        double3 directionCci,
        out float3 egoDirection
    ) {
        egoDirection = new float3(0.0f, 0.0f, 0.0f);
        if(vehicle == null) return false;
        if(camera == null) return false;
        if(parentBody == null) return false;
        double len = Math.Sqrt(
            directionCci.X * directionCci.X +
            directionCci.Y * directionCci.Y +
            directionCci.Z * directionCci.Z
        );

        if(len <= 0.0) {
            return false;
        }
        double3 directionCce = parentBody.GetCci2Cce() * directionCci;
        if(!TryGetVehicleEcl(vehicle, parentBody, out double3 vehicleEcl)) return false;
        double3 centerEgo = camera.EclToEgo(vehicleEcl);
        double3 pointEgo = camera.EclToEgo(vehicleEcl + directionCce);
        double3 directionEgo = pointEgo - centerEgo;
        egoDirection = VectorMath.NormalizeToFloat(directionEgo);
        return !VectorMath.IsZero(egoDirection);
    }

    public static bool TryEclDirectionToEgo(
        Vehicle vehicle,
        Camera camera,
        IParentBody parentBody,
        double3 eclDirection,
        out float3 egoDirection
    ) {
        egoDirection = new float3(0, 0, 0);

        if(vehicle == null || camera == null || parentBody == null)
            return false;

        if(!TryGetVehicleEcl(vehicle, parentBody, out double3 vehicleEcl)) return false;

        double scale = 1000.0;

        double3 centerEgo = camera.EclToEgo(vehicleEcl);
        double3 pointEgo = camera.EclToEgo(vehicleEcl + eclDirection * scale);

        double3 dirEgo = pointEgo - centerEgo;

        egoDirection = VectorMath.NormalizeToFloat(dirEgo);

        return !VectorMath.IsZero(egoDirection);
    }

    private static bool TryGetVehicleEcl(
        Vehicle vehicle,
        IParentBody parentBody,
        out double3 vehicleEcl
    ) {
        vehicleEcl = new double3(0.0, 0.0, 0.0);

        if(vehicle == null) return false;
        if(parentBody == null) return false;

        double3 vehicleCci = vehicle.GetPositionCci();
        double3 vehicleCce = parentBody.GetCci2Cce() * vehicleCci;

        vehicleEcl = parentBody.GetPositionEcl() + vehicleCce;
        return true;
    }
}