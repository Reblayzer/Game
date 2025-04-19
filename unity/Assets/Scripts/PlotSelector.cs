using UnityEngine;
using UnityEngine.EventSystems;

public class PlotSelector : MonoBehaviour
{
  public BuildingButtonSelector buttonSelector;
  public CameraController cameraController;

  void Update()
  {
    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      int plotMask = LayerMask.GetMask("Plot");

      if (Physics.Raycast(ray, out RaycastHit hit, 100f, plotMask))
      {
        GridManager gm = hit.collider.GetComponentInParent<GridManager>();
        if (gm != null)
        {
          buttonSelector.SetActiveGridManager(gm);

          // Move the camera pivot
          if (cameraController != null)
          {
            cameraController.target.position = gm.transform.position;
          }

          // Re-select the current cuboid type to re-enable placement
          buttonSelector.SelectByIndex(buttonSelector.CurrentIndex);

          Debug.Log($"📍 Selected Plot: {gm.name}");
        }
      }
    }
  }
}
