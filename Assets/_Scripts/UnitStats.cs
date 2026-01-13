using UnityEngine;

// Создаем "Тип данных" - Команда. Это удобнее, чем строки.
public enum Team
{
    Player, // Игрок (Слева)
    Enemy   // Враг (Справа)
}

public class UnitStats : MonoBehaviour
{
    [Header("Главное")]
    public string unitName = "Unit";
    public Team team; // Выбираем в инспекторе: Player или Enemy
    public int cost = 10;

    [Header("Боевые параметры")]
    public float maxHealth = 100;
    public float currentHealth;
    public float damage = 10;

    [Header("Скорость и Атака")]
    public float moveSpeed = 2f;    // Для базы ставь 0
    public float attackRange = 1f;  // Дальность стрельбы/удара
    public float attackSpeed = 1f;  // Сколько ударов в секунду

    [Header("Компоненты (Заполнится само)")]
    public Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    // Метод получения урона
    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;

        // Если есть анимация получения урона - можно включить тут
        // if (anim != null) anim.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(unitName + " погиб!");

        // Если есть анимация смерти
        if (anim != null)
        {
            anim.SetTrigger("Die");
            Destroy(gameObject, 1f); // Удалить через 2 сек (после анимации)
        }
        else
        {
            Destroy(gameObject); // Если анимации нет - удаляем сразу
        }
    }
}