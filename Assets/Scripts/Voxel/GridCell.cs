using System.Collections.Generic;
using UnityEngine;

namespace Voxel
{
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

            public Vector3 Midpoint => (A + B) / 2;

            public Vector3 InterpolateMidpoint(float v1, float v2, float surfaceLevel)
            {
                return A + (surfaceLevel - v1) * (B - A) / (v2 - v1);
            }
        }

        public static readonly List<Vector3> Vertices = new List<Vector3>
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

        public static readonly List<Edge> Edges = new List<Edge>
        {
            new Edge(Vertices[0], Vertices[1]),
            new Edge(Vertices[1], Vertices[2]),
            new Edge(Vertices[2], Vertices[3]),
            new Edge(Vertices[3], Vertices[0]),
            new Edge(Vertices[4], Vertices[5]),
            new Edge(Vertices[6], Vertices[5]),
            new Edge(Vertices[7], Vertices[6]),
            new Edge(Vertices[7], Vertices[4]),
            new Edge(Vertices[0], Vertices[4]),
            new Edge(Vertices[1], Vertices[5]),
            new Edge(Vertices[6], Vertices[2]),
            new Edge(Vertices[3], Vertices[7]),
        };

        public Vector3Int Position { get; set; }
        public float Scale { get; set; }
        public float[] Values { get; } = new float[8];

        public bool IsEmpty 
        { 
            get
            {
                bool isEmpty = true;
                for (int i = 0; i < 8; i++)
                {
                    if (Values[i] < 1.0f)
                        isEmpty = false;
                }

                return isEmpty;
            }
        }

        public int GetCubeIndex(float surfaceLevel)
        {
            int cubeIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                if (Values[i] < surfaceLevel)
                    cubeIndex |= 1 << i;
            }

            return cubeIndex;
        }

        public Vector3 GetValuePos(int index)
        {
            return Vertices[index] * Scale + (Vector3)Position * Scale;
        }
    }
}
