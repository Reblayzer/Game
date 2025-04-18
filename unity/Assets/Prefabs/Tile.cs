using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public GridManager GridManager { get; private set; }

    private Renderer tileRenderer;
    private Color originalColor;

    public void Init(Vector2Int position, GridManager manager)
    {
        gridPosition = position;
        GridManager = manager;

        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalColor = tileRenderer.material.color;
        }
    }

    public void SetHighlight(Color color)
    {
        if (tileRenderer != null)
        {
            tileRenderer.material.color = color;
        }
    }

    public void ResetColor()
    {
        if (tileRenderer != null)
        {
            tileRenderer.material.color = originalColor;
        }
    }
}
