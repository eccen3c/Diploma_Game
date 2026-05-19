using UnityEngine;

public class UnitController : MonoBehaviour
{
    [Header("��������� (����������� ����)")]
    public float hp;
    public float damage;
    public float speed;
    public float range;
    public float visionRange;
    public float attackSpeed;

    [Header("Ranged Settings (��� ��������/�����)")]
    public GameObject projectilePrefab; // <-- �����: ���� ����� ������ ������. ���� ����� = �������.

    [Header("Aiming System")]
    public Transform aimPoint;

    [Header("��������� �����")]
    public float avoidanceRadius = 0.5f;
    public float avoidanceForce = 2.0f;

    private float verticalAttackTreshold = 1f;
    private float myLaneOffset;

    private float lastAttackTime;
    private Transform currentTarget;
    private bool isDead = false;

    // ����������
    private Animator anim;
    private Rigidbody2D rb;

    // ��� ����?
    private string myTag;
    private string enemyTag;

    // ���� ����� �������� GameLoopManager ��� ������
    public void SetupUnit(UnitData data)
    {
        hp = data.hp;
        damage = data.damage;
        speed = data.moveSpeed;
        range = data.attackRange;
        visionRange = data.visionRange;
        attackSpeed = data.attackSpeed;
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        myTag = gameObject.tag;

        if (myTag == "Player")
            enemyTag = "Enemy";
        else
            enemyTag = "Player";

        myLaneOffset = Random.Range(-1.5f, 1.5f);
    }

    void Update()
    {
        if (isDead) return;

        FindTarget();

        if (currentTarget != null)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);
            float yDifference = Mathf.Abs(transform.position.y - currentTarget.position.y);

            // --- ����� ������ ����� ---

            // 1. �� �������? (���� �� � ��� ����)
            bool isRanged = (projectilePrefab != null);

            // 2. ����� �� �� �������?
            // �������: ���� ������ ���� � ������� (distance <= range)
            // � ���� � �����:
            // ���� �� ������� (��� ������� �� ������),
            // ���� �� ������� � ����� �� ����� ����� (yDifference <= �����)
            bool canHit = (distance <= range) && (isRanged || yDifference <= verticalAttackTreshold);

            if (canHit)
            {
                // ���� �������� ��� ����� -> ����� � ����
                StopMoving();
                if (Time.time >= lastAttackTime + attackSpeed)
                {
                    Attack();
                }
            }
            else
            {
                // ���� ������ ��� (���� �� ������) �� �� ����� -> ���� � ����
                MoveTo(currentTarget.position);
            }
        }
        else
        {
            // ������ ��� -> ���� ������ � ���� �����
            float dir = (myTag == "Player") ? 1f : -1f;
            Vector3 forwardPos = new Vector3(transform.position.x + (dir * 5f), transform.position.y, 0);
            MoveTo(forwardPos);
        }
    }

    void MoveTo(Vector3 target)
    {
        if (anim) anim.SetBool("isMoving", true);
        if (rb) rb.mass = 1f;

        Vector2 targetDir = (target - transform.position).normalized;

        Vector2 avoidDir = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, avoidanceRadius);

        foreach (var hit in neighbors)
        {
            if (hit.CompareTag(myTag) && hit.gameObject != gameObject)
            {
                if (hit.isTrigger) continue; // ���������� �������� ����� � ����

                Vector2 away = transform.position - hit.transform.position;
                float distance = away.magnitude;

                // ������ �� ������ �������-�-������� (����� �� ���� ������� �� ����)
                if (distance <= 0.001f)
                {
                    away = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                    distance = away.magnitude;
                }

                // 1. ��� ����� �����, ��� ������� �������������
                float pushStrength = Mathf.Clamp01(1f - (distance / avoidanceRadius));

                // 2. ������ "����" (�������������), ����� ������ ��������
                Vector2 sideSlip = new Vector2(-away.y, away.x).normalized * 0.5f;

                // 3. ��������� ������������ ����� � ���������� ����
                avoidDir += (away.normalized + sideSlip) * pushStrength;
            }
        }

        // ������ � ����� (����� ��� �� ������������ ������� ������/�����)
        float desiredY = 0f;
        Vector2 laneCorrection = Vector2.zero;
        if (Mathf.Abs(transform.position.y - desiredY) > 0.5f)
        {
            float personalTargetY = desiredY + myLaneOffset;
            float dirY = (personalTargetY - transform.position.y);
            laneCorrection = new Vector2(0, dirY).normalized * 0.5f;
        }

        // �������� ����������� = ������ + ��������� + ������ � �����
        Vector2 finalDir = (targetDir + avoidDir * avoidanceForce + laneCorrection).normalized;

        if (rb) rb.MovePosition(rb.position + finalDir * speed * Time.fixedDeltaTime);
    }

    void StopMoving()
    {
        if (anim) anim.SetBool("isMoving", false);
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.mass = 100f;
        }
    }

    void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRange);

        float closestUnitDist = Mathf.Infinity;
        float closestBaseDist = Mathf.Infinity;

        Transform targetUnit = null;
        Transform targetBase = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag(myTag)) continue;

            if (hit.CompareTag(enemyTag))
            {
                float d = Vector2.Distance(transform.position, hit.transform.position);
                if (d < closestUnitDist)
                {
                    closestUnitDist = d;
                    targetUnit = hit.transform;
                }
            }
            else if (hit.GetComponent<BaseController>())
            {
                float d = Vector2.Distance(transform.position, hit.transform.position);
                if (d < closestBaseDist)
                {
                    closestBaseDist = d;
                    targetBase = hit.transform;
                }
            }
        }

        if (targetUnit != null)
            currentTarget = targetUnit;
        else if (targetBase != null)
            currentTarget = targetBase;
        else
            currentTarget = null;
    }

    void Attack()
    {
        if (anim) anim.SetTrigger("attack");
        lastAttackTime = Time.time;
    }

    public void DealDamage()
    {
        if (currentTarget == null) return;

        if (AudioManager.Instance != null)
        {
            if (projectilePrefab != null)
                AudioManager.Instance.PlayFireball();
            else
                AudioManager.Instance.PlayHitSound();
        }

        // --- ������ AIM POINT (���� ��������?) ---
        Transform targetTransform = currentTarget; // �� ��������� - � ���� (root)

        // 1. ������� ����� AimPoint � �����
        UnitController enemyUnit = currentTarget.GetComponent<UnitController>();
        if (enemyUnit != null && enemyUnit.aimPoint != null)
        {
            targetTransform = enemyUnit.aimPoint;
        }
        // 2. ������� ����� AimPoint � ����
        else
        {
            BaseController enemyBase = currentTarget.GetComponent<BaseController>();
            if (enemyBase != null && enemyBase.aimPoint != null)
            {
                targetTransform = enemyBase.aimPoint;
            }
        }
        // ----------------------------------------


        if (projectilePrefab != null)
        {
            // �������� �� ������ AimPoint (���� �� ����), ����� �� ���
            Vector3 spawnPos = (aimPoint != null) ? aimPoint.position : transform.position;

            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            ProjectileController pc = proj.GetComponent<ProjectileController>();

            if (pc != null)
            {
                pc.damage = (int)this.damage;
                // �������� ���������� ����� (AimPoint ��� ����)
                pc.SetTarget(targetTransform, this.tag);
            }
        }
        else // ������� ���
        {
            if (enemyUnit) enemyUnit.TakeDamage(damage);
            BaseController baseCtrl = currentTarget.GetComponent<BaseController>();
            if (baseCtrl) baseCtrl.TakeDamage((int)damage);
        }
    }
    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        if (hp <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // --- �����������: ��������� ��� ���������� (� ����, � �����) ---
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        // ---------------------------------------------------------------

        if (rb) rb.simulated = false;

        if (anim) anim.SetTrigger("die");

        Destroy(gameObject, 0.65f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}