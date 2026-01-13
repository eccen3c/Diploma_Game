using UnityEngine;

public class HybridCamera : MonoBehaviour
{
    [Header("Режимы")]
    public float autoModeDelay = 5.0f;
    private float lastInputTime;

    [Header("Настройка Миникарты")]
    public RectTransform minimapRect;

    [Header("Авто-режим")]
    public string playerTag = "Ally";
    public string enemyTag = "Enemy";
    public float smoothSpeed = 3f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Границы Карты")]
    public Vector2 xLimit = new Vector2(-60, 60);
    public Vector2 yLimit = new Vector2(0, 0);

    void Start()
    {
        lastInputTime = Time.time;
    }

    void Update()
    {
        // 🛑 ГЛАВНОЕ ИСПРАВЛЕНИЕ:
        // Если время остановлено (Пауза) — просто выходим из функции.
        // Камера не будет реагировать ни на клики, ни на таймеры.
        if (Time.timeScale == 0) return;


        // 1. Проверяем клик ТОЛЬКО по Миникарте
        if (Input.GetMouseButton(0) && IsPointerOverMinimap())
        {
            lastInputTime = Time.time;
        }

        // 2. Если таймер истек — включаем авто-режим
        if (Time.time - lastInputTime > autoModeDelay)
        {
            HandleAutoMovement();
        }

        // 3. Лимиты
        ClampPosition();
    }

    bool IsPointerOverMinimap()
    {
        if (minimapRect == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(minimapRect, Input.mousePosition);
    }

    void HandleAutoMovement()
    {
        Transform bestTarget = FindFrontlineUnit();

        if (bestTarget != null)
        {
            Vector3 desiredPosition = bestTarget.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime); // Тут deltaTime станет 0 на паузе, но лучше блокировать выше
            smoothedPosition.z = -10;
            transform.position = smoothedPosition;
        }
    }

    Transform FindFrontlineUnit()
    {
        GameObject[] allies = GameObject.FindGameObjectsWithTag(playerTag);
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        if (allies.Length == 0) return null;

        Transform bestCandidate = null;
        float shortestDist = Mathf.Infinity;

        foreach (GameObject ally in allies)
        {
            if (ally.name.Contains("Base")) continue;

            float distToEnemy = GetDistanceToNearestEnemy(ally.transform, enemies);
            if (distToEnemy < shortestDist)
            {
                shortestDist = distToEnemy;
                bestCandidate = ally.transform;
            }
        }
        return bestCandidate;
    }

    float GetDistanceToNearestEnemy(Transform me, GameObject[] enemies)
    {
        float minDst = Mathf.Infinity;
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            float d = Vector2.Distance(me.position, enemy.transform.position);
            if (d < minDst) minDst = d;
        }
        return minDst;
    }

    Transform GetMostAdvancedAlly(GameObject[] allies)
    {
        Transform best = null;
        float maxX = -Mathf.Infinity;
        foreach (GameObject ally in allies)
        {
            if (ally.name.Contains("Base")) continue;

            if (ally.transform.position.x > maxX)
            {
                maxX = ally.transform.position.x;
                best = ally.transform;
            }
        }
        return best;
    }

    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, xLimit.x, xLimit.y);
        pos.y = Mathf.Clamp(pos.y, yLimit.x, yLimit.y);
        transform.position = pos;
    }
}