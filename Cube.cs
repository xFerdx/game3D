namespace game3D;

class Cube
{
    public float[] MiddlePoint { get; private set; }
    public float Size { get; private set; }
    public float[][] Points { get; private set; }
    public int[][] Edges { get; private set; }
    public int[][] Faces { get; private set; }

    public Cube(float[] middlePoint, float size)
    {
        MiddlePoint = middlePoint;
        Size = size;
        CreatePoints();
        CreateEdges();
        CreateFaces();
    }

    private void CreatePoints()
    {
        Points = new float[8][];
        float halfSize = Size / 2;
        Points[0] = [
            MiddlePoint[0] - halfSize,
            MiddlePoint[1] - halfSize,
            MiddlePoint[2] - halfSize
        ];
        Points[1] = [
            MiddlePoint[0] + halfSize,
            MiddlePoint[1] - halfSize,
            MiddlePoint[2] - halfSize
        ];
        Points[2] = [
            MiddlePoint[0] + halfSize,
            MiddlePoint[1] + halfSize,
            MiddlePoint[2] - halfSize
        ];
        Points[3] = [
            MiddlePoint[0] - halfSize,
            MiddlePoint[1] + halfSize,
            MiddlePoint[2] - halfSize
        ];
        Points[4] = [
            MiddlePoint[0] - halfSize,
            MiddlePoint[1] - halfSize,
            MiddlePoint[2] + halfSize
        ];
        Points[5] = [
            MiddlePoint[0] + halfSize,
            MiddlePoint[1] - halfSize,
            MiddlePoint[2] + halfSize
        ];
        Points[6] = [
            MiddlePoint[0] + halfSize,
            MiddlePoint[1] + halfSize,
            MiddlePoint[2] + halfSize
        ];
        Points[7] = [
            MiddlePoint[0] - halfSize,
            MiddlePoint[1] + halfSize,
            MiddlePoint[2] + halfSize
        ];
    }

    private void CreateEdges()
    {
        Edges = new int[12][];
        Edges[0] = [0, 1];
        Edges[1] = [1, 2];
        Edges[2] = [2, 3];
        Edges[3] = [3, 0];
        Edges[4] = [4, 5];
        Edges[5] = [5, 6];
        Edges[6] = [6, 7];
        Edges[7] = [7, 4];
        Edges[8] = [0, 4];
        Edges[9] = [1, 5];
        Edges[10] = [2, 6];
        Edges[11] = [3, 7];
    }

    private void CreateFaces()
    {
        Faces = new int[6][];
        Faces[0] = [0, 1, 2, 3]; // Vorderseite
        Faces[1] = [4, 5, 6, 7]; // Rückseite
        Faces[2] = [0, 1, 5, 4]; // Unterseite
        Faces[3] = [2, 3, 7, 6]; // Oberseite
        Faces[4] = [0, 3, 7, 4]; // Linke Seite
        Faces[5] = [1, 2, 6, 5]; // Rechte Seite
    }

    public void RotateX(float angle)
    {
        float cosA = (float)Math.Cos(angle);
        float sinA = (float)Math.Sin(angle);

        for (int i = 0; i < Points.Length; i++)
        {
            float y = Points[i][1];
            float z = Points[i][2];

            // Verschiebe den Punkt relativ zum Mittelpunkt
            y -= MiddlePoint[1];
            z -= MiddlePoint[2];

            // Rotieren um die X-Achse
            float newY = y * cosA - z * sinA;
            float newZ = y * sinA + z * cosA;

            // Verschiebe den Punkt zurück zum ursprünglichen Mittelpunkt
            Points[i][1] = newY + MiddlePoint[1];
            Points[i][2] = newZ + MiddlePoint[2];
        }
    }

    public void RotateY(float angle)
    {
        float cosA = (float)Math.Cos(angle);
        float sinA = (float)Math.Sin(angle);

        for (int i = 0; i < Points.Length; i++)
        {
            float x = Points[i][0];
            float z = Points[i][2];

            // Verschiebe den Punkt relativ zum Mittelpunkt
            x -= MiddlePoint[0];
            z -= MiddlePoint[2];

            // Rotieren um die Y-Achse
            float newX = x * cosA + z * sinA;
            float newZ = -x * sinA + z * cosA;

            // Verschiebe den Punkt zurück zum ursprünglichen Mittelpunkt
            Points[i][0] = newX + MiddlePoint[0];
            Points[i][2] = newZ + MiddlePoint[2];
        }
    }

    public void RotateZ(float angle)
    {
        float cosA = (float)Math.Cos(angle);
        float sinA = (float)Math.Sin(angle);

        for (int i = 0; i < Points.Length; i++)
        {
            float x = Points[i][0];
            float y = Points[i][1];

            // Verschiebe den Punkt relativ zum Mittelpunkt
            x -= MiddlePoint[0];
            y -= MiddlePoint[1];

            // Rotieren um die Z-Achse
            float newX = x * cosA - y * sinA;
            float newY = x * sinA + y * cosA;

            // Verschiebe den Punkt zurück zum ursprünglichen Mittelpunkt
            Points[i][0] = newX + MiddlePoint[0];
            Points[i][1] = newY + MiddlePoint[1];
        }
    }

}