using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary> 
/// the spline class represents a Vector Array of Points that the spline is made out of
/// </summary>
public class Spline {
    public float pushthreshold;
    public float pushFalloff;

    private Vector3[] spline;

    ///<summary> constructs a vertical spline with a slight variance </summary>
    ///
    ///<param name="radius">distance to the center</param>
    ///<param name="height">height of the spline</param>
    ///<param name="subdivision">subdivision of the spline </param>
    ///<param name="variance">amount of variance in spline-line, default: 0.1f</param>
    public Spline(float radius, float height, int subdivision, float variance = 0.01f)
    {
        float distance = height / subdivision;

        spline = new Vector3[subdivision+2];
        spline[0] = new Vector3(0f, 0f, 0f);
        spline[spline.Length-1] = new Vector3(0f, height, 0f);
        for (int i = 1; i<subdivision+1; i++)
        {
            spline[i] = new Vector3(0f, i*distance, radius+ Random.Range(0, variance*2)-variance);
        }
    }

    /// <summary> 
    /// constructs a spline from a vertex Array
    /// </summary>
    /// <param name = "spline">a vertex arrray with points</param>
    public Spline(Vector3[] spline)
    {
        this.spline = spline;
    }

    ///<summary> 
    /// returns the points of the spline
    /// </summary>
    /// <returns>returns an array of vector3 points</returns>
    public Vector3[] getSpline()
    {
        return spline;
    }

    /// <summary>
    /// returns the Vertex for the given index
    /// </summary>
    /// <param name="index">array index</param>
    /// <returns>returns vertex position</returns>
    public Vector3 getVertex(int index)
    {
        return spline[index];
    }

    /// <summary>
    /// Checks, if given position in in spline
    /// </summary>
    /// <param name="point">point in scene</param>
    /// <returns>distance between spline and given point\nDIST<0 means point is in spline\nDIST=0 means point is on spline\nDIST>0 means point is outside of spline</returns>
    internal float DistanceToPoint(Vector3 point)
    {
        Debug.Log("DistanceToPoint:\tgot Point:"+point.ToString());
        // get corresponding spline vertex
        int vertexIndex = getCorrespondingVertex(point.y);

        // compare vertex with given point
        return Vector3.Distance(new Vector3(0f,point.y, 0f), point) - Vector3.Distance(new Vector3(0f, point.y, 0f), spline[vertexIndex]);

    }

    /// <summary>
    /// deforms clay at given position
    /// </summary>
    /// <param name="point">point to deform</param>
    /// <param name="strength">strength of deformation(e.g. distance of hand in clay)</param>
    internal void PushAtPosition(Vector3 point, float strength, float pushFalloff, float pushThreshold)
    {
        int affectedVertices = (int) Mathf.Floor(spline.Length * pushThreshold);
        if (affectedVertices % 2 == 0)
            affectedVertices += 1;
        int startVertex = getCorrespondingVertex(point.y) - (affectedVertices - 1) / 2;
        int endVertex = getCorrespondingVertex(point.y) + (affectedVertices - 1) / 2 ;

        // generate list with normal-verteilung
        float[] gaussList = new float[affectedVertices];
        //center element is max
        float maxDeform = 0.999f;
        gaussList[(int)Mathf.Floor(affectedVertices / 2) + 1] = maxDeform;
        String gaussvals = "GaussVals: ";
        for (int i=0; i< (affectedVertices-1)/2; i++)
        {
            //sinus
            gaussList[i] = maxDeform +
                (1- maxDeform) * i/affectedVertices;
            gaussList[affectedVertices -1 - i] = gaussList[i];
        }
        for (int i = 0; i < gaussList.Length; i++)
        {
            gaussvals += gaussList[i] + "; ";
        }
        Debug.Log(gaussvals);
        int j = 0;
        for (int i= startVertex; i < endVertex; i++)
        {
            if (i < 0 || i >= spline.Length)
            {
                j++;
                continue;
            }
            // values: strength, pushFalloff, 
            //i-startvertex
            spline[i].z *= gaussList[j];
            j++;
        }
        //spline[getCorrespondingVertex(point.y)].z = Mathf.Max(spline[getCorrespondingVertex(point.y)].z, 0.1f);
        //Debug.Log("PushAtPosition:\tPushing clay at pos:"+ spline[getCorrespondingVertex(point.y)].ToString());
    }

    private int getCorrespondingVertex(float pointheight)
    {
        int vertexIndex = 0;
        for (int i = 0; i < spline.Length; i++)
        {
            if (spline[i].y < pointheight) { 
                vertexIndex = i;
                continue;
            }
            else
            { // first time higher than given point-> break
                vertexIndex = i;
                break;
            }
        }
        return vertexIndex;
    }

    /// <summary>
    /// smoothes spline in the area around given position
    /// </summary>
    /// <param name="tipPosition"></param>
    internal void SmoothAtPosition(Vector3 tipPosition)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// increses spline radius at given position
    /// </summary>
    /// <param name="tipPosition"></param>
    internal void PullAtPosition(Vector3 tipPosition)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// returns array as List
    /// </summary>
    /// <returns></returns>
    internal List<Vector3> getSplineList()
    {
        List<Vector3> tmp = new List<Vector3>(this.spline);
        return tmp;
    }

    /// <summary>
    /// returns the size of the Vector3 array
    /// </summary>
    /// <returns>size of the vector3 array</returns>
    public int getSize()
    {
        return spline.Length;
    }
}
