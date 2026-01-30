using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapInput : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public CameraController mainCamera;
    public RectTransform minimapRect;

    void Start()
    {
        // Если забыл привязать, скрипт попытается найти компонент на этом же объекте
        if (minimapRect == null) minimapRect = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateCameraPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateCameraPosition(eventData);
    }

    void UpdateCameraPosition(PointerEventData eventData)
    {
        if (mainCamera == null) return;

        Vector2 localPoint;
        // Переводим точку клика на экране в локальные координаты прямоугольника миникарты
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        // Вычисляем процент по ширине (от 0 до 1)
        // localPoint.x идет от -Width/2 до +Width/2. 
        // Делим на ширину и прибавляем 0.5, чтобы получить диапазон 0..1
        float pctX = (localPoint.x / minimapRect.rect.width) + 0.5f;
        float pctY = (localPoint.y / minimapRect.rect.height) + 0.5f;

        // Защита: не даем значению выйти за пределы 0..1
        pctX = Mathf.Clamp01(pctX);
        pctY = Mathf.Clamp01(pctY);

        // Переводим проценты в мировые координаты камеры
        // Lerp интерполирует между левой (-75) и правой (+75) границей
        float worldX = Mathf.Lerp(-mainCamera.mapLimitX, mainCamera.mapLimitX, pctX);
        float worldY = Mathf.Lerp(-mainCamera.mapLimitY, mainCamera.mapLimitY, pctY);

        mainCamera.SetPosition(new Vector3(worldX, worldY, -10));
    }
}