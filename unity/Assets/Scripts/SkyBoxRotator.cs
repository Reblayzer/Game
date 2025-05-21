using UnityEngine;

[ExecuteAlways]
public class SkyboxRotator : MonoBehaviour
{
  [Tooltip("Degrees per second.")]
  public float rotationSpeed = 0.1f;

  private Material sky;

  void OnEnable()
  {
    sky = RenderSettings.skybox;
  }

  void Update()
  {
    if (sky == null) return;
    float rot = sky.GetFloat("_Rotation") + rotationSpeed * Time.deltaTime;
    sky.SetFloat("_Rotation", rot % 360f);
  }
}
