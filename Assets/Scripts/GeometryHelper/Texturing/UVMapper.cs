using UnityEngine;
using System.Collections;

public abstract class UVMapper : MonoBehaviour, IUVMapper {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  public abstract Vector2[] Map(Vector3[] vertices);
}
