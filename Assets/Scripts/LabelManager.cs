using UnityEngine;

public class LabelManager : Singleton<LabelManager>
{
    public int NumberOfRayCasts { get; set; }
    public Vector3 CameraPivotOffset { get; set; }

    private void OnGUI()
    {
        GUI.color = Color.red;
        GUI.Label(new Rect(10, 10, 300, 20), "Camera Raycast-Loops: " + NumberOfRayCasts);
        GUI.Label(new Rect(10, 30, 300, 20), "Camera Pivot Offset: " + CameraPivotOffset.ToString("F2"));
    }
}
