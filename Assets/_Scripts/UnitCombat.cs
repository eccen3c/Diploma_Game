using UnityEngine;
using System.Collections;

public class UnitCombat : MonoBehaviour
{
    [Header("Характеристики")]
    public int health = 100;     // Жизни
    public int damage = 20;      // Урон
    public float attackSpeed = 1.0f; // Скорость атаки (раз в сек)

    private float lastAttackTime; // Время последнего удара

    void Start()
    {
        // Важная настройка: чтобы физика не "засыпала", когда юнит стоит
        // Иначе они могут перестать бить друг друга
        GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    // Эта функция вызывается сама, ПОКА мы касаемся кого-то
    void OnCollisionStay2D(Collision2D collision)
    {
        // 1. Проверяем, прошло ли достаточно времени для удара
        if (Time.time - lastAttackTime > attackSpeed)
        {
            // 2. Проверяем, враг ли это (теги должны быть разными)
            if (collision.gameObject.tag != gameObject.tag && collision.gameObject.tag != "Untagged")
            {
                // 3. Пытаемся найти у того, в кого врезались, скрипт UnitCombat
                UnitCombat enemyStats = collision.gameObject.GetComponent<UnitCombat>();

                if (enemyStats != null) // Если у цели есть жизни
                {
                    // БЬЕМ!
                    enemyStats.TakeDamage(damage);
                    lastAttackTime = Time.time; // Сбрасываем таймер
                    Debug.Log(gameObject.name + " ударил " + collision.gameObject.name);
                }
            }
        }
    }

    // Функция получения урона (публичная, чтобы ее могли вызвать другие)
    public void TakeDamage(int dmg)
    {
        health -= dmg;

        // Меняем цвет на секунду, чтобы видеть попадание (Вспышка)
        StartCoroutine(FlashColor());

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " погиб!");
        Destroy(gameObject); // Удалить объект со сцены
    }

    // Небольшая анимация получения урона (мигает белым)
    IEnumerator FlashColor()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;
        sr.color = Color.white; // Вспышка
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor; // Возврат цвета
    }
}