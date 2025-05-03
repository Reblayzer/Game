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

    void Awake()
    {
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
        if (scrollView != null) scrollView.SetActive(isOn);
        if (blueprintInfoContainer != null) blueprintInfoContainer.SetActive(isOn);

        if (isOn)
        {
            plotInfoPanel?.SetActive(true);
            buyPlotInfoPanel?.SetActive(false);
            buildInfoPanel?.SetActive(false);

            SubscribeBlueprintToggles();
        }
        else
        {
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
        else
        {
            bool anyLeft = false;
            if (blueprintInfoContainer != null)
            {
                foreach (var tb in blueprintInfoContainer.GetComponentsInChildren<Toggle>())
                    if (tb.isOn) { anyLeft = true; break; }
            }

            if (!anyLeft)
            {
                buildInfoPanel?.SetActive(false);
                plotInfoPanel?.SetActive(true);
                buyPlotInfoPanel?.SetActive(false);
            }
        }
    }

    public void HideAllBuildUI()
    {
        if (scrollView != null) scrollView.SetActive(false);
        if (blueprintInfoContainer != null)
        {
            blueprintInfoContainer.SetActive(false);
            // clear all toggles so none stay checked
            foreach (var tb in blueprintInfoContainer.GetComponentsInChildren<Toggle>())
                tb.isOn = false;
        }
        buildInfoPanel?.SetActive(false);
    }
}
