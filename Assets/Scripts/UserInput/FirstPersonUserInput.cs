using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is used to process the users input
/// </summary>
public class FirstPersonUserInput : MonoBehaviour
{
  // Rect to specify the position of the restart-button
  private Rect restartButtonRect = new Rect(0, Screen.height - 20, 100, 20);

  void Start()
  {
  }

  /// <summary>
  /// Processing the userinput
  /// </summary>
  void Update()
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

  /// <summary>
  /// Destroying a an object which was selected by a mouse-click
  /// </summary>
  /// <param name="rayCastHit">a raycasthit</param>
  private void DestroyObject(RaycastHit rayCastHit)
  {
    if (rayCastHit.rigidbody != null)
    {
      Destructor toDestroy = rayCastHit.rigidbody.gameObject.GetComponent<Destructor>();
      if (toDestroy != null)
      {
        rayCastHit.rigidbody.gameObject.GetComponent<Destructor>().DestroyGeometry(rayCastHit);
      }
    }
    else
    {
      Debug.LogWarning("Object to destroy needs a rigidbody! Please add a rgidbody to the object if it shall be destroyable!");
    }
  }

  /// <summary>
  /// Rendering the GUI to select if an object shall be destroyed or sliced.
  /// Adding a restart posiblity.
  /// </summary>
  void OnGUI()
  {
    if (GUI.Button(restartButtonRect, "Restart"))
    {
      Application.LoadLevel(0);
    }
  }
}