using UnityEngine;
using System.IO;
using System;


/// <summary>
/// Static class for methods to export a mesh.
/// </summary>
public static class Export {

    /// <summary>
    /// method to export a mesh to the stl format.
    /// </summary>
    /// <param name="mesh">mesh to export</param>
    /// <param name="name">name of the object</param>
    public static void exportSTL(Mesh mesh, string name)
    {
        //check if there is a Folder for Pottery in Documents
        //if not create a new Folder
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Pottery/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        //create file
        StreamWriter streamWriter = File.CreateText(path + name + ".stl");

        //Write the File
        streamWriter.WriteLine("solid " + name);
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        //Write the triangles to the file
        for (int i= 0; i< triangles.Length; i+=3)
        {
            // Calulate Triangle Normal from vertices --> Crossproduct of two triangle edges
            Vector3 triEdge1 = vertices[triangles[i + 1]] - vertices[triangles[i]];
            Vector3 triEdge2 = vertices[triangles[i + 2]] - vertices[triangles[i]];
            Vector3 normal = Vector3.Cross(triEdge1, triEdge2).normalized;

            // Write the normal of the triangle to the file
            streamWriter.WriteLine("  facet normal " + normal.x + " " + normal.y + " " + normal.z);

            // Write the 3 vertices to the file
            streamWriter.WriteLine("    outer loop");
            streamWriter.WriteLine("      vertex " + vertices[triangles[i]].x + " "+ vertices[triangles[i]].y +" "+ vertices[triangles[i]].z);
            streamWriter.WriteLine("      vertex " + vertices[triangles[i+1]].x + " " + vertices[triangles[i+1]].y + " " + vertices[triangles[i+1]].z);
            streamWriter.WriteLine("      vertex " + vertices[triangles[i+2]].x + " " + vertices[triangles[i+2]].y + " " + vertices[triangles[i+2]].z);
            streamWriter.WriteLine("    endloop");
            streamWriter.WriteLine("  endfacet");
        }
        streamWriter.WriteLine("endsolid " + name);
        //Close File
        streamWriter.Close();
        Debug.Log("STL Export finished");
    }
}
