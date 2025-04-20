using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public GridManager GridManager { get; private set; }

    private Renderer tileRenderer;
    private Color originalColor;
    private Color persistentColor;
    private bool hasPersistentColor = false;

    public void Init(Vector2Int position, GridManager manager)
    {
        gridPosition = position;
        GridManager = manager;

        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalColor = tileRenderer.material.color;
            persistentColor = originalColor;
        }
    }

    // 🔵 Used for permanent plot color like green/blue
    public void SetPersistentColor(Color color)
    {
        persistentColor = color;
        hasPersistentColor = true;
        tileRenderer.material.color = color;
    }

    // 🔴 Used for ghost highlight only
    public void SetTemporaryHighlight(Color color)
    {
        tileRenderer.material.color = color;
    }

    public void ClearHighlight()
    {
        if (hasPersistentColor)
            tileRenderer.material.color = persistentColor;
        else
            tileRenderer.material.color = originalColor;
    }
}
