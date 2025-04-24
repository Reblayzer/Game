using UnityEngine;

public class ToggleVisibility : MonoBehaviour
{
    public GameObject target;

    public void SetVisible(bool state)
    {
        if (target != null)
            target.SetActive(state);
    }
}
