using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// This script creates a lathed object from a curve/spline.
/// </summary>
[ExecuteInEditMode]
public class Lathe : MonoBehaviour
{
    public Material material;
    protected LathedObject lathedObject;
    [Range(3, 512)]
    public int sections;

    private Mesh mesh;
    private List<Vector3> spline;

    /// <summary>
    /// Creates a lathed object, which will be added to the scene. The center of the object is the y-axis.
    /// </summary>
    /// <param name="spline">A spline, which will be used to create the lathed mesh.</param>
    public Lathe(List<Vector3> spline)
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        lathedObject = new LathedObject(spline, gameObject, sections, material, mesh, meshFilter);
    }

    public void init(List<Vector3> generatedSpline)
    {
        if (this.spline == null)
            this.spline = generatedSpline;
        MeshFilter meshFilter = this.GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        lathedObject = new LathedObject(spline, gameObject, sections, material, mesh, meshFilter);
    }

    /// <summary>
    /// Updates the mesh with new coordinates from a spline.
    /// </summary>
    /// <param name="spline">The new spline, which will be used to update the lathed mesh.</param>
    public void updateMesh(List<Vector3> spline)
    {
        lathedObject.updateMesh(spline);
    }

    /// <summary>
    /// This class represents a lathed object created from a spline.
    /// </summary>
    protected class LathedObject
    {
        //////////////////////////////
        // constants -----------------

        // ---------------------------

        // public vars ---------------
        public List<Vector3> spline;
        // ---------------------------

        // protected vars ------------
        protected GameObject gameObject;
        protected int sections;
        protected Material mat;

        // private vars --------------
        // needed for mesh creating
        private Mesh mesh;
        private List<Vector3> vertices;
        private List<Vector2> uv;
        private int[] triangles;
        // ---------------------------
        //////////////////////////////

        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="spline">A spline as an array of <c>Vector3</c> elements</param>
        /// <param name="gameObject">The empty <c>GameObject</c> on which the lathe mesh will be drawn</param>
        public LathedObject(List<Vector3> spline, GameObject gameObject, int sections, Material mat, Mesh mesh, MeshFilter meshFilter)
        {
            this.spline = spline;
            this.gameObject = gameObject;
            this.sections = sections;
            this.mat = mat;

            List<List<Vector3>> verticesList2D = getLathedMeshVertices(spline, sections);

            this.mesh = mesh;

            vertices = list2dToSimpleList(verticesList2D);
            //uv = TODO;
            triangles = getTriangleArray(verticesList2D);

            createMesh();
        }

        /// <summary>
        /// Creates the mesh and assigns a material.
        /// </summary>
        private void createMesh() // TODO
        {
            mesh.vertices = vertices.ToArray();
            //mesh.uv = uv.ToArray();
            mesh.triangles = triangles;

            Renderer renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.material = mat;
            mesh.RecalculateNormals();
        }

        /// <summary>
        /// Updates the mesh with the given spline.
        /// </summary>
        /// <param name="spline">The spline for lathing.</param>
        public void updateMesh(List<Vector3> spline)
        {
            this.spline = spline;
            List<List<Vector3>> verticesList2D = getLathedMeshVertices(spline);
            vertices = list2dToSimpleList(verticesList2D);
            mesh.vertices = vertices.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }

        /// <summary>
        /// This is where the magic happens. The function calculates the triangles for the lathed mesh from the generated vertices.
        /// </summary>
        /// <param name="verticesList2D">A 2D list of the vertices of the mesh.</param>
        /// <returns>An array of the vertex indizes of the triangles.</returns>
        private int[] getTriangleArray(List<List<Vector3>> verticesList2D)
        {
            List<int> triangleArray = new List<int>();

            // TODO Does not work for some section values. E.g. 17, 23, 64
            for (int i = 0; i < verticesList2D.Count - 1; i++)
            {
                int vIndex = i * sections; // index of the "spline" vertex

                for (int j = 0; j < sections; j++)
                {
                    int vertexIndex = vIndex + j; // Index of the vertex in the current row

                    if (j != sections - 1) // if it's not the last polygon in row
                    {
                        // first triangle
                        triangleArray.Add(vertexIndex);
                        triangleArray.Add(vertexIndex + 1);
                        triangleArray.Add(vertexIndex + 1 + sections);

                        // second triangle
                        triangleArray.Add(vertexIndex + 1 + sections);
                        triangleArray.Add(vertexIndex + sections);
                        triangleArray.Add(vertexIndex);
                    }
                    else
                    {
                        // first triangle
                        triangleArray.Add(vertexIndex);
                        triangleArray.Add(vIndex);
                        triangleArray.Add(vIndex + sections);

                        // second triangle
                        triangleArray.Add(vIndex + sections);
                        triangleArray.Add(vertexIndex + sections);
                        triangleArray.Add(vertexIndex);
                    }
                }
            }

            return triangleArray.ToArray();
        }

        /// <summary>
        /// Get a computed list of all vertices for a given number of segments on a circle with (0, 0, 0) as origin.
        /// </summary>
        /// <param name="vertex">The vertex on angle 0 from the spline.</param>
        /// <param name="segments">The number of segments the lathed object should be generated</param>
        /// <returns>A list of all vertices on a circle in the same height as the given vertex.</returns>
        private List<Vector3> getVerticesOnCircle(Vector3 vertex, int segments)
        {
            Vector3 circleOrigin = new Vector3(0, vertex.y, 0);
            float radius = Vector3.Distance(vertex, circleOrigin);
            float segmentAngle = getAngleForNumberOfSections(segments);

            List<Vector3> verticesOnCircle = new List<Vector3> { };

            for (float angle = 0f; angle < 2 * Mathf.PI; angle += segmentAngle)
            {
                verticesOnCircle.Add(getPointOnCircle(angle, radius, circleOrigin));
            }

            return verticesOnCircle;
        }

        /// <summary>
        /// Get all a 2D-list of all vertices for the generated lathed mesh. 
        /// </summary>
        /// <param name="spline">The spline from which the lathed object will be created.</param>
        /// <param name="sections">The number of sections the mesh will be created with.</param>
        /// <returns>A 2D-list of all mesh vertices.
        /// e.g.: <code>
        /// [ [0, 1, 0],
        ///   [0, 2, 1],
        ///   [0, 3, 4], 
        ///   [0, 4, 3] ]</code></returns>
        private List<List<Vector3>> getLathedMeshVertices(List<Vector3> spline, int sections)
        {
            List<List<Vector3>> meshVertices = new List<List<Vector3>>();

            foreach (Vector3 splineVertex in spline)
            {
                meshVertices.Add(getVerticesOnCircle(splineVertex, sections));
            }

            return meshVertices;
        }

        /// <summary>
        /// Get all a 2D-list of all vertices for the generated lathed mesh, when the sections are already defined.
        /// </summary>
        /// <param name="spline">The spline from which the lathed object will be created.</param>
        /// <returns>A 2D-list of all mesh vertices.
        /// e.g.: <code>
        /// [ [0, 1, 0],
        ///   [0, 2, 1],
        ///   [0, 3, 4], 
        ///   [0, 4, 3] ]</code></returns>
        private List<List<Vector3>> getLathedMeshVertices(List<Vector3> spline)
        {
            return getLathedMeshVertices(spline, sections);
        }

        /// <summary>
        /// Convert a 2D list into a one dimensional list. This is required for the mesh creation with the Mesh class.
        /// </summary>
        /// <param name="verticesList2d"></param>
        /// <returns>A one dimension <c>List&ltVector3&gt</c></returns>
        private List<Vector3> list2dToSimpleList(List<List<Vector3>> verticesList2d)
        {
            List<Vector3> verticesList = new List<Vector3>() { };

            foreach (List<Vector3> verticesOnCircle in verticesList2d)
            {
                verticesList.AddRange(verticesOnCircle);
            }

            return verticesList;
        }

        /// <summary>
        /// Get the angle of a segment in radians.
        /// </summary>
        /// <param name="segments">The number of segments.</param>
        /// <returns>The angle of a segment in radians.</returns>
        private float getAngleForNumberOfSections(int segments)
        {
            return 2 * Mathf.PI / segments;
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
        /// <param name="rotationAxis">(optional) A normalized <c>Vector3</c> with exaclty two 0 values(e.g. <c>Vector3(0, 1, 0)</c>)</param>
        /// <returns>The distance from the vertex to the axis at 90 degrees</returns>
        private float getRadius(Vector3 vertex, Vector3? rotationAxis = null)
        {
            // default axis is the up vector
            if (rotationAxis == null)
            {
                rotationAxis = Vector3.up;
            }

            return Vector3.Distance(vertex, rotationAxis.Value);
        }

        /// <summary>
        /// Get the angle of a section between the tow vertices an the origin of the circle.
        /// </summary>
        /// <param name="sections">Number of sections of the lathed mesh.</param>
        /// <returns>The radius in radiants of the section on the circle.</returns>
        private float getAngleOfSection(int sections)
        {
            return (2 * Mathf.PI) / sections;
        }

        /// <summary>
        /// Get the point on a circle for the given angle and radius.
        /// </summary>
        /// <param name="angle">The angle between the axis and the desired point from origin.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="origin">The origin of the circle.</param>
        /// <returns>A <c>Vector3</c> object of the point with the given angle
        /// between the axis and the desired point from origin of the circle.</returns>
        private Vector3 getPointOnCircle(float angle, float radius, Vector3 origin)
        {
            // x = cx + r * sin(a)
            // y = cy               | same height as the origin of the circle
            // z = cz + r * cos(a)
            return new Vector3(origin.x + (radius * Mathf.Sin(angle)), origin.y, origin.z + (radius * Mathf.Cos(angle)));
        }

        /// <summary>
        /// Generates a string with the configured number of sections and the generated vertices. 
        /// </summary>
        /// <returns>A <c>String</c> with the number of sections and generated vertices.</returns>
        public override String ToString()
        {
            return "Lathed Object --------\nSections: " + sections + " | Vertices: " + vertices.ToArray().Length;
        }
    }
}
