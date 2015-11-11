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
    public LathedObject lathedObject;
    [Range(3, 64)]
    public int sections;

    private Mesh mesh;


    /// <summary>
    /// This class represents a lathed object created from a spline.
    /// </summary>
    public class LathedObject
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
            this.vertices = list2dToSimpleList(verticesList2D);
            //this.uv = TODO;
            this.triangles = getTriangleArray(verticesList2D);

            createMesh();
        }

        /// <summary>
        /// Standard constructor. (Only for testing)
        /// </summary>
        public LathedObject(GameObject gameObject, int sections, Material mat, Mesh mesh, MeshFilter meshFilter)
        {
            this.spline = new List<Vector3>
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 2),
                new Vector3(0, 2, 2),
                new Vector3(0, 3, 2),
                new Vector3(0, 4, 1),
            };

            // new LathedObject(spline, gameObject, sections, mat, mesh, meshFilter);

            // -------------------------
            this.vertices = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 1)
            };

            this.uv = new List<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(0, 1)
            };

            this.triangles = new int[]
            {
                2, 1, 0,
                1, 2, 3
            };

            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles;

            Renderer renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.material = mat;
        }

        // TODO
        private void createMesh()
        {
            mesh.vertices = vertices.ToArray();
            //mesh.uv = uv.ToArray();
            mesh.triangles = triangles;

            Renderer renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.material = mat;
        }

        /// <summary>
        /// This is where the magic happens. The function calculates the triangles for the lathed mesh from the generated vertices.
        /// </summary>
        /// <param name="verticesList2D">A 2D list of the vertices of the mesh.</param>
        /// <returns>An array of the vertex indizes of the triangles.</returns>
        private int[] getTriangleArray(List<List<Vector3>> verticesList2D)
        {
            // number of triangles = horizontal sections * vertical sections * triangles per polygon
            int[] triangleArray = new int[sections * verticesList2D.Count * 2];

            for (int i = 0; i < spline.Count; i++)
            {
                Vector3 firstVertex = verticesList2D[i][0];
                // TODO add all triangles to the array
            }

            // TODO only for testing
            triangleArray = new int[] {
                0, 1, 1 + sections,
                1 + sections, 0 + sections, 0
            };

            return triangleArray;
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

        // TODO not finished
        private void createPolygon(int[] splineSection, List<int> triangles)
        {
            if (splineSection.Length != 2)
            {
                Debug.Log("createPolygon(): invalid length of spline section Vector");
            }

            createTriangle(splineSection[2], splineSection[1], splineSection[0], triangles);
            createTriangle(splineSection[1], splineSection[2], splineSection[3], triangles);
        }

        // TODO not finished
        private List<Vector3> createTrianglesForLathedMesh()
        {
            List<Vector3> triangleList = new List<Vector3>() { };

            // TODO create all triangles

            return triangleList;
        }

        // TODO not finished
        private List<int> createTriangle(int a, int b, int c, List<int> triangles)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);

            return triangles;
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
        /// 
        /// </summary>
        /// <param name="angle">The angle between the axis and the desired point from origin.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="origin">The origin of the circle.</param>
        /// <returns>A <c>Vector3</c> object of the point with the given angle
        /// between the axis and the desired point from origin of the circle.</returns>
        private Vector3 getPointOnCircle(float angle, float radius, Vector3 origin)
        {
            // x = cx + r * cos(a)
            // y = cy               | same height as the origin of the circle
            // z = cz + r * sin(a)
            return new Vector3(origin.x + (radius * Mathf.Sin(angle)), origin.y, origin.z + (radius * Mathf.Cos(angle)));
        }

        internal void draw()
        {
            throw new NotImplementedException();
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

    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        List<Vector3> spline = new List<Vector3>
        {
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 2),
            new Vector3(0, 2, 2),
            new Vector3(0, 3, 2),
            new Vector3(0, 4, 1),
        };

        lathedObject = new LathedObject(spline, gameObject, sections, material, mesh, meshFilter);
    }

    void Update()
    {
        // redraw the mesh every frame
        // lathedObject.draw();
    }
}
