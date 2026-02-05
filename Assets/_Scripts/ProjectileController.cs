using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float speed = 12f; // Чуть быстрее для стрел
    public int damage = 25;

    private Transform target;
    private string ownerTag; // Тег того, кто выстрелил (чтобы не попадать в своих)
    private Vector3 lastDirection; // Чтобы лететь прямо, если цель умерла

    void Start()
    {
        Destroy(gameObject, 3f); // Убить стрелу через 3 сек, если улетела в молоко
        lastDirection = transform.right; // По умолчанию летим вправо
    }

    // Обновили метод настройки: теперь принимаем и тег стрелка
    public void SetTarget(Transform newTarget, string tag)
    {
        target = newTarget;
        ownerTag = tag;
    }

    void Update()
    {
        if (target != null)
        {
            // --- НОВАЯ ЛОГИКА ДВИЖЕНИЯ (Магнит) ---
            // Стрела летит строго в точку цели. Промах исключен.
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            // Поворачиваем картинку стрелы к цели (чисто визуал)
            Vector3 dir = target.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Запоминаем направление на случай смерти цели
            lastDirection = dir.normalized;
        }
        else
        {
            // Если цель умерла на лету - летим дальше по прямой
            transform.position += lastDirection * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger) return;

        // --- ГЛАВНОЕ: ИГНОРИРУЕМ СВОИХ ---
        // Если мы попали в объект с таким же тегом, как у стрелка - ничего не делаем
        if (collision.CompareTag(ownerTag)) return;

        // Попадание во врага
        UnitController unit = collision.GetComponent<UnitController>();
        BaseController baseCtrl = collision.GetComponent<BaseController>();

        // Если попали в Юнита
        if (unit != null)
        {
            unit.TakeDamage(damage);
            Destroy(gameObject); // Уничтожаем стрелу
        }
        // Если попали в Базу
        else if (baseCtrl != null)
        {
            baseCtrl.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}