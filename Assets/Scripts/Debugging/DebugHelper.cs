using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

/// <summary>
/// This class shall help for the visual debugging process
/// </summary>
[System.Serializable]
public class DebugHelper{

  private static DebugHelper instance = null;
   
  /// <summary>
  /// Some variables to specifiy if different debugging values shall be executed
  /// </summary>
  static bool drawRays = false;
  static bool drawLines = false;
  static bool debugLog = false;
  static bool debugError = false;

  // List for all lines which can be drawn on screen.
  public List<DebugLine> debugLines;

  // List of lines which represent a mesh which is connected to a string.
  public Dictionary<string, List<DebugLine>> namedMeshes;

  // List of all lines connected to a string
  public Dictionary<string, DebugLine> namedLines;

  //storing the names which are used yet for the meshes
  private HashSet<string> usedMeshNames;

  //storing the names of lines which are used yet
  private HashSet<string> usedLineNames;

  // A gameobject which will be used to mark points
  static GameObject marker;

  // A counter to count all evisting marker-objects
  static int spherePointCounter = 0;

  
  /// <summary>
  /// Constructor
  /// </summary>
  private DebugHelper()
  {
    namedMeshes = new Dictionary<string, List<DebugLine>>();

    namedLines = new Dictionary<string, DebugLine>();

    usedMeshNames = new HashSet<string>();

    usedLineNames = new HashSet<string>();
  }

  /// <summary>
  /// Some kind of singleton-implementation.
  /// Not thread-save
  /// </summary>
  /// <returns>the debughelper-instance</returns>
  public static DebugHelper getInstance()
  {
    if (instance == null)
    {
      instance = new DebugHelper();
    }

    return instance;
  }

  /// <summary>
  /// Draws a mesh on the screen
  /// </summary>
  /// <param name="debugname">a name to identify the mesh</param>
  /// <param name="triangles">indices which are a reference to vertices ins the vertices-list</param>
  /// <param name="vertices">the mesh-vertices</param>
  /// <param name="color">The mesh-color</param>
  public void DrawMesh(string debugname, int[] triangles, Vector3[] vertices, Color color)
  {
    String name = GetUnusedName(debugname, usedMeshNames);
    namedMeshes.Add(name, CreateDrawableLineMesh(vertices, triangles, color));
  }

  /// <summary>
  /// Draws a mesh on the screen
  /// </summary>
  /// <param name="debugname">a name to identify the mesh</param>
  /// <param name="triangles">indices which are a reference to vertices ins the vertices-list</param>
  /// <param name="vertices">the mesh-vertices</param>
  /// <param name="transform">a transform obect to be able to transform the meshpoints.</param>
  /// <param name="color">The mesh-color</param>
  public void DrawMesh(string debugname, int[] triangles, Vector3[] vertices, Transform transform, Color color)
  {
    String name = GetUnusedName(debugname, usedMeshNames);
    namedMeshes.Add(name, CreateDrawableLineMesh(vertices, triangles, color, transform));
  }

  /// <summary>
  /// Draws a mesh on the screen
  /// </summary>
  /// <param name="debugname">a name to identify the mesh</param>
  /// <param name="vertices">the mesh-vertices. Three points after another will be interpreted as one triangle.</param>
  /// <param name="transform">a transform obect to be able to transform the meshpoints.</param>
  /// <param name="color">The mesh-color</param>
  public void DrawMesh(string debugname, Vector3[] vertices, Color color, Transform transform = null)
  {  
    int[] triangles = new int[vertices.Length];
    for (int i = 0; i < vertices.Length; i++)
    {
      triangles[i] = i;
    }
    DrawMesh(debugname, triangles, vertices, transform, color);
  }

  /// <summary>
  /// Storing a Line-object in the line-dictionary.
  /// </summary>
  /// <param name="debugname">a name to identify the line</param>
  /// <param name="startpoint">the line-startpoint</param>
  /// <param name="endpoint">the line-endpoint</param>
  /// <param name="color">the line color</param>
  public void DrawLine(string debugname, Vector3 startpoint, Vector3 endpoint, Color color)
  {
    String name = GetUnusedName(debugname, usedLineNames);
    
    DebugLine debugLine = new DebugLine(startpoint, endpoint);
    debugLine.LineColor = color;
    namedLines.Add(name, debugLine);
  }

  /// <summary>
  /// Creating a list of drawable lines out of some vertices and their indices.
  /// </summary>
  /// <param name="vertices">The points of the mesh</param>
  /// <param name="triangles">The indices which define the point order</param>
  /// <param name="color">The color of the mesh</param>
  /// <param name="transform">a transform object, which can be unsed to transform the mesh-points.Optional!</param>
  /// <returns>A list of DebugLines which build up the mesh.</returns>
  private List<DebugLine> CreateDrawableLineMesh(Vector3[] vertices, int[] triangles, Color color, Transform transform = null)
  {
    List<DebugLine> drawableLineMesh = new List<DebugLine>();
    int trianglePointIndex1;
    int trianglePointIndex2;
    int trianglePointIndex3;

    Vector3 point1;
    Vector3 point2;
    Vector3 point3;
    for (int i = 0; i < triangles.Length - Destructor.triangleVertexCount; i += Destructor.triangleVertexCount)
    {

      // Getting the meshpoint and transform than if should
      trianglePointIndex1 = triangles[i];
      trianglePointIndex2 = triangles[i + 1];
      trianglePointIndex3 = triangles[i + 2];

      if (transform != null)
      {
        point1 = transform.TransformPoint(vertices[trianglePointIndex1]);
        point2 = transform.TransformPoint(vertices[trianglePointIndex2]);
        point3 = transform.TransformPoint(vertices[trianglePointIndex3]);
      }
      else
      {
        point1 = vertices[trianglePointIndex1];
        point2 = vertices[trianglePointIndex2];
        point3 = vertices[trianglePointIndex3];
      }

      // Creating three lines which create the triangle specified by the mesh-points
      DebugLine line1 = new DebugLine(point1, point2);
      DebugLine line2 = new DebugLine(point2, point3);
      DebugLine line3 = new DebugLine(point3, point1);

      // Setting the color and adding the lines to the line list the debug mesh consists of.
      line1.LineColor = color;
      line2.LineColor = color;
      line3.LineColor = color;
      drawableLineMesh.Add(line1);
      drawableLineMesh.Add(line2);
      drawableLineMesh.Add(line3);
    }

    return drawableLineMesh;
  }

  /// <summary>
  /// Getting a string which is not used jet in the specified collection.
  /// </summary>
  /// <param name="wantedName">A string the result shall be similar to.</param>
  /// <param name="collection">The collection in which the name must not exist yet.</param>
  /// <returns>a string which does not exist in the collection yet</returns>
  private string GetUnusedName(string wantedName, HashSet<string> collection)
  {
    String newName  = wantedName;
    int usedCounter = 0;
    while (collection.Contains(newName))
    {
      if (collection.Contains(newName))
      {
        newName = wantedName + usedCounter;
        usedCounter++;
      }
    }
    collection.Add(newName);

    return newName;
  }

  /// <summary>
  /// THis method logs a string of collection-elements to the console
  /// </summary>
  /// <typeparam name="T">The array-type</typeparam>
  /// <param name="array">an array which elements shall be printed to the console</param>
  /// <param name="logEvenIfLoggingDeactivated">shall be logged even if logging is deactivated</param>
  public void PrintArrayElements<T>(T[] array, bool logEvenIfLoggingDeactivated = false)
  {
    if (debugLog || logEvenIfLoggingDeactivated)
    {

      string arrayString = BuildArrayString<T>(array);

      Debug.Log(arrayString);
    }
  }

  /// <summary>
  /// Building a comma-seperated string out of the array
  /// </summary>
  /// <typeparam name="T">The array-type.</typeparam>
  /// <param name="array">The array with the elements to print.</param>
  /// <returns>A comma seperated string, consisting of the array element strings.</returns>
  public string BuildArrayString<T>(T[] array)
  {
    StringBuilder builder = new StringBuilder();

    builder.Append("[");
    foreach (T elem in array)
    {
      builder.Append(elem + ", ");
    }
    builder.Append("]");

    return builder.ToString();
  }

  /// <summary>
  /// Build a string out of a vector3-array
  /// </summary>
  /// <param name="array">an array of vectors of which the string will be cerated</param>
  /// <returns>a comma-seperated string with all vectors</returns>
  public string BuildArrayString(Vector3[] array)
  {
    StringBuilder builder = new StringBuilder();

    builder.Append("[");
    foreach (Vector3 elem in array)
    {
      builder.Append(elem.ToString("G4") + ", ");
    }
    builder.Append("]");

    return builder.ToString();
  }

  /// <summary>
  /// Creates a marker object at the specified point
  /// </summary>
  /// <param name="point">the point where to create the marker object</param>
  /// <param name="scale">the size of the marker object</param>
  /// <param name="name">a name to be able to identify the marker-oject</param>
  /// <param name="deleteOld"></param>
  public void ShowPoint(Vector3 point, Vector3 scale, string name, bool deleteOld = true)
  {
    if (marker != null && deleteOld)
    {
      GameObject.Destroy(marker);
      marker = null;
      spherePointCounter--;
    }
    spherePointCounter++;
    marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    marker.transform.position = point;
    marker.transform.localScale = scale;
    marker.name = marker.name + "_" + name +"_" + spherePointCounter;
  }

  /// <summary>
  /// Logging something to the console
  /// </summary>
  /// <param name="text">The text to log.</param>
  /// <param name="printIfDisabled">Specifyy if text shall be logged even if logging is deactivated</param>
  public void Log(string text, bool printIfDisabled = false)
  {
    if (debugLog || printIfDisabled)
    {
      Debug.Log(text);
    }
  }
}
