using UnityEngine;
using System.Collections;

public class UnitBrain : MonoBehaviour
{
    [Header("Параметры")]
    public float speed = 2.0f;
    public float detectRange = 6.0f; // Радиус поиска врага
    public float attackRange = 0.8f; // Дистанция удара

    [Header("Интеллект Толпы (НОВОЕ)")]
    public float separationRadius = 1.0f; // Дистанция личного пространства
    public float separationForce = 2.0f;  // Сила отталкивания от своих

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
                AttackLogic();
            }
            else
            {
                // Идем к врагу с учетом толпы
                MoveWithFlocking(currentTarget.position);
            }
        }
        else
        {
            // Идем вперед, если врагов нет
            float dir = (gameObject.tag == "Ally") ? 1 : -1;
            Vector3 forwardPoint = transform.position + Vector3.right * dir * 10f;
            MoveWithFlocking(forwardPoint);
        }
    }

    // НОВАЯ ФУНКЦИЯ ДВИЖЕНИЯ
    void MoveWithFlocking(Vector3 targetPos)
    {
        // 1. Вектор желания: "Хочу к врагу"
        Vector2 directionToTarget = (targetPos - transform.position).normalized;

        // 2. Вектор отвращения: "Хочу подальше от соседей"
        Vector2 separation = GetSeparationVector();

        // 3. Складываем их (Иду к врагу + Сдвигаюсь от своих)
        Vector2 finalDirection = (directionToTarget + separation).normalized;

        // Двигаем
        transform.position += (Vector3)finalDirection * speed * Time.deltaTime;

        // Поворот лица
        if (finalDirection.x > 0.1f) transform.localScale = new Vector3(1, 1, 1);
        else if (finalDirection.x < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
    }

    // Вычисляем, куда нас толкают друзья
    Vector2 GetSeparationVector()
    {
        Vector2 pushForce = Vector2.zero;

        // Находим всех соседей вокруг
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, separationRadius);

        foreach (var neighbor in neighbors)
        {
            // Если это "Свой" и это не "Я сам"
            if (neighbor.CompareTag(gameObject.tag) && neighbor.gameObject != gameObject)
            {
                // Вектор ОТ соседа ко мне
                Vector2 pushDir = transform.position - neighbor.transform.position;

                // Чем ближе сосед, тем сильнее толкает (1 / дистанция)
                float dist = pushDir.magnitude;
                if (dist > 0.01f) // Защита от деления на ноль
                {
                    pushForce += pushDir.normalized / dist;
                }
            }
        }

        return pushForce * separationForce;
    }

    // ... (Остальные функции FindNearestEnemy, AttackLogic, TakeDamage, Die, FlashColor - без изменений) ...
    // Но для удобства я их скопировал сюда, чтобы ты мог заменить весь файл целиком:

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
            UnitBrain enemyScript = currentTarget.GetComponent<UnitBrain>();
            if (enemyScript != null) enemyScript.TakeDamage(damage);
            else
            {
                BaseController baseCtrl = currentTarget.GetComponent<BaseController>();
                if (baseCtrl != null) baseCtrl.TakeDamage(damage);
            }
            lastAttackTime = Time.time;
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
            // 1. Запоминаем, какого цвета был юнит (Синий или Красный)
            Color original = sr.color;

            // 2. Делаем вспышку БЕЛЫМ (эффект удара)
            sr.color = Color.white;

            // 3. Ждем долю секунды
            yield return new WaitForSeconds(0.1f);

            // 4. Возвращаем родной цвет
            sr.color = original;
        }
    }
}