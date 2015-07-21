using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// This class is a data-structur which represents a triangle which is sliced by a plane
/// </summary>
public class SlicedTriangle {


  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="point1">Trianglepoint1</param>
  /// <param name="point2">Trianglepoint1</param>
  /// <param name="point3">Trianglepoint1</param>
  public SlicedTriangle(Vector3 point1, Vector3 point2, Vector3 point3)
  {
    Point1 = point1;
    Point2 = point2;
    Point3 = point3;
  }

  /// <summary>
  /// Property Triangle-Point1
  /// </summary>
  public Vector3 Point1 { get; set; }
  
  /// <summary>
  /// Property Triangle-Point2
  /// </summary>
  public Vector3 Point2 { get; set; }

  /// <summary>
  /// Property Triangle-Point3
  /// </summary>
  public Vector3 Point3 { get; set; }

  /// <summary>
  /// Property Point1 where the plane slices the triangle
  /// </summary>
  public Vector3 SlicePoint1 { get; set; }

  /// <summary>
  /// Property Point1 where the plane slices the triangle
  /// </summary>
  public Vector3 SlicePoint2 { get; set; }


  /// <summary>
  /// Container for the subtriangles, which build up this triangle after splitting this trianlge into smaller parts
  /// </summary>
  public List<int>[] TriangleParts { get; set; }


  /// <summary>
  /// Equals:
  /// </summary>
  /// <param name="obj">The object to compare</param>
  /// <returns>true, if all trainglepoints an slicepoints are the same </returns>
  public override bool Equals(object obj)
  {
    bool isEqual = false;
    try
    {
      SlicedTriangle compareObj = (SlicedTriangle)obj;

      isEqual = Point1.Equals(compareObj.Point1) && Point2.Equals(compareObj.Point2) && Point3.Equals(compareObj.Point3) &&
        SlicePoint1.Equals(compareObj.SlicePoint1) && SlicePoint2.Equals(compareObj.SlicePoint2);

    }
    catch (InvalidCastException iCE) {
      isEqual = false;
    }
    return isEqual;
  }

  /// <summary>
  /// Hashcode
  /// </summary>
  /// <returns>A integer-hashcode calculated by the triangle-porperties</returns>
  public override int GetHashCode()
  {
    int hash = 13;
    hash = (hash * 7) + Point1.GetHashCode();
    hash = (hash * 7) + Point2.GetHashCode();
    hash = (hash * 7) + Point3.GetHashCode();
    hash = (hash * 7) + SlicePoint1.GetHashCode();
    hash = (hash * 7) + SlicePoint2.GetHashCode();

    return hash;
  }
}
