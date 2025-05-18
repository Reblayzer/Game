using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class EmissionPulseManager : MonoBehaviour
{
  [Header("Global Pulse Settings")]
  [Tooltip("Lowest emissive intensity (can be negative)")]
  public float minIntensity = -1f;
  [Tooltip("Highest emissive intensity")]
  public float maxIntensity = 3f;
  [Tooltip("How fast it pulses (cycles per second)")]
  public float speed = 1f;

  // Holds data for each emissive sub-material
  private class Rock
  {
    public Material mat;
    public Color unitColor;
    public float phaseOffset;
  }

  private List<Rock> rocks = new List<Rock>();

  void Awake()
  {
    // Find every Renderer in our hierarchy
    foreach (var rend in GetComponentsInChildren<Renderer>(true))
    {
      // Grab both shared and instanced arrays
      var sharedMats = rend.sharedMaterials;
      var instanceMats = rend.materials;

      for (int i = 0; i < sharedMats.Length; i++)
      {
        var shared = sharedMats[i];
        if (shared == null)
          continue;

        // Must have an _EmissionColor property…
        if (!shared.HasProperty("_EmissionColor"))
          continue;

        // …and the emission keyword must be ON in that shared material
        if (!shared.IsKeywordEnabled("_EMISSION"))
          continue;

        // OK, this sub-material is truly emissive: grab our unique instance
        var mat = instanceMats[i];
        mat.EnableKeyword("_EMISSION");

        // Sample its current HDR emission and normalize to a “unit” color
        Color c = mat.GetColor("_EmissionColor");
        float maxComp = Mathf.Max(Mathf.Abs(c.r), Mathf.Abs(c.g), Mathf.Abs(c.b));
        Color unit = (maxComp > 0f) ? (c / maxComp) : Color.white;

        // Give it a random start‐phase so they don’t pulse in lock-step
        float phase = Random.Range(0f, 2f);

        rocks.Add(new Rock
        {
          mat = mat,
          unitColor = unit,
          phaseOffset = phase
        });
      }
    }
  }

  void Update()
  {
    float t = Time.time * speed;

    // Drive each emissive mat independently
    foreach (var rock in rocks)
    {
      // PingPong from 0→1→0 plus our random offset
      float p = Mathf.PingPong(t + rock.phaseOffset, 1f);
      float intensity = Mathf.Lerp(minIntensity, maxIntensity, p);

      rock.mat.SetColor("_EmissionColor", rock.unitColor * intensity);
    }
  }
}
