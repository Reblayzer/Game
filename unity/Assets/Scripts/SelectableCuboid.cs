using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableCuboid : MonoBehaviour
{
    // Fired whenever you click a cuboid in Build/Edit mode (or null to clear)
    public static event Action<SelectableCuboid> OnCuboidSelected;
    private static SelectableCuboid _current;

    [Header("Identity")]
    public string cuboidName;
    private int _level = 1;
    public int Level => _level;

    private Toggle _buildToggle, _editToggle;

    void Awake()
    {
        _buildToggle = PlotSelector.Instance.buildToggle;
        _editToggle = EditToggleController.InstanceToggle;
    }

    public static void ClearSelection()
    {
        _current = null;
        OnCuboidSelected?.Invoke(null);
        // also clear any panels
        PlotSelector.Instance.ShowDrillPanels(null);
    }

    void OnMouseDown()
    {
        if (EventSystem.current?.IsPointerOverGameObject() == true) return;

        bool buildMode = _buildToggle.isOn;
        bool editMode = _editToggle.isOn;
        var drill = GetComponent<MiningDrillData>();

        Debug.Log($"[SelectableCuboid] Clicked '{name}': buildMode={buildMode}, editMode={editMode}, drill={(drill != null ? "yes" : "no")}");

        // A) Neither toggle → Collect
        if (!buildMode && !editMode)
        {
            Debug.Log("[SelectableCuboid] → none selected, routing to collect");
            if (drill != null) PlotSelector.Instance.ShowDrillPanels(drill);
            return;
        }

        // B) Build mode → Upgrade
        if (buildMode)
        {
            Debug.Log("[SelectableCuboid] → buildMode, routing to upgrade");
            PlotSelector.Instance.ShowDrillPanels(drill);
        }
        // C) Edit mode → just select for shield/UI, don’t change panels
        else if (editMode)
        {
            Debug.Log("[SelectableCuboid] → editMode, only firing OnCuboidSelected");
        }

        _current = this;
        OnCuboidSelected?.Invoke(this);
    }


    public void Upgrade()
    {
        _level++;
        // if we're still selected, re-fire so any UI updates to the level happen
        if (_current == this)
            OnCuboidSelected?.Invoke(this);
    }
}
