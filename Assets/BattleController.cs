using UnityEngine;
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

    [Header("回合與雙人系統")]
    public int currentTurn = 1;     
    public int currentTeam = 1;     // 1 = 玩家一(下方)回合, 2 = 玩家二(上方)回合
    public int globalPhase = 1;
    public TextMeshProUGUI turnText;

    [Header("玩家一 (P1) 資源")]
    public int p1PP = 1, p1MaxPP = 1;
    public int p1HP = 20, p1MaxHP = 20;
    public GameObject p1HandUI;     // P1 的手牌群組

    [Header("玩家二 (P2) 資源")]
    public int p2PP = 1, p2MaxPP = 1;
    public int p2HP = 20, p2MaxHP = 20;
    public GameObject p2HandUI;     // P2 的手牌群組

    [Header("共用 UI")]
    public TextMeshProUGUI ppText;       // 顯示當前玩家的 PP
    public TextMeshProUGUI playerHpText; // 左邊顯示 P1 血量，右邊顯示 P2 血量

    [Header("單位與預製體")]
    public BoardUnit knightPrefab;
    public BoardUnit archerPrefab;  
    public BoardUnit minionPrefab;    
    private BoardUnit prefabToSpawn;    
    private BoardUnit selectedUnit;

    [Header("技能 UI")]
    public GameObject skillButtonObj;       
    public TextMeshProUGUI skillButtonText; 

    void Awake() => Instance = this;

    void Start()
    {
        globalPhase = 1;
        p1MaxPP = 1; p1PP = 1;
        p2MaxPP = 0; p2PP = 0;
        GenerateGrid();
        UpdateUI();
        if (skillButtonObj != null) skillButtonObj.SetActive(false);
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

    public void DamagePlayer(int teamToDamage, int amount)
    {
        if (teamToDamage == 1)
        {
            p1HP -= amount;
            if (p1HP <= 0) Debug.Log("玩家 2 獲勝！");
        }
        else
        {
            p2HP -= amount;
            if (p2HP <= 0) Debug.Log("玩家 1 獲勝！");
        }
        UpdateUI();
    }

    // --- 核心：切換回合邏輯 ---
    public void OnClickEndTurn()
    {
        globalPhase++; // 每次按下結束回合，總階段數 +1
        int nextMaxPP = Mathf.Min(globalPhase, 10); // 上限鎖定在 10 點

        if (currentTeam == 1)
        {
            // 換 P2 回合，給予對應的 PP
            currentTeam = 2;
            p2MaxPP = nextMaxPP;
            p2PP = p2MaxPP;
        }
        else
        {
            // 換 P1 回合，完整一輪結束，回合數 +1
            currentTeam = 1;
            currentTurn++;
            p1MaxPP = nextMaxPP;
            p1PP = p1MaxPP;
        }

        BoardUnit[] allUnits = FindObjectsByType<BoardUnit>(FindObjectsInactive.Exclude);
        foreach (BoardUnit unit in allUnits)
        {
            if (unit.team == currentTeam)
            {
                unit.ResetTurnActions(); // 恢復行動次數與顏色
            }
        }

        UpdateUI();
        ResetState();
    }

    // 這些改成吃當前玩家的 PP
    public void OnClickSpawnKnight() { TrySpawn(knightPrefab, 3); }
    public void OnClickSpawnArcher() { TrySpawn(archerPrefab, 3); }
    public void OnClickSpawnMinion() { TrySpawn(minionPrefab, 1); }

    private void TrySpawn(BoardUnit prefab, int cost)
    {
        int currentPP = (currentTeam == 1) ? p1PP : p2PP;
        if (currentPP < cost) return;
        prefabToSpawn = prefab;
        currentState = State.ReadyToSpawn;
        HighlightSpawnArea();
    }

    public void OnClickSkillButton()
    {
        if (currentState == State.UnitSelected && selectedUnit != null && selectedUnit.team == currentTeam)
        {
            int currentPP = (currentTeam == 1) ? p1PP : p2PP;
            if (currentPP >= selectedUnit.skillCost && selectedUnit.skillName != "" && !selectedUnit.hasActed)
            {
                if (currentTeam == 1) p1PP -= selectedUnit.skillCost;
                else p2PP -= selectedUnit.skillCost;

                selectedUnit.CastSkill(); 
                selectedUnit.MarkAttacked();
                UpdateUI();
                ResetState();
            }
        }
    }

    public void OnTileClicked(GridTile tile)
    {
        switch (currentState)
        {
            case State.ReadyToSpawn:
                // P1 只能生在 y=0，P2 只能生在 y=height-1
                int validY = (currentTeam == 1) ? 0 : height - 1;

                if (tile.y == validY && tile.occupyingUnit == null)
                {
                    BoardUnit unit = Instantiate(prefabToSpawn);
                    unit.team = currentTeam; // 賦予陣營
                    
                    // 根據陣營換個顏色區分 (P1:原本顏色, P2:偏紅)
                    if(currentTeam == 2) unit.GetComponent<SpriteRenderer>().color = Color.red;

                    unit.Setup(tile);
                    
                    if (currentTeam == 1) p1PP -= prefabToSpawn.spawnCost;
                    else p2PP -= prefabToSpawn.spawnCost;
                    
                    UpdateUI();
                    ResetState();
                }
                break;

            case State.UnitSelected:
                int dist = Mathf.Abs(tile.x - selectedUnit.currentTile.x) + Mathf.Abs(tile.y - selectedUnit.currentTile.y);
                int currentPP = (currentTeam == 1) ? p1PP : p2PP;

                // 移動：必須是「還沒移動過 (!hasMoved)」且「還沒行動過 (!hasActed)」
                if (tile.occupyingUnit == null && dist <= selectedUnit.moveRange && currentPP >= 1 && !selectedUnit.hasMoved)
                {
                    selectedUnit.MoveTo(tile);
                    selectedUnit.MarkMoved(); // 【新增】：標記為已移動

                    if (currentTeam == 1) p1PP--; else p2PP--;
                    UpdateUI();
                    ResetState();
                }
                // 攻擊：必須是「還沒行動過 (!hasActed)」
                else if (tile.occupyingUnit != null && tile.occupyingUnit.team != currentTeam && dist <= selectedUnit.attackRange && currentPP >= 1 && !selectedUnit.hasActed)
                {
                    tile.occupyingUnit.TakeDamage(selectedUnit.atk);
                    
                    // 【新增】：攻擊完畢，宣告行動結束
                    selectedUnit.MarkAttacked();

                    if (currentTeam == 1) p1PP--; else p2PP--;
                    UpdateUI();
                    ResetState();
                }
                else
                {
                    ResetState();
                }
                break;

            case State.Idle:
                // 只能點選「自己陣營」的角色
                if (tile.occupyingUnit != null && tile.occupyingUnit.team == currentTeam)
                {
                    if (tile.occupyingUnit.IsExhausted()) return;
                    selectedUnit = tile.occupyingUnit;
                    currentState = State.UnitSelected;
                    HighlightMoveRange(tile, selectedUnit.moveRange);

                    if (selectedUnit.skillName != "")
                    {
                        if (skillButtonObj != null) skillButtonObj.SetActive(true);
                        if (skillButtonText != null) skillButtonText.text = $"{selectedUnit.skillName} ({selectedUnit.skillCost}PP)";
                    }
                }
                break;
        }
    }

    void HighlightSpawnArea()
    {
        ClearHighlights();
        int validY = (currentTeam == 1) ? 0 : height - 1;
        for (int x = 0; x < width; x++)
            if (grid[x, validY].occupyingUnit == null) grid[x, validY].SetHighlight(Color.cyan);
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
        if (skillButtonObj != null) skillButtonObj.SetActive(false);
    }

    void ClearHighlights()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y].ClearHighlight();
    }

    void UpdateUI()
    {
        // UI 動態顯示當前玩家的 PP
        if (ppText != null) ppText.text = $"P1 PP:{p1PP}/{p1MaxPP}  \nP2 PP:{p2PP}/{p2MaxPP}";
        
        // 顯示雙方血量
        if (playerHpText) playerHpText.text = $"P1 HP:{p1HP}  \nP2 HP:{p2HP}";
        
        if (turnText != null) turnText.text = $"第 {currentTurn} 回合 (P{currentTeam} 階段)";

        // 【核心】手牌顯示切換：誰的回合就顯示誰的手牌
        if (p1HandUI != null) p1HandUI.SetActive(currentTeam == 1);
        if (p2HandUI != null) p2HandUI.SetActive(currentTeam == 2);
    }
}