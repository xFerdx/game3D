namespace game3D;

class Camera{
    public double[] cameraPos = [0, 0, -100];
    public double[] cameraForward = [0, 0, -1, 1];
    public double[] cameraUp = [0, 1, 0, 1];
    public double[] cameraRight = [1, 0, 0, 1];
    public double farPlane = 100;
    public double nearPlane = 1f;
    public double fov = (double)(Math.PI / 3);
    public double moveSpeed = 3;

    public void moveRightRelative(bool opposite)
    {
        cameraPos = Util.AddArrays(cameraPos, Util.MultiplyArrayWithScalar(Util.NormalizeVector([cameraRight[0], 0, cameraRight[2]]), (opposite ? 1 : -1) * moveSpeed));
    }

    public void moveForwardRelative(bool opposite)
    {
        cameraPos = Util.AddArrays(cameraPos, Util.MultiplyArrayWithScalar(Util.NormalizeVector([cameraForward[0], 0, cameraForward[2]]), (opposite ? 1 : -1) * moveSpeed));
    }

    public void moveUpRelative(bool opposite)
    {
        cameraPos = Util.AddArrays(cameraPos, Util.MultiplyArrayWithScalar(Util.NormalizeVector([cameraUp[0], 0, cameraUp[2]]), (opposite ? 1 : -1) * moveSpeed));
    }

    public void moveUpAbsolute(bool opposite)
    {
        cameraPos = Util.AddArrays(cameraPos, Util.MultiplyArrayWithScalar([0, 1, 0], (opposite ? -1 : 1) * moveSpeed));
    }

    public void RotateCamera(double pitchAngle, double yawAngle, double rollAngle)
    {
        //yaw : left/right
        cameraForward = Util.RotateAroundAxis(cameraForward, [0, 1, 0], yawAngle); // Rotate forward around the up axis
        cameraRight = Util.RotateAroundAxis(cameraRight, [0, 1, 0], yawAngle);     // Rotate right around the up axis
        cameraUp = Util.RotateAroundAxis(cameraUp, [0, 1, 0], yawAngle);

        //pitch: up/down
        cameraForward = Util.RotateAroundAxis(cameraForward, cameraRight, pitchAngle); // Rotate forward around the right axis
        cameraUp = Util.RotateAroundAxis(cameraUp, cameraRight, pitchAngle);           // Rotate up around the right axis

        //roll
        cameraRight = Util.RotateAroundAxis(cameraRight, cameraForward, rollAngle);   // Rotate right around the forward axis
        cameraUp = Util.RotateAroundAxis(cameraUp, cameraForward, rollAngle);         // Rotate up around the forward axis
    }

}