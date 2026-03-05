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

    [Header("Aiming System (НОВОЕ)")]
    public Transform aimPoint;

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
            UnitController unit = enemy.GetComponent<UnitController>();
            if (unit != null && unit.hp <= 0) continue;
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

        // Определяем точку ЦЕЛИ (AimPoint врага)
        Transform targetTransform = target.transform;

        UnitController enemyUnit = target.GetComponent<UnitController>();
        if (enemyUnit != null && enemyUnit.aimPoint != null) targetTransform = enemyUnit.aimPoint;

        BaseController enemyBase = target.GetComponent<BaseController>();
        if (enemyBase != null && enemyBase.aimPoint != null) targetTransform = enemyBase.aimPoint;


        // Определяем точку СПАВНА (наш AimPoint или центр базы)
        Vector3 spawnPos = (aimPoint != null) ? aimPoint.position : transform.position;

        GameObject projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        ProjectileController projScript = projObj.GetComponent<ProjectileController>();

        if (projScript != null)
        {
            projScript.SetTarget(targetTransform, gameObject.tag);
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