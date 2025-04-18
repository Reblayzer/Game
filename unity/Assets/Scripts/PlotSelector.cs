using UnityEngine;

public class PlotSelector : MonoBehaviour
{
  public BuildingButtonSelector buttonSelector;
  public CameraController cameraController; // Drag your CameraRig's CameraController here

  void Update()
  {
    if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out RaycastHit hit))
      {
        GridManager gm = hit.collider.GetComponent<GridManager>();
        if (gm != null)
        {
          buttonSelector.SetActiveGridManager(gm);

          // Move the camera pivot to this plot center
          if (cameraController != null)
          {
            cameraController.target.position = gm.transform.position;
          }
        }
      }
    }
  }
}
