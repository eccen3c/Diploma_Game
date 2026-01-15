using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapInput : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public CameraController mainCamera;
    public RectTransform minimapRect;

    // --- НОВАЯ ССЫЛКА ---
    public CameraAutoPilot autoPilot;

    void Start()
    {
        if (minimapRect == null) minimapRect = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Time.timeScale == 0 || GameManager.instance.isGameOver) return;
        ResetCameraTimer(); // Сбрасываем таймер при клике
        MoveCamera(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Time.timeScale == 0 || GameManager.instance.isGameOver) return;
        ResetCameraTimer(); // Сбрасываем таймер при перетягивании
        MoveCamera(eventData);
    }

    // Вспомогательный метод
    void ResetCameraTimer()
    {
        if (autoPilot != null) autoPilot.ResetTimer();
    }

    void MoveCamera(PointerEventData eventData)
    {
        if (mainCamera == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        float normalizedX = (localPoint.x / minimapRect.rect.width) + 0.5f;
        float normalizedY = (localPoint.y / minimapRect.rect.height) + 0.5f;

        float worldX = Mathf.Lerp(-mainCamera.mapLimitX, mainCamera.mapLimitX, normalizedX);
        float worldY = Mathf.Lerp(-mainCamera.mapLimitY, mainCamera.mapLimitY, normalizedY);

        mainCamera.SetPosition(new Vector3(worldX, worldY, -10));
    }
}