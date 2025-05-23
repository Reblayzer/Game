using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// your existing MaterialConfig
[System.Serializable]
public class MaterialConfig
{
  public Sprite iconSprite;
  public float spawnInterval = 1f;
  public bool isMined = true;
}

[RequireComponent(typeof(Canvas))]
public class MiningDrillUI : MonoBehaviour
{
  [Header("Canvas & Prefab")]
  [Tooltip("Drag your MiningRateCanvas (child RectTransform) here")]
  [SerializeField] private RectTransform canvasTransform;
  [SerializeField] private GameObject floatingUIPrefab;

  [Header("Per-Material Settings")]
  [SerializeField] private List<MaterialConfig> materials;
  public IReadOnlyList<MaterialConfig> Materials => materials;

  [Header("Spawn & Lifetime")]
  [SerializeField] private Vector2 startAnchoredPosition = Vector2.zero;
  [SerializeField] private float groupLifetime = 1f;

  [Header("Layout")]
  [Tooltip("Gap between symbol/amount and icons")]
  [SerializeField] private float symbolAmountSpacing = 2f;
  [SerializeField] private float iconSpacing = 5f;
  [SerializeField] private Vector2 iconSize = new Vector2(15, 15);

  private MiningDrillData _data;
  private int[] _pendingCounts;
  public float FadeInDuration => groupLifetime * 0.1f;

  void Awake()
  {
    // grab the DrillData
    _data = GetComponentInParent<MiningDrillData>();
    if (_data == null)
    {
      Debug.LogError("MiningDrillUI: no MiningDrillData in parent!", this);
      enabled = false;
      return;
    }

    // find the world-space canvas
    if (canvasTransform == null)
      canvasTransform = transform.Find("MiningRateCanvas") as RectTransform;
    if (canvasTransform == null)
    {
      Debug.LogError("MiningDrillUI: missing child RectTransform named 'MiningRateCanvas'.", this);
      enabled = false;
      return;
    }

    _pendingCounts = new int[materials.Count];
  }

  void Start()
  {
    // start one coroutine per mined‐material
    for (int i = 0; i < materials.Count; i++)
      if (materials[i].isMined)
        StartCoroutine(PerMaterialLoop(materials[i], i));

    StartCoroutine(GroupLoop());
  }

  private IEnumerator PerMaterialLoop(MaterialConfig mat, int idx)
  {
    yield return new WaitForSeconds(mat.spawnInterval);
    while (true)
    {
      _pendingCounts[idx]++;
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
        if (!materials[i].isMined) continue;
        for (int c = 0; c < _pendingCounts[i]; c++)
          iconsToShow.Add(materials[i].iconSprite);
        _pendingCounts[i] = 0;
      }

      if (iconsToShow.Count > 0)
        SpawnCombinedUI(iconsToShow);
    }
  }

  private void SpawnCombinedUI(List<Sprite> icons)
  {
    // **always** pull from your data
    string symbol = _data.PopupSymbol;
    int amount = _data.PopupAmount;
    var style = _data.RateCanvasStyle;

    // instantiate under the rate canvas
    var go = Instantiate(floatingUIPrefab, canvasTransform, false);
    var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
    var rt = go.GetComponent<RectTransform>();

    // clear any baked-in children
    foreach (Transform t in go.transform)
      Destroy(t.gameObject);

    float currentX = 0f;

    // 1) SYMBOL
    var symGO = new GameObject("Symbol", typeof(RectTransform));
    symGO.transform.SetParent(go.transform, false);
    var symRT = symGO.GetComponent<RectTransform>();
    symRT.pivot = new Vector2(0, .5f);
    var symText = symGO.AddComponent<TextMeshProUGUI>();
    symText.font = style.Font;
    symText.fontSize = style.FontSize;
    symText.color = style.Color;
    symText.alignment = TextAlignmentOptions.MidlineLeft;
    symText.text = symbol;
    symText.ForceMeshUpdate();

    // size the RectTransform to exactly fit the text
    var symSize = symText.GetRenderedValues(false);
    symRT.sizeDelta = symSize;

    float symW = symText.GetRenderedValues(false).x;
    symRT.anchoredPosition = new Vector2(currentX, 0f);
    currentX += symW + symbolAmountSpacing;

    // 2) AMOUNT
    var amtGO = new GameObject("Amount", typeof(RectTransform));
    amtGO.transform.SetParent(go.transform, false);
    var amtRT = amtGO.GetComponent<RectTransform>();
    amtRT.pivot = new Vector2(0, .5f);
    var amtText = amtGO.AddComponent<TextMeshProUGUI>();
    amtText.font = style.Font;
    amtText.fontSize = style.FontSize;
    amtText.color = style.Color;
    amtText.alignment = TextAlignmentOptions.MidlineLeft;
    amtText.text = amount.ToString();
    amtText.ForceMeshUpdate();

    // size the RectTransform to exactly fit the text
    var amtSize = amtText.GetRenderedValues(false);
    amtRT.sizeDelta = amtSize;

    float amtW = amtText.GetRenderedValues(false).x;
    amtRT.anchoredPosition = new Vector2(currentX, 0f);
    currentX += amtW + iconSpacing;

    // 3) ICONS
    for (int i = 0; i < icons.Count; i++)
    {
      var iconGO = new GameObject($"Icon{i}", typeof(RectTransform), typeof(Image));
      iconGO.transform.SetParent(go.transform, false);
      var iconRT = iconGO.GetComponent<RectTransform>();
      iconRT.pivot = new Vector2(0, .5f);
      iconRT.sizeDelta = iconSize;
      iconRT.anchoredPosition = new Vector2(currentX, 0f);
      var img = iconGO.GetComponent<Image>();
      img.sprite = icons[i];
      img.preserveAspect = true;
      currentX += iconSize.x + iconSpacing;
    }

    // animate fade-in/out and destroy
    rt.anchoredPosition = startAnchoredPosition;
    cg.alpha = 0f;
    StartCoroutine(AnimateAndDestroy(cg, rt, groupLifetime, go));
  }

  private IEnumerator AnimateAndDestroy(CanvasGroup cg, RectTransform rt, float lifetime, GameObject go)
  {
    float half = lifetime * .5f;
    Vector2 start = startAnchoredPosition;
    float h = canvasTransform.rect.height * .5f;
    Vector2 mid = start + Vector2.up * (h * .5f),
            end = start + Vector2.up * h;

    float t = 0f;
    while (t < half)
    {
      float p = t / half;
      rt.anchoredPosition = Vector2.Lerp(start, mid, p);
      cg.alpha = p;
      t += Time.deltaTime;
      yield return null;
    }

    t = 0f;
    while (t < half)
    {
      float p = t / half;
      rt.anchoredPosition = Vector2.Lerp(mid, end, p);
      cg.alpha = 1f - p;
      t += Time.deltaTime;
      yield return null;
    }

    Destroy(go);
  }
}
