using UnityEngine;
using Photon.Pun; // Додаємо для безпечної перевірки мережевого статусу та RPC

public class UnitController : MonoBehaviour
{
    [Header("Характеристики (Завантажуються з Data)")]
    public float hp;
    public float damage;
    public float speed;
    public float range;
    public float visionRange;
    public float attackSpeed;

    [Header("Ranged Settings (Для лучників/магів)")]
    public GameObject projectilePrefab; // Якщо пусто = ближній бій

    [Header("Aiming System")]
    public Transform aimPoint;

    [Header("Обходження союзників")]
    public float avoidanceRadius = 0.5f;
    public float avoidanceForce = 2.0f;

    private float verticalAttackTreshold = 1f;
    private float myLaneOffset;

    private float lastAttackTime;
    private Transform currentTarget;
    private bool isDead = false;

    // Компоненти
    private Animator anim;
    private Rigidbody2D rb;
    private PhotonView myPV; // Компонент PhotonView для мережевих перевірок

    // Змінна для уникнення зайвого спаму RPC в мережу
    private bool lastSentMovingState = false;

    // Хто ворог?
    private string myTag;
    private string enemyTag;

    // Метод ініціалізації юніта (Тепер приймає playerNum)
    public void SetupUnit(UnitData data, int playerNum)
    {
        hp = data.hp;
        damage = data.damage;
        speed = data.moveSpeed;
        range = data.attackRange;
        visionRange = data.visionRange;
        attackSpeed = data.attackSpeed;

        // Логіка автоматичного розвороту для Червоної команди (Гравець 2)
        myPV = GetComponent<PhotonView>();
        if (myPV == null || myPV.IsMine)
        {
            if (playerNum == 2)
            {
                // Перевіряємо, куди дивиться префаб за замовчуванням.
                if (transform.right.x > 0f)
                {
                    // Розгортаємо об'єкт по осі Y на 180 градусів.
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                }
            }
        }
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        myPV = GetComponent<PhotonView>();

        myTag = gameObject.tag;

        if (myTag == "Player")
            enemyTag = "Enemy";
        else
            enemyTag = "Player";

        myLaneOffset = Random.Range(-1.5f, 1.5f);
    }

    void Update()
    {
        // КРИТИЧНО ДЛЯ МЕРЕЖІ: Якщо гра в онлайні і цей юніт НЕ наш — ми повністю ігноруємо Update.
        // Але якщо він мертвий, зупиняємо будь-яку логіку і для Гостя.
        if (myPV != null && !myPV.IsMine) return;

        if (isDead) return;

        FindTarget();

        if (currentTarget != null)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);
            float yDifference = Mathf.Abs(transform.position.y - currentTarget.position.y);

            bool isRanged = (projectilePrefab != null);
            bool canHit = (distance <= range) && (isRanged || yDifference <= verticalAttackTreshold);

            if (canHit)
            {
                StopMoving();
                if (Time.time >= lastAttackTime + attackSpeed)
                {
                    Attack();
                }
            }
            else
            {
                MoveTo(currentTarget.position);
            }
        }
        else
        {
            // Якщо ворогів немає — йдемо вперед по своїй лінії.
            Vector3 forwardPos = transform.position + transform.right * 5f;
            MoveTo(forwardPos);
        }
    }

    void MoveTo(Vector3 target)
    {
        // Вмикаємо анімацію руху через мережевий метод
        SetMovingState(true);

        if (rb) rb.mass = 1f;

        Vector2 targetDir = (target - transform.position).normalized;

        Vector2 avoidDir = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, avoidanceRadius);

        foreach (var hit in neighbors)
        {
            if (hit.CompareTag(myTag) && hit.gameObject != gameObject)
            {
                if (hit.isTrigger) continue;

                Vector2 away = transform.position - hit.transform.position;
                float distance = away.magnitude;

                if (distance <= 0.001f)
                {
                    away = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                    distance = away.magnitude;
                }

                float pushStrength = Mathf.Clamp01(1f - (distance / avoidanceRadius));
                Vector2 sideSlip = new Vector2(-away.y, away.x).normalized * 0.5f;

                avoidDir += (away.normalized + sideSlip) * pushStrength;
            }
        }

        float desiredY = 0f;
        Vector2 laneCorrection = Vector2.zero;
        if (Mathf.Abs(transform.position.y - desiredY) > 0.5f)
        {
            float personalTargetY = desiredY + myLaneOffset;
            float dirY = (personalTargetY - transform.position.y);
            laneCorrection = new Vector2(0, dirY).normalized * 0.5f;
        }

        Vector2 finalDir = (targetDir + avoidDir * avoidanceForce + laneCorrection).normalized;

        if (rb) rb.MovePosition(rb.position + finalDir * speed * Time.fixedDeltaTime);
    }

    void StopMoving()
    {
        // Вимикаємо анімацію руху через мережевий метод
        SetMovingState(false);

        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.mass = 100f;
        }
    }

    // Допоміжний метод для відправки RPC стану ходьби (без забивання мережі кожний кадр)
    void SetMovingState(bool moving)
    {
        if (anim) anim.SetBool("isMoving", moving);

        if (myPV != null && myPV.IsMine)
        {
            // Відправляємо RPC тільки тоді, коли стан реально змінився (наприклад, йшов -> зупинився)
            if (moving != lastSentMovingState)
            {
                lastSentMovingState = moving;
                myPV.RPC("RPC_SetMovingState", RpcTarget.Others, moving);
            }
        }
    }

    // Мережевий виклик зміни стану ходьби для Гостя
    [PunRPC]
    void RPC_SetMovingState(bool moving)
    {
        if (anim && !isDead)
        {
            anim.SetBool("isMoving", moving);
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
        lastAttackTime = Time.time;

        // Запуск анімації через RPC для надійної синхронізації
        if (myPV != null && myPV.IsMine)
        {
            myPV.RPC("PlayAttackAnimation", RpcTarget.All);
        }
        else if (myPV == null)
        {
            if (anim) anim.SetTrigger("attack"); // Локально для офлайну
        }
    }

    // Мережевий виклик анімації удару для всіх клієнтів
    [PunRPC]
    void PlayAttackAnimation()
    {
        if (anim) anim.SetTrigger("attack");
    }

    public void DealDamage()
    {
        // Тільки власник прораховує нанесення шкоди, щоб вона не дублювалася
        if (myPV != null && !myPV.IsMine) return;

        if (currentTarget == null) return;

        if (AudioManager.Instance != null)
        {
            if (projectilePrefab != null)
                AudioManager.Instance.PlayFireball();
            else
                AudioManager.Instance.PlayHitSound();
        }

        Transform targetTransform = currentTarget;

        UnitController enemyUnit = currentTarget.GetComponent<UnitController>();
        if (enemyUnit != null && enemyUnit.aimPoint != null)
        {
            targetTransform = enemyUnit.aimPoint;
        }
        else
        {
            BaseController enemyBase = currentTarget.GetComponent<BaseController>();
            if (enemyBase != null && enemyBase.aimPoint != null)
            {
                targetTransform = enemyBase.aimPoint;
            }
        }

        if (projectilePrefab != null)
        {
            Vector3 spawnPos = (aimPoint != null) ? aimPoint.position : transform.position;

            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            ProjectileController pc = proj.GetComponent<ProjectileController>();

            if (pc != null)
            {
                pc.damage = (int)this.damage;
                pc.SetTarget(targetTransform, this.tag);
            }
        }
        else
        {
            if (enemyUnit) enemyUnit.TakeDamage(damage);
            BaseController baseCtrl = currentTarget.GetComponent<BaseController>();
            if (baseCtrl) baseCtrl.TakeDamage((int)damage);
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        hp -= dmg;
        if (hp <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Миттєво вимикаємо фізику на обох клієнтах локально
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        if (rb) rb.simulated = false;

        if (myPV != null)
        {
            // Запускаємо анімацію смерті через RPC, щоб Гість точно її побачив
            if (myPV.IsMine)
            {
                myPV.RPC("PlayDieAnimation", RpcTarget.All);
                // Повністю видаляємо об'єкт з кімнати через корутину із затримкою
                StartCoroutine(DestroyNetworkObjectDelayed(0.65f));
            }
        }
        else
        {
            // Офлайн режим
            if (anim) anim.SetTrigger("die");
            Destroy(gameObject, 0.65f);
        }
    }

    // Мережевий виклик анімації смерті для всіх клієнтів
    [PunRPC]
    void PlayDieAnimation()
    {
        if (anim) anim.SetTrigger("die");
    }

    private System.Collections.IEnumerator DestroyNetworkObjectDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}