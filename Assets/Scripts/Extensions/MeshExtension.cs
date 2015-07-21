using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MeshExtension
{
  /// <summary>
  /// This method can be used to copy a mesh
  /// </summary>
  /// <param name="mesh">The source mesh, which shall be copied</param>
  /// <returns>The copy of the source-mesh.</returns>
  public static Mesh Copy(this Mesh mesh)
  {
    Mesh targetmesh = new Mesh();
    targetmesh.vertices = mesh.vertices;
    targetmesh.normals = mesh.normals;
    targetmesh.uv = mesh.uv;
    targetmesh.triangles = mesh.triangles;
    targetmesh.tangents = mesh.tangents;

    return targetmesh;
  }

  /// <summary>
  /// This method can be used to extend a mesh by some vertices.
  /// </summary>
  /// <param name="mesh">The mesh which shall be extended</param>
  /// <param name="extensionComponent">the information of the extension</param>
  /// <param name="transform">the transformobject of the extension component.</param>
  /// <returns>an extended mesh represented by an mesh-component object.</returns>
  public static MeshComponents ExtendMesh(this Mesh mesh, MeshComponents extensionComponent, Transform transform)
  {
    MeshComponents extendedMeshComponents = new MeshComponents();
    extendedMeshComponents.SetInitialTriangles(mesh.vertices, mesh.triangles);
    extensionComponent.ConvertWorldToLocalVertices(transform);
    extendedMeshComponents.AddTriangles(extensionComponent.GetVertices());
    return extendedMeshComponents;
  }

  /// <summary>
  /// This method translates all meshpoint of the given mesh by the vector specified by the parameter.
  /// </summary>
  /// <param name="mesh">The mesh, which points shall be translated</param>
  /// <param name="translation">The translation-vector</param>
  /// <returns>a new mesh, where the meshpoints are translated by the vector.</returns>
  public static Mesh Translate(this Mesh mesh, Vector3 translation)
  {
    Mesh translatedMesh = new Mesh();
    
    Vector3[] translatedMeshVertices = new Vector3[mesh.vertices.Length];
    for (int i = 0; i < mesh.vertices.Length; i++)
    {
      translatedMeshVertices[i] = mesh.vertices[i] + translation;
    }

    translatedMesh.vertices = translatedMeshVertices;
    translatedMesh.triangles = mesh.triangles;
    translatedMesh.normals = mesh.normals;
    translatedMesh.uv = mesh.uv;
    return translatedMesh;
  }
}