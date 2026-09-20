using UnityEngine;
using UnityEngine.EventSystems; // 處理滑鼠移入/移出事件必須加這行

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("懸停設定")]
    public float hoverYOffset = 60f; // 滑鼠移上去時，往上浮動的距離
    public float moveSpeed = 15f;    // 浮動的平滑速度

    private Vector2 originalPos;
    private Vector2 targetPos;
    private RectTransform rectTransform;

    void Start()
    {
        // 抓取 UI 的位置組件，並記錄遊戲剛開始時的初始位置
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
        targetPos = originalPos;
    }

    void Update()
    {
        // 讓卡牌每一幀都平滑地往「目標位置」移動
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPos, Time.deltaTime * moveSpeed);
    }

    // 當滑鼠「碰到」這張卡牌時會觸發
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetPos = originalPos + new Vector2(0, hoverYOffset);
    }

    // 當滑鼠「離開」這張卡牌時會觸發
    public void OnPointerExit(PointerEventData eventData)
    {
        targetPos = originalPos;
    }
}