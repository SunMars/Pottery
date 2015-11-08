using UnityEngine;
using System.Collections;

public class MeshLathe {

    private Vector3 spline;
    private GameObject go;
    private int sections;
    private float radius;

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

    // TODO rename
    private float getUmfang(float radius)
    {

        return 9f;
    }
}
