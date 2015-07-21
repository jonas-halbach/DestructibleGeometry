using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is used to process the users input
/// </summary>
public class UserInput : MonoBehaviour {

  public List<Destructor> toDestroy;

  // A gameobject, preferably a plane, to visualize the slice
  public GameObject slicePlaneVisualizer;

  // Storing old and new startpositions of the mousecursor
  private Vector3 startPos = Vector3.zero;
  private Vector3 endPos = Vector3.zero;
  private Vector3 oldStartPos = Vector3.zero;
  private Vector3 oldEndPos = Vector3.zero;

  // Renders a line connecting start und endpos to visualize the slice
  private LineRenderer lineRenderer;

  // The destroySelector and destroySelectionPosibilities are used to
  private int destroySelector = 0;
  private string[] destroySelectionPosibilities = { "Slice", "Destroy" };

  // Rects for the positions of the GUI buttons
  private Rect selectionGridRect = new Rect(0, 0, Screen.width, Screen.height - Screen.height / 10);
  private Rect restartButtonRect = new Rect(0, Screen.height - 20, 100, 20);

  // Use this for initialization
	void Start () {
    lineRenderer = (LineRenderer)GetComponent<LineRenderer>();
    lineRenderer.SetWidth(0.001f, 0.001f);
	}
	
	/// <summary>
	/// Processing the userinput
	/// </summary>
	void Update () {
    // Updating the sliceplane coordinates
    if(selectionGridRect.Contains(Input.mousePosition)) {
      if (Input.GetMouseButtonDown(0))
      {
        startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * Camera.main.nearClipPlane);
      }
      if (Input.GetMouseButtonUp(0))
      {
        endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * Camera.main.nearClipPlane);
      }
    }


    // Should an gameobject be sliced or completely destroyed?
    if (destroySelectionPosibilities[destroySelector].Equals("Slice"))
    {
      if (!startPos.Equals(Vector3.zero) && !endPos.Equals(Vector3.zero))
      {
        if (lineRenderer != null)
        {
          lineRenderer.SetVertexCount(2);
          if (PlaneChanged())
          {
            
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
            
            SliceObject(startPos, endPos);
            oldStartPos = startPos;
            oldEndPos = endPos;
          }
        }
      }
    }
    else
    {
      // Checking the mouseposition and if a gameobject with an destructor-script got hit by a raycast, than 
      // destroy it
      bool isMouseButtonDown = Input.GetMouseButtonDown(0);
      if (isMouseButtonDown)
      {
        RaycastHit hitDestructable;
        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition));
        foreach (RaycastHit hit in hits)
        {
          if (hit.collider.gameObject.GetComponent(typeof(Destructor)) != null)
          {
            hitDestructable = hit;
            Debug.Log("hitpoint: " + hit.point); 
            DestroyObject(hitDestructable);

          }
        }
        
      }
    }
	}

  /// <summary>
  /// Cheking if the slice-plane has shall be changed
  /// </summary>
  /// <returns></returns>
  private bool PlaneChanged()
  {
    return !oldEndPos.Equals(endPos) && !oldStartPos.Equals(startPos); 
  }

  /// <summary>
  /// Slice an Gameobject by a sliceplane
  /// </summary>
  /// <param name="startPos">Startpos for the slice plane</param>
  /// <param name="endPos">Endpos for hte sliceplane</param>
  void SliceObject(Vector3 startPos, Vector3 endPos)
  {
    Vector3 v1 = endPos - startPos;
    Vector3 v2 = Camera.main.transform.TransformDirection(Vector3.forward) * 1000;

    //Calculationg the normal for the sliceplane of start and endpoint and the camera forward vector
    Vector3 slicePlaneNormal = Vector3.Cross(v1, v2);
    
    slicePlaneNormal.Normalize();

    // Visualizing the slice
    if (slicePlaneVisualizer != null)
    {
      slicePlaneVisualizer.transform.position = startPos;
      slicePlaneVisualizer.transform.rotation.SetLookRotation(v1, slicePlaneNormal);

      slicePlaneVisualizer.transform.rotation = Quaternion.LookRotation(v1, slicePlaneNormal);
    }

    // Initializing the slice
    Plane slicePlane = new Plane(slicePlaneNormal, startPos);
    
    if (toDestroy != null)
    {
      toDestroy[0].SliceByPlane(slicePlane);
    }
  }

  /// <summary>
  /// Destroying a an object which was selected by a mouse-click
  /// </summary>
  /// <param name="rayCastHit">a raycasthit</param>
  private void DestroyObject(RaycastHit rayCastHit)
  {
    Debug.Log("raycasthit: " + rayCastHit);
    Debug.Log("raycasthit.rigidbody: " + rayCastHit.rigidbody);
    Debug.Log("raycasthit.rigidbody.gameobject: " + rayCastHit.rigidbody.gameObject);

    Destructor toDestroy = rayCastHit.rigidbody.gameObject.GetComponent<Destructor>();
    Debug.Log("toDestroy" + toDestroy);
    if (toDestroy != null)
    {
      rayCastHit.rigidbody.gameObject.GetComponent<Destructor>().DestroyGeometry(rayCastHit);
    }
  }

  /// <summary>
  /// Rendering the GUI to select if an object shall be destroyed or sliced.
  /// Adding a restart posiblity.
  /// </summary>
  void OnGUI()
  {
    destroySelector = GUILayout.SelectionGrid(destroySelector, destroySelectionPosibilities, destroySelectionPosibilities.Length);

    if (GUI.Button(restartButtonRect, "Restart"))
    {
      Application.LoadLevel(0);
    }
  }
}