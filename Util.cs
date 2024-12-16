namespace game3D;
using System.Numerics;

class Util
{
    static public double[] RotateAroundAxis(double[] v, double[] axis, double angle)
    {
        // Normalisiere die Achse (axis muss ein Einheitsvektor sein)
        double axisLength = (double)Math.Sqrt(axis[0] * axis[0] + axis[1] * axis[1] + axis[2] * axis[2]);
        double[] normalizedAxis = [axis[0] / axisLength, axis[1] / axisLength, axis[2] / axisLength];

        // Kreuzprodukt von axis und v
        double[] crossProduct =
        [
        normalizedAxis[1] * v[2] - normalizedAxis[2] * v[1],
        normalizedAxis[2] * v[0] - normalizedAxis[0] * v[2],
        normalizedAxis[0] * v[1] - normalizedAxis[1] * v[0]
        ];

        // Skalarprodukt von axis und v
        double dotProduct = normalizedAxis[0] * v[0] + normalizedAxis[1] * v[1] + normalizedAxis[2] * v[2];

        // Rodrigues' Rotation Formula
        double cosTheta = (double)Math.Cos(angle);
        double sinTheta = (double)Math.Sin(angle);

        // Berechne den rotierten Vektor
        double[] rotatedVector =
        [
            v[0] * cosTheta + crossProduct[0] * sinTheta + normalizedAxis[0] * dotProduct * (1 - cosTheta),
            v[1] * cosTheta + crossProduct[1] * sinTheta + normalizedAxis[1] * dotProduct * (1 - cosTheta),
            v[2] * cosTheta + crossProduct[2] * sinTheta + normalizedAxis[2] * dotProduct * (1 - cosTheta)
        ];

        return rotatedVector;
    }

    static public double[] MultiplyMatrixAndVector(double[,] matrix, double[] vector)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        if (vector.Length != cols)
            throw new ArgumentException("The length of the vector must match the number of columns in the matrix.");
        double[] result = new double[rows];
        for (int row = 0; row < rows; row++)
        {
            result[row] = 0;
            for (int col = 0; col < cols; col++)
                result[row] += matrix[row, col] * vector[col];
        }
        return result;
    }

    static public double Dot(double[] a, double[] b)
    {
        return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
    }

    static public double[] AddArrays(double[] array1, double[] array2)
    {
        if (array1.Length != array2.Length)
            throw new ArgumentException("Arrays must have the same length.");
        double[] result = new double[array1.Length];
        for (int i = 0; i < array1.Length; i++)
            result[i] = array1[i] + array2[i];

        return result;
    }

    static public double[] MultiplyArrayWithScalar(double[] array, double scalar)
    {
        double[] result = new double[array.Length];
        for (int i = 0; i < array.Length; i++)
            result[i] = array[i] * scalar;
        return result;
    }

    static public double[] NormalizeVector(double[] vector)
    {
        double length = (double)Math.Sqrt(vector.Sum(v => v * v));
        if (length == 0)
            throw new ArgumentException("The vector has zero length and cannot be normalized.");
        return vector.Select(v => v / length).ToArray();
    }


    static public (bool, double, Vector3) RayIntersectsSquare(Vector3 rayOrigin, Vector3 rayDirection, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        // Test the first triangle: (v0, v1, v2)
        var result1 = RayIntersectsTriangle(rayOrigin, rayDirection, v0, v1, v2);
        if (result1.Item1)
        {
            return result1; // Intersection found in the first triangle
        }

        // Test the second triangle: (v0, v2, v3)
        var result2 = RayIntersectsTriangle(rayOrigin, rayDirection, v0, v2, v3);
        if (result2.Item1)
        {
            return result2; // Intersection found in the second triangle
        }

        // No intersection with either triangle
        return (false, 0, Vector3.Zero);
    }

    static public (bool, double, Vector3) RayIntersectsTriangle(Vector3 rayOrigin, Vector3 rayDirection, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        Vector3 intersectionPoint = Vector3.Zero;

        // Edge vectors of the triangle
        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;

        // Compute the determinant
        Vector3 h = Vector3.Cross(rayDirection, edge2);
        float a = Vector3.Dot(edge1, h);

        // If the determinant is near zero, the ray is parallel to the triangle plane
        if (Math.Abs(a) < 1e-6)
        {
            return (false, 0, intersectionPoint);
        }

        float f = 1.0f / a;
        Vector3 s = rayOrigin - v0;
        float u = f * Vector3.Dot(s, h);

        // Check if the intersection lies outside the triangle
        if (u < 0.0f || u > 1.0f)
        {
            return (false, 0, intersectionPoint);
        }

        Vector3 q = Vector3.Cross(s, edge1);
        float v = f * Vector3.Dot(rayDirection, q);

        // Check if the intersection lies outside the triangle
        if (v < 0.0f || u + v > 1.0f)
        {
            return (false, 0, intersectionPoint);
        }

        // Compute the distance along the ray to the intersection point
        float t = f * Vector3.Dot(edge2, q);

        // Calculate the intersection point
        intersectionPoint = rayOrigin + t * rayDirection;

        return (true, t, intersectionPoint);
    }

    public static string ArrayToString<T>(T[] array)
    {
        if (array == null || array.Length == 0)
            return "[]";
        return "[" + string.Join(", ", array) + "]";
    }
}