using UnityEngine;
using System.Collections;

/// <summary> 
/// the spline class represents a Vector Array of Points that the spline is made out of
/// </summary>
public class Spline {

    private Vector3[] spline;

    ///<summary> constructs a straight spline </summary>
    ///
    ///<param name="radius">distance to the center</param>
    ///<param name="height">height of the spline</param>
    ///<param name="subdivision">subdivision of the spline </param>
    public Spline(float radius, float height, int subdivision)
    {
        float distance = height / subdivision;

        spline = new Vector3[subdivision +1];
        for(int i = 0; i<=subdivision; i++)
        {
            spline[i] = new Vector3(0f, i*distance, radius);
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
        return new Vector3[0];
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
    /// returns the size of the Vector3 array
    /// </summary>
    /// <returns>size of the vector3 array</returns>
    public int getSize()
    {
        return spline.Length;
    }
}
