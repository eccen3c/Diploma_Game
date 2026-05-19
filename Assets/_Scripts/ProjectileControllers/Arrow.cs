using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;
    public float lifetime = 3f;
    public string targetTag;

    private Vector2 direction;
    private bool hasHit = false;

    public void Init(Vector2 dir, float dmg, string tag)
    {
        direction = dir.normalized;
        damage = dmg;
        targetTag = tag;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (hasHit) return;

        float step = speed * Time.deltaTime;
        Vector2 origin = transform.position;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, step);
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;
            if (TryHit(hit.collider)) return;
        }

        transform.Translate(Vector2.right * step);
    }

    bool TryHit(Collider2D other)
    {
        BaseController baseCtrl = other.GetComponent<BaseController>();
        if (baseCtrl != null && other.CompareTag(targetTag))
        {
            baseCtrl.TakeDamage((int)damage);
            Destroy(gameObject);
            hasHit = true;
            return true;
        }

        if (!other.CompareTag(targetTag)) return false;
        if (!other.isTrigger) return false;

        TestArcherAI archer = other.GetComponent<TestArcherAI>();
        if (archer != null) { archer.TakeDamage(damage); Destroy(gameObject); hasHit = true; return true; }

        TestUnitAI unit = other.GetComponent<TestUnitAI>();
        if (unit != null) { unit.TakeDamage(damage); Destroy(gameObject); hasHit = true; return true; }

        UnitController ctrl = other.GetComponent<UnitController>();
        if (ctrl != null) { ctrl.TakeDamage(damage); Destroy(gameObject); hasHit = true; return true; }

        return false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        TryHit(other);
    }
}
