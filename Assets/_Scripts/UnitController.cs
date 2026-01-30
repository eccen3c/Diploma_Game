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

    [Header("Настройки толпы")]
    public float avoidanceRadius = 0.5f; // Радиус личного пространства
    public float avoidanceForce = 2.0f;  // Сила отталкивания от своих

    private float verticalAttackTreshold = 1f; // Порог по вертикали для атаки (чтобы не атаковать юнита выше себя)
    private float myLaneOffset; // Личная полоса этого юнита

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

        myTag = gameObject.tag; // "Player" или "Enemy"

        // Определяем тэг врага
        if (myTag == "Player")
            enemyTag = "Enemy";
        else
            enemyTag = "Player";

        myLaneOffset = Random.Range(-1.5f, 1.5f);
    }

    void Update()
    {
        if (isDead) return;

        FindTarget(); // Ищем цель (по новому радиусу visionRange)

        if (currentTarget != null)
        {
            // Считаем дистанцию до цели
            float distance = Vector2.Distance(transform.position, currentTarget.position);

            // Считаем РАЗНИЦУ ПО ВЫСОТЕ (Y)
            float yDifference = Mathf.Abs(transform.position.y - currentTarget.position.y);

            // УСЛОВИЕ АТАКИ:
            // 1. Мы достаточно близко (distance <= range)
            // 2. Враг примерно на нашей высоте (yDifference <= порог)
            if (distance <= range && yDifference <= verticalAttackTreshold)
            {
                // Враг рядом и на одной линии -> БЬЕМ
                StopMoving();
                if (Time.time >= lastAttackTime + attackSpeed)
                {
                    Attack();
                }
            }
            else
            {
                // Враг либо далеко, либо слишком высоко/низко -> ИДЕМ К НЕМУ
                // Юнит подойдет вплотную и выровняется по Y
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

        if (rb) rb.mass = 1f; // Легкий пока идет

        // 1. Идем к цели
        Vector2 targetDir = (target - transform.position).normalized;

        // 2. Отталкиваемся от своих (как было)
        Vector2 avoidDir = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, avoidanceRadius);
        foreach (var hit in neighbors)
        {
            if (hit.CompareTag(myTag) && hit.gameObject != gameObject)
            {
                Vector2 away = transform.position - hit.transform.position;
                avoidDir += away.normalized / (away.magnitude + 0.1f);
            }
        }

        // 3. НОВОЕ: Магнит к линии боя (Lane Gravity)
        // Чтобы они не разлетались по всей карте вверх/вниз
        float desiredY = 0f; // <--- ПОДБЕРИ ЭТУ ВЫСОТУ! (Где должны ходить, на уровне баз)

        Vector2 laneCorrection = Vector2.zero;
        // Если мы слишком далеко ушли вверх или вниз от линии
        if (Mathf.Abs(transform.position.y - desiredY) > 0.5f)
        {
            // Теперь цель не просто центр, а центр + личное смещение
            float personalTargetY = desiredY + myLaneOffset;

            // Тянемся к своей личной полосе
            float dirY = (personalTargetY - transform.position.y);
            laneCorrection = new Vector2(0, dirY).normalized * 0.5f; // Сила возврата (0.5f - мягкая)
        }

        // 4. Складываем всё вместе
        // (Цель + Отталкивание + Возврат на линию)
        Vector2 finalDir = (targetDir + avoidDir * avoidanceForce + laneCorrection).normalized;

        if (rb) rb.MovePosition(rb.position + finalDir * speed * Time.fixedDeltaTime);
    }
    void StopMoving()
    {
        if (anim) anim.SetBool("isMoving", false);

        if (rb)
        {
            rb.velocity = Vector2.zero; // Полный стоп
            rb.mass = 100f; // Становимся ТЯЖЕЛЫМ, чтобы свои не толкали
        }
    }

    void FindTarget()
    {
        // 1. Ищем ВСЕХ в радиусе зрения (visionRange)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRange);

        float closestUnitDist = Mathf.Infinity;
        float closestBaseDist = Mathf.Infinity;

        Transform targetUnit = null;
        Transform targetBase = null;

        foreach (var hit in hits)
        {
            // Пропускаем себя и своих
            if (hit.CompareTag(myTag)) continue;

            // Если нашли ВРАГА-ЮНИТА
            if (hit.CompareTag(enemyTag))
            {
                float d = Vector2.Distance(transform.position, hit.transform.position);
                if (d < closestUnitDist)
                {
                    closestUnitDist = d;
                    targetUnit = hit.transform;
                }
            }
            // Если нашли ВРАЖЕСКУЮ БАЗУ (Кристалл)
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

        // ЛОГИКА ПРИОРИТЕТА:
        // Если есть враг-юнит -> бьем его.
        if (targetUnit != null)
        {
            currentTarget = targetUnit;
        }
        // Если врагов нет, но видим базу -> идем ломать базу
        else if (targetBase != null)
        {
            currentTarget = targetBase;
        }
        // Если вообще никого не видим -> currentTarget останется null (и сработает логика "Иди вперед")
        else
        {
            currentTarget = null;
        }
    }
    // 1. ЗАМАХ (Вызывается из Update, когда пришло время бить)
    void Attack()
    {
        // Запускаем анимацию
        if (anim) anim.SetTrigger("attack");

        // Засекаем время перезарядки СРАЗУ, чтобы он не спамил ударами
        lastAttackTime = Time.time;
    }

    // 2. УДАР (Вызывается САМОЙ АНИМАЦИЕЙ в нужный кадр)
    // Обязательно public, иначе аниматор его не увидит!
    public void DealDamage()
    {
        // Проверяем, стоит ли еще враг перед нами
        if (currentTarget != null)
        {
            // Если это юнит
            UnitController unit = currentTarget.GetComponent<UnitController>();
            if (unit) unit.TakeDamage(damage);

            // Если это база
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

        // 1. Отключаем физику и коллайдер СРАЗУ, чтобы труп не мешал ходить
        if (GetComponent<Collider2D>()) GetComponent<Collider2D>().enabled = false;
        if (rb) rb.simulated = false; // Юнит перестает быть физическим объектом

        // 2. Запускаем анимацию смерти
        if (anim) anim.SetTrigger("die");

        // 3. Удаляем объект очень быстро (через 0.3 секунды)
        // Анимация успеет проиграть падение, и он сразу исчезнет
        Destroy(gameObject, 0.65f);
    }

    // ---------------------------------------------------------
    // РИСОВАНИЕ РАДИУСА АТАКИ В РЕДАКТОРЕ
    // ---------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        // Красный круг - куда достает топор
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        // Желтый круг - где замечает врага
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

}