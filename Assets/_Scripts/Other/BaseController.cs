using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class BaseController : MonoBehaviourPun
{
    [Header("Base Stats")]
    public int maxHealth = 9000;
    private int currentHealth;
    public bool isPlayerBase;

    [Header("UI Elements")]
    public TextMeshPro textHP;
    public Image hpFillImage;
    public Image hpBottomImage;
    public Image hpFrameImage;
    public Image heartImage;

    [Header("Sprites (Assign in Inspector)")]
    public Sprite[] hpFillSprites;
    public Sprite[] hpFrameSprites;
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

        // Перевіряємо саме знаходження в кімнаті (Room)
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                enemyTag = isPlayerBase ? "Enemy" : "Player";
            }
            else
            {
                enemyTag = isPlayerBase ? "Player" : "Enemy";
            }
        }
        else
        {
            // Офлайн режим: Гравець 1 (лівий) б'є Enemy, Гравець 2 (правий) б'є Player
            if (isPlayerBase) enemyTag = "Enemy";
            else enemyTag = "Player";
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(500);
        }

        // Якщо ми в кімнаті і ця база не наша — її Update не керує стрільбою
        if (PhotonNetwork.InRoom && !photonView.IsMine) return;

        if (Time.time >= nextFireTime)
        {
            GameObject target = FindClosestEnemy();
            if (target != null)
            {
                if (PhotonNetwork.InRoom)
                {
                    // ОНЛАЙН: Стріляємо через RPC, тільки якщо є кімната
                    PhotonView targetView = target.GetComponent<PhotonView>();
                    if (targetView != null)
                    {
                        photonView.RPC("RPC_Shoot", RpcTarget.All, targetView.ViewID);
                    }
                }
                else
                {
                    // ЛОКАЛЬНО: Стріляємо напряму, Photon не чіпаємо
                    LocalShoot(target);
                }

                nextFireTime = Time.time + fireRate;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (PhotonNetwork.InRoom)
        {
            photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage);
        }
        else
        {
            LocalDamage(damage);
        }
    }

    private void LocalDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0) Die();
    }

    private void LocalShoot(GameObject target)
    {
        if (target == null || projectilePrefab == null) return;

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

    [PunRPC]
    void RPC_TakeDamage(int damage)
    {
        LocalDamage(damage);
    }

    [PunRPC]
    void RPC_Shoot(int targetViewID)
    {
        PhotonView targetView = PhotonView.Find(targetViewID);
        if (targetView == null) return;

        LocalShoot(targetView.gameObject);
    }

    void UpdateUI()
    {
        if (textHP != null) textHP.text = currentHealth.ToString();

        if (hpFillImage != null && hpFillSprites.Length >= 4)
        {
            if (currentHealth <= 0)
            {
                SetUI(0f, 3, 3, 3);
                return;
            }

            float ratio = (float)currentHealth / maxHealth;

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
                SetUI(stageFill, 2, 3, 2);
            }
        }
    }

    void SetUI(float fill, int topIdx, int bottomIdx, int heartIdx)
    {
        hpFillImage.fillAmount = fill;
        hpFillImage.sprite = hpFillSprites[topIdx];

        if (hpBottomImage != null)
        {
            hpBottomImage.sprite = hpFillSprites[bottomIdx];
            hpBottomImage.fillAmount = 1f;
        }

        if (hpFrameImage != null && hpFrameSprites.Length > topIdx)
        {
            hpFrameImage.sprite = hpFrameSprites[topIdx];
        }

        if (heartImage != null && heartSprites.Length > heartIdx)
        {
            heartImage.sprite = heartSprites[heartIdx];
        }
    }

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