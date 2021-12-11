using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

[RequireComponent(typeof(MeshFilter))]
public class SphereGenerator : MonoBehaviour
{
    Mesh sphere;
    public float Radius = 100;
    public int resolution = 1;
    public bool hexagons = false;

    public static int detailsLevel;
    static List<Vector3> baseNormals = new List<Vector3>() { new Vector3(-0.525731f, 0.000000f, 0.850651f), new Vector3(0.525731f, 0.000000f, 0.850651f), new Vector3(-0.525731f, 0.000000f, -0.850651f), new Vector3(0.525731f, 0.000000f, -0.850651f), new Vector3(0.000000f, 0.850651f, 0.525731f), new Vector3(0.000000f, 0.850651f, -0.525731f), new Vector3(0.000000f, -0.850651f, 0.525731f), new Vector3(0.000000f, -0.850651f, -0.525731f), new Vector3(0.850651f, 0.525731f, 0.000000f), new Vector3(-0.850651f, 0.525731f, 0.000000f), new Vector3(0.850651f, -0.525731f, 0.000000f), new Vector3(-0.850651f, -0.525731f, 0.000000f) };
    static List<int> baseTriangles = new List<int>() { 0, 4, 1, 0, 9, 4, 9, 5, 4, 4, 5, 8, 4, 8, 1, 8, 10, 1, 8, 3, 10, 5, 3, 8, 5, 2, 3, 2, 7, 3, 7, 10, 3, 7, 6, 10, 7, 11, 6, 11, 0, 6, 0, 1, 6, 6, 1, 10, 9, 0, 11, 9, 11, 2, 9, 2, 5, 7, 2, 11 };


    static List<Vector3> vertices = new List<Vector3>();
    static List<Vector3> normals = new List<Vector3>();
    static List<int> triangles = new List<int>();
    static List<Vector2> UVS = new List<Vector2>();

    static List<Vector3> hexVertices = new List<Vector3>();
    static List<Vector3> hexNormals = new List<Vector3>();
    static List<int> hexTriangles = new List<int>();
    static List<Vector2> hexUVS = new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        Creator();
    }

    void Creator()
    {
        detailsLevel = resolution;

        sphere = new Mesh();
        GetComponent<MeshFilter>().mesh = sphere;

        normals = baseNormals;
        triangles = baseTriangles;

        SubDivide();

        hexTriangles.Clear();
        hexVertices.Clear();

        //CreateGoldberg();
    }

    // Update is called once per frame
    void Update()
    {
        if (resolution != detailsLevel)
        {
            Creator();

            hexTriangles.Clear();
            hexVertices.Clear();

            //CreateGoldberg();
        }

        if (!hexagons)
        {
            sphere.Clear();

            vertices = normals.ToArray().Select(a => a * Radius).ToList();

            sphere.vertices = vertices.ToArray();
            sphere.triangles = triangles.ToArray();
            sphere.uv = UVS.ToArray();
        }
        else
        {
            sphere.Clear();

            hexVertices = hexNormals.ToArray().Select(a => a * Radius).ToList();

            sphere.vertices = hexVertices.ToArray();
            sphere.triangles = hexTriangles.ToArray();
            sphere.uv = hexUVS.ToArray();
        }
    }

    Vector2 CreateUV(Vector3 point)
    {
        return new Vector2(0.5f + (Mathf.Atan2(point.z, point.x) / (2 * Mathf.PI)), 0.5f - (Mathf.Asin(-point.y) / Mathf.PI));
    }

    public void CreateGoldberg()
    {
        hexNormals.Clear(); 
        hexVertices.Clear();
        hexTriangles.Clear();
        hexUVS.Clear();

        for(int v = 0; v < normals.Count; v++)
        //Parallel.For(0, normals.Count, v =>
        {
            List<int> hexTri = new List<int>();

            for (int i = 0; i < triangles.Count; i = i + 3)
            {
                if (v == triangles[i] || v == triangles[i + 1] || v == triangles[i + 2])
                {
                    var vert = GetCentroid(normals[triangles[i]], normals[triangles[i + 1]], normals[triangles[i + 2]]);

                    hexNormals.Add(vert);

                    int index = hexNormals.FindIndex(a => a == vert);

                    hexTri.Add(index);

                    hexUVS.Add(CreateUV(hexNormals[index]));
                }
            }

            CreateHexTriangles(hexTri);
        }
        //});
    }

    public void CreateHexTriangles(List<int> tri)
    {
        int triCount = tri.Count;

        int first = tri[2];

        tri.RemoveAt(2);

        int min1 = 1000000;
        int min2 = 1000000;

        float minDist1 = 1000000;
        float minDist2 = 1000000;

        for (int t = 0; t < tri.Count; t++)
        {
            var dist = Vector3.Distance(hexNormals[first], hexNormals[tri[t]]);
            if (dist < minDist1 || dist < minDist2)
            {
                if (minDist1 > minDist2)
                {
                    minDist1 = dist;
                    min1 = tri[t];
                }
                else
                {
                    minDist2 = dist;
                    min2 = tri[t];
                }
            }
        }

        hexTriangles.Add(first);
        hexTriangles.Add(min1);
        hexTriangles.Add(min2);

        hexTriangles.Add(first);
        hexTriangles.Add(min2);
        hexTriangles.Add(min1);

        tri.Remove(min1);
        tri.Remove(min2);

        int min3 = 1000000;

        float minDist3 = 1000000;

        for (int t = 0; t < tri.Count; t++)
        {
            var dist = Vector3.Distance(hexNormals[min1], hexNormals[tri[t]]);
            if (dist < minDist3)
            {
                minDist3 = dist;
                min3 = tri[t];
            }
        }

        hexTriangles.Add(min1);
        hexTriangles.Add(min2);
        hexTriangles.Add(min3);

        hexTriangles.Add(min1);
        hexTriangles.Add(min3);
        hexTriangles.Add(min2);

        tri.Remove(min3);

        if (triCount == 6)
        {
            int min4 = 1000000;

            float minDist4 = 1000000;

            for (int t = 0; t < tri.Count; t++)
            {
                var dist = Vector3.Distance(hexNormals[min2], hexNormals[tri[t]]);
                if (dist < minDist4)
                {
                    minDist4 = dist;
                    min4 = tri[t];
                }
            }

            hexTriangles.Add(min2);
            hexTriangles.Add(min3);
            hexTriangles.Add(min4);

            hexTriangles.Add(min2);
            hexTriangles.Add(min4);
            hexTriangles.Add(min3);

            tri.Remove(min4);

            int last = tri[0];

            hexTriangles.Add(last);
            hexTriangles.Add(min3);
            hexTriangles.Add(min4);

            hexTriangles.Add(last);
            hexTriangles.Add(min4);
            hexTriangles.Add(min3);
        }
        else
        {
            int last = tri[0];

            hexTriangles.Add(last);
            hexTriangles.Add(min2);
            hexTriangles.Add(min3);

            hexTriangles.Add(last);
            hexTriangles.Add(min3);
            hexTriangles.Add(min2);
        }
    }

    public Vector3 GetCentroid(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        return (v0 + v1 + v2) / 2;
    }

    public static void SubDivide()
    {
        List<int> newTriangles = new List<int>();
        Dictionary<string, int> midPoints = new Dictionary<string, int>();

        for (int d = 0; d < detailsLevel; d++)
        {
            for (int i = 0; i < triangles.Count - 2; i = i + 3)
            {
                int i0 = triangles[i];
                int i1 = triangles[i + 1];
                int i2 = triangles[i + 2];

                int m01 = GetMidpoints(i0, i1, midPoints);
                int m12 = GetMidpoints(i1, i2, midPoints);
                int m02 = GetMidpoints(i2, i0, midPoints);

                newTriangles.Add(i0);
                newTriangles.Add(m01);
                newTriangles.Add(m02);

                newTriangles.Add(i1);
                newTriangles.Add(m12);
                newTriangles.Add(m01);

                newTriangles.Add(i2);
                newTriangles.Add(m02);
                newTriangles.Add(m12);

                newTriangles.Add(m02);
                newTriangles.Add(m01);
                newTriangles.Add(m12);
            }

            triangles = new List<int>(newTriangles);
            newTriangles.Clear();
        }
    }

    public static int GetMidpoints(int x, int y, Dictionary<string, int> mids)
    {
        int midpointIndex = -1;

        int min = x < y ? x : y;
        int max = x > y ? x : y;

        string edgeKey = min.ToString() + "_" + max.ToString();

        var bMid = mids.TryGetValue(edgeKey, out midpointIndex);

        if (!bMid)
        {
            Vector3 v0 = normals[x];
            Vector3 v1 = normals[y];

            Vector3 midpoint = (v0 + v1) / 2;

            midpoint = Vector3.Normalize(midpoint);

            if (normals.Contains(midpoint))
            {
                midpointIndex = normals.FindIndex(a => a == midpoint);
            }
            else
            {
                midpointIndex = normals.Count;
                normals.Add(midpoint);
                mids.Add(edgeKey, midpointIndex);
            }
        }

        return midpointIndex;
    }
}
