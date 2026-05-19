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
    public TextMeshPro textHP;       // ����� ��� ���������
    public Image hpFillImage;        // ������ ������� (����)
    public Image hpBottomImage;      // ����� ������� (��������)
    public Image hpFrameImage;       // ��'��� ����� (HP_Frame)
    public Image heartImage;         // ��'��� ��������

    [Header("Sprites (Assign in Inspector)")]
    // 0-������, 1-�����, 2-�������, 3-ѳ��
    public Sprite[] hpFillSprites;
    // 0-������, 1-�����, 2-�������
    public Sprite[] hpFrameSprites;
    // 0-�����, 1-��������, 2-�����
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
        // ���� ����� �� Space
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
            // 1. ����²��� �� ������ (0 ��)
            if (currentHealth <= 0)
            {
                // ������� fillAmount � 0, ���� ���� (3), ��� ����� (3) � ����� ����� (3)
                SetUI(0f, 3, 3, 3);
                return; // �������� � ������, ��� �������� �� �����
            }

            float ratio = (float)currentHealth / maxHealth;

            // 2. �������� ����� �����
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
                // ���� �� ����� 0, ��� ����� 33% � �������� �������
                SetUI(stageFill, 2, 3, 2);
            }
        }
    }
    void SetUI(float fill, int topIdx, int bottomIdx, int heartIdx)
    {
        // ������������ �������
        hpFillImage.fillAmount = fill;
        hpFillImage.sprite = hpFillSprites[topIdx];

        if (hpBottomImage != null)
        {
            hpBottomImage.sprite = hpFillSprites[bottomIdx];
            hpBottomImage.fillAmount = 1f;
        }

        // ������������ ����� (���� ���� ������� ���䳿)
        if (hpFrameImage != null && hpFrameSprites.Length > topIdx)
        {
            hpFrameImage.sprite = hpFrameSprites[topIdx];
        }

        // ������������ �����
        if (heartImage != null && heartSprites.Length > heartIdx)
        {
            heartImage.sprite = heartSprites[heartIdx];
        }
    }

    // --- ��ò�� ��в���� ---
    GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        GameObject closest = null;
        float minDistance = attackRange;

        foreach (GameObject enemy in enemies)
        {
            UnitController unit = enemy.GetComponent<UnitController>();
            if (unit != null && unit.hp <= 0) continue;
            TestUnitAI ai = enemy.GetComponent<TestUnitAI>();
            if (ai != null && ai.hp <= 0) continue;
            TestArcherAI archer = enemy.GetComponent<TestArcherAI>();
            if (archer != null && archer.hp <= 0) continue;
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
        AudioManager.Instance?.PlayFireball();
        Vector3 spawnPos = (aimPoint != null) ? aimPoint.position : transform.position;
        GameObject projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        ProjectileController projScript = projObj.GetComponent<ProjectileController>();
        if (projScript == null) return;

        Vector3 offset = Vector3.zero;
        if (target.GetComponent<TestUnitAI>() != null || target.GetComponent<TestArcherAI>() != null)
            offset = new Vector3(0, 0.5f, 0);

        projScript.SetTarget(target.transform, gameObject.tag, offset);
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