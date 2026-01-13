using UnityEngine;

public class ShopInput : MonoBehaviour
{
    public Spawner spawner;

    private int columns = 8;
    private int rows = 2;

    private int x = 0;
    private int y = 0;

    void Start()
    {
        // Сразу ставим курсор на 0
        if (spawner != null) spawner.SetSelection(0);
    }

    void Update()
    {
        // Только движение WASD
        if (Input.GetKeyDown(KeyCode.D)) Move(1, 0);
        if (Input.GetKeyDown(KeyCode.A)) Move(-1, 0);
        if (Input.GetKeyDown(KeyCode.W)) Move(0, -1);
        if (Input.GetKeyDown(KeyCode.S)) Move(0, 1);

        // Пробел удален. Он больше ничего не делает.
    }

    void Move(int dx, int dy)
    {
        x += dx;
        y += dy;

        if (x >= columns) x = 0;
        if (x < 0) x = columns - 1;

        if (y >= rows) y = 0;
        if (y < 0) y = rows - 1;

        int index = y * columns + x;

        // Просто двигаем рамку. Спавнер сам разберется.
        if (spawner != null) spawner.SetSelection(index);
    }
}