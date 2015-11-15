using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class UIManager : MonoBehaviour {

    public MeshFilter exportObject;

    /*
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}*/

    public void export()
    {
        Export.exportSTL(exportObject.mesh, "sphere3");
    }

    public void compareSpline()
    {

        //Create Splines for Testing the SplineComparison
        Vector3[] spline1 = new Vector3[5];
        Vector3[] spline2 = new Vector3[5];
        Vector3[] spline3 = new Vector3[5];
        Vector3[] spline4 = new Vector3[5];
        List<float>time = new List<float>();

        for (int i = 0; i < 5; i++)
        {
            spline1[i] = new Vector3(0.0f, i / 5f, Random.Range(0.4f, 0.6f));
            spline2[i] = new Vector3(0.0f, i / 5f, Random.Range(0.4f, 0.6f));
            spline3[i] = new Vector3(0.0f, i / 5f, Random.Range(0.4f, 0.6f));
            spline4[i] = new Vector3(0.0f, i / 5f, Random.Range(0.4f, 0.6f));
            time.Add(Random.Range(4.0f, 8.0f));
        }
        List<Spline> targetSpline = new List<Spline>();
        targetSpline.Add(new Spline(spline1));
        targetSpline.Add(new Spline(spline2));
        targetSpline.Add(new Spline(spline3));

        List<Spline> userSpline = new List<Spline>();
        userSpline.Add(new Spline(spline2));
        userSpline.Add(new Spline(spline3));
        userSpline.Add(new Spline(spline4));

        SplineComparison.compare(targetSpline, userSpline, time, "Thereza");
        
    }
}
