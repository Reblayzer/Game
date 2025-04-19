using UnityEngine;

[ExecuteInEditMode]
public class SetLayerIgnoreRaycast : MonoBehaviour
{
  void Start()
  {
    gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    foreach (Transform child in transform)
    {
      child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }
    Debug.Log($"🧼 {gameObject.name} and children set to Ignore Raycast.");
  }
}
