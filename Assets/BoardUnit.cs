using UnityEngine;
using TMPro;

public class BoardUnit : MonoBehaviour
{
    public int team = 1;
    public string unitName = "Knight";
    public int spawnCost = 3;
    public int hp = 6;
    public int atk = 2;
    public int moveRange = 2;
    public int attackRange = 1;
    public int skillCost = 2;
    
    // PPT機制：這個角色死掉時，玩家會扣多少血？
    public int playerDamageOnDeath = 2; 
    
    public string skillName = ""; // 例如輸入 "CrossAttack" 或 "Snipe"

    public bool hasMoved = false;
    public bool hasActed = false;
    
    public GridTile currentTile;
    public TextMeshPro statusText;

    public void Setup(GridTile tile)
    {
        currentTile = tile;
        tile.occupyingUnit = this;
        transform.position = tile.transform.position;
        UpdateUI();
    }

    public void MoveTo(GridTile targetTile)
    {
        currentTile.occupyingUnit = null;
        currentTile = targetTile;
        targetTile.occupyingUnit = this;
        transform.position = targetTile.transform.position;
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        UpdateUI();
        if (hp <= 0)
        {
            currentTile.occupyingUnit = null;
            // 通知控制器：扣除玩家血量
            BattleController.Instance.DamagePlayer(this.team, playerDamageOnDeath);
            Destroy(gameObject);
        }
    }

    public void CastSkill()
    {
        if (skillName == "CrossAttack")
        {
            CastCrossAttack();
        }
        else if (skillName == "Snipe")
        {
            CastSnipe();
        }
    }

    public void ResetTurnActions()
    {
        hasMoved = false;
        hasActed = false;
        
        // 恢復原本的顏色（P1 是白色/預設，P2 是紅色）
        GetComponent<SpriteRenderer>().color = (team == 2) ? Color.red : Color.white;
    }

    // 檢查是否移動和攻擊都做完了
    public bool IsExhausted()
    {
        return hasMoved && hasActed;
    }

    // 【新增】標記為已攻擊/放技能
    public void MarkAttacked()
    {
        hasActed = true;
        
        // 只有兩件事都做完，才變成灰色
        if (IsExhausted()) GetComponent<SpriteRenderer>().color = Color.gray;
    }

    // 【新增】標記為已移動
    public void MarkMoved()
    {
        hasMoved = true;
        
        if (IsExhausted()) GetComponent<SpriteRenderer>().color = Color.gray;
    }

    private void CastCrossAttack()
    {
        int[,] dirs = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };
        for (int i = 0; i < 4; i++)
        {
            int tx = currentTile.x + dirs[i, 0];
            int ty = currentTile.y + dirs[i, 1];
            GridTile t = BattleController.Instance.GetTile(tx, ty);
            if (t != null && t.occupyingUnit != null && t.occupyingUnit != this)
            {
                t.occupyingUnit.TakeDamage(2);
            }
        }
    }

    private void CastSnipe()
    {
        // 之後可以在這裡寫弓箭手的專屬邏輯，例如對直線最遠敵人造成傷害
        Debug.Log("弓箭手施放了 Snipe!");
    }

    public void UpdateUI()
    {
        if (statusText) statusText.text = $"{unitName}\n{atk}/{hp}";
    }
}