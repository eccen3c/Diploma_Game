using UnityEngine;
using System.Collections;
using Photon.Pun;

public class ProjectileController : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 25;

    [Header("Settings")]
    public bool hasExplosion = false; // <-- ����� �������!
    public float explosionDuration = 0.4f; // ����� ������ (���� �� ����)

    private Transform target;
    private Vector3 targetOffset;
    private string ownerTag;
    private Vector3 lastDirection;

    private bool hasHit = false;
    private Animator anim;
    [HideInInspector] public bool isCosmetic = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        Destroy(gameObject, 5f);
        lastDirection = transform.right;
    }

    public void SetTarget(Transform newTarget, string tag, Vector3 offset = default)
    {
        target = newTarget;
        ownerTag = tag;
        targetOffset = offset;
    }

    public void SetFallbackTarget(Vector3 pos)
    {
        target = null;
        lastDirection = (pos - transform.position).normalized;
    }

    void Update()
    {
        if (hasHit) return;

        if (target != null)
        {
            Vector3 aimPos = target.position + targetOffset;
            transform.position = Vector3.MoveTowards(transform.position, aimPos, speed * Time.deltaTime);

            Vector3 dir = aimPos - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            lastDirection = dir.normalized;

            if (isCosmetic && Vector3.Distance(transform.position, aimPos) < 0.25f)
            {
                StartCoroutine(DestroyRoutine());
                return;
            }
        }
        else
        {
            transform.position += lastDirection * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;
        if (collision.CompareTag(ownerTag)) return;

        UnitController unit = collision.GetComponent<UnitController>();
        TestUnitAI unitAI = collision.GetComponent<TestUnitAI>();
        TestArcherAI archerAI = collision.GetComponent<TestArcherAI>();
        BaseController baseCtrl = collision.GetComponent<BaseController>();

        if (unit != null || unitAI != null || archerAI != null || baseCtrl != null)
        {
            if (!isCosmetic)
            {
                if (unit) unit.TakeDamage(damage);
                if (unitAI) unitAI.TakeDamage(damage);
                if (archerAI) archerAI.TakeDamage(damage);
                if (baseCtrl) baseCtrl.TakeDamage(damage);
            }
            StartCoroutine(DestroyRoutine());
        }
    }

    IEnumerator DestroyRoutine()
    {
        hasHit = true;

        // ���������� ������ ���� ����� ������� � ���� ��������
        if (hasExplosion && anim != null)
        {
            anim.SetTrigger("explode"); // ��������� ��������
            yield return new WaitForSeconds(explosionDuration); // ���� ���� ������
        }

        // ���� ������� ��� � ������� ����� (��� ��������)
        Destroy(gameObject);
    }
}