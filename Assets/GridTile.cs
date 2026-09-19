using UnityEngine;

public class GridTile : MonoBehaviour
{
    public int x, y;
    public BoardUnit occupyingUnit;
    private SpriteRenderer sr;
    private Color defaultColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    void Awake() => sr = GetComponent<SpriteRenderer>();

    public void Init(int posX, int posY)
    {
        x = posX;
        y = posY;
        sr.color = defaultColor;
    }

    public void SetHighlight(Color color) => sr.color = color;
    public void ClearHighlight() => sr.color = defaultColor;

    void OnMouseDown() => BattleController.Instance.OnTileClicked(this);
}