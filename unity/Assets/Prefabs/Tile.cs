using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public GridManager gridManager;

    private Renderer tileRenderer;
    private Color originalColor;

    public void Init(Vector2Int pos, GridManager manager)
    {
        gridPosition = pos;
        gridManager = manager;

        tileRenderer = GetComponent<Renderer>();
        originalColor = tileRenderer.material.color;
    }

    private void OnMouseEnter()
    {
        gridManager.HighlightTiles(gridPosition.x, gridPosition.y);
    }

    private void OnMouseExit()
    {
        gridManager.ClearHighlights();
    }

    private void OnMouseDown()
    {
        gridManager.TryPlaceCuboidAt(gridPosition.x, gridPosition.y);
    }

    public void SetHighlight(Color color)
    {
        tileRenderer.material.color = color;
    }

    public void ResetColor()
    {
        tileRenderer.material.color = originalColor;
    }
}
