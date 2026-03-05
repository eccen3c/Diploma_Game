using UnityEngine;

public class UnitController : MonoBehaviour
{
    [Header("Параметры (Заполняются сами)")]
    public float hp;
    public float damage;
    public float speed;
    public float range;
    public float visionRange;
    public float attackSpeed;

    [Header("Ranged Settings (Для лучников/магов)")]
    public GameObject projectilePrefab; // <-- НОВОЕ: Сюда кидай префаб стрелы. Если пусто = ближник.

    [Header("Aiming System")]
    public Transform aimPoint;

    [Header("Настройки толпы")]
    public float avoidanceRadius = 0.5f;
    public float avoidanceForce = 2.0f;

    private float verticalAttackTreshold = 1f;
    private float myLaneOffset;

    private float lastAttackTime;
    private Transform currentTarget;
    private bool isDead = false;

    // Компоненты
    private Animator anim;
    private Rigidbody2D rb;

    // Кто враг?
    private string myTag;
    private string enemyTag;

    // Этот метод вызывает GameLoopManager при спавне
    public void SetupUnit(UnitData data)
    {
        hp = data.hp;
        damage = data.damage;
        speed = data.moveSpeed;
        range = data.attackRange;
        visionRange = data.visionRange;
        attackSpeed = data.attackSpeed;
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        myTag = gameObject.tag;

        if (myTag == "Player")
            enemyTag = "Enemy";
        else
            enemyTag = "Player";

        myLaneOffset = Random.Range(-1.5f, 1.5f);
    }

    void Update()
    {
        if (isDead) return;

        FindTarget();

        if (currentTarget != null)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);
            float yDifference = Mathf.Abs(transform.position.y - currentTarget.position.y);

            // --- НОВАЯ ЛОГИКА АТАКИ ---

            // 1. Мы стрелок? (Есть ли у нас пули)
            bool isRanged = (projectilePrefab != null);

            // 2. Можем ли мы ударить?
            // Условие: Враг должен быть в радиусе (distance <= range)
            // И ПЛЮС К ЭТОМУ:
            // Либо мы стрелок (нам плевать на высоту),
            // Либо мы ближник и стоим на одной линии (yDifference <= порог)
            bool canHit = (distance <= range) && (isRanged || yDifference <= verticalAttackTreshold);

            if (canHit)
            {
                // Враг доступен для удара -> СТОИМ И БЬЕМ
                StopMoving();
                if (Time.time >= lastAttackTime + attackSpeed)
                {
                    Attack();
                }
            }
            else
            {
                // Враг далеко или (если мы мечник) не на линии -> ИДЕМ К НЕМУ
                MoveTo(currentTarget.position);
            }
        }
        else
        {
            // Врагов нет -> Идем вперед к базе врага
            float dir = (myTag == "Player") ? 1f : -1f;
            Vector3 forwardPos = new Vector3(transform.position.x + (dir * 5f), transform.position.y, 0);
            MoveTo(forwardPos);
        }
    }

    void MoveTo(Vector3 target)
    {
        if (anim) anim.SetBool("isMoving", true);
        if (rb) rb.mass = 1f;

        Vector2 targetDir = (target - transform.position).normalized;

        Vector2 avoidDir = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, avoidanceRadius);

        foreach (var hit in neighbors)
        {
            if (hit.CompareTag(myTag) && hit.gameObject != gameObject)
            {
                if (hit.isTrigger) continue; // Игнорируем хитбоксы груди и базы

                Vector2 away = transform.position - hit.transform.position;
                float distance = away.magnitude;

                // Защита от спавна пиксель-в-пиксель (чтобы не было деления на ноль)
                if (distance <= 0.001f)
                {
                    away = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                    distance = away.magnitude;
                }

                // 1. Чем ближе сосед, тем сильнее отталкиваемся
                float pushStrength = Mathf.Clamp01(1f - (distance / avoidanceRadius));

                // 2. Вектор "вбок" (перпендикуляр), чтобы плавно обтекать
                Vector2 sideSlip = new Vector2(-away.y, away.x).normalized * 0.5f;

                // 3. Смешиваем отталкивание назад и скольжение вбок
                avoidDir += (away.normalized + sideSlip) * pushStrength;
            }
        }

        // Магнит к линии (чтобы они не разбредались слишком высоко/низко)
        float desiredY = 0f;
        Vector2 laneCorrection = Vector2.zero;
        if (Mathf.Abs(transform.position.y - desiredY) > 0.5f)
        {
            float personalTargetY = desiredY + myLaneOffset;
            float dirY = (personalTargetY - transform.position.y);
            laneCorrection = new Vector2(0, dirY).normalized * 0.5f;
        }

        // Итоговое направление = Вперед + Обтекание + Магнит к линии
        Vector2 finalDir = (targetDir + avoidDir * avoidanceForce + laneCorrection).normalized;

        if (rb) rb.MovePosition(rb.position + finalDir * speed * Time.fixedDeltaTime);
    }

    void StopMoving()
    {
        if (anim) anim.SetBool("isMoving", false);
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.mass = 100f;
        }
    }

    void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRange);

        float closestUnitDist = Mathf.Infinity;
        float closestBaseDist = Mathf.Infinity;

        Transform targetUnit = null;
        Transform targetBase = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag(myTag)) continue;

            if (hit.CompareTag(enemyTag))
            {
                float d = Vector2.Distance(transform.position, hit.transform.position);
                if (d < closestUnitDist)
                {
                    closestUnitDist = d;
                    targetUnit = hit.transform;
                }
            }
            else if (hit.GetComponent<BaseController>())
            {
                float d = Vector2.Distance(transform.position, hit.transform.position);
                if (d < closestBaseDist)
                {
                    closestBaseDist = d;
                    targetBase = hit.transform;
                }
            }
        }

        if (targetUnit != null)
            currentTarget = targetUnit;
        else if (targetBase != null)
            currentTarget = targetBase;
        else
            currentTarget = null;
    }

    void Attack()
    {
        if (anim) anim.SetTrigger("attack");
        lastAttackTime = Time.time;
    }

    public void DealDamage()
    {
        if (currentTarget == null) return;

        // --- ЛОГИКА AIM POINT (Куда стрелять?) ---
        Transform targetTransform = currentTarget; // По умолчанию - в ноги (root)

        // 1. Пробуем найти AimPoint у Юнита
        UnitController enemyUnit = currentTarget.GetComponent<UnitController>();
        if (enemyUnit != null && enemyUnit.aimPoint != null)
        {
            targetTransform = enemyUnit.aimPoint;
        }
        // 2. Пробуем найти AimPoint у Базы
        else
        {
            BaseController enemyBase = currentTarget.GetComponent<BaseController>();
            if (enemyBase != null && enemyBase.aimPoint != null)
            {
                targetTransform = enemyBase.aimPoint;
            }
        }
        // ----------------------------------------


        if (projectilePrefab != null)
        {
            // Стреляем от СВОЕГО AimPoint (если он есть), иначе от ног
            Vector3 spawnPos = (aimPoint != null) ? aimPoint.position : transform.position;

            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            ProjectileController pc = proj.GetComponent<ProjectileController>();

            if (pc != null)
            {
                pc.damage = (int)this.damage;
                // Передаем КОНКРЕТНУЮ точку (AimPoint или ноги)
                pc.SetTarget(targetTransform, this.tag);
            }
        }
        else // Ближний бой
        {
            if (enemyUnit) enemyUnit.TakeDamage(damage);
            BaseController baseCtrl = currentTarget.GetComponent<BaseController>();
            if (baseCtrl) baseCtrl.TakeDamage((int)damage);
        }
    }
    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        if (hp <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // --- ИСПРАВЛЕНИЕ: Отключаем ВСЕ коллайдеры (и ноги, и грудь) ---
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        // ---------------------------------------------------------------

        if (rb) rb.simulated = false;

        if (anim) anim.SetTrigger("die");

        Destroy(gameObject, 0.65f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}