using UnityEngine;

public class CameraAutoPilot : MonoBehaviour
{
    [Header("Настройки")]
    public float autoModeDelay = 6f;   // Сколько ждать бездействия (6 сек)
    public float smoothSpeed = 2f;     // Плавность полета
    public string playerUnitTag = "Ally"; // Тэг твоих юнитов (НЕ БАЗЫ)

    [Header("Ссылки")]
    public CameraController camController; // Чтобы соблюдать границы карты

    private float currentTimer;
    private bool isAutoMode = false;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (Time.timeScale == 0 || GameManager.instance.isGameOver) return; // Игра на паузе

        // 1. Обратный отсчет таймера
        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
        }
        else
        {
            // Таймер вышел -> Включаем режим слежения
            isAutoMode = true;
        }

        // 2. Логика слежения
        if (isAutoMode)
        {
            FollowLeadingUnit();
        }
    }

    // Этот метод мы будем вызывать из MinimapInput, когда ты трогаешь мышку
    public void ResetTimer()
    {
        currentTimer = autoModeDelay;
        isAutoMode = false;
    }

    void FollowLeadingUnit()
    {
        // Ищем всех с тэгом Ally (тут будут и Юниты, и База)
        GameObject[] allAllies = GameObject.FindGameObjectsWithTag(playerUnitTag);

        GameObject leader = null;
        float maxX = -100000f; // Очень маленькое число

        foreach (GameObject obj in allAllies)
        {
            // --- ГЛАВНАЯ ПРОВЕРКА ---
            // Если у объекта НЕТ мозгов юнита (UnitAI), значит это База или стена.
            // Мы их игнорируем!
            if (obj.GetComponent<UnitBrain>() == null) continue;

            // Дальше логика та же: ищем того, кто дальше всех справа
            if (obj.transform.position.x > maxX)
            {
                maxX = obj.transform.position.x;
                leader = obj;
            }
        }

        if (leader != null)
        {
            // Плавно летим к лидеру
            Vector3 currentPos = transform.position;
            Vector3 targetPos = new Vector3(leader.transform.position.x, currentPos.y, currentPos.z);

            // Используем Lerp для плавности
            Vector3 smoothedPos = Vector3.Lerp(currentPos, targetPos, smoothSpeed * Time.deltaTime);

            // Отдаем контроллеру, чтобы он следил за границами
            camController.SetPosition(smoothedPos);
        }
    }
}