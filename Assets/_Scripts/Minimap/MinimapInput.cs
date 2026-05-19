using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapInput : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public CameraController mainCamera;
    public RectTransform minimapRect;

    void Start()
    {
        // ���� ����� ���������, ������ ���������� ����� ��������� �� ���� �� �������
        if (minimapRect == null) minimapRect = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Time.timeScale == 0) return;
        UpdateCameraPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Time.timeScale == 0) return;
        UpdateCameraPosition(eventData);
    }

    void UpdateCameraPosition(PointerEventData eventData)
    {
        if (mainCamera == null) return;

        Vector2 localPoint;
        // ��������� ����� ����� �� ������ � ��������� ���������� �������������� ���������
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        // ��������� ������� �� ������ (�� 0 �� 1)
        // localPoint.x ���� �� -Width/2 �� +Width/2. 
        // ����� �� ������ � ���������� 0.5, ����� �������� �������� 0..1
        float pctX = (localPoint.x / minimapRect.rect.width) + 0.5f;
        float pctY = (localPoint.y / minimapRect.rect.height) + 0.5f;

        // ������: �� ���� �������� ����� �� ������� 0..1
        pctX = Mathf.Clamp01(pctX);
        pctY = Mathf.Clamp01(pctY);

        // ��������� �������� � ������� ���������� ������
        // Lerp ������������� ����� ����� (-75) � ������ (+75) ��������
        float worldX = Mathf.Lerp(-mainCamera.mapLimitX, mainCamera.mapLimitX, pctX);
        float worldY = Mathf.Lerp(-mainCamera.mapLimitY, mainCamera.mapLimitY, pctY);

        mainCamera.SetPosition(new Vector3(worldX, worldY, -10));
    }
}