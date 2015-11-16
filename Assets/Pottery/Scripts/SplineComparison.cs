using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

/// <summary>
/// Static class for SplineComparison. Gives a method to compare two Lists of Splines.
/// </summary>
public static class SplineComparison {

    /// <summary>
    /// Use this method to compare to Lists of splines with each other. The result will be saved to a csv file in the "Documents/Pottery" Folger.
    /// For every Spline of the user a difference to the original spline is calculated, the time given and the points of the spline exported.
    /// </summary>
    /// <param name="targetSpline">a List of the target splines </param>
    /// <param name="userSpline">a List of the splines the user created after the image of the target splines</param>
    /// <param name="time">a List of times the user took to finish each spline</param>
    /// <param name="name">name or ID of the User</param>
	public static void compare(List<Spline> targetSpline, List<Spline> userSpline, List<float> time, string name)
    {
        //check if there is a Folder for Pottery in Documents
        //if not create a new Folder 
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Pottery/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        //create File
        StreamWriter streamWriter = File.CreateText(path + name + ".csv");

        //Write first column of the csv
        streamWriter.Write("TARGETSHAPE;TIME;DIFFERENCE");
        int numVertices = targetSpline[0].getSize();
        for (int i = 0; i<numVertices; i++)
        {
            streamWriter.Write(";VERTEX: " + i);
        }
        streamWriter.WriteLine(";;;");

        //Write the information to the csv for every shape
        //TO DO: depends on the input spline - perhabs which points are compared
        for (int i = 0;  i < targetSpline.Count; i++)
        {
            Vector3[] target = targetSpline[i].getSpline();
            Vector3[] user = userSpline[i].getSpline();
            float difference = 0.0f;
            //calculate difference of targetshape and usershape
            for (int j = 0; j < target.Length; j++)
            {
                difference += Mathf.Abs(target[j].z - user[j].z);
            }

            //write to the csv file
            streamWriter.Write(i +";"+ time[i] +";" + difference);
            for (int j = 0; j < user.Length; j++)
            {
                streamWriter.Write("; z:" + user[j].z + " y:" + user[j].y);
            }
            streamWriter.WriteLine(";;;");
        }
        //Close File
        streamWriter.Close();
        Debug.Log("Comparison of Splines is saved to: " + name + ".csv");
    }
}
