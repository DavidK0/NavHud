using Brutal.ImGuiApi;
using KSA;
using Brutal.Numerics;

namespace NavHud;

internal class RendezvousTrackRenderer {
    IHudLineRenderer lineRender;
    float4 red = new float4(1, 0, 0, 1);

    public RendezvousTrackRenderer(IHudLineRenderer lineRender) {
        this.lineRender = lineRender;
    }

    public void Draw(ImDrawListPtr draw_list, Camera camera, Vehicle vehicle, Vehicle target, double maxTime) {
        double timeStep = 5d;
        SimTime startTime = Universe.GetElapsedSimTime();
        double3 targetNowCci = target.GetPositionCci(startTime);
        double3 vehicleNowCci = vehicle.GetPositionCci(startTime);
        double3 orbitNormalCci = target.Orbit.GetOrbitNormalCci();
        double3 targetRadiusNow = VectorMath.Normalize(targetNowCci);
        double3 relativeNow = vehicleNowCci - targetNowCci;
        double3 previousPointCci = targetNowCci + relativeNow;
        double3 previousPointEgo = EgoTransform.CciToEgo(previousPointCci, camera, target.Orbit.Parent);
        SimTime currentTime = startTime;
        SimTime endTime = startTime + maxTime;
        while(currentTime < endTime) {
            currentTime += timeStep;
            double3 vehicleFutureCci = vehicle.GetPositionCci(currentTime);
            double3 targetFutureCci = target.GetPositionCci(currentTime);
            double3 relativeFutureCci = vehicleFutureCci - targetFutureCci;
            double3 targetRadiusFuture = VectorMath.Normalize(targetFutureCci);
            double sweptAngle = SignedAngleAroundAxis(
                targetRadiusNow,
                targetRadiusFuture,
                orbitNormalCci
            );
            double3 relativeLvlhLike = RotateAroundAxis(
                relativeFutureCci,
                orbitNormalCci,
                -sweptAngle
            );
            double3 pointCci = targetNowCci + relativeLvlhLike;
            double3 pointEgo = EgoTransform.CciToEgo(pointCci, camera, target.Orbit.Parent);
            lineRender.Line(draw_list, previousPointEgo, pointEgo, red);
            previousPointEgo = pointEgo;
        }
    }

    private static double SignedAngleAroundAxis(double3 from, double3 to, double3 axis) {
        from = VectorMath.Normalize(from);
        to = VectorMath.Normalize(to);
        axis = VectorMath.Normalize(axis);
        double sin = double3.Dot(axis, double3.Cross(from, to));
        double cos = double3.Dot(from, to);
        return Math.Atan2(sin, cos);
    }

    private static double3 RotateAroundAxis(double3 v, double3 axis, double angleRadians) {
        axis = VectorMath.Normalize(axis);
        double cos = Math.Cos(angleRadians);
        double sin = Math.Sin(angleRadians);
        return
            v * cos +
            double3.Cross(axis, v) * sin +
            axis * double3.Dot(axis, v) * (1.0 - cos);
    }
}