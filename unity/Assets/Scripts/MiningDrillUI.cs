using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MaterialConfig
{
  [Tooltip("Which sprite to show when this material pops")]
  public Sprite iconSprite;
  [Tooltip("Seconds between +1 for this material")]
  public float spawnInterval = 1f;
}

public class MiningDrillUI : MonoBehaviour
{
  [Header("Canvas & Prefab")]
  [Tooltip("Your world‐space Canvas to parent popups under")]
  [SerializeField] private RectTransform canvasTransform;
  [Tooltip("The empty root prefab (RectTransform + CanvasGroup)")]
  [SerializeField] private GameObject floatingUIPrefab;

  [Header("Per-Material Settings")]
  [Tooltip("One entry per mineral: icon + interval")]
  [SerializeField] private List<MaterialConfig> materials;

  [Header("Spawn & Lifetime")]
  [Tooltip("Where in canvas‐local coords each popup starts")]
  [SerializeField] private Vector2 startAnchoredPosition = Vector2.zero;
  [Tooltip("Total seconds each popup animates")]
  [SerializeField] private float groupLifetime = 1f;

  [Header("Popup Text")]
  [Tooltip("What text to show (e.g. +1)")]
  [SerializeField] private string popupText = "+1";
  [Tooltip("Which TextMeshPro font to use")]
  [SerializeField] private TMP_FontAsset popupFont;
  [Tooltip("Font size in points")]
  [SerializeField] private float popupFontSize = 20f;
  [Tooltip("Color of the popup text")]
  [SerializeField] private Color popupTextColor = Color.white;

  [Header("Icon Layout")]
  [Tooltip("Gap between end of text and first icon")]
  [SerializeField] private float iconSpacing = 5f;
  [Tooltip("Size of each icon in canvas units")]
  [SerializeField] private Vector2 iconSize = new Vector2(15, 15);

  // internal counters
  private int[] _pendingCounts;

  private void Awake()
  {
    _pendingCounts = new int[materials.Count];
  }

  private void Start()
  {
    for (int i = 0; i < materials.Count; i++)
      StartCoroutine(PerMaterialLoop(materials[i], i));

    StartCoroutine(GroupLoop());
  }

  private IEnumerator PerMaterialLoop(MaterialConfig mat, int index)
  {
    yield return new WaitForSeconds(mat.spawnInterval);
    while (true)
    {
      _pendingCounts[index]++;
      yield return new WaitForSeconds(mat.spawnInterval);
    }
  }

  private IEnumerator GroupLoop()
  {
    while (true)
    {
      yield return new WaitForSeconds(1f);

      var iconsToShow = new List<Sprite>();
      for (int i = 0; i < materials.Count; i++)
      {
        int count = _pendingCounts[i];
        var sprite = materials[i].iconSprite;
        for (int j = 0; j < count; j++)
          iconsToShow.Add(sprite);

        _pendingCounts[i] = 0;
      }

      if (iconsToShow.Count > 0)
        SpawnCombinedUI(iconsToShow);
    }
  }

  private void SpawnCombinedUI(List<Sprite> icons)
  {
    // instantiate the empty root under the canvas
    var go = Instantiate(floatingUIPrefab, canvasTransform, false);
    var cg = go.GetComponent<CanvasGroup>();
    var rt = go.GetComponent<RectTransform>();

    // clear any leftover children
    foreach (Transform child in go.transform)
      Destroy(child.gameObject);

    // create the TMP text
    var textGO = new GameObject("PopupText", typeof(RectTransform));
    textGO.transform.SetParent(go.transform, false);

    var tmp = textGO.AddComponent<TextMeshProUGUI>();
    tmp.text = popupText;
    tmp.font = popupFont;
    tmp.fontSize = popupFontSize;
    tmp.color = popupTextColor;
    tmp.alignment = TextAlignmentOptions.MidlineLeft;
    tmp.raycastTarget = false;

    // measure its width
    tmp.ForceMeshUpdate();
    float textWidth = tmp.GetRenderedValues(false).x;

    // position the text at left
    var textRT = textGO.GetComponent<RectTransform>();
    textRT.pivot = new Vector2(0f, 0.5f);
    textRT.anchoredPosition = Vector2.zero;

    // spawn icons in a row immediately to the right
    for (int i = 0; i < icons.Count; i++)
    {
      var sprite = icons[i];
      var iconGO = new GameObject("icon", typeof(RectTransform), typeof(Image));
      iconGO.transform.SetParent(go.transform, false);

      var iconRT = iconGO.GetComponent<RectTransform>();
      iconRT.sizeDelta = iconSize;
      iconRT.pivot = new Vector2(0f, 0.5f);

      float x = textWidth + iconSpacing + i * (iconSize.x + iconSpacing);
      iconRT.anchoredPosition = new Vector2(x, 0f);

      var img = iconGO.GetComponent<Image>();
      img.sprite = sprite;
      img.preserveAspect = true;
    }

    // animate and destroy
    rt.anchoredPosition = startAnchoredPosition;
    cg.alpha = 0f;
    StartCoroutine(AnimateAndDestroy(cg, rt, groupLifetime, go));
  }

  private IEnumerator AnimateAndDestroy(CanvasGroup cg, RectTransform rt, float lifetime, GameObject go)
  {
    float halfTime = lifetime * 0.5f;
    float halfH = canvasTransform.rect.height * 0.5f;
    Vector2 start = startAnchoredPosition;
    Vector2 mid = new Vector2(start.x, halfH * 0.5f);
    Vector2 end = new Vector2(start.x, halfH);

    float t = 0f;
    while (t < halfTime)
    {
      float p = t / halfTime;
      rt.anchoredPosition = Vector2.Lerp(start, mid, p);
      cg.alpha = p;
      t += Time.deltaTime;
      yield return null;
    }

    t = 0f;
    while (t < halfTime)
    {
      float p = t / halfTime;
      rt.anchoredPosition = Vector2.Lerp(mid, end, p);
      cg.alpha = 1f - p;
      t += Time.deltaTime;
      yield return null;
    }

    Destroy(go);
  }
}
