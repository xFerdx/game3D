namespace game3D;

class Projection
{
    public static PointF?[] ProjectCube(float[][] points, Camera cam, int screenWidth, int screenHeight)
    {
        PointF?[] projectedPoints = new PointF?[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            projectedPoints[i] = ProjectPoint(points[i], cam, screenWidth, screenHeight);
        }

        return projectedPoints;
    }

    private static PointF? ProjectPoint(float[] point, Camera cam, int screenWidth, int screenHeight)
    {
        float[] point4 = [.. point, 1.0f];
        float[,] viewMatrix = ComputeViewMatrix(cam);
        float[,] projectionMatrix = ComputeProjectionMatrix(cam, screenWidth, screenHeight);
        float[] cameraSpacePoint = Util.MultiplyMatrixAndVector(viewMatrix, point4);

        if (cameraSpacePoint[2] <= 0)
            return null;
        float[] clipSpacePoint = Util.MultiplyMatrixAndVector(projectionMatrix, cameraSpacePoint);

        float w = clipSpacePoint[3];
        float[] ndcPoint =
        [
            clipSpacePoint[0] / w,
                clipSpacePoint[1] / w,
                clipSpacePoint[2] / w
        ];

        return new PointF(
            (ndcPoint[0] + 1) * 0.5f * screenWidth,
            (ndcPoint[1] + 1) * 0.5f * screenHeight
            );
    }

    private static float[,] ComputeViewMatrix(Camera cam)
    {
        float[,] viewMatrix = new float[4, 4];
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

    private static float[,] ComputeProjectionMatrix(Camera cam, int screenWidth, int screenHeight)
    {
        float aspectRatio = screenWidth / (float)screenHeight;
        float[,] projectionMatrix = new float[4, 4];
        float tanHalfFov = (float)Math.Tan(cam.fov / 2);
        projectionMatrix[0, 0] = 1 / (aspectRatio * tanHalfFov);
        projectionMatrix[1, 1] = 1 / tanHalfFov;
        projectionMatrix[2, 2] = -(cam.farPlane + cam.nearPlane) / (cam.farPlane - cam.nearPlane);
        projectionMatrix[2, 3] = -(2 * cam.farPlane * cam.nearPlane) / (cam.farPlane - cam.nearPlane);
        projectionMatrix[3, 2] = -1;
        return projectionMatrix;
    }
}