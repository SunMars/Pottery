using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// This script created a lathed object from a curve/spline.
/// </summary>
[ExecuteInEditMode]
public class Lathe : MonoBehaviour
{
    public Material material;
    public LathedObject test;
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

        // ---------------------------

        // protected vars ------------
        protected List<Vector3> spline;
        protected GameObject gameObject;
        protected int sections;
        protected Material mat;

        // private vars --------------
        // needed for mesh creating
        private List<Vector3> vertices;
        private List<Vector2> uv;
        private int[] triangles;
        // ---------------------------
        //////////////////////////////

        /// <summary>
        /// Main contructor.
        /// </summary>
        /// <param name="spline">A spline as an array of <c>Vector3</c> elements</param>
        /// <param name="gameObject">The empty <c>GameObject</c> on which the lathe mesh will be drawn</param>
        public LathedObject(List<Vector3> spline, GameObject gameObject, int sections, Material mat, Mesh mesh, MeshFilter meshFilter)
        {
            this.spline = spline;
            this.gameObject = gameObject;
            this.sections = sections;
            this.mat = mat;

            // make sure there are minimum 3 sections
            if (this.sections > 3)
            {
                this.sections = 3;
            }

            Renderer renderer = gameObject.GetComponent<MeshRenderer>();

            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles;

            renderer.material = mat;
        }

        /// <summary>
        /// Standard contructor. (Only for testing)
        /// </summary>
        public LathedObject(GameObject gameObject, int sections, Material mat, Mesh mesh, MeshFilter meshFilter)
        {
            spline = new List<Vector3>
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 2),
                new Vector3(0, 2, 2),
                new Vector3(0, 3, 2),
                new Vector3(0, 4, 1),
            };

            // new LathedObject(spline, gameObject, sections, mat, mesh, meshFilter);

            // -------------------------
            vertices = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 1)
            };

            uv = new List<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(0, 1)
            };

            triangles = new int[]
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

        /// <summary>
        /// Get a computed list of all vertices for a given number of segments on a circle with (0, 0, 0) as origin.
        /// </summary>
        /// <param name="vertex">The vertex on angle 0 from the spline.</param>
        /// <param name="segments">The number of segments the lathed object should be generated</param>
        /// <returns>A list of all vertices on a circle in the same height as the given vertex.</returns>
        private List<Vector3> getVerticesOnCircle(Vector3 vertex, int segments)
        {
            float radius = Vector3.Distance(vertex, Vector3.up);
            float segmentAngle = getAngleForNumberOfSections(segments);
            Vector3 circleOrigin = new Vector3(0, vertex.y, 0);

            List<Vector3> verticesOnCircle = new List<Vector3>{ vertex };

            for (float angle = 0f; angle < 2 * Mathf.PI; angle += segmentAngle)
            {
                verticesOnCircle.Add(getPointOnCircle(angle, radius, circleOrigin));
            }

            return verticesOnCircle;
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

        private void createPolygon(int[] splineSection, List<int> triangles)
        {
            if (splineSection.Length != 2)
            {
                Debug.Log("createPolygon(): invalid length of spline section Vector");
            }

            createTriangle(splineSection[2], splineSection[1], splineSection[0], triangles);
            createTriangle(splineSection[1], splineSection[2], splineSection[3], triangles);
        }

        private void createTriangle(int a, int b, int c, List<int> triangles)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
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

        private float getRadiusOfSection(int sections)
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
            return new Vector3(origin.x + (radius * Mathf.Cos(angle)), origin.y, origin.z + (radius * Mathf.Sin(angle)));
        }

        internal void draw()
        {
            throw new NotImplementedException();
        }
    }


    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        test = new LathedObject(gameObject, sections, material, mesh, meshFilter);
    }

    void Update()
    {
        // redraw the mesh every frame
        // test.draw();
    }
}
