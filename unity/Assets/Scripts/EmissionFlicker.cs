using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.Rendering;
#endif

[System.Serializable]
public class FlickerSetting
{
  [Tooltip("Substring to match against Material.name")]
  public string materialNameContains;

  [Tooltip("Top brightness when fully ON")]
  public float maxIntensity = 1f;

  [Tooltip("How fast the flicker wanders through Perlin Noise")]
  public float flickerSpeed = 1f;

  [Range(0f, 1f)]
  [Tooltip("Below this noise value, emission is forced OFF")]
  public float offThreshold = 0.2f;

  [Range(0f, 1f)]
  [Tooltip("Above this noise value, emission is forced fully ON")]
  public float onThreshold = 0.8f;

  // runtime
  [HideInInspector] public List<Material> materials;
  [HideInInspector] public List<Color> baseColors;
}

public class EmissionFlicker : MonoBehaviour
{
  [Header("Configure one entry per material-type")]
  public FlickerSetting[] settings;

  void Start()
  {
    foreach (var rend in GetComponentsInChildren<Renderer>(true))
    {
      var mats = rend.materials;
      for (int i = 0; i < mats.Length; i++)
      {
        var m = mats[i];
        foreach (var set in settings)
        {
          if (m.name.Contains(set.materialNameContains))
          {
            if (set.materials == null)
            {
              set.materials = new List<Material>();
              set.baseColors = new List<Color>();
            }

            m.EnableKeyword("_EMISSION");

            // store the “unit” emission color
            Color c = m.GetColor("_EmissionColor");
            float origBright = Mathf.Max(c.r, c.g, c.b);
            Color unitColor = (origBright > 0f) ? c / origBright : c;

            set.materials.Add(m);
            set.baseColors.Add(unitColor);
          }
        }
      }
    }
  }

  void Update()
  {
    float t = Time.time;

    foreach (var set in settings)
    {
      if (set.materials == null) continue;

      for (int i = 0; i < set.materials.Count; i++)
      {
        var m = set.materials[i];
        var unitColor = set.baseColors[i];

        float noise = Mathf.PerlinNoise(t * set.flickerSpeed, i * 37f);
        float intensity;

        if (noise < set.offThreshold)
        {
          intensity = 0f;
        }
        else if (noise > set.onThreshold)
        {
          intensity = set.maxIntensity;
        }
        else
        {
          // ramp between offThreshold…onThreshold → 0…maxIntensity
          float tNorm = (noise - set.offThreshold)
                           / (set.onThreshold - set.offThreshold);
          intensity = Mathf.Lerp(0f, set.maxIntensity, tNorm);
        }

        m.SetColor("_EmissionColor", unitColor * intensity);

        // If you want dynamic GI updates, you could also:
        // DynamicGI.SetEmissive(rend, unitColor * intensity);
      }
    }
  }
}
