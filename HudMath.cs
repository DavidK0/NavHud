using Brutal.Logging;
using Brutal.Numerics;
using KSA;
using StarMap.API;
using System;

namespace NavHud;

public static class HudMath {
    public static double3 VehicleToEGO(Vehicle vehicle, Camera camera) {
        if(vehicle == null) return new double3(0.0, 0.0, 0.0);
        if(camera == null) return new double3(0.0, 0.0, 0.0);
        if(vehicle.Orbit.Parent is not Celestial celestial) return new double3(0.0, 0.0, 0.0);

        double3 vehicleCci = vehicle.GetPositionCci();
        double3 vehicleCce = celestial.GetCci2Cce() * vehicleCci;
        double3 celestialEcl = celestial.GetPositionEcl();
        double3 vehicleEcl = celestialEcl + vehicleCce;

        return camera.EclToEgo(vehicleEcl);
    }

    public static float3 VehicleToEgoFloat(Vehicle vehicle, Camera camera) {
        double3 vehicleEgo = VehicleToEGO(vehicle, camera);

        return new float3(
            (float)vehicleEgo.X,
            (float)vehicleEgo.Y,
            (float)vehicleEgo.Z
        );
    }

    public static float3 BodyDirectionToEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial,
        double3 bodyDirection
    ) {
        if(vehicle == null) return new float3(0.0f, 0.0f, 0.0f);
        if(camera == null) return new float3(0.0f, 0.0f, 0.0f);
        if(celestial == null) return new float3(0.0f, 0.0f, 0.0f);

        double3 vehicleCci = vehicle.GetPositionCci();

        // Body-local direction -> inertial CCI direction.
        double3 dirCci = vehicle.GetBody2Cci() * bodyDirection;

        // CCI direction -> CCE/ECL direction.
        // This is a direction, not a position, so do not add the celestial position yet.
        double3 dirCce = celestial.GetCci2Cce() * dirCci;

        // Vehicle center in ECL.
        double3 vehicleCce = celestial.GetCci2Cce() * vehicleCci;
        double3 vehicleEcl = celestial.GetPositionEcl() + vehicleCce;

        // Convert the direction into EGO by transforming a nearby point and subtracting center.
        double3 centerEgo = camera.EclToEgo(vehicleEcl);
        double3 pointEgo = camera.EclToEgo(vehicleEcl + dirCce);

        double3 dirEgo = pointEgo - centerEgo;

        return new float3(
            (float)dirEgo.X,
            (float)dirEgo.Y,
            (float)dirEgo.Z
        );
    }

    public static float3 BodyDirectionToEgo(
        Vehicle vehicle,
        Camera camera,
        double3 bodyDirection
    ) {
        if(vehicle == null) return new float3(0.0f, 0.0f, 0.0f);
        if(vehicle.Orbit.Parent is not Celestial celestial) {
            return new float3(0.0f, 0.0f, 0.0f);
        }

        return BodyDirectionToEgo(vehicle, camera, celestial, bodyDirection);
    }

    public static bool GetLocalHorizontalBasisEgo(
        Vehicle vehicle,
        Camera camera,
        out float3 east,
        out float3 north,
        out float3 up
    ) {
        east = new float3(1.0f, 0.0f, 0.0f);
        north = new float3(0.0f, 1.0f, 0.0f);
        up = new float3(0.0f, 0.0f, 1.0f);

        if(vehicle == null) return false;
        if(camera == null) return false;
        if(vehicle.Orbit.Parent is not Celestial celestial) return false;

        return GetLocalHorizontalBasisEgo(
            vehicle,
            camera,
            celestial,
            out east,
            out north,
            out up
        );
    }

    public static bool GetLocalHorizontalBasisEgo(
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

        // Vehicle position in CCI.
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
        //
        // east  = increasing longitude
        // north = increasing latitude
        // up    = radial direction away from planet center
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
        // CCE has the same orientation as ECL, just centered on the celestial body.
        double3 eastEcl = celestial.GetCcf2Cce() * eastCcf;
        double3 northEcl = celestial.GetCcf2Cce() * northCcf;
        double3 upEcl = celestial.GetCcf2Cce() * upCcf;

        // Vehicle center in ECL.
        double3 vehicleCce = celestial.GetCci2Cce() * vehicleCci;
        double3 vehicleEcl = celestial.GetPositionEcl() + vehicleCce;

        double3 centerEgo = camera.EclToEgo(vehicleEcl);

        // Transform directions into EGO by transforming nearby points and subtracting center.
        double3 eastEgoD = camera.EclToEgo(vehicleEcl + eastEcl) - centerEgo;
        double3 northEgoD = camera.EclToEgo(vehicleEcl + northEcl) - centerEgo;
        double3 upEgoD = camera.EclToEgo(vehicleEcl + upEcl) - centerEgo;

        east = NormalizeFloat(eastEgoD);
        north = NormalizeFloat(northEgoD);
        up = NormalizeFloat(upEgoD);

        return true;
    }

    public static float3 NormalizeFloat(double3 v) {
        double len = Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);

        if(len <= 0.0) {
            return new float3(0.0f, 0.0f, 0.0f);
        }

        return new float3(
            (float)(v.X / len),
            (float)(v.Y / len),
            (float)(v.Z / len)
        );
    }

    public static float3 Normalize(float3 v) {
        float len = Length(v);

        if(len <= 0.0f) {
            return new float3(0.0f, 0.0f, 0.0f);
        }

        return new float3(
            v.X / len,
            v.Y / len,
            v.Z / len
        );
    }

    public static float Length(float3 v) {
        return MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
    }

    public static double Length(double3 v) {
        return Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
    }

    public static bool IsZero(float3 v) {
        return Length(v) <= 0.0f;
    }

    public static double3 GetSurfaceAttitude(Vehicle vehicle) {
        if(vehicle == null) return new double3(0.0, 0.0, 0.0);

        doubleQuat enuBody2Cci =
            VehicleReferenceFrameEx.GetEnuBody2Cci(vehicle.GetPositionCci())
            ?? doubleQuat.Identity;

        doubleQuat attitudeQuat = doubleQuat.Concatenate(
            vehicle.GetBody2Cci(),
            enuBody2Cci.Inverse()
        );

        return attitudeQuat.ToRollPitchYawRadians();
    }

    public static float3 AzAltToEgoOffset(
        float az,
        float alt,
        float radius,
        float3 east,
        float3 north,
        float3 up
    ) {
        return
            east * (MathF.Cos(alt) * MathF.Sin(az) * radius) +
            north * (MathF.Cos(alt) * MathF.Cos(az) * radius) +
            up * (MathF.Sin(alt) * radius);
    }


    public static double3 VehicleToEgo(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial
    ) {
        double3 vehicleCci = vehicle.GetPositionCci();
        double3 vehicleCce = celestial.GetCci2Cce() * vehicleCci;
        double3 vehicleEcl = celestial.GetPositionEcl() + vehicleCce;

        return camera.EclToEgo(vehicleEcl);
    }

    public static float3 VehicleToEgoFloat(
        Vehicle vehicle,
        Camera camera,
        Celestial celestial
    ) {
        double3 ego = VehicleToEgo(vehicle, camera, celestial);

        return new float3(
            (float)ego.X,
            (float)ego.Y,
            (float)ego.Z
        );
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

        east = NormalizeFloat(eastEgoD);
        north = NormalizeFloat(northEgoD);
        up = NormalizeFloat(upEgoD);

        return !IsZero(east) && !IsZero(north) && !IsZero(up);
    }

    public static float3 CciDirectionToEgo(
Vehicle vehicle,
Camera camera,
Celestial celestial,
double3 directionCci
) {
        double len = Math.Sqrt(
            directionCci.X * directionCci.X +
            directionCci.Y * directionCci.Y +
            directionCci.Z * directionCci.Z
        );

        if(len <= 0.0) {
            return new float3(0.0f, 0.0f, 0.0f);
        }

        // CCI direction -> CCE/ECL direction.
        // This is a direction, not a position.
        double3 directionCce = celestial.GetCci2Cce() * directionCci;

        // Vehicle center in ECL.
        double3 vehicleCci = vehicle.GetPositionCci();
        double3 vehicleCce = celestial.GetCci2Cce() * vehicleCci;
        double3 vehicleEcl = celestial.GetPositionEcl() + vehicleCce;

        // Convert direction to EGO by transforming a nearby point and subtracting center.
        double3 centerEgo = camera.EclToEgo(vehicleEcl);
        double3 pointEgo = camera.EclToEgo(vehicleEcl + directionCce);

        double3 directionEgo = pointEgo - centerEgo;

        return new float3(
            (float)directionEgo.X,
            (float)directionEgo.Y,
            (float)directionEgo.Z
        );
    }


    public static float3 Cross(float3 a, float3 b) {
        return new float3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    public static bool IsZero(double3 v) {
        const double epsilon = 1e-12;

        return
            Math.Abs(v.X) < epsilon &&
            Math.Abs(v.Y) < epsilon &&
            Math.Abs(v.Z) < epsilon;
    }

    public static double3 Cross(double3 a, double3 b) {
        return new double3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
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

        if(IsZero(positionCci) || IsZero(velocityCci)) return false;

        double3 velocityDir = Normalize(positionCci: velocityCci);
        double3 radialDir = Normalize(positionCci: positionCci);

        double3 downCci = -radialDir;
        double3 rightCci = Normalize(Cross(downCci, velocityDir));
        if(IsZero(rightCci)) return false;

        double3 forwardCci = Normalize(Cross(rightCci, downCci));
        if(IsZero(forwardCci)) return false;

        forward = Normalize(CciDirectionToEgo(vehicle, camera, celestial, forwardCci));
        right = Normalize(CciDirectionToEgo(vehicle, camera, celestial, rightCci));
        down = Normalize(CciDirectionToEgo(vehicle, camera, celestial, downCci));

        return !IsZero(forward) && !IsZero(right) && !IsZero(down);
    }

    public static bool TryGetVlfBasisEgo(
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

        if(IsZero(positionCci) || IsZero(velocityCci)) return false;

        doubleQuat? vlfBody2Cci =
            VehicleReferenceFrameEx.GetVlfBody2Cci(positionCci, velocityCci);

        if(!vlfBody2Cci.HasValue) return false;

        forward = Normalize(CciDirectionToEgo(vehicle, camera, celestial, new double3(1, 0, 0).Transform(vlfBody2Cci.Value)));
        right = Normalize(CciDirectionToEgo(vehicle, camera, celestial, new double3(0, 1, 0).Transform(vlfBody2Cci.Value)));
        up = Normalize(CciDirectionToEgo(vehicle, camera, celestial, new double3(0, 0, 1).Transform(vlfBody2Cci.Value)));

        return !IsZero(forward) && !IsZero(right) && !IsZero(up);
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

    public static double3 Normalize(double3 positionCci) {
        double len = Length(positionCci);

        if(len <= 0.0) {
            return new double3(0.0, 0.0, 0.0);
        }

        return new double3(
            positionCci.X / len,
            positionCci.Y / len,
            positionCci.Z / len
        );
    }

    public static float3 EclDirectionToEgo(
    Vehicle vehicle,
    Camera camera,
    Celestial celestial,
    double3 eclDirection
) {
        if(vehicle == null || camera == null || celestial == null)
            return new float3(0, 0, 0);

        double3 vehicleCci = vehicle.GetPositionCci();
        double3 vehicleCce = celestial.GetCci2Cce() * vehicleCci;
        double3 vehicleEcl = celestial.GetPositionEcl() + vehicleCce;

        double scale = 1000.0;

        double3 centerEgo = camera.EclToEgo(vehicleEcl);
        double3 pointEgo = camera.EclToEgo(vehicleEcl + eclDirection * scale);

        double3 dirEgo = pointEgo - centerEgo;

        return Normalize(new float3(
            (float)dirEgo.X,
            (float)dirEgo.Y,
            (float)dirEgo.Z
        ));
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

        if(IsZero(positionCci) || IsZero(velocityCci)) return false;

        // Convert position and inertial velocity into the rotating body-fixed frame.
        double3 positionCcf = celestial.GetCci2Ccf() * positionCci;
        double3 velocityCcf = celestial.GetCci2Ccf() * velocityCci;

        // Surface velocity should be horizontal motion over the rotating body's surface.
        // Remove radial motion so the forward axis lies in the local tangent plane.
        double3 upCcf = Normalize(positionCcf);
        if(IsZero(upCcf)) return false;

        double radialSpeed = Dot(velocityCcf, upCcf);
        double3 surfaceVelocityCcf = velocityCcf - upCcf * radialSpeed;

        if(IsZero(surfaceVelocityCcf)) return false;

        double3 forwardCcf = Normalize(surfaceVelocityCcf);

        // Local right for a forward-facing surface frame:
        // forward = surface velocity
        // up      = local radial out
        // right   = forward x up
        double3 rightCcf = Normalize(Cross(forwardCcf, upCcf));
        if(IsZero(rightCcf)) return false;

        // Recompute up to make the frame exactly orthonormal.
        upCcf = Normalize(Cross(rightCcf, forwardCcf));
        if(IsZero(upCcf)) return false;

        double3 forwardCci = celestial.GetCcf2Cci() * forwardCcf;
        double3 rightCci = celestial.GetCcf2Cci() * rightCcf;
        double3 upCci = celestial.GetCcf2Cci() * upCcf;

        forward = Normalize(CciDirectionToEgo(vehicle, camera, celestial, forwardCci));
        right = Normalize(CciDirectionToEgo(vehicle, camera, celestial, rightCci));
        up = Normalize(CciDirectionToEgo(vehicle, camera, celestial, upCci));

        return !IsZero(forward) && !IsZero(right) && !IsZero(up);
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

        if(IsZero(positionCci) || IsZero(velocityCci)) return false;

        double3 velocityCciDir = Normalize(velocityCci);
        double3 radialOutCci = Normalize(positionCci);

        if(IsZero(velocityCciDir) || IsZero(radialOutCci)) return false;

        // Orbit normal. This is perpendicular to the orbital plane.
        double3 normalCci = Normalize(Cross(positionCci, velocityCci));
        if(IsZero(normalCci)) return false;

        // For a clean orthonormal VLH basis:
        // X = velocity
        // Z = orbit normal
        // Y = Z x X, which is mostly radial-out but exactly perpendicular to velocity.
        //
        // In circular orbit this equals radial-out. In eccentric orbit it differs slightly,
        // because velocity is not exactly perpendicular to radial-out.
        radialOutCci = Normalize(Cross(normalCci, velocityCciDir));
        if(IsZero(radialOutCci)) return false;

        velocity = Normalize(CciDirectionToEgo(vehicle, camera, celestial, velocityCciDir));
        radialOut = Normalize(CciDirectionToEgo(vehicle, camera, celestial, radialOutCci));
        normal = Normalize(CciDirectionToEgo(vehicle, camera, celestial, normalCci));

        return !IsZero(velocity) && !IsZero(radialOut) && !IsZero(normal);
    }

    public static double Dot(double3 a, double3 b) {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
}

