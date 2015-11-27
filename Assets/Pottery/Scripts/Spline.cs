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
    public const bool UsePercentageHeight = false;
    public const bool UseAbsolutegeHeight = true;
    public const float minRadius = 0.03f;
    public const float maxRadius = 0.8f;

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
                this.SmoothAtPosition(spline[i], 0.5f, 0.2f, delegate (float input) { return Mathf.Sin(input); });
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
        // get corresponding spline vertex
        int vertexIndex;
        if(point.y > spline[spline.Length-1].y && point.y < 1.5f * spline[spline.Length - 1].y)
        {
            vertexIndex = spline.Length - 2;
            // hoovering close over clay object
            return Vector3.Distance(new Vector3(0f, point.y, 0f), point) - Vector3.Distance(new Vector3(0f, spline[vertexIndex].y, 0f), spline[vertexIndex]);
        }
        if (point.y < spline[0].y && point.y > -0.2f)
        {
            vertexIndex = 1;
            // hoovering close over clay object
            return Vector3.Distance(new Vector3(0f, point.y, 0f), point) - Vector3.Distance(new Vector3(0f, spline[vertexIndex].y, 0f), spline[vertexIndex]);
        }
        vertexIndex = getCorrespondingVertex(point.y);
        // compare vertex with given point
        return Vector3.Distance(new Vector3(0f, point.y, 0f), point) - Vector3.Distance(new Vector3(0f, point.y, 0f), spline[vertexIndex]);
    }

    /// <summary>
    /// pushes into clay at given position. affects nearby vertices as well
    /// </summary>
    /// <param name="position">point of maximum deformation</param>
    /// <param name="strength">distance of hand to clayegde</param>
    /// <param name="effectStrength">how strongly nearby vertices are affected (1.0 = sinus, 0.5 = half sinus, 0 = no smoothing)</param>
    /// <param name="affectedArea">Percantage of affectedVertices (1.0 = 100% of all vertices)</param>
    internal void PushAtPosition(Vector3 position, float strength, float effectStrength, float affectedArea, Func<float, float> deformFunc, bool absoluteHeight = Spline.UsePercentageHeight)
    {
        this.deformAtPosition(false, position, strength, effectStrength, affectedArea, deformFunc, absoluteHeight);
    }

    /// <summary>
    /// increses spline radius at given position
    /// </summary>
    /// <param name="position"> Point of maximal Pull</param>
    /// <param name="effectStrength">how strongly nearby vertices are affected (1.0 = sinus, 0.5 = half sinus, 0 = no smoothing)</param>
    /// <param name="affectedArea">Percantage of affectedVertices (1.0 = 100% of all vertices)</param>
    internal void PullAtPosition(Vector3 position, float effectStrength, float affectedArea, Func<float, float> deformFunc, bool absoluteHeight = Spline.UsePercentageHeight)
    {
        // todo: set strength of pull via gesture, fornow: hardcoded
        float strength = 0.005f;
        this.deformAtPosition(true, position, strength, effectStrength, affectedArea, deformFunc, absoluteHeight);
    }

    /// <summary>
    /// smoothes spline in the area around given position
    /// </summary>
    /// <param name="tipPosition"></param>
    /// <param name="effectStrength">how strong smooth effect is (1.0 = sinus, 0.5 = half sinus, 0 = no smoothing)</param>
    /// <param name="affectedArea">Percantage of affectedVertices (1.0 = 100% of all vertices)</param>
    internal void SmoothAtPosition(Vector3 position, float effectStrength, float affectedArea, Func<float, float> smoothProfileFunc)
    {
        // calculate number of affected vertices,first and last affected vertex
        float affectedVertices = (int)Mathf.Floor(spline.Length * affectedArea);
        if (affectedVertices % 2 == 0)
            affectedVertices += 1;
        int startVertex = getCorrespondingVertex(position.y) - ((int)affectedVertices - 1) / 2;
        int endVertex = getCorrespondingVertex(position.y) + ((int)affectedVertices - 1) / 2;

        // generate a smoothprofile
        float[] smoothprofile = new float[(int)affectedVertices];
        smoothprofile[(int)Mathf.Floor(affectedVertices / 2) + 1] = 1f;
        for (int i = 0; i < (affectedVertices - 1) / 2; i++)
        {
            smoothprofile[i] = smoothProfileFunc(i / affectedVertices) * effectStrength;
            smoothprofile[(int)affectedVertices - 1 - i] = smoothprofile[i];
        }

        //accumulate surounding spline-vertices
        float accuValues = 0;
        float accuCount = 0; //extra value, if smooth near object borders, endVertex-startVertex does not work
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

        //todo apply effect on nearby vertices as well
        // apply new smoothed value
        spline[getCorrespondingVertex(position.y)].z = accuValues / accuCount;
    }

    /// <summary>
    /// smoothes affecedArea, areaCenter is at the center (duh.)
    /// uses function, where input=0 is max
    /// </summary>
    /// <param name="areaCenter"></param>
    /// <param name="effectStrength"></param>
    /// <param name="affectedArea"></param>
    /// <param name="smoothProfileFunc"></param>
    /// <param name="absoluteHeight"></param>
    internal void SmoothArea(Vector3 areaCenter, float effectStrength, float affectedArea, float vertexSmoothRange, Func<float, float> smoothProfileFunc, bool absoluteHeight = Spline.UsePercentageHeight)
    {
        // calculate number of affected vertices,first and last affected vertex
        float affectedVertices;
        int startVertex, endVertex;
        if (absoluteHeight)
        {
            startVertex = setInRange(1,getCorrespondingVertex(areaCenter.y - affectedArea / 2f), spline.Length-1);
            endVertex = setInRange(1, getCorrespondingVertex(areaCenter.y + affectedArea / 2f), spline.Length - 1);

            affectedVertices = endVertex - startVertex;
        }
        else
        {
            affectedVertices = (int)Mathf.Floor(spline.Length * affectedArea);
            if (affectedVertices % 2 == 0)
                affectedVertices += 1;
            startVertex = setInRange(1, getCorrespondingVertex(areaCenter.y) - ((int)affectedVertices - 1) / 2, spline.Length - 1);
            endVertex = setInRange(1, getCorrespondingVertex(areaCenter.y) + ((int)affectedVertices - 1) / 2, spline.Length - 1);
        }

        // generate a smoothprofile
        float[] smoothprofile = new float[(int)affectedVertices];
        int tmp = (int)Mathf.Floor(affectedVertices / 2); 
        for (int i = 0; i < (int)Mathf.Floor(affectedVertices / 2); i++)
        {
            //start at center
            smoothprofile[tmp + i] = smoothProfileFunc(i / affectedVertices);
            smoothprofile[tmp - i] = smoothprofile[tmp + i];
        }


        Vector3[] newSplinePart = new Vector3[(int)affectedVertices];

        //calculate new spline part
        for(int i =0;i< (int)affectedVertices; i++)
        {
            if (startVertex + i < 1 || startVertex + i > spline.Length - 2){
                newSplinePart[i] = new Vector3();
                continue;
            }
            newSplinePart[i] = getSmoothedVertex(startVertex + i, vertexSmoothRange, smoothProfileFunc);
            float dif = newSplinePart[i].z - spline[startVertex + i].z;
            // old point +- fraction of dif between old and new point
            newSplinePart[i].z = spline[startVertex + i].z + dif * smoothprofile[i]*effectStrength;
        }

        // apply effect to spline
        // in new for-loop, so that calculation does not get effected by new values
        for (int i= startVertex; i < endVertex;i++)
        {
            if(i < 1) // error prevention
            {
                continue;
            }
            spline[i] = newSplinePart[i - startVertex];
        }
    }

    /// <summary>
    /// returns value if min < value < max
    /// otherwise returs min or max
    /// </summary>
    /// <param name="min"></param>
    /// <param name="value"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    private int setInRange(int min, int value, int max)
    {
        int res = Mathf.Max(min, value);
        res = Mathf.Min(value, max);
        return res;
    }

    private Vector3 getSmoothedVertex(int vertexIndex, float affectedVertices, Func<float, float> smoothProfileFunc)
    {
        //float affectedVertices = spline.Length * 0.1f; 
        if (affectedVertices % 2 == 0)
            affectedVertices += 1;
        // generate a smoothprofile
        float[] smoothprofile = new float[(int)affectedVertices];
        int tmp = (int)Mathf.Floor(affectedVertices / 2);
        for (int i = 0; i < (int)Mathf.Floor(affectedVertices / 2); i++)
        {
            smoothprofile[tmp + i] = smoothProfileFunc(i / affectedVertices);
            smoothprofile[tmp - i] = smoothprofile[tmp + i];
        }

        //accumulate surounding spline-vertices
        float accuValues = 0;
        float accuCount = 0; //extra value, if smooth near object borders, endVertex-startVertex does not work
        int startVertex = vertexIndex - (int)(affectedVertices - 1) / 2;
        //for (int i = startVertex; i < vertexIndex + (int)(affectedVertices+1) / 2; i++)
        for (int i = 0; i < affectedVertices; i++)
        {
            //avoid vertices with radius 0
            if (i+ startVertex < 1 || i+ startVertex > spline.Length - 1)
            {
                continue;
            }
            
            accuValues += spline[i+startVertex].z * smoothprofile[i];
            accuCount += smoothprofile[i];
            /*
            accuValues += spline[i+startVertex].z ;
            accuCount += 1;*/
            
        }

        //todo apply effect on nearby vertices as well
        // apply new smoothed value
        return new Vector3(spline[vertexIndex].x, spline[vertexIndex].y, accuValues / accuCount);
    }

    /// <summary>
    /// scales a value with vMin and vMax to new Min and Max
    /// </summary>
    /// <param name="valueIn"></param>
    /// <param name="baseMin"></param>
    /// <param name="baseMax"></param>
    /// <param name="limitMin"></param>
    /// <param name="limitMax"></param>
    /// <returns></returns>
    public float scale(float valueIn, float baseMin, float baseMax, float limitMin, float limitMax)
    {
        return ((limitMax - limitMin) * (valueIn - baseMin) / (baseMax - baseMin)) + limitMin;
    }

    /// <summary>
    /// private deformation class, used bei internal classes
    /// </summary>
    /// <param name="pull">true=pull, false = push</param>
    /// <param name="position"></param>
    /// <param name="strength"></param>
    /// <param name="effectStrength"></param>
    /// <param name="affectedArea"></param>
    /// <param name="deformFunc"></param>
    private void deformAtPosition(bool pull, Vector3 position, float strength, float effectStrength, float affectedArea, Func<float, float> deformFunc, bool absoluteHeight)
    {
        // check deform strenght against min & max
        float strengthOfDeformation = Mathf.Min(0.01f, Mathf.Abs(strength));
        strengthOfDeformation = Mathf.Max(0.0001f, strengthOfDeformation);

        // calculate number of affected vertices,first and last affected vertex
        float affectedVertices;
        int startVertex, endVertex;
        if (absoluteHeight) {
            startVertex = getCorrespondingVertex(position.y - affectedArea / 2f);
            endVertex = getCorrespondingVertex(position.y + affectedArea / 2f);
            affectedVertices = endVertex - startVertex;
        }
        else { 
            affectedVertices = (int)Mathf.Floor(spline.Length * affectedArea);
            if (affectedVertices % 2 == 0)
                affectedVertices += 1;
            startVertex = getCorrespondingVertex(position.y) - ((int)affectedVertices - 1) / 2;
            endVertex = getCorrespondingVertex(position.y) + ((int)affectedVertices - 1) / 2;
        }

        float[] deformFactors = new float[(int)affectedVertices];
        int tmp = (int)Mathf.Floor(affectedVertices / 2);
        for (int i = 0; i < (int)Mathf.Floor(affectedVertices / 2); i++)
        {
            //start at center
            deformFactors[tmp + i] = deformFunc(scale(i, 0, (affectedVertices+2)/4f, 0, 1)) * effectStrength;
            deformFactors[tmp - i] = deformFactors[tmp + i];
        }

        // apply deformation to spline
        for (int i = startVertex; i < endVertex; i++)
        {
            if (i < 0 || i >= spline.Length)
            {
                continue;
            }
            float sign = 1f;
            if (!pull)
            {
                sign = -1f; 
            }

            
            if (spline[i].z > minRadius * 1.5f && spline[i].z < maxRadius * 0.9f  ) {
                //Debug.Log("deform abs");

                spline[i].z += sign*strengthOfDeformation * deformFactors[i - startVertex];
            }
            else
            {
                //Debug.Log("deform percentage");

                spline[i].z += sign*spline[i].z * strengthOfDeformation * deformFactors[i - startVertex] * 3f;
            }

            spline[i].z = Mathf.Min(spline[i].z, maxRadius);
            spline[i].z = Mathf.Max(spline[i].z, minRadius);
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
