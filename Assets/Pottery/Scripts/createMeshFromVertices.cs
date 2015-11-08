using UnityEngine;
using System.Collections;

public class createMeshFromVertices : MonoBehaviour {

    public float radius;
    public Material mat;

    private Vector3[] spline;

    private Vector3[] newVertices;
    private Vector2[] newUV;
    private int[] newTriangles;

    void Start() {

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        Renderer renderer = gameObject.GetComponent<MeshRenderer>();

        spline = new []
        {
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 2),
            new Vector3(0, 2, 2),
            new Vector3(0, 3, 2),
            new Vector3(0, 4, 1),
        };

        newVertices = new []
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 0),
            new Vector3(0, 1, 1)
        };

        newUV = new []
        {
            new Vector2(0, 0),
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(0, 1)
        };

        newTriangles = new[]
        {
            2, 1, 0,
            1, 2, 3
        };

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.triangles = newTriangles;

        renderer.material = mat;
    }

    // Update is called once per frame
    void Update () {
	
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spline"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    Vector3[] createMeshVertices (Vector3[] spline)
    {
        Vector3[] meshVertices = new Vector3[spline.Length - 1];

        foreach(Vector3 vert in spline)
        {
            //meshVertices.
        }

        return new Vector3[] { };
    }
}
