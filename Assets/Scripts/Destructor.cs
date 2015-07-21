using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// This class can be used to visually destroy the mesh of an gameobject or to slice it into two parts.
/// Attention: This script should be used just by gameobjects with fully convex gameobjects. 
/// If will not be checked if the mesh is convex, but if it is not the result can weird meshes or maybe an exception.
/// </summary>
public class Destructor : MonoBehaviour {

  // will be used in differnt classes for destruction. Just the number of vertices a triangle contains of
  public const int triangleVertexCount = 3;

  //a number to determine in how many pieces a gameobjects-mesh will be destroyed. the number of fractures will be
  // fracturecount * fracturecount
  public int fractureCount  = 1;

  // default value for the explosion effect. This is used by rigidbody.addExplosionforce
  public float explosionForce = 200;
  public float explosionRadius = 50;

  // A gameobject with a script implementing the IUVMapper-Interface which is used to calculate the UV-Coordinates
  public GameObject uvMapperObject;
  // The IUVMapper-Script
  private IUVMapper uvMapper;
	
	// Update is called once per frame
	void Update () {
    
	}

  /// <summary>
  /// This method destroys the mesh of an gameobject!
  /// This algorithm works by splitting the mesh into smaller objects by slicing the gameobject
  /// by different planes, which will be generated randomly, but all Planes will intersect at one
  /// point specified by the raycastHit.
  /// </summary>
  /// <param name="raycastHit">The center point of the destruction</param>
  /// <returns>a list of all fragments</returns>
  public List<Destructor> DestroyGeometry(RaycastHit raycastHit)
  {
    return DestroyGeometry(raycastHit, this.explosionForce, this.explosionRadius);
  }

  /// <summary>
  /// This method destroys the mesh of an gameobject!
  /// This algorithm works by splitting the mesh into smaller objects by slicing the gameobject
  /// by different planes, which will be generated randomly, but all Planes will intersect at one
  /// point specified by the raycastHit.
  /// TODO: Refactoring!
  /// </summary>
  /// <param name="raycastHit">The center point of the destruction</param>
  /// <param name="explosionForce">The force of the explosion</param>
  /// <param name="explosionRadius">The distance from the explosion center</param>
  /// <returns>a list of all destruchtion-fragments</returns>
  public List<Destructor> DestroyGeometry(RaycastHit raycastHit, float explosionForce, float explosionRadius)
  {
    List<Destructor> destructionObjects = new List<Destructor>();

    Vector3 hitpoint = raycastHit.point;
    Vector3 n;
    Plane randomPlane;
    Mesh startMesh = this.GetComponent<MeshFilter>().mesh;

    List<Mesh> destructionParts = new List<Mesh>();
    List<Mesh> currentDestructionParts = new List<Mesh>();
    destructionParts.Add(startMesh);

    uvMapper = uvMapperObject.GetComponent<UVMapper>();

    if (uvMapper != null)
    {
      MeshSlicer slicer = new MeshSlicer(uvMapper);
      //Iterating over all jet existing mesh-fractures.
      //The algorithm uses old created fratures to create new.
      for (int i = 0; i < fractureCount; i++)
      {
        // A random vetor which is used to generate a new random slicing plane.
        n = GenerateRandomVector();
        randomPlane = new Plane(n, hitpoint);

        // Iterating over all yet existing destruction parts and slicing them by the plane
        foreach (Mesh mesh in destructionParts)
        {
          currentDestructionParts.AddRange(slicer.SliceByPlane(mesh, randomPlane, this.transform));
        }
        // the new created destruction parts are the base for the next iteration
        destructionParts = currentDestructionParts;
        currentDestructionParts = new List<Mesh>();
      }

      GameObject destructedObject;
      MeshFilter destructedObjectMeshFilter;
      MeshRenderer destructedObjectMeshRenderer;
      Mesh destructedObjectMesh;
      Vector3 meshMidPoint;
      Mesh currentMesh;
      Vector3 newMeshMidpoint;

      // Creating new Gameobjects by using the meshes generated in the previous step.
      foreach (Mesh mesh in destructionParts)
      {
        if (mesh.vertices.Length > 0 && mesh.triangles.Length > 0)
        {
          // Initializing a new gameobject and bringing its pivot to the center of the objects mesh. 
          meshMidPoint = transform.TransformPoint(MeshComponents.GetVerticesMidpoint(mesh.vertices));
          destructedObject = (GameObject)GameObject.Instantiate(this.gameObject, meshMidPoint, Quaternion.identity);
          destructedObjectMeshFilter = destructedObject.GetComponent<MeshFilter>();
          currentMesh = mesh.Translate(DevideVectors(destructedObjectMeshFilter.transform.InverseTransformDirection(transform.position - destructedObject.transform.position), destructedObject.transform.lossyScale));
          newMeshMidpoint = transform.TransformPoint(MeshComponents.GetVerticesMidpoint(currentMesh.vertices));

          // Adding the generated mesh-geometrie to the new created gameobject 
          destructedObjectMesh = destructedObjectMeshFilter.mesh;
          destructedObjectMeshRenderer = destructedObject.GetComponent<MeshRenderer>();
          destructedObjectMesh.Clear();
          destructedObjectMeshFilter.mesh = currentMesh;

          //Updating the collider of the gameobject
          ChangeColliderToMeshCollider(destructedObject);
          destructedObject.GetComponent<MeshCollider>().sharedMesh = currentMesh;

          //Adding some explosiontforce to the gameobject foe vfx
          destructedObject.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, raycastHit.point, explosionRadius);

          destructionObjects.Add(destructedObject.GetComponent<Destructor>());
        }
      }
      //After all new fractureparts are generated -> destroy the source.
      DestroyImmediate(this.gameObject);
    }

    return destructionObjects;
  }

  /// <summary>
  /// This method generates a normalized random vector
  /// </summary>
  /// <returns>a normalized random vector</returns>
  private Vector3 GenerateRandomVector()
  {
    float randomX = Random.Range(0.0f, 1.0f);
    float randomY = Random.Range(0.0f, 1.0f);
    float randomZ = Random.Range(0.0f, 1.0f);

    return new Vector3(randomX, randomY, randomZ).normalized; 
  }

  /// <summary>
  /// This method slices the mesh of the Gameobject by the specified plane.
  /// </summary>
  /// <param name="plane">The plane which shall seperate the objects mesh</param>
  public void SliceByPlane(Plane plane)
  {
    uvMapper = uvMapperObject.GetComponent<UVMapper>();

    if (uvMapper != null)
    {
      MeshSlicer slicer = new MeshSlicer(uvMapper);
      MeshFilter meshFilter = this.GetComponent<MeshFilter>();
      Mesh mesh = meshFilter.mesh;
      Mesh[] slicedMeshes = slicer.SliceByPlane(mesh, plane, this.transform);

      meshFilter.mesh.Clear();

      if (slicedMeshes[0].vertices.Length > 0 && slicedMeshes[0].triangles.Length > 0)
      {

        //Setting the mesh of the gameobject which shall be splitted(Fracture1).
        meshFilter.mesh = slicedMeshes[0];

        if (slicedMeshes[1].vertices.Length > 0 && slicedMeshes[1].triangles.Length > 0)
        {
          // copying the the gameobject which should be drestroyed(source)
          GameObject copy = (GameObject)GameObject.Instantiate(this.gameObject);


          // setting the mesh of the copy
          MeshFilter meshFilter2 = copy.GetComponent<MeshFilter>();
          meshFilter2.mesh.Clear();
          meshFilter2.mesh = slicedMeshes[1];

          //Updating the collider of the copied gameobject
          copy.GetComponent<MeshCollider>().sharedMesh = meshFilter2.mesh;
        }
      }
      else if (slicedMeshes[1].vertices.Length > 0 && slicedMeshes[1].triangles.Length > 0)
      {
        meshFilter.mesh = slicedMeshes[1];
      }

      //Updating the collider of this gameobject
      DestroyImmediate(this.GetComponent<MeshCollider>());
      MeshCollider collider1 = this.gameObject.AddComponent<MeshCollider>();
      collider1.convex = true;

    }
  }

  /// <summary>
  /// This method switches the current collider of the gameobject to a mesh-collider.
  /// </summary>
  /// <param name="gameObject">The gameobject of which the collider shall be switched.</param>
  private void ChangeColliderToMeshCollider(GameObject gameObject)
  {
    Collider[] collider = gameObject.GetComponents<Collider>();
    bool hasMeshCollider = false;
    foreach (Collider currentCollider in collider)
    {
      if (!(currentCollider is MeshCollider))
      {
        Destroy(currentCollider);
      }
      else
      {
        hasMeshCollider = true;
      }
    }

    if (!hasMeshCollider)
    {
      MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
      meshCollider.convex = true;
    }
  }

  /// <summary>
  /// Destroy the meshcollidercomponent of this gameobject.
  /// </summary>
  public void DestroyMeshCollider()
  {
    Destroy(GetComponent(typeof(MeshCollider)));
  }

  /// <summary>
  /// Multiply each parameter of an vector by its counterpart of an other vector
  /// </summary>
  /// <param name="v1">Vector1 used to multiply</param>
  /// <param name="v2">Vector2 to multiply </param>
  /// <returns>an vector where each parameter is the result of the multiplication of the pparameter of the other vectors</returns>
  public Vector3 MultiplyVectors(Vector3 v1, Vector3 v2)
  {
    return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
  }

  /// <summary>
  /// Deviding the paramter of v1 by the parameter of v2 
  /// </summary>
  /// <param name="v1">Devisor1</param>
  /// <param name="v2">Devisor2</param>
  /// <returns>A vector with the parameter where the parameter of v1 are devided by the parameter of v2</returns>
  public Vector3 DevideVectors(Vector3 v1, Vector3 v2)
  {
    return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
  }
}

/// <summary>
/// Enum to classify on which side of a plane a point is.
/// </summary>
enum PointSide
{
  Positive,
  Negative,
  Neutral
}
