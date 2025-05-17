using UnityEngine;

[DisallowMultipleComponent]
public class RandomYRotation : MonoBehaviour
{
  // Four possible Yaw angles
  private static readonly float[] YawOptions = { 0f, 90f, 180f, 270f };

  void Start()
  {
    // Pick one at random and apply it
    int idx = Random.Range(0, YawOptions.Length);
    transform.rotation = Quaternion.Euler(0f, YawOptions[idx], 0f);
  }
}
