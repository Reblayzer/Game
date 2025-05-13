using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class BlueprintToggle : MonoBehaviour
{
    public GameObject infoPanel;

    private Toggle _toggle;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnValueChanged);
        // initialize
        OnValueChanged(_toggle.isOn);
    }

    void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(bool isOn)
    {
        var ps = PlotSelector.Instance;
        if (ps == null) return;

        if (isOn)
        {
            ps.HideCollectPanel();
            // you just *opened* this blueprint:
            // 1) hide any plot panels
            ps.plotInfoPanel?.SetActive(false);
            ps.buyPlotInfoPanel?.SetActive(false);
            // 2) show the build‐info container
            ps.buildInfoPanel?.SetActive(true);
        }
        else
        {
            // you *closed* this blueprint toggle again:
            // 1) hide its own detail panel
            //    (infoPanel will be turned off by the next line anyway)
            // 2) hide the build‐info container entirely
            ps.buildInfoPanel?.SetActive(false);

            // 3) restore the correct plot panel based on ownership
            var gm = ps.buttonSelector.GetActiveGridManager();
            if (gm != null && gm.ownership == Ownership.Unclaimed)
                ps.buyPlotInfoPanel?.SetActive(true);
            else
                ps.plotInfoPanel?.SetActive(true);
        }

        // finally, show/hide this blueprint’s own panel
        if (infoPanel != null)
            infoPanel.SetActive(isOn);
    }
}
