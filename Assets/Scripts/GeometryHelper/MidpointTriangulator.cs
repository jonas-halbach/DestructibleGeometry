using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// This class brings the functionality of a simple triangulator.
/// The triangulated triangles will be starshaped, which means that
/// one midpoint is connected to all points off the polygons and builds a triangle
/// with two of this polygon points.
/// 
/// Attention: The algorithm works just for complete convex polygons which lie on a
/// flat plane. It wont be prooved if the polygon is convex. If it is not convex the 
/// result of triangultion can look weird! 
/// </summary>
public class MidpointTriangulator {

  // The degree of the reference vector, which allways shall be zero
  const int REFERENCE_DEGREE = 0;

  private Dictionary<Vector3, int> pointsIndexDict = new Dictionary<Vector3, int>();

  // a list of the polygon points
  private List<Vector3> points = new List<Vector3>();
  
  // the polygon points in sorted order 
  private List<Vector3> sortedPoints = new List<Vector3>();

  // a point which is used as comparepoint for the sorting process
  private Vector3 startpoint;

  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="points">The points of the polygon</param>
  public MidpointTriangulator(Vector3[] points)
  {
    // Setting the startpoint for the triangulation process if more than one points exist
    if (points.Length > 0)
    {
      startpoint = points[0];
    }

    // Saving the sort-order of the current point-array
    for (int i = 0; i < points.Length; i++)
    {
      if (!this.pointsIndexDict.ContainsKey(points[i]))
      {
        if (!pointsIndexDict.ContainsKey(points[i]))
        {
          this.pointsIndexDict.Add(points[i], i);
        }
      }
    }

    // Saving the points of the polygon 
    this.points.AddRange(points);
  }

  /// <summary>
  /// This method processes the triangulation,
  /// </summary>
  /// <returns>
  /// A list of Vectors in the order that they build the triangulated polygon.
  /// Points will be redundant.
  /// </returns>
  public List<Vector3> Triangulate()
  {

    List<Vector3> triangulatedPoints = null;
    
    // just if more than three points exist the polygon needs to be triangulated,
    // else the existing vectors will be returned in the existing order
    if(points.Count > Destructor.triangleVertexCount) {
      triangulatedPoints = new List<Vector3>();
      SortVerticesByDegrees();
      RemoveRedundantPoints();

      //TODO: instance variable?
      Vector3 polygonMidPoint = MeshComponents.GetVerticesMidpoint(points.ToArray());

      // Here the real triangulation process takes place, just the last triangle has to be created
      // seperately
      for (int i = 0; i < sortedPoints.Count - 1; i++)
      {
        triangulatedPoints.Add(polygonMidPoint);
        triangulatedPoints.Add(sortedPoints[i]);
        triangulatedPoints.Add(sortedPoints[i + 1]);
      }

      //Closing the triangulated polygon
      triangulatedPoints.Add(polygonMidPoint);
      triangulatedPoints.Add(sortedPoints[sortedPoints.Count - 1]);
      triangulatedPoints.Add(sortedPoints[0]);

    } else {
      triangulatedPoints = new List<Vector3>(points);
    }
    return triangulatedPoints;

  }

  /// <summary>
  /// This method sorts the points of the polygon by degrees.
  /// The reference vector will be created by the midpoint and the first point
  /// of the polygon point list
  /// </summary>
  private void SortVerticesByDegrees()
  {
    if (points.Count > 2)
    {
      //TODO: instance variable?
      Vector3 midpoint = MeshComponents.GetVerticesMidpoint(points.ToArray());

      // Building the reference vector
      Vector3 startvector = startpoint - midpoint;

      Vector3 comparevector;
      float angle;

      // Storing angles to their corresponding vectors
      Dictionary<float, Vector3> angleVectorDict = new Dictionary<float, Vector3>();

      // Storing angles which is needed for sorting
      List<float> angles = new List<float>();
      Dictionary<Vector3, int> pointCountDict = new Dictionary<Vector3, int>();

      // Adding the reference Vector
      angleVectorDict.Add(REFERENCE_DEGREE, startpoint);
      angles.Add(REFERENCE_DEGREE);

      foreach (Vector3 point in points)
      {
        if (!point.Equals(startpoint))
        {
          // Building a new vector between mid- and current point,
          // which is needed to calculate the degrees between this points
          comparevector = point - midpoint;

          angle = Angle360(startvector, comparevector);

          // Counting the number of yet existing vectors with the same degree to the reference vector
          if (!angleVectorDict.ContainsKey(angle))
          {
            if (pointCountDict.ContainsKey(point))
            {
              pointCountDict[point]++;
            }
            else
            {
              pointCountDict.Add(point, 0);
            }

            angleVectorDict.Add(angle, point);
            angles.Add(angle);
          }
        }
      }

      angles.Sort();

      // Sorting the points of the polygon by their angles to the reference vector
      foreach (float sortangle in angles)
      {
        sortedPoints.Add(angleVectorDict[sortangle]);
      }
    }
    else
    {
      sortedPoints = new List<Vector3>(points);
    }
  }

  /// <summary>
  /// Calculating the degrees between the two vectors by respecting their orientation
  /// </summary>
  /// <param name="a">Vector1 is the start-vector</param>
  /// <param name="b">Vector2 is the compare vector</param>
  /// <returns>the angle between the two vectors which can reach f</returns>
  private float Angle360(Vector3 a, Vector3 b)
  {
    float angle = Vector3.Angle(a, b); // calculate angle
    // assume the sign of the cross product's Y component:
    return Mathf.Sign(Vector3.Cross(a, b).z) >= 0 ? angle : 360 - angle;
  }

  /// <summary>
  /// Removes points in the List of sorted points, so that just corner points remain in this list
  /// </summary>
  private void RemoveRedundantPoints()
  {
    if (sortedPoints.Count > 2)
    {
      List<Vector3> vectors = new List<Vector3>();
      List<Vector3> redundantPoints = new List<Vector3>();

      List<Vector3> tempSortedPoints = new List<Vector3>();
      
      tempSortedPoints.AddRange(sortedPoints);

      // this Vector is used as root point to create two vectors, of which the parallelism will be checked
      Vector3 currStartPoint = tempSortedPoints[0];
      Vector3 startvector = tempSortedPoints[1] - currStartPoint;

      Vector3 comparevector;
      bool vectorsAreParallel = true;

      int i = 0;
      for (int j = 1; j < tempSortedPoints.Count - 1; j++)
      {
        i = j + 1;

        // Building a vector from current startpoint to the next point in the polygon point list
        comparevector = tempSortedPoints[i] - currStartPoint;

        // If both vectors are parallel the first of the both polygonpoints is redundant, and will be removed.
        // Otherwise change the startpoint and the startvector.
        vectorsAreParallel = startvector.IsParallel(comparevector);
        if (vectorsAreParallel)
        {
          redundantPoints.Add(sortedPoints[j]);
        } else {
          
          currStartPoint = tempSortedPoints[j];
          startvector = tempSortedPoints[i] - currStartPoint;
        }
      }

      // Removing all redundant points in the sorted points list.
      foreach (Vector3 point in redundantPoints)
      {
        sortedPoints.Remove(point);
      }
    }
  }
}
