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
        // make sure we have our own Toggle
        if (buildToggle == null)
            buildToggle = GetComponent<Toggle>();

        // only fetch the ToggleGroup if someone wired up a ScrollView
        if (scrollView != null)
        {
            scrollViewToggleGroup =
                scrollView.GetComponentInChildren<ToggleGroup>(includeInactive: true);

            if (scrollViewToggleGroup == null)
                Debug.LogWarning("No ToggleGroup found under ScrollView!", this);
            else
            {
                // wire each blueprint‐list toggle
                foreach (var tb in scrollViewToggleGroup.GetComponentsInChildren<Toggle>())
                    tb.onValueChanged.AddListener(OnBlueprintToggleChanged);
            }
        }

        // only wire the build toggle callback once
        buildToggle.onValueChanged.AddListener(OnBuildToggleChanged);

        // initial state
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
        // Show/hide blueprint list only if it exists
        if (scrollView != null)
            scrollView.SetActive(isOn);

        if (blueprintInfoContainer != null)
            blueprintInfoContainer.SetActive(isOn);

        // clear any selected blueprint
        if (scrollViewToggleGroup != null)
            scrollViewToggleGroup.SetAllTogglesOff();

        if (isOn)
        {
            // hide all world‐markers
            foreach (var pt in Object.FindObjectsByType<PlotTriggerController>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (pt.markerCanvas != null)
                    pt.markerCanvas.SetActive(false);
            }

            // show only the base plot info
            plotInfoPanel?.SetActive(true);
            buyPlotInfoPanel?.SetActive(false);
            buildInfoPanel?.SetActive(false);
        }
        else
        {
            // exiting build mode: back to base plot info
            plotInfoPanel?.SetActive(true);
            buyPlotInfoPanel?.SetActive(false);
            buildInfoPanel?.SetActive(false);
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
        if (!isOn)
        {
            // if nothing else is on, back to plot info
            if (!(scrollViewToggleGroup?.ActiveToggles().Any() ?? false))
            {
                buildInfoPanel?.SetActive(false);
                buyPlotInfoPanel?.SetActive(false);
                plotInfoPanel?.SetActive(true);
            }
            return;
        }

        // picking a blueprint → show build info
        plotInfoPanel?.SetActive(false);
        buyPlotInfoPanel?.SetActive(false);
        buildInfoPanel?.SetActive(true);
    }

    public void HideBlueprintInfoContainer()
    {
        if (blueprintInfoContainer != null)
        {
            blueprintInfoContainer.SetActive(false);
        }
    }
}
