using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MaterialConfig
{
  public Sprite iconSprite;
  public float spawnInterval = 1f;
}

public class MiningDrillUI : MonoBehaviour
{
  [Header("Canvas & Prefab")]
  [SerializeField] private RectTransform canvasTransform;
  [SerializeField] private GameObject floatingUIPrefab;

  [Header("Per-Material Settings")]
  [SerializeField] private List<MaterialConfig> materials;

  [Header("Spawn & Lifetime")]
  [SerializeField] private Vector2 startAnchoredPosition = Vector2.zero;
  [SerializeField] private float groupLifetime = 1f;

  [Header("Popup Symbol & Amount")]
  [Tooltip("The symbol prefix (e.g. “+”)")]
  [SerializeField] private string popupSymbol = "+";
  [Tooltip("The numeric amount to show")]
  [SerializeField] private int popupAmount = 1;
  [Tooltip("Space between symbol and amount text")]
  [SerializeField] private float symbolAmountSpacing = 2f;

  [Header("Text Style")]
  [SerializeField] private TMP_FontAsset popupFont;
  [SerializeField] private float popupFontSize = 20f;
  [SerializeField] private Color popupTextColor = Color.white;

  [Header("Icon Layout")]
  [SerializeField] private float iconSpacing = 5f;
  [SerializeField] private Vector2 iconSize = new Vector2(15, 15);

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
    // 1) Instantiate container
    var go = Instantiate(floatingUIPrefab, canvasTransform, false);
    var cg = go.GetComponent<CanvasGroup>();
    var rt = go.GetComponent<RectTransform>();

    // 2) Clear old children
    foreach (Transform t in go.transform) Destroy(t.gameObject);

    // 3) Create the symbol TMP
    float currentX = 0f;
    var symGO = new GameObject("Symbol", typeof(RectTransform));
    symGO.transform.SetParent(go.transform, false);
    var symRT = symGO.GetComponent<RectTransform>();
    symRT.pivot = new Vector2(0, .5f);

    var symTMP = symGO.AddComponent<TextMeshProUGUI>();
    symTMP.text = popupSymbol;
    symTMP.font = popupFont;
    symTMP.fontSize = popupFontSize;
    symTMP.color = popupTextColor;
    symTMP.alignment = TextAlignmentOptions.MidlineLeft;
    symTMP.ForceMeshUpdate();
    float symW = symTMP.GetRenderedValues(false).x;

    symRT.anchoredPosition = new Vector2(currentX, 0);
    currentX += symW + symbolAmountSpacing;

    // 4) Create the amount TMP
    var amtGO = new GameObject("Amount", typeof(RectTransform));
    amtGO.transform.SetParent(go.transform, false);
    var amtRT = amtGO.GetComponent<RectTransform>();
    amtRT.pivot = new Vector2(0, .5f);

    var amtTMP = amtGO.AddComponent<TextMeshProUGUI>();
    amtTMP.text = popupAmount.ToString();
    amtTMP.font = popupFont;
    amtTMP.fontSize = popupFontSize;
    amtTMP.color = popupTextColor;
    amtTMP.alignment = TextAlignmentOptions.MidlineLeft;
    amtTMP.ForceMeshUpdate();
    float amtW = amtTMP.GetRenderedValues(false).x;

    amtRT.anchoredPosition = new Vector2(currentX, 0);
    currentX += amtW + iconSpacing;

    // 5) Spawn icons after that
    for (int i = 0; i < icons.Count; i++)
    {
      var spr = icons[i];
      var iconGO = new GameObject("icon", typeof(RectTransform), typeof(Image));
      iconGO.transform.SetParent(go.transform, false);

      var iconRT = iconGO.GetComponent<RectTransform>();
      iconRT.pivot = new Vector2(0, .5f);
      iconRT.sizeDelta = iconSize;
      iconRT.anchoredPosition = new Vector2(currentX, 0);

      var img = iconGO.GetComponent<Image>();
      img.sprite = spr;
      img.preserveAspect = true;

      currentX += iconSize.x + iconSpacing;
    }

    // 6) Animate & destroy
    rt.anchoredPosition = startAnchoredPosition;
    cg.alpha = 0f;
    StartCoroutine(AnimateAndDestroy(cg, rt, groupLifetime, go));
  }

  private IEnumerator AnimateAndDestroy(CanvasGroup cg, RectTransform rt, float lifetime, GameObject go)
  {
    float half = lifetime * 0.5f;
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
      cg.alpha = 1 - p;
      t += Time.deltaTime;
      yield return null;
    }
    Destroy(go);
  }
}
