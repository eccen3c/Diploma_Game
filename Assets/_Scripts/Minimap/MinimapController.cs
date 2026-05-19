using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapController : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public static MinimapController instance;
    [Header("������ (�����������!)")]
    public CameraController cameraScript;
    public Camera mainCameraComp;
    public Camera minimapCameraComp;

    [Header("UI ��������")]
    public RectTransform minimapRect;     // ��� (800x150)
    public RectTransform yellowFrame;     // ������ �����

    [Header("������ ���")]
    public Transform p1_Base_World;
    public RectTransform p1_Base_Icon;
    public Transform p2_Base_World;
    public RectTransform p2_Base_Icon;

    // �������
    private float worldWidth;
    private float worldHeight;
    private float mapWidth;
    private float mapHeight;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // --- 1. �������� � ������������� ---
        if (minimapRect == null || cameraScript == null || mainCameraComp == null)
        {
            Debug.LogError("MinimapController: ����� ��������� ������ � ����������!");
            return;
        }

        // ����� ������� ���� �� ������� ������
        if (minimapCameraComp != null)
        {
            worldHeight = minimapCameraComp.orthographicSize * 2f;
            worldWidth = minimapCameraComp.orthographicSize * minimapCameraComp.aspect * 2f;
        }
        else
        {
            worldWidth = cameraScript.mapLimitX * 2f;
            worldHeight = cameraScript.mapLimitY * 2f;
        }

        // ����� ������� UI
        mapWidth = minimapRect.rect.width;
        mapHeight = minimapRect.rect.height;

        // --- 2. ������ ������� ����� (X � Y) ---          
        CalculateFrameSize();
    }

    void CalculateFrameSize()
    {
        float camHeight = 2f * mainCameraComp.orthographicSize;
        float camWidth = camHeight * mainCameraComp.aspect;

        float ratioX = camWidth / worldWidth;
        float ratioY = camHeight / worldHeight;

        float frameW = mapWidth * ratioX;
        float frameH = mapHeight * ratioY;

        yellowFrame.sizeDelta = new Vector2(frameW, frameH);
    }

    void Update()
    {
        if (cameraScript == null) return;

        CalculateFrameSize();

        // --- 3. ������� ����� �� ������� (X � Y) ---
        float camX = cameraScript.transform.position.x;
        float camY = cameraScript.transform.position.y;

        MoveUIElement(yellowFrame, camX, camY);

        // --- 4. ������� ������ ��� (������ X, ��� ��� ���� ������ �� ����� Y=0) ---
        // ���� ���� ����� ���� ����/����, ����� �������� Y. ���� ������� 0 �� Y.
        if (p1_Base_World && p1_Base_Icon)
            MoveIconX(p1_Base_Icon, p1_Base_World.position.x);

        if (p2_Base_World && p2_Base_Icon)
            MoveIconX(p2_Base_Icon, p2_Base_World.position.x);
    }

    void MoveUIElement(RectTransform ui, float wX, float wY)
    {
        float tX = Mathf.InverseLerp(-worldWidth / 2f, worldWidth / 2f, wX);
        float uiX = (tX * mapWidth) - (mapWidth / 2f);

        float tY = Mathf.InverseLerp(-worldHeight / 2f, worldHeight / 2f, wY);
        float uiY = (tY * mapHeight) - (mapHeight / 2f);

        float halfW = ui.sizeDelta.x / 2f;
        float halfH = ui.sizeDelta.y / 2f;
        uiX = Mathf.Clamp(uiX, -mapWidth / 2f + halfW, mapWidth / 2f - halfW);
        uiY = Mathf.Clamp(uiY, -mapHeight / 2f + halfH, mapHeight / 2f - halfH);

        ui.anchoredPosition = new Vector2(uiX, uiY);
    }

    public RectTransform GetMinimapRect() => minimapRect;

    public Vector2 WorldToMinimapPos(float wX, float wY)
    {
        float tX = Mathf.InverseLerp(-worldWidth / 2f, worldWidth / 2f, wX);
        float uiX = (tX * mapWidth) - (mapWidth / 2f);
        float tY = Mathf.InverseLerp(-worldHeight / 2f, worldHeight / 2f, wY);
        float uiY = (tY * mapHeight) - (mapHeight / 2f);
        return new Vector2(uiX, uiY);
    }

    // ������� ������ ������ �� X (��� ���)
    void MoveIconX(RectTransform ui, float wX)
    {
        float tX = Mathf.InverseLerp(-worldWidth / 2f, worldWidth / 2f, wX);
        float uiX = (tX * mapWidth) - (mapWidth / 2f);
        ui.anchoredPosition = new Vector2(uiX, 0);
    }

    // --- 5. ���������� ������ (X � Y) ---
    public void OnPointerDown(PointerEventData eventData) { if (Time.timeScale == 0) return; MoveCamera(eventData); }
    public void OnDrag(PointerEventData eventData) { if (Time.timeScale == 0) return; MoveCamera(eventData); }

    void MoveCamera(PointerEventData eventData)
    {
        if (minimapRect == null || cameraScript == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect, eventData.position, eventData.pressEventCamera, out localPoint);

        // --- ������ X ---
        float tX = (localPoint.x + (mapWidth / 2f)) / mapWidth;
        tX = Mathf.Clamp01(tX);
        float targetX = Mathf.Lerp(-worldWidth / 2f, worldWidth / 2f, tX);

        float tY = (localPoint.y + (mapHeight / 2f)) / mapHeight;
        tY = Mathf.Clamp01(tY);
        float targetY = Mathf.Lerp(-worldHeight / 2f, worldHeight / 2f, tY);

        // ������� ������
        cameraScript.SetPosition(new Vector3(targetX, targetY, -10));
    }
}