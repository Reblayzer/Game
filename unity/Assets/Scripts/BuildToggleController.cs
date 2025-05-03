using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class BuildToggleController : MonoBehaviour
{
    [Header("UI References")]
    public Toggle buildToggle;
    public GameObject scrollView;
    public GameObject blueprintInfoContainer;
    public GameObject plotInfoPanel;
    public GameObject buyPlotInfoPanel;
    public GameObject buildInfoPanel;

    void Awake()
    {
        // --- clear any “Missing” pointers so we don't crash below ---
        try { var _ = scrollView; }
        catch (UnassignedReferenceException) { scrollView = null; }

        try { var _ = blueprintInfoContainer; }
        catch (UnassignedReferenceException) { blueprintInfoContainer = null; }

        // now safe to wire up the toggle
        if (buildToggle == null)
            buildToggle = GetComponent<Toggle>();

        buildToggle.onValueChanged.AddListener(OnBuildToggleChanged);
        OnBuildToggleChanged(buildToggle.isOn);
    }

    void OnDestroy()
    {
        buildToggle.onValueChanged.RemoveListener(OnBuildToggleChanged);
        UnsubscribeBlueprintToggles();
    }

    private void OnBuildToggleChanged(bool isOn)
    {
        // scrollView and blueprintInfoContainer may now be *null* without crashing
        if (scrollView != null) scrollView.SetActive(isOn);
        if (blueprintInfoContainer != null) blueprintInfoContainer.SetActive(isOn);

        if (isOn)
        {
            plotInfoPanel?.SetActive(true);
            buyPlotInfoPanel?.SetActive(false);
            buildInfoPanel?.SetActive(false);

            UnsubscribeBlueprintToggles();
            SubscribeBlueprintToggles();
        }
        else
        {
            // turn off any individual blueprint toggles living under scrollView
            if (scrollView != null)
                foreach (var tb in scrollView.GetComponentsInChildren<Toggle>())
                    tb.isOn = false;

            UnsubscribeBlueprintToggles();

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
        if (isOn)
        {
            plotInfoPanel?.SetActive(false);
            buyPlotInfoPanel?.SetActive(false);
            buildInfoPanel?.SetActive(true);
        }
        else if (blueprintInfoContainer != null)
        {
            // if none of the blueprint‐toggles are still on, revert to plot info
            bool anyLeftOn = false;
            foreach (var tb in blueprintInfoContainer.GetComponentsInChildren<Toggle>())
            {
                if (tb.isOn) { anyLeftOn = true; break; }
            }

            if (!anyLeftOn)
            {
                buildInfoPanel?.SetActive(false);
                plotInfoPanel?.SetActive(true);
                buyPlotInfoPanel?.SetActive(false);
            }
        }
    }
}
