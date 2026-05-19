using UnityEngine;
using UnityEngine.UI;

public class MinimapUnitIcon : MonoBehaviour
{
    public Color playerColor = new Color(0.2f, 0.5f, 1f);
    public Color enemyColor = new Color(1f, 0.2f, 0.2f);
    public Vector2 iconSize = new Vector2(6f, 6f);

    private RectTransform dot;

    void Start()
    {
        if (MinimapController.instance == null) return;

        RectTransform minimapRect = MinimapController.instance.GetMinimapRect();
        if (minimapRect == null) return;

        GameObject go = new GameObject("UnitDot", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(minimapRect, false);

        dot = go.GetComponent<RectTransform>();
        dot.sizeDelta = iconSize;

        Image img = go.GetComponent<Image>();
        img.color = CompareTag("Player") ? playerColor : enemyColor;
    }

    void Update()
    {
        if (dot == null || MinimapController.instance == null) return;
        dot.anchoredPosition = MinimapController.instance.WorldToMinimapPos(
            transform.position.x, transform.position.y);
    }

    void OnDestroy()
    {
        if (dot != null) Destroy(dot.gameObject);
    }
}
