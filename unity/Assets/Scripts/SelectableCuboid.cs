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
    }

    void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        bool buildMode = _buildToggle.isOn;
        bool editMode = _editToggle.isOn;

        // 1) Not in build/edit → only show collect‐panel for drills
        if (!buildMode && !editMode)
        {
            var drill = GetComponent<MiningDrillData>();
            if (drill != null)
                PlotSelector.Instance.ShowCollectPanel(drill);
            return;
        }

        // 2) In build/edit → always hide collect‐panel
        PlotSelector.Instance.HideCollectPanel();

        // 3) Select this cuboid (firing the global event drives your BuildingInfoDisplay)
        _current = this;
        OnCuboidSelected?.Invoke(this);
    }

    public void Upgrade()
    {
        _level++;
        if (_current == this)
            OnCuboidSelected?.Invoke(this);
    }
}
