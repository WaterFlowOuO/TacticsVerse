using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleController : MonoBehaviour
{
    public static BattleController Instance;

    public enum State { Idle, ReadyToSpawn, UnitSelected }
    public State currentState = State.Idle;

    [Header("棋盤配置")]
    public int width = 5;
    public int height = 5;
    public GridTile tilePrefab;
    private GridTile[,] grid;

    [Header("資源系統")]
    public int currentPP = 10;
    public TextMeshProUGUI ppText;

    [Header("單位與預製體")]
    public BoardUnit knightPrefab;
    private BoardUnit selectedUnit;

    void Awake() => Instance = this;

    void Start()
    {
        GenerateGrid();
        UpdatePPUI();
    }

    void GenerateGrid()
    {
        grid = new GridTile[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridTile t = Instantiate(tilePrefab, new Vector3(x * 1.1f, y * 1.1f, 0), Quaternion.identity);
                t.Init(x, y);
                grid[x, y] = t;
            }
        }
    }

    public GridTile GetTile(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height) return grid[x, y];
        return null;
    }

    // UI 按鈕綁定：召喚
    public void OnClickSpawnButton()
    {
        if (currentPP < 3) return;
        currentState = State.ReadyToSpawn;
        HighlightSpawnArea();
    }

    // UI 按鈕綁定：施放技能
    public void OnClickSkillButton()
    {
        if (currentState == State.UnitSelected && selectedUnit != null)
        {
            if (currentPP >= selectedUnit.skillCost)
            {
                currentPP -= selectedUnit.skillCost;
                selectedUnit.CastSkillAoE();
                UpdatePPUI();
                ResetState();
            }
        }
    }

    public void OnTileClicked(GridTile tile)
    {
        switch (currentState)
        {
            case State.ReadyToSpawn:
                // 限制只能在底線 (y == 0) 召喚
                if (tile.y == 0 && tile.occupyingUnit == null)
                {
                    BoardUnit unit = Instantiate(knightPrefab);
                    unit.Setup(tile);
                    currentPP -= 3;
                    UpdatePPUI();
                    ResetState();
                }
                break;

            case State.UnitSelected:
                int dist = Mathf.Abs(tile.x - selectedUnit.currentTile.x) + Mathf.Abs(tile.y - selectedUnit.currentTile.y);
                // 空格且在移動範圍內：移動
                if (tile.occupyingUnit == null && dist <= selectedUnit.moveRange && currentPP >= 1)
                {
                    selectedUnit.MoveTo(tile);
                    currentPP -= 1;
                    UpdatePPUI();
                    ResetState();
                }
                // 目標格有敵人且在射程內：普通攻擊
                else if (tile.occupyingUnit != null && tile.occupyingUnit != selectedUnit && dist <= 1 && currentPP >= 1)
                {
                    tile.occupyingUnit.TakeDamage(selectedUnit.atk);
                    currentPP -= 1;
                    UpdatePPUI();
                    ResetState();
                }
                else
                {
                    ResetState();
                }
                break;

            case State.Idle:
                if (tile.occupyingUnit != null)
                {
                    selectedUnit = tile.occupyingUnit;
                    currentState = State.UnitSelected;
                    HighlightMoveRange(tile, selectedUnit.moveRange);
                }
                break;
        }
    }

    void HighlightSpawnArea()
    {
        ClearHighlights();
        for (int x = 0; x < width; x++)
            if (grid[x, 0].occupyingUnit == null) grid[x, 0].SetHighlight(Color.cyan);
    }

    void HighlightMoveRange(GridTile center, int range)
    {
        ClearHighlights();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int dist = Mathf.Abs(x - center.x) + Mathf.Abs(y - center.y);
                if (dist <= range && grid[x, y].occupyingUnit == null)
                    grid[x, y].SetHighlight(Color.green);
            }
        }
    }

    public void ResetState()
    {
        currentState = State.Idle;
        selectedUnit = null;
        ClearHighlights();
    }

    void ClearHighlights()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y].ClearHighlight();
    }

    void UpdatePPUI()
    {
        if (ppText) ppText.text = $"PP: {currentPP}";
    }
}