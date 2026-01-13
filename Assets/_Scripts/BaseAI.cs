using UnityEngine;

public class BaseAI : MonoBehaviour
{
    [Header("Снаряд")]
    public GameObject projectilePrefab; // Сюда кинем фаербол
    public Transform firePoint; // Откуда вылетает (центр кристалла)

    private UnitStats myStats;
    private float attackCooldown = 0f;

    void Start()
    {
        myStats = GetComponent<UnitStats>();
    }

    void Update()
    {
        attackCooldown -= Time.deltaTime;

        if (attackCooldown <= 0)
        {
            // Ищем ближайшего врага
            Transform target = FindClosestEnemy();

            if (target != null)
            {
                Shoot(target);
                attackCooldown = 1f / myStats.attackSpeed; // Перезарядка
            }
        }
    }

    void Shoot(Transform target)
    {
        // Создаем пулю
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        // Настраиваем пулю
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            // Передаем цель, свою команду и свой урон
            p.Setup(target, myStats.team, myStats.damage);
        }
    }

    Transform FindClosestEnemy()
    {
        // Находим ВСЕХ юнитов с UnitStats
        UnitStats[] allUnits = FindObjectsOfType<UnitStats>();

        Transform closest = null;
        float minDistance = myStats.attackRange; // Ищем только в радиусе атаки

        foreach (UnitStats unit in allUnits)
        {
            // Если это враг И он жив
            if (unit.team != myStats.team && unit.currentHealth > 0)
            {
                float dist = Vector2.Distance(transform.position, unit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = unit.transform;
                }
            }
        }
        return closest;
    }

    // Рисуем радиус атаки в редакторе (чтобы видеть круг)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (GetComponent<UnitStats>() != null)
            Gizmos.DrawWireSphere(transform.position, GetComponent<UnitStats>().attackRange);
    }
}