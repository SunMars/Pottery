using UnityEngine;
using System.Collections;

public class createMeshFromVertices : MonoBehaviour {

    public float radius;
    public GameObject inputSpline;

    private Vector3[] newVertices;
    private Vector2[] newUV;
    private int[] newTriangles;

    void Start() {

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

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
    }

    // Update is called once per frame
    void Update () {
	
	}
}
