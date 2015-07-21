using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class MeshSlicer {

  /// The Muultiplicator is used to deal with flaot-inaccuracy
  public const int FLOAT_MULTIPLICATOR = 10000;

  private Vector3[] meshPointsInWorldSpace;
  List<Vector3> allSlicePoints;

  Mesh mesh;
  Vector3 meshMidPointInWorldSpace;

  Transform transform;

  IUVMapper uvMapper;

  //slicePlanecounter temporary used for debugging
  private static int sliceCounter = 0;

  public MeshSlicer(IUVMapper uvMapper)
  {
    this.uvMapper = uvMapper;
  }

  // Use this for initialization
  void Start(Mesh mesh, Transform transform)
  {
    this.mesh = mesh;
    this.transform = transform;
    meshMidPointInWorldSpace = transform.TransformPoint(MeshComponents.GetVerticesMidPoint(mesh));
    allSlicePoints = new List<Vector3>();
    ConvertMeshPointsToWorldSpace();
    sliceCounter++;
  }

  /// <summary>
  /// This method converts all meshpoints from local to world-space, because the sliceplane is in worldspace
  /// </summary>
  private void ConvertMeshPointsToWorldSpace()
  {

    meshPointsInWorldSpace = new Vector3[mesh.vertices.Length];
    for (int i = 0; i < mesh.vertices.Length; i++)
    {
      meshPointsInWorldSpace[i] = transform.TransformPoint(mesh.vertices[i]);
    }
  }

  /// <summary>
  /// Creates two new meshes by seperating the old mesh by a sliceplane.
  /// </summary>
  /// <param name="plane">The sliceplane</param>
  /// <returns>An array of the two slice mesh parts.</returns>
  public Mesh[] SliceByPlane(Mesh mesh, Plane plane, Transform transform)
  {

    Start(mesh, transform);

    Mesh[] seperatedMeshes = SeperateMeshPointsByPlane(plane);

    return seperatedMeshes;
  }

  /// <summary>
  /// Creates two new meshes by seperating the old mesh by a sliceplane.
  /// Todo: Refactoring
  /// </summary>
  /// <param name="plane">The sliceplane</param>
  /// <returns>An array of the two slice mesh parts.</returns>
  Mesh[] SeperateMeshPointsByPlane(Plane plane)
  {

    Mesh part1Mesh = new Mesh();

    Mesh part2Mesh = new Mesh();

    //This three Lists contain the indexes of the triangles of different triangle groups(over plane, under plane, mixed)
    List<int> part1MeshIndizes = new List<int>();
    List<int> part2MeshIndizes = new List<int>();
    List<int> partMixedMeshInizes = new List<int>();
    
    Vector3[] vertices = mesh.vertices;
    Vector3[] currPoints = new Vector3[3];
    List<int> indicesToAddToMesh = new List<int>();

    bool isInititalPointOnPositivePlaneSide = false;
    bool sideIsSame;

    Mesh[] seperatedMeshes = new Mesh[2];

    // Iterating over all triangle-points in group of three points.
    for (int i = 0; i <= mesh.triangles.Length - Destructor.triangleVertexCount; i += Destructor.triangleVertexCount)
    {

      //Checking if all three triangle points are on the same side of the sliceplane
      sideIsSame = AllPointsSameSide(mesh.triangles, i, Destructor.triangleVertexCount, plane);
      indicesToAddToMesh = new List<int>();

      //if all points are on the same side and the side of the first point is positive, all points are on the positive side. 
      //Else all points are in the negative side
      isInititalPointOnPositivePlaneSide = plane.GetSide(meshPointsInWorldSpace[mesh.triangles[i]]);

      //generating a list of vectors to add them to one of the triangle-groups later
      for (int j = 0; j < Destructor.triangleVertexCount; j++)
      {
        indicesToAddToMesh.Add(mesh.triangles[i + j]);
      }

      // Storting the points
      if (sideIsSame)
      {
        if (isInititalPointOnPositivePlaneSide)
        {
          part1MeshIndizes.AddRange(indicesToAddToMesh);
        }
        else
        {
          part2MeshIndizes.AddRange(indicesToAddToMesh);
        }
      }
      else
      {
        partMixedMeshInizes.AddRange(indicesToAddToMesh);
      }
    }

    // Generating the two sliced mesh-parts out of the list of sliced triangles.
    MeshComponents[] seperationTrianglesMeshes = CalculateSeperationMeshes(partMixedMeshInizes, plane);

    // Adds the seperationTriangleMeshes and Converts them from world to local space
    MeshComponents tempMeshComponent = new MeshComponents();
    tempMeshComponent.AddTriangles(GetMeshPointsByIndices(part1MeshIndizes.ToArray()));
    tempMeshComponent.AddTriangles(seperationTrianglesMeshes[0].GetVertices());
    MeshComponents part1MeshComponents = part1Mesh.ExtendMesh(tempMeshComponent, transform);

    tempMeshComponent = new MeshComponents();
    tempMeshComponent.AddTriangles(GetMeshPointsByIndices(part2MeshIndizes.ToArray()));
    tempMeshComponent.AddTriangles(seperationTrianglesMeshes[1].GetVertices());

    MeshComponents part2MeshComponents = part2Mesh.ExtendMesh(tempMeshComponent, transform);

    // Triangulating the hole-polygon created by the slicing process.
    List<Vector3> triangulatedHolePoints = GetCloseHolePoints(part1MeshComponents);

    part1MeshComponents.AddTriangles(triangulatedHolePoints);

    triangulatedHolePoints.Reverse();

    part2MeshComponents.AddTriangles(triangulatedHolePoints);

    // Updating the two meshed with the new calculated mesh-data
    part1Mesh.Clear();
    part1Mesh.vertices = part1MeshComponents.GetVertices().ToArray();
    part1Mesh.triangles = part1MeshComponents.GetTriangles().ToArray();
    part1Mesh.uv = CalculateUVCoordinates(part1Mesh.vertices);
    part1Mesh.RecalculateNormals();
    part1Mesh.RecalculateBounds();
    part1Mesh.Optimize();

    part2Mesh.Clear();
    part2Mesh.vertices = part2MeshComponents.GetVertices().ToArray();
    part2Mesh.triangles = part2MeshComponents.GetTriangles().ToArray();
    part2Mesh.uv = CalculateUVCoordinates(part2Mesh.vertices);
    part2Mesh.RecalculateNormals();
    part2Mesh.RecalculateBounds();
    part2Mesh.Optimize();

    seperatedMeshes[0] = part1Mesh;
    seperatedMeshes[1] = part2Mesh;

    return seperatedMeshes;
  }

  /// <summary>
  /// Checks if all points are on the same side of the triangle-plane
  /// </summary>
  /// <param name="indexes">the pointindizes</param>
  /// <param name="startIndex">the startindex where to start the check</param>
  /// <param name="length">how many points shall be checked</param>
  /// <param name="slicePlane">the sliceplane</param>
  /// <returns>true if all points are on the same side of the slice-plane</returns>
  private bool AllPointsSameSide(int[] indexes, int startIndex, int length, Plane slicePlane)
  {
    int vertexIndex = 0;
    Vector3 point;
    Vector3 oldPoint;
    bool isPointOnPositivePlaneSide;
    bool allPointsAreOnSamePlaneSide = true;

    PointSide newPointSide = PointSide.Neutral;
    PointSide oldPointSide = PointSide.Neutral;

    StringBuilder sameSideBuilder = new StringBuilder();

    for (int j = 0; j < length; j++)
    {
      vertexIndex = indexes[startIndex + j];

      point = meshPointsInWorldSpace[vertexIndex];
      sameSideBuilder.Append("PlaneDistance: " + slicePlane.GetDistanceToPoint(point) + " ");
      if (slicePlane.GetDistanceToPoint(point) != 0)
      {

        isPointOnPositivePlaneSide = slicePlane.GetSide(point);

        newPointSide = isPointOnPositivePlaneSide ? PointSide.Positive : PointSide.Negative;
        sameSideBuilder.Append(newPointSide);
        allPointsAreOnSamePlaneSide = NewPointIsOnSamePlaneSide(newPointSide, oldPointSide) && allPointsAreOnSamePlaneSide;
        oldPointSide = newPointSide;
        oldPoint = point;
      }
    }

    sameSideBuilder.Append(allPointsAreOnSamePlaneSide);

    return allPointsAreOnSamePlaneSide;
  }

  /// <summary>
  /// Checks if the PointSides are the same. If one of the point-sides is neutral both pointsides are the same.
  /// </summary>
  /// <param name="newPointSide">pointside1</param>
  /// <param name="oldPointSide">pointside2</param>
  /// <returns>Are this two pointsides on the same side</returns>
  private bool NewPointIsOnSamePlaneSide(PointSide newPointSide, PointSide oldPointSide)
  {
    return newPointSide == oldPointSide || newPointSide == PointSide.Neutral || oldPointSide == PointSide.Neutral;
  }

  /// <summary>
  /// This Method calculates the two meshes, which are the result of the slice-plane split.
  /// </summary>
  /// <param name="slicedTriangles">The triangles-indexes of the triangles which are seperated into two
  /// triangles during the splitting process. </param>
  /// <param name="plane">The slice-plane</param>
  /// <returns>The two MeshComponents which are the result of the splitting process.
  /// Meshpoints will be in world space!</returns>
  private MeshComponents[] CalculateSeperationMeshes(List<int> slicedTriangleIndizes, Plane plane)
  {
    MeshComponents[] components = new MeshComponents[2];
    MeshComponents meshComponent1 = new MeshComponents();
    MeshComponents meshComponent2 = new MeshComponents();
    components[0] = meshComponent1;
    components[1] = meshComponent2;

    List<int>[] seperatedTriangleParts = new List<int>[2];

    List<Vector3>[] sliceTrianglesVertices;

    List<SlicedTriangle> slicedTriangles = CreateAllSlicePoints(slicedTriangleIndizes, plane);

    foreach (SlicedTriangle slicedTriangle in slicedTriangles)
    {

      //Create new triangles out of the old triangle- and the intersection points.
      sliceTrianglesVertices = CreateSliceTriangles(slicedTriangle);

      // Add the connectionPoints to each side
      meshComponent1.AddTriangles(sliceTrianglesVertices[0]);
      meshComponent2.AddTriangles(sliceTrianglesVertices[1]);
    }

    return components;
  }
  /// <summary>
  /// Calculating the slicepoints of triangles sliced by the plane
  /// </summary>
  /// <param name="slicedTriangleIndizes">The indizes of the triangle-points, which are sliced by the plane</param>
  /// <param name="plane">the sliceplane</param>
  /// <returns>A List of slicedTriangles</returns>
  private List<SlicedTriangle> CreateAllSlicePoints(List<int> slicedTriangleIndizes, Plane plane)
  {
    List<int> currTriangle;
    List<int>[] seperatedTriangleParts;
    Vector3[] slicePoints;
    List<SlicedTriangle> slicedTriangles = new List<SlicedTriangle>();
    SlicedTriangle currSlicedTriangle;
    for (int i = 0; i < slicedTriangleIndizes.Count - Destructor.triangleVertexCount + 1; i += Destructor.triangleVertexCount)
    {
      currTriangle = slicedTriangleIndizes.GetRange(i, Destructor.triangleVertexCount);

      // Attach every point to one plane side
      seperatedTriangleParts = SeperateTriangle(currTriangle, plane);

      // Find the connection points between the sides
      slicePoints = CalculateSlicePoints(seperatedTriangleParts, plane);

      currSlicedTriangle = new SlicedTriangle(meshPointsInWorldSpace[currTriangle[0]], meshPointsInWorldSpace[currTriangle[1]], meshPointsInWorldSpace[currTriangle[2]]);
      currSlicedTriangle.SlicePoint1 = slicePoints[0];
      currSlicedTriangle.SlicePoint2 = slicePoints[1];
      currSlicedTriangle.TriangleParts = seperatedTriangleParts;

      slicedTriangles.Add(currSlicedTriangle);

      // Store all Slicepoints for the later triangulation to close the polygon
      allSlicePoints.AddRange(slicePoints);

    }

    return slicedTriangles;
  }


  /// <summary>
  /// Generated three new triangles out of the points specified by the parameter
  /// </summary>
  /// <param name="seperatedTriangle">A SlicedTriangle-Object which represents a triangle sliced by a plane</param>
  /// <returns></returns>
  private List<Vector3>[] CreateSliceTriangles(SlicedTriangle seperatedTriangle)
  {
    List<Vector3>[] slicedTriangles = new List<Vector3>[2];
    slicedTriangles[0] = new List<Vector3>();
    slicedTriangles[1] = new List<Vector3>();

    List<Vector3> allPoints = new List<Vector3>();
    List<int>[] seperatedTriangleParts = seperatedTriangle.TriangleParts;

    // Specifying which index contains the array with one and two trianglepoints
    int twoPointSideIndex = seperatedTriangleParts[0].Count == 2 ? 0 : 1;
    int onePointSideIndex = twoPointSideIndex == 0 ? 1 : 0;


    // Creating the single-triangle on one side of the plane 
    List<Vector3> twoPointSideTriangle;

    slicedTriangles[onePointSideIndex].Add(meshPointsInWorldSpace[seperatedTriangleParts[onePointSideIndex][0]]);
    slicedTriangles[onePointSideIndex].Add(seperatedTriangle.SlicePoint2);
    slicedTriangles[onePointSideIndex].Add(seperatedTriangle.SlicePoint1);

    Vector3 debugMidPoint = MeshComponents.GetVerticesMidpoint(slicedTriangles[onePointSideIndex].ToArray());
    Vector3 n = Vector3.Cross(slicedTriangles[onePointSideIndex][0], slicedTriangles[onePointSideIndex][1]);


    // Creating the triangle of the two triangles ofthe triangle base with one base- and the two slicepoints
    twoPointSideTriangle = new List<Vector3>();
    twoPointSideTriangle.Add(seperatedTriangle.SlicePoint1);
    twoPointSideTriangle.Add(seperatedTriangle.SlicePoint2);
    twoPointSideTriangle.Add(meshPointsInWorldSpace[seperatedTriangleParts[twoPointSideIndex][0]]);

    debugMidPoint = MeshComponents.GetVerticesMidpoint(twoPointSideTriangle.ToArray());
    n = Vector3.Cross(twoPointSideTriangle[0], twoPointSideTriangle[1]);

    bool arePointsVisible = IsVisible(meshMidPointInWorldSpace, twoPointSideTriangle, false, "seperation", true);

    if (arePointsVisible)
    {
      twoPointSideTriangle.Reverse();
    }

    slicedTriangles[twoPointSideIndex].AddRange(twoPointSideTriangle);

    //Creating the triangle of the two triangles ofthe triangle base with two base- and one slicepoint
    twoPointSideTriangle = new List<Vector3>();
    twoPointSideTriangle.Add(meshPointsInWorldSpace[seperatedTriangleParts[twoPointSideIndex][0]]);
    twoPointSideTriangle.Add(seperatedTriangle.SlicePoint2);
    twoPointSideTriangle.Add(meshPointsInWorldSpace[seperatedTriangleParts[twoPointSideIndex][1]]);

    debugMidPoint = MeshComponents.GetVerticesMidpoint(twoPointSideTriangle.ToArray());
    n = Vector3.Cross(twoPointSideTriangle[0], twoPointSideTriangle[1]);

    arePointsVisible = IsVisible(meshMidPointInWorldSpace, twoPointSideTriangle, false, "seperation", true);

    // Making the triangles visible
    if (arePointsVisible)
    {
      twoPointSideTriangle.Reverse();
    }

    slicedTriangles[twoPointSideIndex].AddRange(twoPointSideTriangle);

    arePointsVisible = IsVisible(meshMidPointInWorldSpace, slicedTriangles[onePointSideIndex], false, "seperation", true);

    // Making the triangles visible
    if (arePointsVisible)
    {
      slicedTriangles[onePointSideIndex].Reverse();
    }

    return slicedTriangles;
  }

  /// <summary>
  /// This method calculates the points which intersection points of the mesh-triangöes an a plane. 
  /// </summary>
  /// <param name="seperatedTriangles">The triangle-point-indizes of the points which are 
  /// seperated by the plane.</param>
  /// <param name="plane">The seperation-plane</param>
  /// <returns>An array of vectors, which are the intersection-points of the triangles and the 
  /// plane. The points are located in the world coordinate system!</returns>
  private Vector3[] CalculateSlicePoints(List<int>[] seperatedTriangles, Plane plane)
  {
    int twoPointSideIndex = seperatedTriangles[0].Count == 2 ? 0 : 1;
    int onePointSideIndex = twoPointSideIndex == 0 ? 1 : 0;
    Vector3[] slicePoints = new Vector3[2];

    Vector3 pointOnSide1;

    //Getting the point where no other point is on the same plane side
    Vector3 pointOnSide2 = meshPointsInWorldSpace[seperatedTriangles[onePointSideIndex][0]];

    int i = 0;

    //Iterating over the points of the triangle-side with two points.
    foreach (int trianglePointIndex in seperatedTriangles[twoPointSideIndex])
    {
      // Getting the point of the plane side, where two points are
      pointOnSide1 = meshPointsInWorldSpace[trianglePointIndex];

      //Calculating the intersection point of the line between pointOnSide1 and pointOnSide2 and the plane
      slicePoints[i] = CalculateConnectionPlanePoint(pointOnSide1, pointOnSide2, plane);
      i++;
    }
    return slicePoints;
  }

  /// <summary>
  /// Sorts the points of the triangle if they are on the positive or negative side of the slice-plane. 
  /// </summary>
  /// <param name="triangle">The indizes of the triangle-points</param>
  /// <param name="plane">the sliceplane</param>
  /// <returns></returns>
  private List<int>[] SeperateTriangle(List<int> triangle, Plane plane)
  {
    List<int>[] seperatedTriangle = new List<int>[2];
    seperatedTriangle[0] = new List<int>();
    seperatedTriangle[1] = new List<int>();

    Vector3 meshPointInWorld;

    int side;

    for (int i = 0; i < triangle.Count; i++)
    {
      meshPointInWorld = meshPointsInWorldSpace[triangle[i]];
      side = plane.GetSide(meshPointInWorld) ? 0 : 1;
      seperatedTriangle[side].Add(triangle[i]);
    }

    return seperatedTriangle;
  }

  /// <summary>
  /// Calculates the point where the plane intersects the connection between pointStart and pointEnd.
  /// </summary>
  /// <param name="pointStart">The startpint of the connection</param>
  /// <param name="pointEnd">The endpoint of the connection</param>
  /// <param name="plane">The plane which seperates start and- endpoint</param>
  /// <returns>the intersection-point</returns>
  private Vector3 CalculateConnectionPlanePoint(Vector3 pointStart, Vector3 pointEnd, Plane plane)
  {
    float enterDistance = 0;
    Vector3 hitPoint = Vector3.zero;
    Ray ray = new Ray(pointStart, (pointEnd - pointStart) * 10000);

    if (pointStart.Equals(pointEnd))
    {
      hitPoint = pointStart;

    } 
    else if (plane.Raycast(ray, out enterDistance))
    {
      hitPoint = ray.GetPoint(enterDistance);
    }
    else
    {
      
      Debug.LogError("The connection between the Points " + pointStart + " and " + pointEnd + " do not intersect with the plane "
        + plane + "!");
    }

    return hitPoint;
  }

  /// <summary>
  /// Triangulates and closes the polygon-hole which is the result of the slicing process. 
  /// </summary>
  /// <param name="meshComponents">The generated mesh with a hole at the slice-plane.
  /// If not visible the plane will also be made visible</param>
  /// <returns>The triangulated slicepoints</returns>
  private List<Vector3> GetCloseHolePoints(MeshComponents meshComponents)
  {
    List<Vector3> closedHolePoints = new List<Vector3>();
    if (allSlicePoints.Count > 0)
    {
      meshMidPointInWorldSpace = transform.TransformPoint(meshComponents.GetMidPoint());
      // Triangulating all slicapoints
      MidpointTriangulator triangulator = new MidpointTriangulator(allSlicePoints.ToArray());

      List<Vector3> closedHolePointsWorldSpace = triangulator.Triangulate();

      //MakeSlicepoints visible
      if (IsVisible(meshMidPointInWorldSpace, closedHolePointsWorldSpace, false, "triangulation", true))
      {
        closedHolePointsWorldSpace.Reverse();
      }

      // Converting triangulated points form world to local-space
      for (int i = 0; i < closedHolePointsWorldSpace.Count; i++)
      {
        closedHolePoints.Add(transform.InverseTransformPoint(closedHolePointsWorldSpace[i]));
      }
    }

    return closedHolePoints;
  }

  /// <summary>
  /// Checks if the triagnle builded up by the trianglepoints are visible from viewpoint
  /// </summary>
  /// <param name="viewpoint">the viewpoint to check if the triangle is visible</param>
  /// <param name="trianglePoints">The point building the triangle to test</param>
  /// <param name="transformPoints">true if points shall be transformed</param>
  /// <param name="addInfo">Info for debugging stuff</param>
  /// <param name="debug">if shall log info</param>
  /// <returns>true if triangle is visible from meshpoints</returns>
  private bool IsVisible(Vector3 viewpoint, List<Vector3> trianglePoints, bool transformPoints, string addInfo, bool debug = false)
  {
    bool isVisible = false;
    bool sideFound = false;
    Vector3 n = Vector3.zero;

    //Iterate over all triangles and stop if visibility is specified
    for (int i = 0; (i < trianglePoints.Count - Destructor.triangleVertexCount + 1 && sideFound == false); i += Destructor.triangleVertexCount)
    {
      //Transorming the point if they shall be transformed
      Vector3 trianglePoint1 = transformPoints ? transform.TransformPoint(trianglePoints[i]) : trianglePoints[i];
      Vector3 trianglePoint2 = transformPoints ? transform.TransformPoint(trianglePoints[i + 1]) : trianglePoints[i + 1];
      Vector3 trianglePoint3 = transformPoints ? transform.TransformPoint(trianglePoints[i + 2]) : trianglePoints[i + 2];

      // Creating the vectors which build up the triangle and multipliying by great value to minimize float-problems
      Vector3 v1 = (trianglePoint2 - trianglePoint1) * FLOAT_MULTIPLICATOR;
      Vector3 v2 = (trianglePoint3 - trianglePoint1) * FLOAT_MULTIPLICATOR;
      n = Vector3.Cross(v1, v2);

      Vector3 planePoint2 = trianglePoint1 + v1;
      Vector3 planePoint3 = trianglePoint1 + v2;

      //Building the plane which is needed to make the view-test
      Plane testplane = new Plane(n.normalized, trianglePoint1);
      float distanceToSlicePlane = testplane.GetDistanceToPoint(viewpoint);

      //If distance to plane is 0 log some stuff
      if (distanceToSlicePlane == 0)
      {
        DebugHelper.getInstance().Log("Distance to SlicePlane " + addInfo + " : " + distanceToSlicePlane, debug);
        DebugHelper.getInstance().Log("Testplane normal: " + testplane.normal + " distance: " + testplane.distance, debug);
      }
      else
      {
        isVisible = testplane.GetDistanceToPoint(viewpoint) > 0;
        sideFound = true;
      }
    }

    // Log if visiblity could bot be specified.
    if (!sideFound)
    {
      Debug.LogWarning("Keine Seite eindeutig gefunden!");
      Debug.LogWarning(DebugHelper.getInstance().BuildArrayString(trianglePoints.ToArray()));

      DebugHelper.getInstance().DrawMesh("Undefined Plane Facing", trianglePoints.ToArray(), Color.yellow);

    }
    return isVisible;
  }

  /// <summary>
  /// Removes dublicates in an array
  /// </summary>
  /// <typeparam name="T">The type of the object-array</typeparam>
  /// <param name="array">the array of which dublicates shall be removed.</param>
  /// <returns></returns>
  private T[] RemoveDublicates<T>(T[] array)
  {
    Dictionary<T, bool> dublicateChecker = new Dictionary<T, bool>();
    List<T> withoutDublicates = new List<T>();

    foreach (T value in array)
    {
      if (!dublicateChecker.ContainsKey(value))
      {
        dublicateChecker.Add(value, true);
        withoutDublicates.Add(value);
      }
    }
    return withoutDublicates.ToArray();
  }

  /// <summary>
  /// Calculating the uv-cordinated for the sliceobject
  /// </summary>
  /// <param name="vertices">The mesh-points</param>
  /// <returns>an array of uv-coordinates</returns>
  private Vector2[] CalculateUVCoordinates(Vector3[] vertices)
  {
    return uvMapper.Map(vertices);
  }

  /// <summary>
  /// Returning the mesh-points specified by indices
  /// </summary>
  /// <param name="indices">the indices to get the points for.</param>
  /// <returns></returns>
  private List<Vector3> GetMeshPointsByIndices(int[] indices)
  {
    List<Vector3> vertices = new List<Vector3>();
    foreach(int index in indices) {
      vertices.Add(meshPointsInWorldSpace[index]);
    }

    return vertices;
  }
}
