using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSmoothGen : MonoBehaviour {
    public int subdivision = 2;
    [Range(0, 0.5f)]
    public float smoothRange = 0.5f;
    public Material testMaterial;

    [ContextMenu("GenerateSampleCube")]
    public void GenerateSampleCube()
    {

    }

    [ContextMenu("ShowBaseBottom")]
    public void ShowBaseBottom()
    {
        var m = GenerateBaseBottom();
        GameObject go = new GameObject("ShowBaseBottom");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mf.sharedMesh = m;
        mr.sharedMaterial = testMaterial;
    }

    public Mesh GenerateBaseBottom()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        for(int i = 0; i <= subdivision; i++)
        {
            float x = smoothRange * (1f - Mathf.Cos((float)i / subdivision * Mathf.PI / 2));
            float y = smoothRange * (1f - Mathf.Sin((float)i / subdivision * Mathf.PI / 2));
            vertices.Add(new Vector3(x, y, 0));
            vertices.Add(new Vector3(x, y, 1));
            uvs.Add(new Vector2(1-y, 0));
            uvs.Add(new Vector2(1-y, 1));
            var normal = new Vector3(0.5f, 0.5f, 0) - new Vector3(x, y, 0);
            normal.Normalize();
            normals.Add(normal);
            normals.Add(normal);
            if (i < subdivision)
            {
                triangles.Add(i * 2);
                triangles.Add(i * 2 + 1);
                triangles.Add(i * 2 + 3);

                triangles.Add(i * 2);
                triangles.Add(i * 2 + 3);
                triangles.Add(i * 2 + 2);
            }
        }
        Mesh m = new Mesh();
        m.SetVertices(vertices);
        m.SetTriangles(triangles, 0);
        m.SetUVs(0, uvs);
        return m;
    }


    [ContextMenu("ShowBaseMid")]
    public void ShowBaseMid()
    {
        var m = GenerateBaseMid();
        GameObject go = new GameObject("ShowBaseMid");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mf.sharedMesh = m;
        mr.sharedMaterial = testMaterial;
    }

    [ContextMenu("Recalculate Normals")]
    public void RecalculateNormals()
    {
        var mfs = GetComponentsInChildren<MeshFilter>();
        foreach(var mf in mfs)
        {
            mf.sharedMesh.RecalculateNormals();
        }
    }

    public Mesh GenerateBaseMid()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        for (int i = 0; i <= subdivision; i++)
        {
            float t = (float)i / subdivision;
            float x = smoothRange * (1f - Mathf.Cos(t * Mathf.PI / 2));
            float z = smoothRange * (1f - Mathf.Sin(t * Mathf.PI / 2));
            vertices.Add(new Vector3(x, 0, z));
            vertices.Add(new Vector3(x, 1, z));
            uvs.Add(new Vector2(0.5f + t * 0.5f, 0));
            uvs.Add(new Vector2(0.5f + t * 0.5f, 1));
            var normal = new Vector3(0.5f, 0, 0.5f) - new Vector3(x, 0, z);
            normal.Normalize();
            normals.Add(normal);
            normals.Add(normal);
            if (i < subdivision)
            {
                triangles.Add(i * 2);
                triangles.Add(i * 2 + 3);
                triangles.Add(i * 2 + 1);

                triangles.Add(i * 2);
                triangles.Add(i * 2 + 2);
                triangles.Add(i * 2 + 3);
            }
        }
        Mesh m = new Mesh();
        m.SetVertices(vertices);
        m.SetTriangles(triangles, 0);
        m.SetUVs(0, uvs);
        return m;
    }

    public Mesh GenerateBaseCorner()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        for (int i = 0; i <= subdivision; i++)
        {
            float t = (float)i / subdivision;
            float x = smoothRange * (1f - Mathf.Cos(t * Mathf.PI / 2));
            float z = smoothRange * (1f - Mathf.Sin(t * Mathf.PI / 2));
            vertices.Add(new Vector3(x, 0, z));
            vertices.Add(new Vector3(x, 1, z));
            uvs.Add(new Vector2(0.5f + t * 0.5f, 0));
            uvs.Add(new Vector2(0.5f + t * 0.5f, 1));
            var normal = new Vector3(0.5f, 0, 0.5f) - new Vector3(x, 0, z);
            normal.Normalize();
            normals.Add(normal);
            normals.Add(normal);
            if (i < subdivision)
            {
                triangles.Add(i * 2);
                triangles.Add(i * 2 + 3);
                triangles.Add(i * 2 + 1);

                triangles.Add(i * 2);
                triangles.Add(i * 2 + 2);
                triangles.Add(i * 2 + 3);
            }
        }
        Mesh m = new Mesh();
        m.SetVertices(vertices);
        m.SetTriangles(triangles, 0);
        m.SetUVs(0, uvs);
        return m;
    }
}
