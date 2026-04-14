using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseController : MonoBehaviour
{
    [Header("Base Stats")]
    public int maxHealth = 9000;
    private int currentHealth;
    public bool isPlayerBase;

    [Header("UI Elements")]
    public TextMeshPro textHP;       // Текст над кристалом
    public Image hpFillImage;        // ВЕРХНЯ заливка (тане)
    public Image hpBottomImage;      // НИЖНЯ заливка (підкладка)
    public Image hpFrameImage;       // Об'єкт рамки (HP_Frame)
    public Image heartImage;         // Об'єкт сердечка

    [Header("Sprites (Assign in Inspector)")]
    // 0-Зелена, 1-Жовта, 2-Червона, 3-Сіра
    public Sprite[] hpFillSprites;
    // 0-Зелена, 1-Жовта, 2-Червона
    public Sprite[] hpFrameSprites;
    // 0-Повне, 1-Половина, 2-Пусте
    public Sprite[] heartSprites;

    [Header("Aiming System")]
    public Transform aimPoint;

    [Header("Combat Settings")]
    public GameObject projectilePrefab;
    public float attackRange = 15f;
    public float fireRate = 1.5f;
    private float nextFireTime;
    private string enemyTag;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();

        if (isPlayerBase) enemyTag = "Enemy";
        else enemyTag = "Player";
    }

    void Update()
    {
        // Тест урону на Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(500);
        }

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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0) Die();
    }

    void UpdateUI()
    {
        if (textHP != null) textHP.text = currentHealth.ToString();

        if (hpFillImage != null && hpFillSprites.Length >= 4)
        {
            // 1. ПЕРЕВІРКА НА СМЕРТЬ (0 ХП)
            if (currentHealth <= 0)
            {
                // Ставимо fillAmount в 0, сірий колір (3), сіру рамку (3) і пусте серце (3)
                SetUI(0f, 3, 3, 3);
                return; // Виходимо з методу, далі рахувати не треба
            }

            float ratio = (float)currentHealth / maxHealth;

            // 2. Звичайна логіка стадій
            if (ratio > 0.66f)
            {
                float stageFill = (ratio - 0.66f) / 0.34f;
                SetUI(stageFill, 0, 1, 0);
            }
            else if (ratio > 0.33f)
            {
                float stageFill = (ratio - 0.33f) / 0.33f;
                SetUI(stageFill, 1, 2, 1);
            }
            else
            {
                float stageFill = ratio / 0.33f;
                // Поки ХП більше 0, але менше 33% — показуємо червоне
                SetUI(stageFill, 2, 3, 2);
            }
        }
    }
    void SetUI(float fill, int topIdx, int bottomIdx, int heartIdx)
    {
        // Налаштування заливок
        hpFillImage.fillAmount = fill;
        hpFillImage.sprite = hpFillSprites[topIdx];

        if (hpBottomImage != null)
        {
            hpBottomImage.sprite = hpFillSprites[bottomIdx];
            hpBottomImage.fillAmount = 1f;
        }

        // Налаштування рамки (бере колір поточної стадії)
        if (hpFrameImage != null && hpFrameSprites.Length > topIdx)
        {
            hpFrameImage.sprite = hpFrameSprites[topIdx];
        }

        // Налаштування серця
        if (heartImage != null && heartSprites.Length > heartIdx)
        {
            heartImage.sprite = heartSprites[heartIdx];
        }
    }

    // --- ЛОГІКА СТРІЛЬБИ ---
    GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        GameObject closest = null;
        float minDistance = attackRange;

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
        Vector3 spawnPos = (aimPoint != null) ? aimPoint.position : transform.position;
        GameObject projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        ProjectileController projScript = projObj.GetComponent<ProjectileController>();
        if (projScript != null) projScript.SetTarget(target.transform, gameObject.tag);
    }

    void Die()
    {
        if (GameManager.instance) GameManager.instance.GameOver(gameObject.tag);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}