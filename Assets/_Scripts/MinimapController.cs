using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapController : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Ссылки (Обязательно!)")]
    public CameraController cameraScript; // Тут берем mapLimitX и mapLimitY
    public Camera mainCameraComp;         // Тут берем Zoom (orthographicSize)

    [Header("UI Элементы")]
    public RectTransform minimapRect;     // Фон (800x150)
    public RectTransform yellowFrame;     // Желтая рамка

    [Header("Иконки Баз")]
    public Transform p1_Base_World;
    public RectTransform p1_Base_Icon;
    public Transform p2_Base_World;
    public RectTransform p2_Base_Icon;

    // Размеры
    private float worldWidth;
    private float worldHeight;
    private float mapWidth;
    private float mapHeight;

    void Start()
    {
        // --- 1. ПРОВЕРКА И ИНИЦИАЛИЗАЦИЯ ---
        if (minimapRect == null || cameraScript == null || mainCameraComp == null)
        {
            Debug.LogError("MinimapController: ЗАБЫЛ ПРИВЯЗАТЬ ССЫЛКИ В ИНСПЕКТОРЕ!");
            return;
        }

        // Берем размеры мира из лимитов камеры
        worldWidth = cameraScript.mapLimitX * 2f;  // Например 75*2 = 150
        worldHeight = cameraScript.mapLimitY * 2f; // Например 15*2 = 30 (если лимит Y = 15)

        // Берем размеры UI
        mapWidth = minimapRect.rect.width;
        mapHeight = minimapRect.rect.height;

        // --- 2. РАСЧЕТ РАЗМЕРА РАМКИ (X и Y) ---
        CalculateFrameSize();
    }

    void CalculateFrameSize()
    {
        // Высота камеры в метрах
        float camHeight = 2f * mainCameraComp.orthographicSize;
        // Ширина камеры в метрах
        float camWidth = camHeight * mainCameraComp.aspect;

        // Пропорции (сколько % мира мы видим)
        float ratioX = camWidth / worldWidth;
        float ratioY = camHeight / worldHeight;

        // Применяем пропорции к размеру рамки на UI
        float frameW = mapWidth * ratioX;
        float frameH = mapHeight * ratioY;

        // Устанавливаем размер рамки
        yellowFrame.sizeDelta = new Vector2(frameW, frameH);
    }

    void Update()
    {
        if (cameraScript == null) return;

        // --- 3. ДВИГАЕМ РАМКУ ЗА КАМЕРОЙ (X и Y) ---
        float camX = cameraScript.transform.position.x;
        float camY = cameraScript.transform.position.y;

        MoveUIElement(yellowFrame, camX, camY);

        // --- 4. ДВИГАЕМ ИКОНКИ БАЗ (Только X, так как базы обычно на земле Y=0) ---
        // Если базы могут быть выше/ниже, можно добавить Y. Пока оставим 0 по Y.
        if (p1_Base_World && p1_Base_Icon)
            MoveIconX(p1_Base_Icon, p1_Base_World.position.x);

        if (p2_Base_World && p2_Base_Icon)
            MoveIconX(p2_Base_Icon, p2_Base_World.position.x);
    }

    // Двигает элемент и по X, и по Y (для Рамки)
    void MoveUIElement(RectTransform ui, float wX, float wY)
    {
        // X
        float tX = Mathf.InverseLerp(-cameraScript.mapLimitX, cameraScript.mapLimitX, wX);
        float uiX = (tX * mapWidth) - (mapWidth / 2f);

        // Y
        float tY = Mathf.InverseLerp(-cameraScript.mapLimitY, cameraScript.mapLimitY, wY);
        float uiY = (tY * mapHeight) - (mapHeight / 2f);

        ui.anchoredPosition = new Vector2(uiX, uiY);
    }

    // Двигает иконку только по X (для Баз)
    void MoveIconX(RectTransform ui, float wX)
    {
        float tX = Mathf.InverseLerp(-cameraScript.mapLimitX, cameraScript.mapLimitX, wX);
        float uiX = (tX * mapWidth) - (mapWidth / 2f);
        // Y оставляем как было (или 0)
        ui.anchoredPosition = new Vector2(uiX, 0);
    }

    // --- 5. УПРАВЛЕНИЕ МЫШКОЙ (X и Y) ---
    public void OnPointerDown(PointerEventData eventData) { MoveCamera(eventData); }
    public void OnDrag(PointerEventData eventData) { MoveCamera(eventData); }

    void MoveCamera(PointerEventData eventData)
    {
        if (minimapRect == null || cameraScript == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect, eventData.position, eventData.pressEventCamera, out localPoint);

        // --- Расчет X ---
        float tX = (localPoint.x + (mapWidth / 2f)) / mapWidth;
        tX = Mathf.Clamp01(tX);
        float targetX = Mathf.Lerp(-cameraScript.mapLimitX, cameraScript.mapLimitX, tX);

        // --- Расчет Y ---
        float tY = (localPoint.y + (mapHeight / 2f)) / mapHeight;
        tY = Mathf.Clamp01(tY);
        float targetY = Mathf.Lerp(-cameraScript.mapLimitY, cameraScript.mapLimitY, tY);

        // Двигаем камеру
        cameraScript.SetPosition(new Vector3(targetX, targetY, -10));
    }
}