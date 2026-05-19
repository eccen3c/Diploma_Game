using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Границы Карты (Например: 64 и 18)")]
    public float mapLimitX = 64f; // Расстояние от центра до края травы по X
    public float mapLimitY = 18f; // Расстояние от центра до края травы по Y

    private Camera cam;
    private float camHalfHeight;
    private float camHalfWidth;

    void Start()
    {
        cam = GetComponent<Camera>();
        // Считаем половинки размера камеры, чтобы знать, где её остановить
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = cam.orthographicSize * cam.aspect;
    }

    public void SetPosition(Vector3 targetPosition)
    {
        if (cam == null) return;

        // Если экран игры меняется, пересчитываем ширину
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = cam.orthographicSize * cam.aspect;

        Vector3 pos = targetPosition;
        pos.z = -10; // Всегда держим камеру над полем

        // --- ГЛАВНАЯ МАТЕМАТИКА ---
        // Мы берем край карты (mapLimit) и вычитаем половину ширины камеры.
        // Это точка, дальше которой ЦЕНТР камеры не имеет права ехать.

        float maxX = mapLimitX - camHalfWidth;
        float minX = -mapLimitX + camHalfWidth;

        float maxY = mapLimitY - camHalfHeight;
        float minY = -mapLimitY + camHalfHeight;

        // Если карта меньше, чем экран камеры -> ставим в 0
        if (maxX < minX) pos.x = 0;
        else pos.x = Mathf.Clamp(pos.x, minX, maxX);

        if (maxY < minY) pos.y = 0;
        else pos.y = Mathf.Clamp(pos.y, minY, maxY);
        

        transform.position = pos;
    }
}