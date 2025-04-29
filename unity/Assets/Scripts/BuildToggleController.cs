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

    [Tooltip("The parent GameObject of all the individual Blueprint info panels.")]
    public GameObject blueprintInfoContainer;

    void Awake()
    {
        if (buildToggle == null)
            buildToggle = GetComponent<Toggle>();

        // Whenever buildToggle flips, we update both ScrollView and the info‐container
        buildToggle.onValueChanged.AddListener(OnBuildToggleChanged);

        // initialize to current state
        OnBuildToggleChanged(buildToggle.isOn);
    }

    void OnDestroy()
    {
        buildToggle.onValueChanged.RemoveListener(OnBuildToggleChanged);
    }

    private void OnBuildToggleChanged(bool isOn)
    {
        // show/hide the scroll view
        if (scrollView != null)
            scrollView.SetActive(isOn);

        // show/hide the entire block of blueprint info panels
        if (blueprintInfoContainer != null)
            blueprintInfoContainer.SetActive(isOn);

        // **Optional**: if you also want to reset any individual blueprint toggles
        // when turning off, you can un‐check them here:
        if (!isOn && blueprintInfoContainer != null)
        {
            foreach (var tb in blueprintInfoContainer.GetComponentsInChildren<Toggle>())
                tb.isOn = false;
        }
    }
}
