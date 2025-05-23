using UnityEngine;

[ExecuteAlways]
public class SurroundingLayerController : MonoBehaviour
{
  [Header("References")]
  [Tooltip("Reference to your PlotManager in the scene.")]
  public PlotManager plotManager;

  [Header("Settings")]
  [Tooltip("Parent transform containing layer rings and their decorations (Layer1, Layer2, Layer3 and Layer1Decorations, etc.).")]
  public Transform layersParent;

  private void Awake()
  {
    // Use the non-obsolete API
    if (plotManager == null)
      plotManager = Object.FindFirstObjectByType<PlotManager>();

    if (layersParent == null)
      Debug.LogError("Layers Parent is not assigned on SurroundingLayerController.");
  }

  private void Start()
  {
    UpdateSurroundingLayers();
  }

  // Call this whenever your PlotManager layout changes size.
  public void UpdateSurroundingLayers()
  {
    int rows = plotManager.plotRows;
    int cols = plotManager.plotCols;

    // Determine which rings should be visible
    bool showLayer1 = true;
    bool showLayer2 = true;
    bool showLayer3 = true;

    if (rows >= 30 && cols >= 30)
      showLayer1 = false;
    if (rows >= 50 && cols >= 50)
      showLayer2 = false;
    // Layer3 remains true for all sizes by default

    // Apply ring visibility
    SetLayerActive(1, showLayer1);
    SetLayerActive(2, showLayer2);
    SetLayerActive(3, showLayer3);

    // Apply decorations: show the innermost visible ring's decorations
    UpdateDecorations(showLayer1, showLayer2, showLayer3);
  }

  // Toggles the entire Layer{index} GameObject on or off.
  private void SetLayerActive(int index, bool isActive)
  {
    string layerName = "Layer" + index;
    Transform layerTf = layersParent.Find(layerName);
    if (layerTf != null)
      layerTf.gameObject.SetActive(isActive);
    else
      Debug.LogWarning($"Could not find {layerName} under {layersParent.name}.");
  }

  // Shows only the decorations for the first visible layer (1, 2, or 3).
  private void UpdateDecorations(bool layer1Visible, bool layer2Visible, bool layer3Visible)
  {
    int decoToShow = layer1Visible ? 1
                    : layer2Visible ? 2
                    : layer3Visible ? 3
                    : -1;

    for (int i = 1; i <= 3; i++)
    {
      string decoName = "Layer" + i + "Decorations";
      Transform decoTf = layersParent.Find(decoName);
      if (decoTf != null)
        decoTf.gameObject.SetActive(i == decoToShow);
    }
  }
}
