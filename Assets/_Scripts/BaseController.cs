using UnityEngine;
using TMPro; // Для текста HP над базой

public class BaseController : MonoBehaviour
{
    public int maxHealth = 500;
    private int currentHealth;

    [Header("UI")]
    public TextMeshPro textHP; // Ссылка на текст с жизнями
    
    [Header("Настройки")]
    public bool isPlayerBase; // <-- Галочка: это база игрока?

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        if (textHP != null)
        {
            textHP.text = currentHealth.ToString();
        }
    }

    void Die()
    {
        // Если умерла база игрока -> Игрок проиграл (playerWon = false)
        // Если умер враг -> Игрок выиграл (playerWon = true)

        // !isPlayerBase означает "НЕ база игрока", то есть вражеская
        GameManager.instance.EndGame(!isPlayerBase);

        Destroy(gameObject);
    }
}