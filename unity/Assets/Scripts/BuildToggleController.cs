using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class BuildToggleController : MonoBehaviour
{
    [Header("UI References (optional)")]
    public Toggle buildToggle;
    public GameObject scrollView;
    public GameObject blueprintInfoContainer;

    [Header("Panels")]
    public GameObject plotInfoPanel;
    public GameObject buyPlotInfoPanel;
    public GameObject buildInfoPanel;

    private ToggleGroup scrollViewToggleGroup;

    void Awake()
    {
        buildToggle ??= GetComponent<Toggle>();

        // pull the ToggleGroup straight from the ScrollView’s content
        scrollViewToggleGroup = scrollView
          ?.GetComponentInChildren<ToggleGroup>(includeInactive: true);

        if (scrollViewToggleGroup == null)
            Debug.LogWarning("No ToggleGroup found under ScrollView!", this);

        buildToggle.onValueChanged.AddListener(OnBuildToggleChanged);

        // wire each blueprint toggle callback
        if (scrollViewToggleGroup != null)
        {
            foreach (var tb in scrollViewToggleGroup.GetComponentsInChildren<Toggle>())
                tb.onValueChanged.AddListener(OnBlueprintToggleChanged);
        }

        OnBuildToggleChanged(buildToggle.isOn);
    }

    void OnDestroy()
    {
        buildToggle?.onValueChanged.RemoveListener(OnBuildToggleChanged);

        if (scrollViewToggleGroup != null)
        {
            foreach (var tb in scrollViewToggleGroup.GetComponentsInChildren<Toggle>())
                tb.onValueChanged.RemoveListener(OnBlueprintToggleChanged);
        }
    }

    private void OnBuildToggleChanged(bool isOn)
    {
        // Show/hide the list of blueprints and its info container
        scrollView?.SetActive(isOn);
        blueprintInfoContainer?.SetActive(isOn);

        // Always clear any previously selected blueprint
        scrollViewToggleGroup?.SetAllTogglesOff();

        if (isOn)
        {
            // Entering build mode: hide all world‐markers
            foreach (var pt in Object.FindObjectsByType<PlotTriggerController>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                pt.markerCanvas?.SetActive(false);
            }

            // Show just the base plot info
            buildInfoPanel?.SetActive(false);
            buyPlotInfoPanel?.SetActive(false);
            plotInfoPanel?.SetActive(true);
        }
        else
        {
            // Exiting build mode: reset panels
            buildInfoPanel?.SetActive(false);
            buyPlotInfoPanel?.SetActive(false);
            plotInfoPanel?.SetActive(true);
        }
    }

    private void SubscribeBlueprintToggles()
    {
        if (blueprintInfoContainer == null) return;
        foreach (var tb in blueprintInfoContainer.GetComponentsInChildren<Toggle>())
        {
            tb.isOn = false;
            tb.onValueChanged.AddListener(OnBlueprintToggleChanged);
        }
    }

    private void UnsubscribeBlueprintToggles()
    {
        if (blueprintInfoContainer == null) return;
        foreach (var tb in blueprintInfoContainer.GetComponentsInChildren<Toggle>())
        {
            tb.onValueChanged.RemoveListener(OnBlueprintToggleChanged);
            tb.isOn = false;
        }
    }

    private void OnBlueprintToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // A blueprint was selected → show build info
            plotInfoPanel?.SetActive(false);
            buyPlotInfoPanel?.SetActive(false);
            buildInfoPanel?.SetActive(true);
        }
        else if (!(scrollViewToggleGroup?.ActiveToggles().Any() ?? false))
        {
            // No blueprint selected → back to plot info
            buildInfoPanel?.SetActive(false);
            buyPlotInfoPanel?.SetActive(false);
            plotInfoPanel?.SetActive(true);
        }
    }

    public void HideBlueprintInfoContainer()
    {
        if (blueprintInfoContainer != null)
        {
            blueprintInfoContainer.SetActive(false);
        }
    }
}
