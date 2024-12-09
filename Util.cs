namespace game3D;

class Util
{
    static public float[] RotateAroundAxis(float[] v, float[] axis, float angle)
    {
        // Normalisiere die Achse (axis muss ein Einheitsvektor sein)
        float axisLength = (float)Math.Sqrt(axis[0] * axis[0] + axis[1] * axis[1] + axis[2] * axis[2]);
        float[] normalizedAxis = [axis[0] / axisLength, axis[1] / axisLength, axis[2] / axisLength];

        // Kreuzprodukt von axis und v
        float[] crossProduct =
        [
        normalizedAxis[1] * v[2] - normalizedAxis[2] * v[1],
        normalizedAxis[2] * v[0] - normalizedAxis[0] * v[2],
        normalizedAxis[0] * v[1] - normalizedAxis[1] * v[0]
        ];

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

    static public float[] MultiplyMatrixAndVector(float[,] matrix, float[] vector)
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

    static public float Dot(float[] a, float[] b)
    {
        return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
    }

    static public float[] AddArrays(float[] array1, float[] array2)
    {
        if (array1.Length != array2.Length)
            throw new ArgumentException("Arrays must have the same length.");
        float[] result = new float[array1.Length];
        for (int i = 0; i < array1.Length; i++)
            result[i] = array1[i] + array2[i];

        return result;
    }

    static public float[] MultiplyArrayWithScalar(float[] array, float scalar)
    {
        float[] result = new float[array.Length];
        for (int i = 0; i < array.Length; i++)
            result[i] = array[i] * scalar;
        return result;
    }

    static public float[] NormalizeVector(float[] vector)
    {
        float length = (float)Math.Sqrt(vector.Sum(v => v * v));
        if (length == 0)
            throw new ArgumentException("The vector has zero length and cannot be normalized.");
        return vector.Select(v => v / length).ToArray();
    }
}