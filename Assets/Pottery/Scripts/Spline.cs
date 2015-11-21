using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary> 
/// the spline class represents a Vector Array of Points that the spline is made out of
/// </summary>
public class Spline
{
    //public float pushthreshold;
    //public float pushFalloff;

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

        spline = new Vector3[subdivision + 2];
        spline[0] = new Vector3(0f, 0f, 0f);
        spline[spline.Length - 1] = new Vector3(0f, height, 0f);
        for (int i = 1; i < subdivision + 1; i++)
        {
            spline[i] = new Vector3(0f, i * distance, radius + Random.Range(0, variance * 2) - variance);
        }

        // smooth sline with variance
        if(variance > 0f)
        {
            for(int i =0; i < spline.Length - 1; i++)
            {
                this.SmoothAtPosition(spline[i], 0.5f, 0.2f);
            }
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
        //Debug.Log("DistanceToPoint:\tgot Point:"+point.ToString());
        // get corresponding spline vertex
        int vertexIndex = getCorrespondingVertex(point.y);

        // compare vertex with given point
        return Vector3.Distance(new Vector3(0f, point.y, 0f), point) - Vector3.Distance(new Vector3(0f, point.y, 0f), spline[vertexIndex]);

    }

    /// <summary>
    /// pushes into clay at given position. affects nearby vertices as well
    /// </summary>
    /// <param name="position">point of maximum deformation</param>

    /// <param name="effectStrength">how strongly nearby vertices are affected (1.0 = sinus, 0.5 = half sinus, 0 = no smoothing)</param>
    /// <param name="affectedHeight">Height around position that is affected by the push effect</param>
    /// <param name="maximumHeight">sets the size of the maximum of the push curve, should never be bigger than affectedHeight </param>
    /// <param name="deformFunc">a method that calculates the look of the pushing curve</param>
    internal void PushAtPosition(Vector3 position, float effectStrength, float affectedHeight, float maximumHeight, Func<float, float> deformFunc)
    {
        float maxDeform = 0.001f;
        int startVertex = getCorrespondingVertex(position.y - affectedHeight / 2);
        int endVertex = getCorrespondingVertex(position.y + affectedHeight / 2);

        int maxStartVertex = getCorrespondingVertex(position.y - maximumHeight / 2);
        int maxEndVertex = getCorrespondingVertex(position.y + maximumHeight / 2);

        int affectedVertices = endVertex - startVertex;
        int affectedMaximumVertices = maxEndVertex - maxStartVertex;

        float piStep = 1f / (affectedVertices - affectedMaximumVertices) * (float)Math.PI;

        for (int i = 0; i <= affectedVertices; i++)
        {
            float angle;

            if (startVertex + i > 0 || startVertex + i < spline.Length - 1)
            {
                if (startVertex + i >= maxStartVertex && startVertex + i <= maxEndVertex)
                {
                    angle = Mathf.PI / 2;
                }
                else if (startVertex + i < maxStartVertex)
                {
                    angle = i * piStep;
                }
                else
                {
                    angle = (i - affectedMaximumVertices) * piStep;
                }

                spline[startVertex + i].z = Mathf.Max(spline[startVertex + i].z - effectStrength * maxDeform * deformFunc(angle), 0.1f);

            }
        }
    }

    /// <summary>
    /// smoothes spline in the area around given position
    /// </summary>
    /// <param name="tipPosition"></param>
    /// <param name="effectStrength">how strong smooth effect is (1.0 = sinus, 0.5 = half sinus, 0 = no smoothing)</param>
    /// <param name="affectedArea">Percantage of affectedVertices (1.0 = 100% of all vertices)</param>
    internal void SmoothAtPosition(Vector3 position, float effectStrength, float affectedArea)
    {
        float affectedVertices = (int)Mathf.Floor(spline.Length * affectedArea);
        if (affectedVertices % 2 == 0)
            affectedVertices += 1;
        int startVertex = getCorrespondingVertex(position.y) - ((int)affectedVertices - 1) / 2;
        int endVertex = getCorrespondingVertex(position.y) + ((int)affectedVertices - 1) / 2;

        float[] smoothprofile = new float[(int)affectedVertices];
        smoothprofile[(int)Mathf.Floor(affectedVertices / 2) + 1] = 1f;
        for (int i = 0; i < (affectedVertices - 1) / 2; i++)
        {
            smoothprofile[i] = Mathf.Sin(i / affectedVertices) * effectStrength;
            smoothprofile[(int)affectedVertices - 1 - i] = smoothprofile[i];
        }
        //accumulate surounding spline-vertices
        float accuValues = 0;
        float accuCount = 0;
        for (int i = startVertex; i < endVertex; i++)
        {
            //avoid vertices with radius 0
            if (i < 1 || i > spline.Length - 1)
            {
                continue;
            }
            accuValues += spline[i].z * smoothprofile[i - startVertex];
            accuCount += smoothprofile[i - startVertex];
        }

        //set new smoothed value
        spline[getCorrespondingVertex(position.y)].z = accuValues / accuCount;
    }

    /// <summary>
    /// increses spline radius at given position
    /// </summary>
    /// <param name="position"> Point of maximal Pull</param>
    /// <param name="effectStrength">how strongly nearby vertices are affected</param>
    /// <param name="affectedHeight">Height as float that is affected by the pull effect</param>
    /// <param name="maximumHeight">sets the size of the maximum of the pull curve, should never be bigger than affectedHeight </param>
    /// <param name="deformFunc">a method that calculates the look of the pulling curve</param>
    internal void PullAtPosition(Vector3 position, float effectStrength, float affectedHeight, float maximumHeight, Func<float, float> deformFunc)
    {
        float maxDeform = 0.001f;
        int startVertex = getCorrespondingVertex(position.y - affectedHeight/2);
        int endVertex = getCorrespondingVertex(position.y + affectedHeight/2);

        int maxStartVertex = getCorrespondingVertex(position.y - maximumHeight / 2);
        int maxEndVertex = getCorrespondingVertex(position.y + maximumHeight / 2);

        int affectedVertices = endVertex - startVertex;
        int affectedMaximumVertices = maxEndVertex - maxStartVertex;

        float piStep = 1f / (affectedVertices-affectedMaximumVertices) * (float)Math.PI;

        for(int i = 0; i <= affectedVertices; i++)
        {
            float angle;
            
            if (startVertex + i > 0||startVertex + i < spline.Length-1)
            {    
                if(startVertex + i >= maxStartVertex && startVertex + i <= maxEndVertex)
                {
                    angle = Mathf.PI / 2;
                }else if (startVertex + i < maxStartVertex)
                {
                    angle = i * piStep;
                }else
                {
                    angle = (i - affectedMaximumVertices) * piStep;
                }

                spline[startVertex + i].z = Mathf.Min(spline[startVertex + i].z + effectStrength * maxDeform * deformFunc(angle), 0.8f);
                
            }
        }
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


    private int getCorrespondingVertex(float pointheight)
    {
        int vertexIndex = 0;
        for (int i = 0; i < spline.Length; i++)
        {
            if (spline[i].y < pointheight)
            {
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
}
