using UnityEngine;
using TMPro; // Для текста HP

public class BaseController : MonoBehaviour
{
    [Header("Base Stats")]
    public int maxHealth = 500;
    private int currentHealth;
    public bool isPlayerBase; // Галочка: это база игрока?

    [Header("UI")]
    public TextMeshPro textHP; // Ссылка на текст с жизнями

    [Header("Combat Settings (New)")]
    public GameObject projectilePrefab; // Ссылка на префаб фаербола
    public float attackRange = 15f;     // Дальность стрельбы
    public float fireRate = 1.5f;       // Пауза между выстрелами
    private float nextFireTime;

    // В кого мы будем стрелять (определим автоматически)
    private string enemyTag;

    void Start()
    {
        // 1. Настройка здоровья
        currentHealth = maxHealth;
        UpdateUI();

        // 2. Автоматически определяем врага
        // Если это база Игрока -> Враг "Enemy". Если база Врага -> Враг "Player".
        if (isPlayerBase) enemyTag = "Enemy";
        else enemyTag = "Player";
    }

    void Update()
    {
        // Таймер стрельбы
        if (Time.time >= nextFireTime)
        {
            GameObject target = FindClosestEnemy();
            if (target != null)
            {
                Shoot(target);
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    // --- ЛОГИКА СТРЕЛЬБЫ ---
    GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        GameObject closest = null;
        float minDistance = attackRange; // Ищем только в радиусе атаки

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = enemy;
            }
        }
        return closest;
    }

    void Shoot(GameObject target)
    {
        if (projectilePrefab == null) return;

        // 1. Создаем пулю
        GameObject projObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        // 2. Получаем скрипт пули
        ProjectileController projScript = projObj.GetComponent<ProjectileController>();

        // 3. Если скрипт есть — передаем ему цель
        if (projScript != null)
        {
            // Передаем цель И свой тег (gameObject.tag), чтобы кристалл не бил своих
            projScript.SetTarget(target.transform, gameObject.tag);
            // Важная деталь: Чтобы пуля не ударила саму базу при рождении, 
            // можно временно отключить коллизию или настроить слои, 
            // но пока оставим так (триггер пули обычно не бьет базу).
        }
    }

    // --- ЛОГИКА ЗДОРОВЬЯ ---
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        if (textHP != null)
        {
            textHP.text = currentHealth.ToString();
        }
    }

    void Die()
    {
        // Вызываем GameOver из GameManager
        // Используем тег объекта, чтобы менеджер понял, кто проиграл
        if (GameManager.instance)
            GameManager.instance.GameOver(gameObject.tag);

        Destroy(gameObject);
    }

    // Рисуем круг радиуса в редакторе, чтобы было удобно настраивать
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}