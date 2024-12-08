namespace game3D;

public partial class Form1 : Form
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    public static extern bool AllocConsole();

    private List<Cube> cubes;
    private System.Windows.Forms.Timer timer;

    private float[] cameraPos = new float[] { 20, 30, 30 };
    private float[] cameraForward = new float[] { 1, 0, 0, 1 };
    private float[] cameraUp = new float[] { 0, 1, 0, 1 };
    private float[] cameraRight = new float[] { 0, 0, -1, 1 };
    private float farPlane = 100;
    private float nearPlane = 1;
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

    private float mouseSensitivity = 0.001f;

    private bool noCursor = false;

    public Form1()
    {
        InitializeComponent();
        this.DoubleBuffered = true;
        this.KeyPreview = true;

        AllocConsole();
        InitializeDebugPanel();

        cubes = new List<Cube>
        {
            //new Cube(new float[] { 0, 10, 0 }, 10),
            //new Cube(new float[] { 0, 0, 0 }, 10),
            //new Cube(new float[] { 0, 20, 0 }, 10),
            //new Cube(new float[] { 0, 30, 0 }, 10),
            //new Cube(new float[] { 10, 30, 0 }, 10),
            new Cube(new float[] { -10, -10, 0 }, 10),
        };

        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 9; j++)
            {
                //cubes.Add(new Cube(new float[] { i * 10, 0, j * 10 }, 10));
                //cubes.Add(new Cube(new float[] { i * 10, 10, j * 10 }, 10));
            }
        timer = new System.Windows.Forms.Timer();
        timer.Interval = 20;
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        updatePos();
        Invalidate();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
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
        Console.WriteLine();
        foreach (var p in cube2D)
            Console.WriteLine(p.X + " " + p.Y);

        foreach (var edge in cube.Edges)
        {
            PointF p1 = cube2D[edge[0]];
            PointF p2 = cube2D[edge[1]];
            bool p1OutsideView = p1.X < 0 || p1.X > this.ClientSize.Width || p1.Y < 0 || p1.Y > this.ClientSize.Height;
            bool p2OutsideView = p2.X < 0 || p2.X > this.ClientSize.Width || p2.Y < 0 || p2.Y > this.ClientSize.Height;
            if (p1OutsideView || p2OutsideView) ;
            //continue;
            g.DrawLine(Pens.Black, p1, p2);
        }
        Console.WriteLine("pos" + string.Join(",", cameraPos));
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
            moveUpAbsolute(true);
        if (pressedSpace)
            moveUpAbsolute(false);
    }

    private void moveRightRelative(bool opposite)
    {
        cameraPos = AddArrays(cameraPos, MultiplyArrayWithScalar(NormalizeVector([cameraRight[0], 0, cameraRight[2]]), (opposite ? 1 : -1) * moveSpeed));
    }

    private void moveForwardRelative(bool opposite)
    {
        cameraPos = AddArrays(cameraPos, MultiplyArrayWithScalar(NormalizeVector([cameraForward[0], 0, cameraForward[2]]), (opposite ? 1 : -1) * moveSpeed));
    }

    private void moveUpRelative(bool opposite)
    {
        cameraPos = AddArrays(cameraPos, MultiplyArrayWithScalar(NormalizeVector([cameraUp[0], 0, cameraUp[2]]), (opposite ? 1 : -1) * moveSpeed));
    }

    private void moveUpAbsolute(bool opposite)
    {
        cameraPos = AddArrays(cameraPos, MultiplyArrayWithScalar([0, 1, 0], (opposite ? 1 : -1) * moveSpeed));
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
        cameraForward = RotateAroundAxis(cameraForward, [0, 1, 0], yawAngle); // Rotate forward around the up axis
        cameraRight = RotateAroundAxis(cameraRight, [0, 1, 0], yawAngle);     // Rotate right around the up axis
        cameraUp = RotateAroundAxis(cameraUp, [0, 1, 0], yawAngle);

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

    float[] NormalizeVector(float[] vector)
    {
        float length = (float)Math.Sqrt(vector.Sum(v => v * v));
        if (length == 0)
            throw new ArgumentException("The vector has zero length and cannot be normalized.");
        return vector.Select(v => v / length).ToArray();
    }

    private void InitializeDebugPanel()
    {
        Panel debugPanel = new Panel
        {
            Dock = DockStyle.Right,
            Width = 220,
            BackColor = Color.LightGray,
            AutoScroll = true // Enable scrolling for many controls
        };

        int controlY = 10; // Y position for placing controls

        // Dictionary to store references to the TextBox controls
        var controlReferences = new Dictionary<string, TextBox>();

        // Helper function to add a label and TextBox control
        void AddTextControl(string labelText, float variable, Action<float> setter, string key)
        {
            Label label = new Label
            {
                Text = labelText,
                Location = new Point(10, controlY),
                AutoSize = true
            };

            TextBox control = new TextBox
            {
                Text = variable.ToString(),
                Location = new Point(10, controlY + 20),
                Width = 100
            };
            control.TextChanged += (sender, e) =>
            {
                if (float.TryParse(control.Text, out float value))
                {
                    setter(value); // Update the variable if the value is valid
                }
            };

            controlReferences[key] = control; // Store the control reference for later updates

            debugPanel.Controls.Add(label);
            debugPanel.Controls.Add(control);
            controlY += 50; // Increment Y for the next control
        }

        // Helper function to add a label with non-editable text
        void AddTextLabel(string labelText, string value, string key)
        {
            Label label = new Label
            {
                Text = labelText + value,
                Location = new Point(10, controlY),
                AutoSize = true
            };

            // Store the label reference for later updates
            controlReferences[key] = new TextBox
            {
                Text = value,
                Location = new Point(10, controlY + 20),
                Width = 100,
                ReadOnly = true
            };

            debugPanel.Controls.Add(label);
            debugPanel.Controls.Add(controlReferences[key]);
            controlY += 50; // Increment Y for the next control
        }

        // Add controls for each variable
        AddTextControl("Move Speed", moveSpeed, value => moveSpeed = value, nameof(moveSpeed));
        AddTextControl("Far Plane", farPlane, value => farPlane = value, nameof(farPlane));
        AddTextControl("Near Plane", nearPlane, value => nearPlane = value, nameof(nearPlane));
        AddTextControl("FOV", fov, value => fov = value, nameof(fov));
        AddTextControl("Mouse Sensitivity", mouseSensitivity, value => mouseSensitivity = value, nameof(mouseSensitivity));

        // Add controls for camera position
        for (int i = 0; i < cameraPos.Length; i++)
        {
            string axis = i == 0 ? "X" : i == 1 ? "Y" : "Z";
            int index = i; // Capture the loop variable for use in lambda
            AddTextControl($"Camera Pos {axis}", cameraPos[i], value => cameraPos[index] = value, $"cameraPos{index}");
        }

        // Add controls for angles
        AddTextControl("Pitch Angle", pitchAngle, value => pitchAngle = value, nameof(pitchAngle));
        AddTextControl("Yaw Angle", yawAngle, value => yawAngle = value, nameof(yawAngle));
        AddTextControl("Roll Angle", rollAngle, value => rollAngle = value, nameof(rollAngle));

        // Add non-editable text labels for camera vectors
        AddTextLabel("Camera Forward: ", string.Join(", ", cameraForward), nameof(cameraForward));
        AddTextLabel("Camera Up: ", string.Join(", ", cameraUp), nameof(cameraUp));
        AddTextLabel("Camera Right: ", string.Join(", ", cameraRight), nameof(cameraRight));

        // Add the debug panel to the form
        this.Controls.Add(debugPanel);

        // Unfocus logic to deselect controls when clicking outside
        this.Click += (sender, e) => Unfocus();
        debugPanel.Click += (sender, e) => { }; // Prevent unfocus when clicking inside the panel
        foreach (Control control in debugPanel.Controls)
            control.Click += (sender, e) => { };

        // Timer to refresh control values
        System.Windows.Forms.Timer refreshTimer = new System.Windows.Forms.Timer { Interval = 100 }; // Refresh every 100ms
        refreshTimer.Tick += (sender, e) =>
        {
            // Update values for editable controls
            UpdateControlValue(nameof(moveSpeed), moveSpeed);
            UpdateControlValue(nameof(farPlane), farPlane);
            UpdateControlValue(nameof(nearPlane), nearPlane);
            UpdateControlValue(nameof(fov), fov);
            UpdateControlValue(nameof(mouseSensitivity), mouseSensitivity);

            // Update camera position values
            for (int i = 0; i < cameraPos.Length; i++)
            {
                UpdateControlValue($"cameraPos{i}", cameraPos[i]);
            }

            // Update angle values
            UpdateControlValue(nameof(pitchAngle), pitchAngle);
            UpdateControlValue(nameof(yawAngle), yawAngle);
            UpdateControlValue(nameof(rollAngle), rollAngle);

            // Update non-editable camera vectors
            UpdateTextLabel(nameof(cameraForward), string.Join(", ", cameraForward));
            UpdateTextLabel(nameof(cameraUp), string.Join(", ", cameraUp));
            UpdateTextLabel(nameof(cameraRight), string.Join(", ", cameraRight));
        };
        refreshTimer.Start();

        // Helper to update control values dynamically
        void UpdateControlValue(string key, float value)
        {
            if (controlReferences.TryGetValue(key, out var control) && control is TextBox textBox)
            {
                string newValue = value.ToString("G"); // Convert to string without rounding
                if (textBox.Text != newValue) // Avoid unnecessary updates
                {
                    textBox.Text = newValue;
                }
            }
        }

        // Helper to update non-editable text labels
        void UpdateTextLabel(string key, string value)
        {
            if (controlReferences.TryGetValue(key, out var control) && control is TextBox textBox)
            {
                string newValue = value;
                if (textBox.Text != newValue) // Avoid unnecessary updates
                {
                    textBox.Text = newValue;
                }
            }
        }
    }


    private void Unfocus()
    {
        this.ActiveControl = null;
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
            case Keys.Escape:
                if (noCursor)
                {
                    Cursor.Show();
                }
                else
                {
                    Cursor.Hide();
                    CenterMouse();
                }
                noCursor = !noCursor;
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

    private void CenterMouse()
    {
        Cursor.Position = PointToScreen(new Point(ClientSize.Width / 2, ClientSize.Height / 2));
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!noCursor)
            return;
        base.OnMouseMove(e);

        Point centerScreen = new Point(ClientSize.Width / 2, ClientSize.Height / 2);
        Point mousePos = PointToClient(Cursor.Position);

        if (mousePos != centerScreen)
        {
            int deltaX = mousePos.X - centerScreen.X;
            int deltaY = mousePos.Y - centerScreen.Y;
            float yaw = deltaX * mouseSensitivity;
            float pitch = deltaY * mouseSensitivity;
            RotateCamera(pitch, yaw, 0);
            CenterMouse();
        }
    }

}

