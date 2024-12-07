namespace game3D;

public partial class Form1 : Form
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    public static extern bool AllocConsole();

    private Cube[] cubes;
    private System.Windows.Forms.Timer timer;

    private float[] cameraPos = new float[] { 5, 30, 100 };
    private float[] cameraForward = new float[] { 0, 0, 1, 1 };
    private float[] cameraUp = new float[] { 0, 1, 0, 1 };
    private float[] cameraRight = new float[] { 1, 0, 0, 1 };
    private float farPlane = 100;
    private float nearPlane = 0.1f;
    private float fov = (float)(Math.PI / 3);
    private float moveSpeed = 1;

    float pitchAngle = 0.07f;
    float yawAngle = 0.07f;
    float rollAngle = 0.07f;

    public Form1()
    {
        InitializeComponent();
        this.DoubleBuffered = true;
        this.KeyPreview = true;

        AllocConsole();

        cubes =
        [
            new Cube([-7, 1, 1], 10),
                new Cube([-2, 15, 15], 10),
                new Cube([1, 1, 1], 10),
                new Cube([70, 25, 15], 10),
                new Cube([110, 30, 20], 10),
            ];

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 20;
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        foreach (var cube in cubes)
        {
            cube.RotateX(0);
            cube.RotateY(0);
            cube.RotateZ(0);
        }
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
        float[,] Rx = GetRotationMatrixX(pitchAngle);
        float[,] Ry = GetRotationMatrixY(yawAngle);
        float[,] Rz = GetRotationMatrixZ(rollAngle);

        Console.WriteLine();
        Console.WriteLine(pitchAngle + " " + yawAngle + " " + rollAngle);
        Console.WriteLine("Camera F: " + string.Join(", ", cameraForward));
        cameraForward = MultiplyMatrixAndVector(Rx, cameraForward);
        for (int i = 0; i < Rx.GetLength(0); i++)
        {
            for (int j = 0; j < Rx.GetLength(1); j++)
            {
                Console.Write(Rx[i, j] + "\t");
            }
            Console.WriteLine();
        }
        Console.WriteLine("Camera Right: " + string.Join(", ", cameraForward));

        cameraForward = MultiplyMatrixAndVector(Ry, cameraForward);
        cameraForward = MultiplyMatrixAndVector(Rz, cameraForward);

        cameraUp = MultiplyMatrixAndVector(Rx, cameraUp);
        cameraUp = MultiplyMatrixAndVector(Ry, cameraUp);
        cameraUp = MultiplyMatrixAndVector(Rz, cameraUp);

        cameraRight = MultiplyMatrixAndVector(Rx, cameraRight);
        cameraRight = MultiplyMatrixAndVector(Ry, cameraRight);
        cameraRight = MultiplyMatrixAndVector(Rz, cameraRight);
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

    static float[] MultiplyMatrixAndVector(float[,] matrix, float[] vector)
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

    static float Dot(float[] a, float[] b)
    {
        return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.KeyCode)
        {
            case Keys.W:
                cameraPos[2] -= moveSpeed;
                Console.WriteLine("W pressed");
                break;
            case Keys.S:
                cameraPos[2] += moveSpeed;
                Console.WriteLine("S pressed");
                break;
            case Keys.A:
                cameraPos[0] += moveSpeed;
                break;
            case Keys.D:
                cameraPos[0] -= moveSpeed;
                break;
            case Keys.Space:
                cameraPos[1] -= moveSpeed;
                break;
            case Keys.ShiftKey:
                cameraPos[1] += moveSpeed;
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
                Console.Write("Down pressed");
                RotateCamera(0, -yawAngle, 0);
                break;
        }

        this.Invalidate();
    }
}

