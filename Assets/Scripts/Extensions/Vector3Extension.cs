using UnityEngine;
using System.Collections;

/// <summary>
/// This class extends the Vector3-class by some functionality
/// </summary>
public static class Vector3Extension {

  //this number is used to compensate the float inaccuracy
  public static float epsylon = 0.1f;

  /// <summary>
  /// This method calculates if two vectors are parallel
  /// </summary>
  /// <param name="v1">Vector1 to check</param>
  /// <param name="v2">Vector2 to check</param>
  /// <returns>true, if vectors are parellel</returns>
  public static bool IsParallel(this Vector3 v1, Vector3 v2) {

    float vectorAngle = Vector3.Angle(v1, v2);

    //if the angle between the two given vectors is inside a small range around 0 or 180 degrees the vectors are parallel
    return vectorAngle < epsylon || vectorAngle > 180 - epsylon;
  }
	
}
