using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class MaterialConfig
{
  public Sprite iconSprite;
  public float spawnInterval = 1f;
  public bool isMined = true;
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
  [SerializeField] private string popupSymbol = "+";
  [SerializeField] private int popupAmount = 1;
  [SerializeField] private float symbolAmountSpacing = 2f;

  [Header("Text Style")]
  [SerializeField] private TMP_FontAsset popupFont;
  [SerializeField] private float popupFontSize = 20f;
  [SerializeField] private Color popupTextColor = Color.white;

  [Header("Icon Layout")]
  [SerializeField] private float iconSpacing = 5f;
  [SerializeField] private Vector2 iconSize = new Vector2(15, 15);

  private int[] _pendingCounts;

  /// <summary>Fired immediately after each new batch of floating icons is spawned.</summary>
  public event Action OnIconsSpawned;

  /// <summary>Half of <see cref="groupLifetime"/>, i.e. how long the fade-in takes.</summary>
  public float FadeInDuration => groupLifetime * 0.1f;

  void Awake()
  {
    _pendingCounts = new int[materials.Count];
  }

  private void Start()
  {
    // only kick off loops for *enabled* materials
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

      // gather sprites
      var iconsToShow = new List<Sprite>();
      for (int i = 0; i < materials.Count; i++)
        if (materials[i].isMined)
        {
          for (int c = 0; c < _pendingCounts[i]; c++)
            iconsToShow.Add(materials[i].iconSprite);
          _pendingCounts[i] = 0;
        }

      if (iconsToShow.Count > 0)
      {
        // spawn and then notify
        SpawnCombinedUI(iconsToShow);
        OnIconsSpawned?.Invoke();
      }
    }
  }

  private void SpawnCombinedUI(List<Sprite> icons)
  {
    // instantiate container
    var go = Instantiate(floatingUIPrefab, canvasTransform, false);
    var cg = go.GetComponent<CanvasGroup>();
    var rt = go.GetComponent<RectTransform>();

    // clear old children
    foreach (Transform t in go.transform)
      Destroy(t.gameObject);

    float currentX = 0f;

    // Symbol
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
    symRT.anchoredPosition = new Vector2(currentX, 0f);
    currentX += symW + symbolAmountSpacing;

    // Amount
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
    amtRT.anchoredPosition = new Vector2(currentX, 0f);
    currentX += amtW + iconSpacing;

    // icons
    for (int i = 0; i < icons.Count; i++)
    {
      var iconGO = new GameObject("icon", typeof(RectTransform), typeof(Image));
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

    // animate & destroy
    rt.anchoredPosition = startAnchoredPosition;
    cg.alpha = 0f;
    StartCoroutine(AnimateAndDestroy(cg, rt, groupLifetime, go));
  }

  private IEnumerator AnimateAndDestroy(CanvasGroup cg, RectTransform rt, float lifetime, GameObject go)
  {
    float half = lifetime * 0.5f;
    Vector2 start = startAnchoredPosition;
    float h = canvasTransform.rect.height * 0.5f;
    Vector2 mid = start + Vector2.up * (h * 0.5f),
            end = start + Vector2.up * h;

    float t = 0f;
    // fade in & move up
    while (t < half)
    {
      float p = t / half;
      rt.anchoredPosition = Vector2.Lerp(start, mid, p);
      cg.alpha = p;
      t += Time.deltaTime;
      yield return null;
    }

    // fade out & continue up
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

  // expose readonly collections:
  public IReadOnlyList<MaterialConfig> Materials => materials;
  public int[] PendingCounts => _pendingCounts;
}
