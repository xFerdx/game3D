namespace game3D;

public partial class Form1 : Form
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    public static extern bool AllocConsole();

    private List<Cube> cubes;
    private System.Windows.Forms.Timer timer;

    private float[] cameraPos = new float[] { 20, 30, 0 };
    //private float[] cameraForward = new float[] { 0, 0, 1, 1 };
    //private float[] cameraUp = new float[] { 0, 1, 0, 1 };
    //private float[] cameraRight = new float[] { 1, 0, 0, 1 };

    private float[] cameraForward = new float[] { 1, 0, 0, -1 };
    private float[] cameraUp = new float[] { 0, 1, 0, 1 };
    private float[] cameraRight = new float[] { 0, 0, -1, 1 };
    private float farPlane = 100;
    private float nearPlane = 0.1f;
    private float fov = (float)(Math.PI / 3);
    private float moveSpeed = 3;
    private bool pressedW = false;
    private bool pressedS = false;
    private bool pressedA = false;
    private bool pressedD = false;
    private bool pressedShift = false;
    private bool pressedSpace = false;

    float pitchAngle = 0.07f;
    float yawAngle = 0.07f;
    float rollAngle = 0.07f;

    public Form1()
    {
        InitializeComponent();
        this.DoubleBuffered = true;
        this.KeyPreview = true;

        AllocConsole();

        cubes = new List<Cube>
        {
            new Cube(new float[] { 0, 10, 0 }, 10),
            new Cube(new float[] { 0, 0, 0 }, 10),
            new Cube(new float[] { 0, 20, 0 }, 10),
            new Cube(new float[] { 0, 30, 0 }, 10),
            new Cube(new float[] { 10, 30, 0 }, 10),
            new Cube(new float[] { -10, -10, 0 }, 10),
        };

        for (int i = 0; i < 99; i++)
            for (int j = 0; j < 99; j++);
                //cubes.Add(new Cube(new float[] { i * 10, 0, j * 10 }, 10)); // Cube zur Liste hinzufügen

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 20;
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        updatePos();
        foreach (var cube in cubes)
        {
            //cube.RotateX(0);
            //cube.RotateY(0);
            //cube.RotateZ(0);
        }

        //Console.WriteLine("Camera Pos: " + cameraPos[0] + " " + cameraPos[1] + " " + cameraPos[2]);

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        g.Clear(Color.White);

        foreach (var cube in cubes)
        {
            DrawCube(g, cube);
        }
    }

    private void DrawCube(Graphics g, Cube cube)
    {
        PointF[] cube2D = ProjectCube(cube.Points);

        foreach (var edge in cube.Edges)
        {
            PointF p1 = cube2D[edge[0]];
            PointF p2 = cube2D[edge[1]];
            g.DrawLine(Pens.Black, p1, p2);
        }
    }

    private void updatePos()
    {
        if (pressedA)
            moveRightRelative(true);
        if (pressedD)
            moveRightRelative(false);
        if (pressedS)
            moveForwardRelative(true);
        if (pressedW)
            moveForwardRelative(false);
        if (pressedShift)
            moveUpRelative(true);
        if (pressedSpace)
            moveUpRelative(false);
    }

    private void moveRightRelative(bool opposite)
    {
        cameraPos = AddArrays(cameraPos, MultiplyArrayWithScalar(cameraRight[0..3], (opposite ? 1 : -1) * moveSpeed));
    }

    private void moveForwardRelative(bool opposite)
    {
        cameraPos = AddArrays(cameraPos, MultiplyArrayWithScalar(cameraForward[0..3], (opposite ? 1 : -1) * moveSpeed));
    }

    private void moveUpRelative(bool opposite)
    {
        cameraPos = AddArrays(cameraPos, MultiplyArrayWithScalar(cameraUp[0..3], (opposite ? 1 : -1) * moveSpeed));
    }


    private PointF[] ProjectCube(float[][] points)
    {
        PointF[] projectedPoints = new PointF[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            projectedPoints[i] = ProjectPoint(points[i]);
        }

        return projectedPoints;
    }

    private PointF ProjectPoint(float[] point)
    {
        float[] point4 = point.Concat(new float[] { 1.0f }).ToArray();
        float[,] viewMatrix = ComputeViewMatrix();
        float[,] projectionMatrix = ComputeProjectionMatrix();
        float[] cameraSpacePoint = MultiplyMatrixAndVector(viewMatrix, point4);
        float[] clipSpacePoint = MultiplyMatrixAndVector(projectionMatrix, cameraSpacePoint);

        float w = clipSpacePoint[3];
        float[] ndcPoint =
        [
            clipSpacePoint[0] / w,
                clipSpacePoint[1] / w,
                clipSpacePoint[2] / w
        ];

        return new PointF(
            (ndcPoint[0] + 1) * 0.5f * this.ClientSize.Width,
            (1 - ndcPoint[1]) * 0.5f * this.ClientSize.Width
            );
    }

    private float[,] ComputeViewMatrix()
    {
        float[,] viewMatrix = new float[4, 4];
        viewMatrix[0, 0] = cameraRight[0];
        viewMatrix[0, 1] = cameraRight[1];
        viewMatrix[0, 2] = cameraRight[2];
        viewMatrix[1, 0] = cameraUp[0];
        viewMatrix[1, 1] = cameraUp[1];
        viewMatrix[1, 2] = cameraUp[2];
        viewMatrix[2, 0] = -cameraForward[0];
        viewMatrix[2, 1] = -cameraForward[1];
        viewMatrix[2, 2] = -cameraForward[2];
        viewMatrix[0, 3] = -Dot(cameraRight, cameraPos);
        viewMatrix[1, 3] = -Dot(cameraUp, cameraPos);
        viewMatrix[2, 3] = Dot(cameraForward, cameraPos);
        viewMatrix[3, 3] = 1;
        return viewMatrix;
    }

    private float[,] ComputeProjectionMatrix()
    {
        int screenWidth = this.ClientSize.Width;
        int screenHeight = this.ClientSize.Height;
        float aspectRatio = screenWidth / (float)screenHeight;
        float[,] projectionMatrix = new float[4, 4];
        float tanHalfFov = (float)Math.Tan(fov / 2);
        projectionMatrix[0, 0] = 1 / (aspectRatio * tanHalfFov);
        projectionMatrix[1, 1] = 1 / tanHalfFov;
        projectionMatrix[2, 2] = -(farPlane + nearPlane) / (farPlane - nearPlane);
        projectionMatrix[2, 3] = -(2 * farPlane * nearPlane) / (farPlane - nearPlane);
        projectionMatrix[3, 2] = -1;
        return projectionMatrix;
    }

    public void RotateCamera(float pitchAngle, float yawAngle, float rollAngle)
    {
        //yaw : left/right
        cameraForward = RotateAroundAxis(cameraForward, cameraUp, yawAngle); // Rotate forward around the up axis
        cameraRight = RotateAroundAxis(cameraRight, cameraUp, yawAngle);     // Rotate right around the up axis
        cameraUp = RotateAroundAxis(cameraUp, cameraUp, yawAngle);           // Rotate up around itself (should not change)

        //pitch: up/down
        cameraForward = RotateAroundAxis(cameraForward, cameraRight, pitchAngle); // Rotate forward around the right axis
        cameraUp = RotateAroundAxis(cameraUp, cameraRight, pitchAngle);           // Rotate up around the right axis

        //roll
        cameraRight = RotateAroundAxis(cameraRight, cameraForward, rollAngle);   // Rotate right around the forward axis
        cameraUp = RotateAroundAxis(cameraUp, cameraForward, rollAngle);         // Rotate up around the forward axis

        // Print the new camera orientation for debugging
        Console.WriteLine("Forward: " + string.Join(", ", cameraForward));
        Console.WriteLine("Up: " + string.Join(", ", cameraUp));
        Console.WriteLine("Right: " + string.Join(", ", cameraRight));
    }



    private float[] RotateAroundAxis(float[] v, float[] axis, float angle)
    {
        // Normalisiere die Achse (axis muss ein Einheitsvektor sein)
        float axisLength = (float)Math.Sqrt(axis[0] * axis[0] + axis[1] * axis[1] + axis[2] * axis[2]);
        float[] normalizedAxis = new float[3] { axis[0] / axisLength, axis[1] / axisLength, axis[2] / axisLength };

        // Kreuzprodukt von axis und v
        float[] crossProduct = new float[3]
        {
        normalizedAxis[1] * v[2] - normalizedAxis[2] * v[1],
        normalizedAxis[2] * v[0] - normalizedAxis[0] * v[2],
        normalizedAxis[0] * v[1] - normalizedAxis[1] * v[0]
        };

        // Skalarprodukt von axis und v
        float dotProduct = normalizedAxis[0] * v[0] + normalizedAxis[1] * v[1] + normalizedAxis[2] * v[2];

        // Rodrigues' Rotation Formula
        float cosTheta = (float)Math.Cos(angle);
        float sinTheta = (float)Math.Sin(angle);

        // Berechne den rotierten Vektor
        float[] rotatedVector =
        [
            v[0] * cosTheta + crossProduct[0] * sinTheta + normalizedAxis[0] * dotProduct * (1 - cosTheta),
            v[1] * cosTheta + crossProduct[1] * sinTheta + normalizedAxis[1] * dotProduct * (1 - cosTheta),
            v[2] * cosTheta + crossProduct[2] * sinTheta + normalizedAxis[2] * dotProduct * (1 - cosTheta)
        ];

        return rotatedVector;
    }

    private float[,] GetRotationMatrixX(float angle)
    {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);

        return new float[,] {
            { 1, 0, 0, 0 },
            { 0, cos, -sin, 0 },
            { 0, sin, cos, 0 },
            { 0, 0, 0, 1 }
        };
    }

    private float[,] GetRotationMatrixY(float angle)
    {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);

        return new float[,] {
            { cos, 0, sin, 0 },
            { 0, 1, 0, 0 },
            { -sin, 0, cos, 0 },
            { 0, 0, 0, 1 }
        };
    }

    private float[,] GetRotationMatrixZ(float angle)
    {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);

        return new float[,] {
            { cos, -sin, 0, 0 },
            { sin, cos, 0, 0 },
            { 0, 0, 1, 0 },
            { 0, 0, 0, 1 }
        };
    }

    float[] MultiplyMatrixAndVector(float[,] matrix, float[] vector)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        if (vector.Length != cols)
            throw new ArgumentException("The length of the vector must match the number of columns in the matrix.");
        float[] result = new float[rows];
        for (int row = 0; row < rows; row++)
        {
            result[row] = 0;
            for (int col = 0; col < cols; col++)
                result[row] += matrix[row, col] * vector[col];
        }
        return result;
    }

    float Dot(float[] a, float[] b)
    {
        return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
    }

    float[] AddArrays(float[] array1, float[] array2)
    {
        if (array1.Length != array2.Length)
            throw new ArgumentException("Arrays must have the same length.");
        float[] result = new float[array1.Length];
        for (int i = 0; i < array1.Length; i++)
            result[i] = array1[i] + array2[i];

        return result;
    }

    float[] MultiplyArrayWithScalar(float[] array, float scalar)
    {
        float[] result = new float[array.Length];
        for (int i = 0; i < array.Length; i++)
            result[i] = array[i] * scalar;
        return result;
    }

    float[] CrossProduct(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != 3 || vectorB.Length != 3)
            throw new ArgumentException("Both vectors must be 3-dimensional.");
        return new float[]
        {
            vectorA[1] * vectorB[2] - vectorA[2] * vectorB[1],
            vectorA[2] * vectorB[0] - vectorA[0] * vectorB[2],
            vectorA[0] * vectorB[1] - vectorA[1] * vectorB[0]
        };
    }



    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.KeyCode)
        {
            case Keys.W:
                pressedW = true;
                break;
            case Keys.S:
                pressedS = true;
                break;
            case Keys.A:
                pressedA = true;
                break;
            case Keys.D:
                pressedD = true;
                break;
            case Keys.Space:
                pressedSpace = true;
                break;
            case Keys.ShiftKey:
                pressedShift = true;
                break;
            case Keys.Down:
                RotateCamera(pitchAngle, 0, 0);
                break;
            case Keys.Up:
                RotateCamera(-pitchAngle, 0, 0);
                break;
            case Keys.Right:
                RotateCamera(0, yawAngle, 0);
                break;
            case Keys.Left:
                RotateCamera(0, -yawAngle, 0);
                break;
        }
    }


    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        switch (e.KeyCode)
        {
            case Keys.W:
                pressedW = false;
                break;
            case Keys.S:
                pressedS = false;
                break;
            case Keys.A:
                pressedA = false;
                break;
            case Keys.D:
                pressedD = false;
                break;
            case Keys.Space:
                pressedSpace = false;
                break;
            case Keys.ShiftKey:
                pressedShift = false;
                break;
        }
    }

}

