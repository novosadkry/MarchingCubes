using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
    public struct Edge
    {
        public Edge(Vector3 a, Vector3 b)
        {
            A = a;
            B = b;
        }

        public Vector3 A { get; }
        public Vector3 B { get; }

        public Vector3 Midpoint { get => (A + B) / 2; }
    }

    public static Vector3[] vertices =
    {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(1, 1, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(1, 1, 1),
        new Vector3(0, 1, 1),
    };

    public static Edge[] edges =
    {
        new Edge(vertices[0], vertices[1]),
        new Edge(vertices[1], vertices[2]),
        new Edge(vertices[2], vertices[3]),
        new Edge(vertices[3], vertices[0]),
        new Edge(vertices[4], vertices[5]),
        new Edge(vertices[6], vertices[5]),
        new Edge(vertices[7], vertices[6]),
        new Edge(vertices[7], vertices[4]),
        new Edge(vertices[0], vertices[4]),
        new Edge(vertices[1], vertices[5]),
        new Edge(vertices[6], vertices[2]),
        new Edge(vertices[3], vertices[7]),
    };

    public float[] Values { get; } = new float[8];

    public Mesh ConstructMesh(float surfaceLevel)
    {
        int cubeIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (Values[i] < surfaceLevel)
                cubeIndex |= 1 << i;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        int[] tri = Table.triangulation[cubeIndex];
        int vertexCount = 0;

        foreach (int edgeIndex in tri)
        {
            if (edgeIndex == -1)
                continue;

            Edge edge = edges[edgeIndex];
            vertices.Add(edge.Midpoint);
            triangles.Add(vertexCount++);
        }

        foreach (int edgeIndex in tri.Reverse())
        {
            if (edgeIndex == -1)
                continue;

            Edge edge = edges[edgeIndex];
            vertices.Add(edge.Midpoint);
            triangles.Add(vertexCount++);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}
