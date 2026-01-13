using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapInput : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Ссылки")]
    public Camera minimapCamera;
    public Transform mainCamera;

    private RectTransform minimapRect;

    void Start()
    {
        minimapRect = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Если пауза — игнорируем нажатие
        if (Time.timeScale == 0) return;

        MoveCameraToPoint(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Если пауза — игнорируем перетаскивание
        if (Time.timeScale == 0) return;

        MoveCameraToPoint(eventData);
    }

    void MoveCameraToPoint(PointerEventData eventData)
    {
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
        {
            float normX = (localPoint.x - minimapRect.rect.x) / minimapRect.rect.width;
            float normY = (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height;

            Vector3 worldPoint = minimapCamera.ViewportToWorldPoint(new Vector3(normX, normY, 0));

            Vector3 newPos = mainCamera.position;
            newPos.x = worldPoint.x;

            // --- ВЕРНУЛ ЭТУ СТРОКУ ---
            newPos.y = worldPoint.y; // Теперь Y тоже меняется!

            mainCamera.position = newPos;
        }
    }
}