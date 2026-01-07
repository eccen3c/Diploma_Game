using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed = 20f;
    public float limitX = 25f; // Граница влево-вправо
    public float limitY = 10f; // Граница вверх-вниз (НОВОЕ)

    void Update()
    {
        float moveX = 0;
        float moveY = 0;

        // Влево-Вправо
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveX = -1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveX = 1;

        // Вверх-Вниз (НОВОЕ)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveY = 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveY = -1;

        // Двигаем
        Vector3 moveDir = new Vector3(moveX, moveY, 0);
        transform.Translate(moveDir * speed * Time.deltaTime);

        // Ограничиваем (Clamp) и X, и Y
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
        pos.y = Mathf.Clamp(pos.y, -limitY, limitY); // Ограничение высоты
        transform.position = pos;
    }
}