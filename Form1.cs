using System.Data;

namespace game3D;

public partial class Form1 : Form
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    public static extern bool AllocConsole();
    private List<Cube> cubes;
    private Camera cam = new Camera();
    private System.Windows.Forms.Timer timer;
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

        this.WindowState = FormWindowState.Maximized;

        cubes = new List<Cube>
        {
            new([0, 10, 0], 10),
            //new([0, 0, 0], 10),
            //new([0, 20, 0], 10),
            //new([0, 30, 0], 10),
            //new([10, 30, 0], 10),
        };

        for (int i = 0; i < 19; i++)
            for (int j = 0; j < 19; j++)
                for (int k = 0; k < 2; k++)
                    //cubes.Add(new Cube([i * 10, -10 * k, j * 10], 10));
        timer = new System.Windows.Forms.Timer();
        timer.Interval = 1;
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private double elapsedTime = 0;
    private double fps = 0;
    private int counter = 0;

    private readonly System.Diagnostics.Stopwatch stopwatch = new();
    private void Timer_Tick(object sender, EventArgs e)
    {
        if (!stopwatch.IsRunning)
            stopwatch.Start();
        updatePos();
        Invalidate();
        counter++;
        elapsedTime += stopwatch.Elapsed.TotalSeconds;
        stopwatch.Restart();

        if (counter == 10)
        {
            fps = 10 / elapsedTime;
            counter = 0;
            elapsedTime = 0;
        }
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
            DrawCube(g, cube);

        g.DrawLine(Pens.Black, new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2 - 20), new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2 + 20));
        g.DrawLine(Pens.Black, new PointF(this.ClientSize.Width / 2 - 20, this.ClientSize.Height / 2), new PointF(this.ClientSize.Width / 2 + 20, this.ClientSize.Height / 2));

        Font font = new Font("Arial", 12, FontStyle.Regular);
        Brush brush = Brushes.Black;
        string cameraForwardText = $"Forward: {string.Join(", ", cam.cameraForward.Select(v => Math.Round(v, 3)))}";
        string cameraUpText = $"Up: {string.Join(", ", cam.cameraUp.Select(v => Math.Round(v, 3)))}";
        string cameraRightText = $"Right: {string.Join(", ", cam.cameraRight.Select(v => Math.Round(v, 3)))}";
        g.DrawString("fps: " + Math.Round(fps), font, brush, 10, 10);
        int x = 10;
        int y = 30;
        int lineHeight = 20;
        g.DrawString(cameraForwardText, font, brush, x, y);
        g.DrawString(cameraUpText, font, brush, x, y + lineHeight);
        g.DrawString(cameraRightText, font, brush, x, y + 2 * lineHeight);

    }

    private void DrawCube(Graphics g, Cube cube)
    {
        PointF?[] cube2D = Projection.ProjectCube(cube.Points, cam, this.ClientSize.Width, this.ClientSize.Height);

        foreach (var face in cube.Faces)
        {
            PointF[] facePoints = new PointF[4];
            for (int i = 0; i < 4; i++)
            {
                int pointIndex = face[i];
                if (cube2D[pointIndex].HasValue)
                    facePoints[i] = cube2D[pointIndex].Value;
                else
                    return;
            }

            g.FillPolygon(Brushes.Green, facePoints);
        }

        foreach (var edge in cube.Edges)
        {
            PointF? p1e = cube2D[edge[0]];
            PointF? p2e = cube2D[edge[1]];
            if (!p1e.HasValue || !p2e.HasValue)
                continue;
            PointF p1 = p1e.Value;
            PointF p2 = p2e.Value;

            bool p1OutsideView = p1.X < 0 || p1.X > this.ClientSize.Width || p1.Y < 0 || p1.Y > this.ClientSize.Height;
            bool p2OutsideView = p2.X < 0 || p2.X > this.ClientSize.Width || p2.Y < 0 || p2.Y > this.ClientSize.Height;
            //if (p1OutsideView && p2OutsideView) 
            //Console.WriteLine(p1.X+" "+p1.Y+" "+p2.X+" "+p2.Y);
            if (!float.IsFinite(p1.X) || !float.IsFinite(p1.Y) || !float.IsFinite(p2.X) || !float.IsFinite(p2.Y))
                continue;
            if (p1.X < -100000 || p1.X > 100000 || p1.Y < -100000 || p1.Y > 100000 || p2.X < -100000 || p2.X > 100000 || p2.Y < -100000 || p2.Y > 100000)
                continue;
            g.DrawLine(Pens.Black, p1, p2);
        }
    }

    private void updatePos()
    {
        if (pressedA)
            cam.moveRightRelative(true);
        if (pressedD)
            cam.moveRightRelative(false);
        if (pressedS)
            cam.moveForwardRelative(true);
        if (pressedW)
            cam.moveForwardRelative(false);
        if (pressedShift)
            cam.moveUpAbsolute(true);
        if (pressedSpace)
            cam.moveUpAbsolute(false);
    }

    private void InitializeDebugPanel()
    {
        Panel debugPanel = new Panel { Dock = DockStyle.Right, Width = 220, BackColor = Color.LightGray, AutoScroll = true };
        int controlY = 10;
        var controlReferences = new Dictionary<string, TextBox>();

        void AddTextControl(string labelText, float variable, Action<float> setter, string key)
        {
            Label label = new Label { Text = labelText, Location = new Point(10, controlY), AutoSize = true };
            TextBox control = new TextBox { Text = variable.ToString(), Location = new Point(10, controlY + 20), Width = 100 };
            control.TextChanged += (sender, e) => { if (float.TryParse(control.Text, out float value)) setter(value); };
            controlReferences[key] = control;
            debugPanel.Controls.Add(label);
            debugPanel.Controls.Add(control);
            controlY += 50;
        }

        void UpdateControlValue(string key, float value)
        {
            if (controlReferences.TryGetValue(key, out var control) && control is TextBox textBox)
                textBox.Text = value.ToString("G");
        }

        AddTextControl("Move Speed", cam.moveSpeed, value => cam.moveSpeed = value, nameof(cam.moveSpeed));
        AddTextControl("Far Plane", cam.farPlane, value => cam.farPlane = value, nameof(cam.farPlane));
        AddTextControl("Near Plane", cam.nearPlane, value => cam.nearPlane = value, nameof(cam.nearPlane));
        AddTextControl("FOV", cam.fov, value => cam.fov = value, nameof(cam.fov));
        AddTextControl("Mouse Sensitivity", mouseSensitivity, value => mouseSensitivity = value, nameof(mouseSensitivity));

        for (int i = 0; i < cam.cameraPos.Length; i++)
        {
            string axis = i == 0 ? "X" : i == 1 ? "Y" : "Z";
            int index = i;
            AddTextControl($"Camera Pos {axis}", cam.cameraPos[i], value => cam.cameraPos[index] = value, $"cam.cameraPos{index}");
        }

        AddTextControl("Pitch Angle", pitchAngle, value => pitchAngle = value, nameof(pitchAngle));
        AddTextControl("Yaw Angle", yawAngle, value => yawAngle = value, nameof(yawAngle));
        AddTextControl("Roll Angle", rollAngle, value => rollAngle = value, nameof(rollAngle));


        this.Controls.Add(debugPanel);
        this.Click += (sender, e) => { this.ActiveControl = null; };
        debugPanel.Click += (sender, e) => { };
        foreach (Control control in debugPanel.Controls) control.Click += (sender, e) => { };

        System.Windows.Forms.Timer refreshTimer = new System.Windows.Forms.Timer { Interval = 100 };
        refreshTimer.Tick += (sender, e) =>
        {
            UpdateControlValue(nameof(cam.moveSpeed), cam.moveSpeed);
            UpdateControlValue(nameof(cam.farPlane), cam.farPlane);
            UpdateControlValue(nameof(cam.nearPlane), cam.nearPlane);
            UpdateControlValue(nameof(cam.fov), cam.fov);
            UpdateControlValue(nameof(mouseSensitivity), mouseSensitivity);
            for (int i = 0; i < cam.cameraPos.Length; i++)
                UpdateControlValue($"cam.cameraPos{i}", cam.cameraPos[i]);
            UpdateControlValue(nameof(pitchAngle), pitchAngle);
            UpdateControlValue(nameof(yawAngle), yawAngle);
            UpdateControlValue(nameof(rollAngle), rollAngle);
        };
        refreshTimer.Start();
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
                cam.RotateCamera(pitchAngle, 0, 0);
                break;
            case Keys.Up:
                cam.RotateCamera(-pitchAngle, 0, 0);
                break;
            case Keys.Right:
                cam.RotateCamera(0, yawAngle, 0);
                break;
            case Keys.Left:
                cam.RotateCamera(0, -yawAngle, 0);
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

        Point centerScreen = new(ClientSize.Width / 2, ClientSize.Height / 2);
        Point mousePos = PointToClient(Cursor.Position);

        if (mousePos != centerScreen)
        {
            int deltaX = mousePos.X - centerScreen.X;
            int deltaY = mousePos.Y - centerScreen.Y;
            float yaw = -deltaX * mouseSensitivity;
            float pitch = deltaY * mouseSensitivity;
            cam.RotateCamera(pitch, yaw, 0);
            CenterMouse();
        }
    }

}