using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Параметры")]
    public float speed = 12f; // Чуть быстрее, чтобы догоняла

    private int damage = 10;
    private string targetTag;
    private Transform targetTransform; // Конкретная жертва
    private Vector3 lastKnownPosition; // Куда лететь, если жертва умерла

    // Теперь мы принимаем Transform target
    public void Setup(Transform target, string tagToHit, int dmg)
    {
        targetTransform = target;
        targetTag = tagToHit;
        damage = dmg;

        // Если цель сразу мертва, летим просто вперед
        if (target != null) lastKnownPosition = target.position;
        else lastKnownPosition = transform.position + transform.right * 10f;

        Destroy(gameObject, 4.0f); // Защита от бесконечного полета
    }

    void Update()
    {
        // 1. Если цель жива - летим прямо в неё
        if (targetTransform != null)
        {
            // Обновляем последнюю позицию
            lastKnownPosition = targetTransform.position;

            // Двигаемся к цели (MoveTowards делает точное движение в точку)
            transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, speed * Time.deltaTime);

            // Поворачиваем стрелу носом к цели (для красоты)
            Vector3 dir = targetTransform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else
        {
            // 2. Если цель умерла в полете - летим в то место, где она была в последний раз
            transform.position = Vector3.MoveTowards(transform.position, lastKnownPosition, speed * Time.deltaTime);

            // Если долетели до пустого места - уничтожаемся
            if (Vector3.Distance(transform.position, lastKnownPosition) < 0.1f)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Логика попадания та же самая
        if (collision.CompareTag(targetTag) ||
           (collision.GetComponent<BaseController>() && !collision.CompareTag(gameObject.tag)))
        {
            UnitBrain unit = collision.GetComponent<UnitBrain>();
            if (unit != null) unit.TakeDamage(damage);

            BaseController baseCtrl = collision.GetComponent<BaseController>();
            if (baseCtrl != null) baseCtrl.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}