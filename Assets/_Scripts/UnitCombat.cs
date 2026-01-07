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

    void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time - lastAttackTime > attackSpeed)
        {
            // Проверка "свой-чужой"
            if (collision.gameObject.tag != gameObject.tag && collision.gameObject.tag != "Untagged")
            {
                // ВАРИАНТ 1: Мы бьем Юнита
                UnitCombat enemyUnit = collision.gameObject.GetComponent<UnitCombat>();
                if (enemyUnit != null)
                {
                    enemyUnit.TakeDamage(damage);
                    lastAttackTime = Time.time;
                    return; // Ударили и выходим
                }

                // ВАРИАНТ 2: Мы бьем Базу (НОВОЕ)
                BaseController enemyBase = collision.gameObject.GetComponent<BaseController>();
                if (enemyBase != null)
                {
                    enemyBase.TakeDamage(damage);
                    lastAttackTime = Time.time;
                    Debug.Log("Бью базу!");
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