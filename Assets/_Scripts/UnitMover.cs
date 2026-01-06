using UnityEngine;

public class UnitMover : MonoBehaviour
{
    public float speed = 2.0f;
    public int direction = 1;

    private float defaultSpeed; // 1. Сюда запомним скорость

    void Start()
    {
        defaultSpeed = speed; // 2. Запоминаем скорость при старте
    }

    void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Логика остановки (та же, что и была)
        if ((gameObject.tag == "Ally" && collision.gameObject.tag == "Enemy") ||
            (gameObject.tag == "Enemy" && collision.gameObject.tag == "Ally"))
        {
            speed = 0;
        }
    }

    // 3. НОВАЯ ЧАСТЬ: Срабатывает, когда враг умирает или отходит
    void OnCollisionExit2D(Collision2D collision)
    {
        // Если мы перестали касаться врага (потому что он умер)
        if ((gameObject.tag == "Ally" && collision.gameObject.tag == "Enemy") ||
            (gameObject.tag == "Enemy" && collision.gameObject.tag == "Ally"))
        {
            speed = defaultSpeed; // Восстанавливаем скорость!
        }
    }
}