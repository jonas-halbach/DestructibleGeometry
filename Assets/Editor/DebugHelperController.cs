using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// This is an unity3d editor window which shall help during the visual debugging process.
/// </summary>
public class DebugHelperController : EditorWindow
{

  static int i = 0;

  int selectedMesh = 0;
  int selectedLine = 0;

  Color selectedLineColor = Color.black;
  Color selectedMeshColor = Color.black;

  bool showMesh = true;
  bool showAllMeshes = true;

  bool showLine = true;
  bool showAllLines = true;

  [MenuItem("Window/Debugging")]
  public static void ShowWindow()
  {
    EditorWindow.GetWindow(typeof(DebugHelperController));
  }

  /// <summary>
  /// Draw different lines and meshes to the screen
  /// </summary>
  void Update()
  {
    foreach (List<DebugLine> debugMeshes in DebugHelper.getInstance().namedMeshes.Values)
    {
      foreach (DebugLine debugLine in debugMeshes)
      {
        debugLine.Draw();
      }
    }
    foreach (DebugLine debugLine in DebugHelper.getInstance().namedLines.Values)
    {
      debugLine.Draw();
    }
  }

  /// <summary>
  /// Showing the window GUI
  /// </summary>
  void OnGUI()
  {
    DebugHelper debughelper = DebugHelper.getInstance();

    if (debughelper != null)
    {
      // Creating an array with all meshes stored in the debug-helper
      if (debughelper.namedMeshes != null)
      {
        string[] selectableMeshes = new string[debughelper.namedMeshes.Count];
        int i = 0;
        foreach (string meshName in debughelper.namedMeshes.Keys)
        {
          selectableMeshes[i] = meshName;
          i++;
        }

        // Creating an array with all lines stored in the debug-helper
        string[] selectableLines = new string[debughelper.namedLines.Count];
        int j = 0;
        foreach (string lineName in debughelper.namedLines.Keys)
        {
          selectableLines[j] = lineName;
          j++;
        }

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        //Showing the availible meshes and lines in a dropdown-list.
        selectedMesh = EditorGUILayout.Popup(selectedMesh, selectableMeshes);
        selectedLine = EditorGUILayout.Popup(selectedLine, selectableLines);

        EditorGUILayout.EndHorizontal();
        selectedMeshColor = EditorGUILayout.ColorField(selectedMeshColor);

        //Possibility to deactivate the drawing-process for the selected mesh
        GUIContent showMeshLabel = new GUIContent("show mesh: ");
        showMesh = EditorGUILayout.Toggle(showMeshLabel, showMesh);

        //Possibility to deactivate the drawing-process for all meshes
        GUIContent showAllMeshesLabel = new GUIContent("show all meshes: ");
        showAllMeshes = EditorGUILayout.Toggle(showAllMeshesLabel, showAllMeshes);

        // Possibility to specify the color of the selected line
        selectedLineColor = EditorGUILayout.ColorField(selectedLineColor);

        //Possibility to deactivate the drawing-process for the selected line
        GUIContent showLineLabel = new GUIContent("show line: ");
        showLine = EditorGUILayout.Toggle(showLineLabel, showLine);

        //Possibility to deactivate the drawing-process for all lines
        GUIContent showAllLinesLabel = new GUIContent("show all lines: ");
        showAllLines = EditorGUILayout.Toggle(showAllLinesLabel, showAllLines);

        EditorGUILayout.EndVertical();

        //Coloring and hiding the meshes
        if (selectableMeshes.Length > 0 && debughelper.namedMeshes.Count > 0)
        {
          Hide(debughelper.namedMeshes.Values, !showAllMeshes);

          if (debughelper.namedMeshes[selectableMeshes[selectedMesh]].Count > 0)
          {
            Hide(debughelper.namedMeshes[selectableMeshes[selectedMesh]], !showMesh);
            ColorMesh(debughelper.namedMeshes[selectableMeshes[selectedMesh]], selectedMeshColor);
          }
        }

        //Coloring and hiding the lines
        if (selectableLines.Length > 0 && debughelper.namedLines.Count > 0)
        {
          Hide(debughelper.namedLines.Values, !showAllLines);

          if (debughelper.namedLines.Count > 0)
          {
            debughelper.namedLines[selectableLines[selectedLine]].ShallDraw = showLine;
            debughelper.namedLines[selectableLines[selectedLine]].LineColor = selectedLineColor;
          }
        }

      }
    }
  }

  /// <summary>
  /// This method hides/shows a mesh.
  /// </summary>
  /// <param name="meshes">co collection of debugline lists of which the hide value shall be changed</param>
  /// <param name="hide">if the debuglines shall be hidden/visible</param>
  private void Hide(Dictionary<string, List<DebugLine>>.ValueCollection meshes, bool hide)
  {
    foreach (List<DebugLine> mesh in meshes)
    {
      Hide(mesh, hide);
    }
  }

  /// <summary>
  /// This method hides/shows a mesh.
  /// </summary>
  /// <param name="meshes">co collection of debugline lists of which the hide value shall be changed</param>
  /// <param name="hide">if the debuglines shall be hidden/visible</param>
  private void Hide(Dictionary<string, DebugLine>.ValueCollection lines, bool hide)
  {
    foreach (DebugLine line in lines)
    {
      line.ShallDraw = !hide;
    }
  }

  /// <summary>
  /// Setting the hide value for a list of lines.
  /// </summary>
  /// <param name="debugLines">The lines of which to change the hide value</param>
  /// <param name="hide">hide/show</param>
  private void Hide(List<DebugLine> debugLines, bool hide)
  {
    foreach (DebugLine debugLine in debugLines)
    {
      debugLine.ShallDraw = !hide;
    }
  }

  /// <summary>
  /// Changing the color of the debug-mesh
  /// </summary>
  /// <param name="mesh">the name of the mesh where to change the color of</param>
  /// <param name="color">The color the mesh shall be changed to</param>
  private void ColorMesh(List<DebugLine> mesh, Color color)
  {
    foreach (DebugLine line in mesh)
    {
      line.LineColor = color;
    }
  }
}
