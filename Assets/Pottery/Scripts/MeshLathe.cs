using UnityEngine;
using System.Collections;

public class MeshLathe {

    private Vector3 Y_AXIS = new Vector3(0, 1, 0);

    private Vector3 spline;
    private GameObject go;
    private int sections;
    private float radius;
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uV;
    private int[] triangles;

    /// <summary>
    /// Main contructor.
    /// </summary>
    /// <param name="spline">A spline as an array of <c>Vector3</c> elements</param>
    /// <param name="go">The empty <c>GameObject</c> on which the lathe mesh will be drawn</param>
    /// <param name="radius">default is <c>0</c></param>
    MeshLathe(Vector3 spline, GameObject go, int sections, float radius = 0)
    {
        this.spline = spline;
        this.go = go;
        this.sections = sections;
        this.radius = radius;

        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();

        mesh = new Mesh();


        mesh.vertices = this.vertices;
        mesh.uv = this.uV;
        mesh.triangles = this.triangles;
    }

    private Vector3[] createPolygon (Vector3[] splineSection)
    {
        if (splineSection.Length != 2)
        {
            Debug.Log("createPolygon(): invalid length of spline section Vector");
        }



        return new Vector3[] { };
    }

    private void createTriangle(Vector3 a, Vector3 b, Vector3 c)
    {

    }

    /// <summary>
    /// Get the perimeter of a circle.
    /// </summary>
    /// <param name="radius">The radius of the circle</param>
    /// <returns>The perimeter of a circle with the given radius.</returns>
    private float getPerimeter(float radius)
    {
        return 2 * Mathf.PI * radius;
    }

    /// <summary>
    /// Get the Radius for the given vertex to the given rotation axis.
    /// </summary>
    /// <param name="vertex">A vertex with x, y and z-coordinates of type <c>Vector3</c></param>
    /// <param name="rotationAxis">A normalized <c>Vector3</c> with exaclty two 0 values(e.g. <c>Vector3(0, 1, 0)</c>)</param>
    /// <returns>The distance from the vertex to the axis at 90 degrees</returns>
    private float getRadius(Vector3 vertex, Vector3? rotationAxis = null)
    {
        // default axis is he Y-axis
        if(rotationAxis == null)
        {
            rotationAxis = Y_AXIS;
        }

        return Mathf.Sqrt(Mathf.Pow(vertex.x, 2) + Mathf.Pow(vertex.z, 2));
    }
}
