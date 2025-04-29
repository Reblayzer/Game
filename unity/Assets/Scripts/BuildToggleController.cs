using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class BuildToggleController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Toggle component on this icon (will auto-grab if unset).")]
    public Toggle buildToggle;

    [Tooltip("The Scroll View (or any GameObject) you want to show/hide when this is toggled.")]
    public GameObject scrollView;

    void Awake()
    {
        if (buildToggle == null)
            buildToggle = GetComponent<Toggle>();

        buildToggle.onValueChanged.AddListener(SetScrollViewActive);

        SetScrollViewActive(buildToggle.isOn);
    }

    void OnDestroy()
    {
        buildToggle.onValueChanged.RemoveListener(SetScrollViewActive);
    }

    private void SetScrollViewActive(bool isOn)
    {
        if (scrollView != null)
            scrollView.SetActive(isOn);
    }
}
