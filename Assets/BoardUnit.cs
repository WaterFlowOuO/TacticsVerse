using UnityEngine;
using TMPro;

public class BoardUnit : MonoBehaviour
{
    public string unitName;
    public int hp = 6;
    public int atk = 2;
    public int moveRange = 2;
    public int skillCost = 2;
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
            Destroy(gameObject);
        }
    }

    public void UpdateUI()
    {
        if (statusText) statusText.text = $"{unitName}\n{atk}/{hp}";
    }

    // 示範技能：旋風斬（對自身十字 1 格內所有敵對/非己單位造成 3 點傷害）
    public void CastSkillAoE()
    {
        int[,] dirs = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };
        for (int i = 0; i < 4; i++)
        {
            int tx = currentTile.x + dirs[i, 0];
            int ty = currentTile.y + dirs[i, 1];
            GridTile t = BattleController.Instance.GetTile(tx, ty);
            if (t != null && t.occupyingUnit != null && t.occupyingUnit != this)
            {
                t.occupyingUnit.TakeDamage(3);
            }
        }
    }
}