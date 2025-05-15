using UnityEngine;
using System.Linq;   // only if you really want to use .Max()

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class TileWithNoise : MonoBehaviour
{
  [Header("Noise Settings")]
  public float noiseScale = 5f;    // higher = more bumps per tile
  public float heightAmp = 0.2f;  // bump height

  void Awake()
  {
    // 1) Get a unique mesh instance
    var mf = GetComponent<MeshFilter>();
    Mesh mesh = mf.mesh;      // this clones sharedMesh the first time
    mf.mesh = mesh;           // assign it back just to be explicit

    // 2) Extract verts and find the top-face Y value
    var verts = mesh.vertices;
    float maxY = verts.Max(v => v.y);

    // 3) Displace every vertex that was on the original top face
    float eps = 0.0001f;
    for (int i = 0; i < verts.Length; i++)
    {
      if (Mathf.Abs(verts[i].y - maxY) < eps)
      {
        // world-space position so neighbors line up
        Vector3 worldPt = transform.TransformPoint(verts[i]);
        float n = Mathf.PerlinNoise(worldPt.x * noiseScale,
                                    worldPt.z * noiseScale);

        // center around zero and apply
        verts[i].y = maxY + (n - 0.5f) * heightAmp;
      }
    }

    // 4) Write it back and rebuild everything
    mesh.vertices = verts;
    mesh.RecalculateNormals();
    mesh.RecalculateBounds();
    GetComponent<MeshCollider>().sharedMesh = mesh;
  }
}