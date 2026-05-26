using Brutal.Numerics;
using KSA;
using System;
using System.Collections.Generic;
using System.Text;

namespace NavHud {
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

            HudMath.TryGetLocalHorizontalBasisEgo(
                vehicle,
                camera,
                celestial,
                out float3 east,
                out float3 north,
                out float3 up
            );

            frame = new NavHudFrame {
                Vehicle = vehicle,
                Camera = camera,
                Celestial = celestial,
                CenterEgo = center,
                Radius = radius,
                EastEgo = east,
                NorthEgo = north,
                UpEgo = up,
                BodyForwardEgo = HudMath.BodyDirectionToEgo(vehicle, camera, celestial, new double3(1, 0, 0)),
                BodyRightEgo = HudMath.BodyDirectionToEgo(vehicle, camera, celestial, new double3(0, 1, 0)),
                BodyUpEgo = HudMath.BodyDirectionToEgo(vehicle, camera, celestial, new double3(0, 0, 1))
            };

            return true;
        }
    }
}
