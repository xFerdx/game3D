namespace game3D;

class Projection
{
    public static (PointF?[], double[]) ProjectCube(double[][] points, Camera cam, int screenWidth, int screenHeight)
    {
        double[] zValues = new double[points.Length];
        PointF?[] projectedPoints = new PointF?[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            (projectedPoints[i], zValues[i]) = ProjectPoint(points[i], cam, screenWidth, screenHeight);
        }

        return (projectedPoints, zValues);
    }

    private static (PointF?,double) ProjectPoint(double[] point, Camera cam, int screenWidth, int screenHeight)
    {
        double[] point4 = [.. point, 1.0f];
        double[,] viewMatrix = ComputeViewMatrix(cam);
        double[,] projectionMatrix = ComputeProjectionMatrix(cam, screenWidth, screenHeight);
        double[] cameraSpacePoint = Util.MultiplyMatrixAndVector(viewMatrix, point4);

        if (cameraSpacePoint[2] <= 0)
            return (null, 0);
        double[] clipSpacePoint = Util.MultiplyMatrixAndVector(projectionMatrix, cameraSpacePoint);

        double w = clipSpacePoint[3];
        double[] ndcPoint =
        [
            clipSpacePoint[0] / w,
                clipSpacePoint[1] / w,
                clipSpacePoint[2] / w
        ];

        return (new PointF(
            (float)((ndcPoint[0] + 1) * 0.5f * screenWidth),
            (float)((ndcPoint[1] + 1) * 0.5f * screenHeight)
            ),ndcPoint[2]);
    }

    private static double[,] ComputeViewMatrix(Camera cam)
    {
        double[,] viewMatrix = new double[4, 4];
        viewMatrix[0, 0] = cam.cameraRight[0];
        viewMatrix[0, 1] = cam.cameraRight[1];
        viewMatrix[0, 2] = cam.cameraRight[2];
        viewMatrix[1, 0] = cam.cameraUp[0];
        viewMatrix[1, 1] = cam.cameraUp[1];
        viewMatrix[1, 2] = cam.cameraUp[2];
        viewMatrix[2, 0] = -cam.cameraForward[0];
        viewMatrix[2, 1] = -cam.cameraForward[1];
        viewMatrix[2, 2] = -cam.cameraForward[2];
        viewMatrix[0, 3] = -Util.Dot(cam.cameraRight, cam.cameraPos);
        viewMatrix[1, 3] = -Util.Dot(cam.cameraUp, cam.cameraPos);
        viewMatrix[2, 3] = Util.Dot(cam.cameraForward, cam.cameraPos);
        viewMatrix[3, 3] = 1;
        return viewMatrix;
    }

    private static double[,] ComputeProjectionMatrix(Camera cam, int screenWidth, int screenHeight)
    {
        double aspectRatio = screenWidth / (double)screenHeight;
        double[,] projectionMatrix = new double[4, 4];
        double tanHalfFov = (double)Math.Tan(cam.fov / 2);
        projectionMatrix[0, 0] = 1 / (aspectRatio * tanHalfFov);
        projectionMatrix[1, 1] = 1 / tanHalfFov;
        projectionMatrix[2, 2] = -(cam.farPlane + cam.nearPlane) / (cam.farPlane - cam.nearPlane);
        projectionMatrix[2, 3] = -(2 * cam.farPlane * cam.nearPlane) / (cam.farPlane - cam.nearPlane);
        projectionMatrix[3, 2] = -1;
        return projectionMatrix;
    }
}