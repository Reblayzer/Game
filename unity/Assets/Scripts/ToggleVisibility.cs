using UnityEngine;

public class ToggleVisibility : MonoBehaviour
{
    public GameObject target;

    public void SetVisible(bool isOn)
    {
        if (target != null)
            target.SetActive(isOn);
    }
}
