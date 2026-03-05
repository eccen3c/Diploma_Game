using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 25;

    [Header("Settings")]
    public bool hasExplosion = false; // <-- НОВАЯ ГАЛОЧКА!
    public float explosionDuration = 0.4f; // Время взрыва (если он есть)

    private Transform target;
    private string ownerTag;
    private Vector3 lastDirection;

    private bool hasHit = false;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        Destroy(gameObject, 5f);
        lastDirection = transform.right;
    }

    public void SetTarget(Transform newTarget, string tag)
    {
        target = newTarget;
        ownerTag = tag;
    }

    void Update()
    {
        if (hasHit) return;

        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            Vector3 dir = target.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            lastDirection = dir.normalized;
        }
        else
        {
            transform.position += lastDirection * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        // Убрали проверку на триггер, чтобы попадать в тело!
        if (collision.CompareTag(ownerTag)) return;

        UnitController unit = collision.GetComponent<UnitController>();
        BaseController baseCtrl = collision.GetComponent<BaseController>();

        if (unit != null || baseCtrl != null)
        {
            if (unit) unit.TakeDamage(damage);
            if (baseCtrl) baseCtrl.TakeDamage(damage);

            StartCoroutine(DestroyRoutine());
        }
    }

    IEnumerator DestroyRoutine()
    {
        hasHit = true;

        // Взрываемся ТОЛЬКО если стоит галочка И есть аниматор
        if (hasExplosion && anim != null)
        {
            anim.SetTrigger("explode"); // Запускаем анимацию
            yield return new WaitForSeconds(explosionDuration); // Ждем пока бахнет
        }

        // Если галочки нет — удаляем СРАЗУ (без задержки)
        Destroy(gameObject);
    }
}