using Brutal.Numerics;
using KSA;
namespace NavHud;

public static class VectorMath {
    private const float FloatEpsilon = 1e-6f;
    private const double DoubleEpsilon = 1e-12;

    public static float Length(float3 v) {
        return MathF.Sqrt(
            v.X * v.X +
            v.Y * v.Y +
            v.Z * v.Z
        );
    }

    public static double Length(double3 v) {
        return Math.Sqrt(
            v.X * v.X +
            v.Y * v.Y +
            v.Z * v.Z
        );
    }

    public static float3 Normalize(float3 v) {
        float len = Length(v);

        if(len <= FloatEpsilon) {
            return new float3(0.0f, 0.0f, 0.0f);
        }

        return new float3(
            v.X / len,
            v.Y / len,
            v.Z / len
        );
    }

    public static double3 Normalize(double3 v) {
        double len = Length(v);

        if(len <= DoubleEpsilon) {
            return new double3(0.0, 0.0, 0.0);
        }

        return new double3(
            v.X / len,
            v.Y / len,
            v.Z / len
        );
    }

    public static float3 NormalizeToFloat(double3 v) {
        double len = Length(v);

        if(len <= DoubleEpsilon) {
            return new float3(0.0f, 0.0f, 0.0f);
        }

        return new float3(
            (float)(v.X / len),
            (float)(v.Y / len),
            (float)(v.Z / len)
        );
    }

    public static bool IsZero(float3 v) {
        return Length(v) <= FloatEpsilon;
    }

    public static bool IsZero(double3 v) {
        return Length(v) <= DoubleEpsilon;
    }

    public static float3 Cross(float3 a, float3 b) {
        return new float3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    public static double3 Cross(double3 a, double3 b) {
        return new double3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    public static double Dot(double3 a, double3 b) {
        return
            a.X * b.X +
            a.Y * b.Y +
            a.Z * b.Z;
    }
}

public static class EgoTransform {
    public static bool TryVehicleToEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        out double3 ego
    ) {
        ego = new double3(0.0, 0.0, 0.0);
        if(vehicle == null) return false;
        if(camera == null) return false;
        if(celestial == null) return false;
        double3 vehicleCci = vehicle.GetPositionCci();
        double3 vehicleCce = celestial.GetCci2Cce() * vehicleCci;
        double3 vehicleEcl = celestial.GetPositionEcl() + vehicleCce;
        ego = camera.EclToEgo(vehicleEcl);
        return true;
    }

    public static bool TryBodyDirectionToEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        double3 bodyDirection,
        out float3 egoDirection
    ) {
        egoDirection = new float3(0.0f, 0.0f, 0.0f);
        if(vehicle == null) return false;
        if(camera == null) return false;
        if(celestial == null) return false;
        double3 vehicleCci = vehicle.GetPositionCci();
        double3 dirCci = vehicle.GetBody2Cci() * bodyDirection;
        double3 dirCce = celestial.GetCci2Cce() * dirCci;
        double3 vehicleCce = celestial.GetCci2Cce() * vehicleCci;
        double3 vehicleEcl = celestial.GetPositionEcl() + vehicleCce;
        double3 centerEgo = camera.EclToEgo(vehicleEcl);
        double3 pointEgo = camera.EclToEgo(vehicleEcl + dirCce);
        double3 dirEgo = pointEgo - centerEgo;
        egoDirection = VectorMath.NormalizeToFloat(dirEgo);
        return !VectorMath.IsZero(egoDirection);
    }

    public static bool TryCciDirectionToEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        double3 directionCci,
        out float3 egoDirection
    ) {
        egoDirection = new float3(0.0f, 0.0f, 0.0f);
        if(vehicle == null) return false;
        if(camera == null) return false;
        if(celestial == null) return false;
        double len = Math.Sqrt(
            directionCci.X * directionCci.X +
            directionCci.Y * directionCci.Y +
            directionCci.Z * directionCci.Z
        );
        if(len <= 0.0) {
            return false;
        }
        double3 directionCce = celestial.GetCci2Cce() * directionCci;
        if(!TryGetVehicleEcl(vehicle, celestial, out double3 vehicleEcl)) return false;
        double3 centerEgo = camera.EclToEgo(vehicleEcl);
        double3 pointEgo = camera.EclToEgo(vehicleEcl + directionCce);
        double3 directionEgo = pointEgo - centerEgo;
        egoDirection = VectorMath.NormalizeToFloat(directionEgo);
        return !VectorMath.IsZero(egoDirection);
    }

    public static bool TryEclDirectionToEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        double3 eclDirection,
        out float3 egoDirection
    ) {
        egoDirection = new float3(0, 0, 0);

        if(vehicle == null || camera == null || celestial == null)
            return false;

        if(!TryGetVehicleEcl(vehicle, celestial, out double3 vehicleEcl)) return false;

        double scale = 1000.0;

        double3 centerEgo = camera.EclToEgo(vehicleEcl);
        double3 pointEgo = camera.EclToEgo(vehicleEcl + eclDirection * scale);

        double3 dirEgo = pointEgo - centerEgo;

        egoDirection = VectorMath.NormalizeToFloat(dirEgo);

        return !VectorMath.IsZero(egoDirection);
    }

    private static bool TryGetVehicleEcl(
        Vehicle vehicle,
        Celestial celestial,
        out double3 vehicleEcl
    ) {
        vehicleEcl = new double3(0.0, 0.0, 0.0);

        if(vehicle == null) return false;
        if(celestial == null) return false;

        double3 vehicleCci = vehicle.GetPositionCci();
        double3 vehicleCce = celestial.GetCci2Cce() * vehicleCci;

        vehicleEcl = celestial.GetPositionEcl() + vehicleCce;
        return true;
    }

    private static float3 DirectionToEgo(
        Camera camera,
        double3 originEcl,
        double3 directionEcl
    ) {
        double3 originEgo = camera.EclToEgo(originEcl);
        double3 pointEgo = camera.EclToEgo(originEcl + directionEcl);

        double3 directionEgo = pointEgo - originEgo;

        return VectorMath.NormalizeToFloat(directionEgo);
    }
}


public static class HudBasis {

    public static bool TryGetSurfaceAttitude(Vehicle vehicle, out double3 attitude) {
        attitude = new double3(0.0, 0.0, 0.0);
        if(vehicle == null) return false;

        doubleQuat enuBody2Cci =
            VehicleReferenceFrameEx.GetEnuBody2Cci(vehicle.GetPositionCci())
            ?? doubleQuat.Identity;

        doubleQuat attitudeQuat = doubleQuat.Concatenate(
            vehicle.GetBody2Cci(),
            enuBody2Cci.Inverse()
        );

        attitude = attitudeQuat.ToRollPitchYawRadians();
        return true;
    }


    public static bool TryGetLocalHorizontalBasisEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        out float3 east,
        out float3 north,
        out float3 up
    ) {
        east = new float3(1.0f, 0.0f, 0.0f);
        north = new float3(0.0f, 1.0f, 0.0f);
        up = new float3(0.0f, 0.0f, 1.0f);

        if(vehicle == null) return false;
        if(camera == null) return false;
        if(celestial == null) return false;

        double3 vehicleCci = vehicle.GetPositionCci();

        // Vehicle position in planet-fixed CCF.
        double3 vehicleCcf = celestial.GetCci2Ccf() * vehicleCci;

        double x = vehicleCcf.X;
        double y = vehicleCcf.Y;
        double z = vehicleCcf.Z;

        double r = Math.Sqrt(x * x + y * y + z * z);
        if(r <= 0.0) return false;

        double lat = Math.Asin(z / r);
        double lon = Math.Atan2(y, x);

        // Local ENU basis in CCF.
        double3 eastCcf = new double3(
            -Math.Sin(lon),
            Math.Cos(lon),
            0.0
        );

        double3 northCcf = new double3(
            -Math.Sin(lat) * Math.Cos(lon),
            -Math.Sin(lat) * Math.Sin(lon),
            Math.Cos(lat)
        );

        double3 upCcf = new double3(
            Math.Cos(lat) * Math.Cos(lon),
            Math.Cos(lat) * Math.Sin(lon),
            Math.Sin(lat)
        );

        // Convert basis directions from CCF -> CCE.
        double3 eastEcl = celestial.GetCcf2Cce() * eastCcf;
        double3 northEcl = celestial.GetCcf2Cce() * northCcf;
        double3 upEcl = celestial.GetCcf2Cce() * upCcf;

        // Vehicle center in ECL.
        double3 vehicleCce = celestial.GetCci2Cce() * vehicleCci;
        double3 vehicleEcl = celestial.GetPositionEcl() + vehicleCce;

        double3 centerEgo = camera.EclToEgo(vehicleEcl);

        // Convert directions into EGO by transforming nearby points
        // and subtracting the transformed center.
        double3 eastEgoD = camera.EclToEgo(vehicleEcl + eastEcl) - centerEgo;
        double3 northEgoD = camera.EclToEgo(vehicleEcl + northEcl) - centerEgo;
        double3 upEgoD = camera.EclToEgo(vehicleEcl + upEcl) - centerEgo;

        east = VectorMath.NormalizeToFloat(eastEgoD);
        north = VectorMath.NormalizeToFloat(northEgoD);
        up = VectorMath.NormalizeToFloat(upEgoD);

        return !VectorMath.IsZero(east) && !VectorMath.IsZero(north) && !VectorMath.IsZero(up);
    }



    public static bool TryGetLvlhBasisEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        out float3 forward,
        out float3 right,
        out float3 down
    ) {
        forward = new float3(1, 0, 0);
        right = new float3(0, 1, 0);
        down = new float3(0, 0, 1);

        if(vehicle == null || camera == null || celestial == null) return false;

        double3 positionCci = vehicle.GetPositionCci();
        double3 velocityCci = vehicle.GetVelocityCci();

        if(VectorMath.IsZero(positionCci) || VectorMath.IsZero(velocityCci)) return false;

        double3 velocityDir = VectorMath.Normalize(velocityCci);
        double3 radialDir = VectorMath.Normalize(positionCci);

        double3 downCci = -radialDir;
        double3 rightCci = VectorMath.Normalize(VectorMath.Cross(downCci, velocityDir));
        if(VectorMath.IsZero(rightCci)) return false;

        double3 forwardCci = VectorMath.Normalize(VectorMath.Cross(rightCci, downCci));
        if(VectorMath.IsZero(forwardCci)) return false;

        if(!EgoTransform.TryCciDirectionToEgo(vehicle, camera, celestial, rightCci, out right)) return false;
        if(!EgoTransform.TryCciDirectionToEgo(vehicle, camera, celestial, downCci, out down)) return false;
        if(!EgoTransform.TryCciDirectionToEgo(vehicle, camera, celestial, forwardCci, out forward)) return false;

        return !VectorMath.IsZero(forward) && !VectorMath.IsZero(right) && !VectorMath.IsZero(down);
    }

    public static bool TryGetBurnBasisEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        out float3 forward,
        out float3 right,
        out float3 up
    ) {
        forward = new float3(1, 0, 0);
        right = new float3(0, 1, 0);
        up = new float3(0, 0, 1);

        // Needs access to BurnTarget.BurnBody2Cci.
        return false;
    }

    public static bool TryGetTargetBasisEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        out float3 forward,
        out float3 right,
        out float3 up
    ) {
        forward = new float3(1, 0, 0);
        right = new float3(0, 1, 0);
        up = new float3(0, 0, 1);

        // Needs access to NavigationTarget.PositionCci.
        return false;
    }

    public static bool TryGetTargetVelocityBasisEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        out float3 forward,
        out float3 right,
        out float3 up
    ) {
        forward = new float3(1, 0, 0);
        right = new float3(0, 1, 0);
        up = new float3(0, 0, 1);

        // Needs access to NavigationTarget.VelocityCci.
        return false;
    }

    public static bool TryGetDockBasisEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        out float3 forward,
        out float3 right,
        out float3 up
    ) {
        forward = new float3(1, 0, 0);
        right = new float3(0, 1, 0);
        up = new float3(0, 0, 1);

        // Needs access to NavigationTarget.Body2Cci.
        return false;
    }

    public static bool TryGetSurfaceVelocityBasisEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        out float3 forward,
        out float3 right,
        out float3 up
    ) {
        forward = new float3(1, 0, 0);
        right = new float3(0, 1, 0);
        up = new float3(0, 0, 1);

        if(vehicle == null || camera == null || celestial == null) return false;

        double3 positionCci = vehicle.GetPositionCci();
        double3 velocityCci = vehicle.GetVelocityCci();

        if(VectorMath.IsZero(positionCci) || VectorMath.IsZero(velocityCci)) return false;

        // Convert position and inertial velocity into the rotating body-fixed frame.
        double3 positionCcf = celestial.GetCci2Ccf() * positionCci;
        double3 velocityCcf = celestial.GetCci2Ccf() * velocityCci;

        // Surface velocity should be horizontal motion over the rotating body's surface.
        // Remove radial motion so the forward axis lies in the local tangent plane.
        double3 upCcf = VectorMath.Normalize(positionCcf);
        if(VectorMath.IsZero(upCcf)) return false;

        double radialSpeed = VectorMath.Dot(velocityCcf, upCcf);
        double3 surfaceVelocityCcf = velocityCcf - upCcf * radialSpeed;

        if(VectorMath.IsZero(surfaceVelocityCcf)) return false;

        double3 forwardCcf = VectorMath.Normalize(surfaceVelocityCcf);

        // Local right for a forward-facing surface frame:
        // forward = surface velocity
        // up      = local radial out
        // right   = forward x up
        double3 rightCcf = VectorMath.Normalize(VectorMath.Cross(forwardCcf, upCcf));
        if(VectorMath.IsZero(rightCcf)) return false;

        // Recompute up to make the frame exactly orthonormal.
        upCcf = VectorMath.Normalize(VectorMath.Cross(rightCcf, forwardCcf));
        if(VectorMath.IsZero(upCcf)) return false;

        double3 forwardCci = celestial.GetCcf2Cci() * forwardCcf;
        double3 rightCci = celestial.GetCcf2Cci() * rightCcf;
        double3 upCci = celestial.GetCcf2Cci() * upCcf;

        if(!EgoTransform.TryCciDirectionToEgo(vehicle, camera, celestial, forwardCci, out forward)) return false;
        if(!EgoTransform.TryCciDirectionToEgo(vehicle, camera, celestial, rightCci, out right)) return false;
        if(!EgoTransform.TryCciDirectionToEgo(vehicle, camera, celestial, upCci, out up)) return false;

        return !VectorMath.IsZero(forward) && !VectorMath.IsZero(right) && !VectorMath.IsZero(up);
    }

    public static bool TryGetVlhBasisEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        out float3 velocity,
        out float3 radialOut,
        out float3 normal
    ) {
        velocity = new float3(1, 0, 0);
        radialOut = new float3(0, 1, 0);
        normal = new float3(0, 0, 1);

        if(vehicle == null || camera == null || celestial == null) return false;

        double3 positionCci = vehicle.GetPositionCci();
        double3 velocityCci = vehicle.GetVelocityCci();

        if(VectorMath.IsZero(positionCci) || VectorMath.IsZero(velocityCci)) return false;

        double3 velocityCciDir = VectorMath.Normalize(velocityCci);
        double3 radialOutCci = VectorMath.Normalize(positionCci);

        if(VectorMath.IsZero(velocityCciDir) || VectorMath.IsZero(radialOutCci)) return false;

        // Orbit normal. This is perpendicular to the orbital plane.
        double3 normalCci = VectorMath.Normalize(VectorMath.Cross(positionCci, velocityCci));
        if(VectorMath.IsZero(normalCci)) return false;

        // Orthonormal VLH basis:
        // X = velocity
        // Z = orbit normal
        // Y = Z x X, which is mostly radial-out but exactly perpendicular to velocity.
        radialOutCci = VectorMath.Normalize(VectorMath.Cross(normalCci, velocityCciDir));
        if(VectorMath.IsZero(radialOutCci)) return false;

        if(!EgoTransform.TryCciDirectionToEgo(vehicle, camera, celestial, velocityCciDir, out velocity)) return false;
        if(!EgoTransform.TryCciDirectionToEgo(vehicle, camera, celestial, radialOutCci, out radialOut)) return false;
        if(!EgoTransform.TryCciDirectionToEgo(vehicle, camera, celestial, normalCci, out normal)) return false;

        return !VectorMath.IsZero(velocity) && !VectorMath.IsZero(radialOut) && !VectorMath.IsZero(normal);
    }
}