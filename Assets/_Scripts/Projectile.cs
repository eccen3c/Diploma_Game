using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target; // Сама цель
    private Collider2D targetCollider; // Зеленая рамка цели (чтобы искать центр)

    private float speed = 5f;
    private float damage = 10f;
    private Team myTeam;

    public void Setup(Transform _target, Team _team, float _damage)
    {
        target = _target;
        myTeam = _team;
        damage = _damage;

        // Сразу запоминаем коллайдер цели, если он есть
        if (target != null)
        {
            targetCollider = target.GetComponent<Collider2D>();
        }
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // --- УМНОЕ ПРИЦЕЛИВАНИЕ ---
        // Если у врага есть коллайдер — летим в центр коллайдера (в грудь)
        // Если нет — летим просто в позицию (в ноги)
        Vector3 aimPoint = (targetCollider != null) ? targetCollider.bounds.center : target.position;

        // Летим к этой точке
        transform.position = Vector2.MoveTowards(transform.position, aimPoint, speed * Time.deltaTime);

        // Поворот носа пули
        Vector3 dir = aimPoint - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Проверка попадания
        if (Vector2.Distance(transform.position, aimPoint) < 0.2f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        UnitStats enemyStats = target.GetComponent<UnitStats>();
        if (enemyStats != null)
        {
            if (enemyStats.team != myTeam)
            {
                enemyStats.TakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }
}