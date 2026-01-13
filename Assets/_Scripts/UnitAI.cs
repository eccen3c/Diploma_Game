using UnityEngine;

public class UnitAI : MonoBehaviour
{
    private UnitStats myStats;
    private Animator anim;
    private UnitStats target; // Текущая цель
    private float attackCooldown = 0f;
    private Rigidbody2D rb;

    [Header("Настройки ИИ")]
    public float detectRange = 0.8f; // Дистанция поиска врага (чуть меньше AttackRange)
    public LayerMask hitLayer; // Кого можно бить (слой)

    void Start()
    {
        myStats = GetComponent<UnitStats>();
        anim = GetComponent<Animator>();
        rb= GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. Уменьшаем таймер перезарядки
        attackCooldown -= Time.deltaTime;

        // 2. Ищем врага
        DetectEnemy();

        // 3. Логика поведения
        if (target != null)
        {
            // Если цель есть -> Атакуем
            if (anim != null) anim.SetBool("IsMoving", false);

            if (attackCooldown <= 0)
            {
                Attack();
                attackCooldown = 1f / myStats.attackSpeed;
            }
        }
        else
        {
            // Если цели нет -> Идем вперед
            if (anim != null)
            {
                anim.SetBool("IsMoving", true);
            }
            Move();
        }
    }

    void Move()
    {
        float direction = (myStats.team == Team.Player) ? 1f : -1f;

        // Двигаем через физику, а не телепортацию!
        // ПРАВИЛЬНО:
        // Мы берем текущую rb.velocity.y, чтобы позволить физике расталкивать их вверх/вниз
        rb.velocity = new Vector2(direction * myStats.moveSpeed, rb.velocity.y);

        // Поворот спрайта
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, 1);
    }

    void DetectEnemy()
    {
        // Если цель уже есть и она жива (и в радиусе атаки) — не ищем новую
        if (target != null)
        {
            if (target.currentHealth <= 0)
            {
                target = null; // Цель умерла, забываем
                return;
            }

            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (dist > myStats.attackRange + 0.5f) // Если цель убежала
            {
                target = null;
            }
            return;
        }

        // Бросаем луч вперед, чтобы найти новую жертву
        float dir = (myStats.team == Team.Player) ? 1f : -1f;

        // Используем RaycastAll, чтобы видеть сквозь своих
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.right * dir, myStats.attackRange);

        foreach (RaycastHit2D hit in hits)
        {
            UnitStats unit = hit.collider.GetComponent<UnitStats>();
            // Если нашли юнита И он из чужой команды
            if (unit != null && unit.team != myStats.team && unit.currentHealth > 0)
            {
                target = unit;
                break; // Нашли первого попавшегося — хватит
            }
        }
    }

    // Этот метод вызывает Update, чтобы начать анимацию
    void Attack()
    {
        if (target == null) return;
        if (anim != null) anim.SetTrigger("Attack");

        // ВАЖНО: Мы БОЛЬШЕ НЕ наносим урон здесь!
        // Мы ждем, пока анимация дойдет до кадра удара.
    }

    // А этот метод вызовет САМА АНИМАЦИЯ в нужный момент
    public void DealDamageFromAnimation()
    {
        // Проверяем, жив ли еще враг перед нами
        if (target != null)
        {
            target.TakeDamage(myStats.damage);
        }
    }

    // Рисуем луч атаки в редакторе
    void OnDrawGizmosSelected()
    {
        if (myStats != null)
        {
            float dir = (myStats.team == Team.Player) ? 1f : -1f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, Vector2.right * dir * myStats.attackRange);
        }
    }
}