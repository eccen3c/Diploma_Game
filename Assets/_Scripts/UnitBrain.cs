using UnityEngine;
using System.Collections;

public class UnitBrain : MonoBehaviour
{
    [Header("Тип Юнита")]
    public bool isRanged = false; // ГАЛОЧКА: Если true - стреляем, false - бьем мечом
    public GameObject projectilePrefab; // Сюда закинем префаб Стрелы
    public float projectileMaxRange = 7.0f; // Максимальная дальность полета стрелы
    public Transform firePoint; // Точка вылета стрелы (опционально)
    private Color defaultColor;

    [Header("Параметры")]
    public float speed = 2.0f;
    public float detectRange = 6.0f;
    public float attackRange = 0.8f; // Для лучника ставь 5-6!

    [Header("Толпа")]
    public float separationRadius = 0.8f;
    public float separationForce = 2.0f;

    [Header("Бой")]
    public int health = 100;
    public int damage = 20;
    public float attackCooldown = 1.0f;

    [Header("Цели")]
    public string enemyTag;

    private Transform currentTarget;
    private float lastAttackTime;
    private bool isDead = false;

    void Start()
    {
        if (gameObject.tag == "Ally") enemyTag = "Enemy";
        else enemyTag = "Ally";

        GetComponent<Rigidbody2D>().freezeRotation = true;
        GetComponent<Rigidbody2D>().gravityScale = 0;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) defaultColor = sr.color;
    }

    void Update()
    {
        if (isDead) return;

        FindNearestEnemy();

        if (currentTarget != null)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);

            if (distance <= attackRange)
            {
                // Если мы на дистанции атаки -> Бьем
                AttackLogic();
            }
            else
            {
                MoveWithFlocking(currentTarget.position);
            }
        }
        else
        {
            float dir = (gameObject.tag == "Ally") ? 1 : -1;
            Vector3 forwardPoint = transform.position + Vector3.right * dir * 10f;
            MoveWithFlocking(forwardPoint);
        }
    }

    void MoveWithFlocking(Vector3 targetPos)
    {
        Vector2 directionToTarget = (targetPos - transform.position).normalized;
        Vector2 separation = GetSeparationVector();
        Vector2 finalDirection = (directionToTarget + separation).normalized;

        transform.position += (Vector3)finalDirection * speed * Time.deltaTime;

        // Поворот лица
        if (finalDirection.x > 0.1f) transform.localScale = new Vector3(1, 1, 1);
        else if (finalDirection.x < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
    }

    Vector2 GetSeparationVector()
    {
        Vector2 pushForce = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, separationRadius);

        foreach (var neighbor in neighbors)
        {
            if (neighbor.CompareTag(gameObject.tag) && neighbor.gameObject != gameObject)
            {
                Vector2 pushDir = transform.position - neighbor.transform.position;
                float dist = pushDir.magnitude;
                if (dist > 0.01f) pushForce += pushDir.normalized / dist;
            }
        }
        return pushForce * separationForce;
    }

    void FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRange);
        float closestDist = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag(enemyTag) || (hit.GetComponent<BaseController>() && !hit.CompareTag(gameObject.tag)))
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestEnemy = hit.transform;
                }
            }
        }
        currentTarget = closestEnemy;
    }

    void AttackLogic()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            if (isRanged)
            {
                // === СТРЕЛЬБА ===
                Shoot();
            }
            else
            {
                // === БЛИЖНИЙ БОЙ ===
                MeleeAttack();
            }

            lastAttackTime = Time.time;
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null || currentTarget == null) return;

        // 1. Точка вылета (чуть спереди)
        float dirX = transform.localScale.x;
        Vector3 spawnPos = transform.position + (Vector3.right * dirX * 0.6f);

        // 2. Создаем стрелу
        GameObject arrow = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // 3. МАТЕМАТИКА: Вычисляем угол поворота к врагу
        // Вектор от стрелы к врагу
        Vector3 difference = currentTarget.position - spawnPos;

        // Вычисляем угол в градусах (Arc Tangent)
        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        // Применяем поворот к стреле
        arrow.transform.rotation = Quaternion.Euler(0, 0, rotZ);

        // 4. Настраиваем скрипт стрелы
        ProjectileController arrowScript = arrow.GetComponent<ProjectileController>();
        if (arrowScript != null)
        {
            arrowScript.Setup(currentTarget, enemyTag, damage);
        }

        // 5. Игнорируем столкновения с собой
        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D arrowCollider = arrow.GetComponent<Collider2D>();
        if (myCollider != null && arrowCollider != null)
        {
            Physics2D.IgnoreCollision(myCollider, arrowCollider);
        }
    }

    void MeleeAttack()
    {
        UnitBrain enemyScript = currentTarget.GetComponent<UnitBrain>();
        if (enemyScript != null) enemyScript.TakeDamage(damage);
        else
        {
            BaseController baseCtrl = currentTarget.GetComponent<BaseController>();
            if (baseCtrl != null) baseCtrl.TakeDamage(damage);
        }
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        StartCoroutine(FlashColor());
        if (health <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        Destroy(gameObject);
    }

    IEnumerator FlashColor()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr)
        {
            // Красим в белый
            sr.color = Color.white;

            yield return new WaitForSeconds(0.1f);

            // Возвращаем РОДНОЙ цвет (а не тот, который был секунду назад)
            sr.color = defaultColor;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}