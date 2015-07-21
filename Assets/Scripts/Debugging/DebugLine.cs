using UnityEngine;
using System.Collections;

[System.Serializable]
public class DebugLine {


  public string Name { get; set; }
  public Vector3 StartPoint { get; set; }
  public Vector3 EndPoint { get; set; }
  public Color LineColor { get; set; }
  public bool ShallDraw { get; set; }

  public DebugLine(Vector3 startPoint, Vector3 endPoint)
  {
    this.StartPoint = startPoint;
    this.EndPoint = endPoint;
    LineColor = Color.black;
    ShallDraw = true;
  }

  public void Draw()
  {
    if (ShallDraw)
    {
      Debug.DrawLine(StartPoint, EndPoint, LineColor, 0.1f);
    }
  }





}

