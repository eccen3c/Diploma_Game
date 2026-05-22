using System.Collections;
using UnityEngine;

public class TestUnitAI : MonoBehaviour
{
    [Header("Stats")]
    public float hp = 100f;
    public float damage = 10f;
    public float speed = 2f;
    public float attackRange = 1.2f;
    public float visionRange = 8f;
    public float attackCooldown = 1f;
    public float hitDelay = 0.3f;

    [Header("Body")]
    public Vector2 bodyOffset = new Vector2(0f, 0.5f);

    [Header("Spreading")]
    public float laneY = 0f;          // задається ззовні при спавні
    public float separationRadius = 1.5f;
    public float separationForce = 3f;

    private string enemyTag;
    private Transform target;
    private Rigidbody2D rb;
    private Animator anim;
    private float lastAttackTime;
    private float findTargetTimer;
    private const float FIND_INTERVAL = 0.2f;
    private bool isDead;
    private bool isPlayer;
    private string currentAnim;
    private float avoidSide = 0f;
    private float avoidResetTimer = 0f;
    private const float AVOID_HOLD_TIME = 1f;

    [HideInInspector] public UnitData unitData;

    public void SetupUnit(UnitData data)
    {
        unitData = data;
        hp = data.hp;
        damage = data.damage;
        speed = data.moveSpeed;
        attackRange = data.attackRange;
        visionRange = data.visionRange;
        attackCooldown = data.attackSpeed;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        isPlayer = CompareTag("Player");
        enemyTag = isPlayer ? "Enemy" : "Player";
        lastAttackTime = -Random.Range(0f, attackCooldown);

        if (!isPlayer)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void Update()
    {
        if (isDead) return;

        findTargetTimer -= Time.deltaTime;
        if (findTargetTimer <= 0f)
        {
            FindTarget();
            findTargetTimer = FIND_INTERVAL;
        }

        if (currentAnim == "Attack01" && anim != null)
        {
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("Attack01") && info.normalizedTime >= 1f)
                currentAnim = "";
        }

        if (target != null)
        {
            Vector2 myCenter = (Vector2)transform.position + bodyOffset;
            Vector2 targetCenter = (Vector2)target.position + (target.GetComponent<TestUnitAI>()?.bodyOffset ?? Vector2.zero);
            float dist = Vector2.Distance(myCenter, targetCenter);
            float yDiff = Mathf.Abs(myCenter.y - targetCenter.y);

            if (dist <= attackRange)
            {
                if (yDiff > 0.25f)
                {
                    StartMoving();
                    AlignY(targetCenter.y);
                }
                else
                {
                    StopMoving();
                    TryAttack();
                }
            }
            else
            {
                StartMoving();
                MoveToward(target.position);
            }
        }
        else
        {
            StartMoving();
            MoveForward();
        }
    }

    void FindTarget()
    {
        if (target != null)
        {
            // перевір чи ціль ще жива
            if (target.gameObject == null || !target.gameObject.activeInHierarchy)
                target = null;
        }

        Vector2 center = (Vector2)transform.position + bodyOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, visionRange);
        float bestScore = Mathf.Infinity;
        Transform best = null;
        Transform bestBase = null;
        float bestBaseDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.GetComponent<BaseController>() != null)
            {
                if (hit.CompareTag(tag)) continue; // своя база — пропустить
                float d = Vector2.Distance(center, hit.transform.position);
                if (d < bestBaseDist) { bestBaseDist = d; bestBase = hit.transform; }
                continue;
            }

            if (!hit.CompareTag(enemyTag)) continue;
            if (hit.isTrigger) continue;

            Vector2 hitCenter = (Vector2)hit.transform.position + (hit.GetComponent<TestUnitAI>()?.bodyOffset ?? Vector2.zero);
            float dist = Vector2.Distance(center, hitCenter);

            // штраф за каждого союзника который стоит на пути
            float penalty = 0f;
            Vector2 dir = (hitCenter - center).normalized;
            RaycastHit2D[] blocking = Physics2D.RaycastAll(center, dir, dist);
            foreach (var r in blocking)
            {
                if (r.collider.gameObject == gameObject) continue;
                if (r.collider.CompareTag(tag) && !r.collider.isTrigger)
                    penalty += 4f;
            }

            float score = dist + penalty;
            if (score < bestScore)
            {
                bestScore = score;
                best = hit.transform;
            }
        }

        target = best ?? bestBase;
    }

    void MoveToward(Vector3 destination)
    {
        PlayAnim("Walk");

        float forwardX = isPlayer ? 1f : -1f;
        Vector2 dir = ((Vector2)destination - rb.position).normalized;
        if (IsBlockedAhead(new Vector2(forwardX, 0f)))
        {
            if (avoidSide == 0f) avoidSide = (Random.value > 0.5f ? 1f : -1f);
            avoidResetTimer = AVOID_HOLD_TIME;
            dir = new Vector2(forwardX * 0.4f, avoidSide).normalized;
        }
        else
        {
            avoidResetTimer -= Time.deltaTime;
            if (avoidResetTimer <= 0f) avoidSide = 0f;
        }
        dir = ApplySeparation(dir);
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
    }

    void MoveForward()
    {
        PlayAnim("Walk");

        float forwardX = isPlayer ? 1f : -1f;
        float yDiff = laneY - transform.position.y;
        Vector2 dir = new Vector2(forwardX, Mathf.Clamp(yDiff, -1f, 1f)).normalized;
        if (IsBlockedAhead(new Vector2(forwardX, 0f)))
        {
            if (avoidSide == 0f) avoidSide = (Random.value > 0.5f ? 1f : -1f);
            avoidResetTimer = AVOID_HOLD_TIME;
            dir = new Vector2(forwardX * 0.4f, avoidSide).normalized;
        }
        else
        {
            avoidResetTimer -= Time.deltaTime;
            if (avoidResetTimer <= 0f) avoidSide = 0f;
        }
        dir = ApplySeparation(dir);
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
    }

    void AlignY(float targetY)
    {
        PlayAnim("Walk");
        float yDiff = targetY - ((Vector2)transform.position + bodyOffset).y;
        Vector2 dir = new Vector2(0f, Mathf.Sign(yDiff));
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
    }

    bool IsBlockedAhead(Vector2 forwardDir)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll((Vector2)transform.position + bodyOffset, forwardDir, separationRadius * 2f);
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;
            if (!hit.collider.CompareTag(tag)) continue;
            if (hit.collider.isTrigger) continue;
            Rigidbody2D nrb = hit.collider.GetComponent<Rigidbody2D>();
            if (nrb != null && nrb.velocity.sqrMagnitude < 0.05f)
                return true;
        }
        return false;
    }

    Vector2 ApplySeparation(Vector2 currentDir)
    {
        Collider2D[] neighbors = Physics2D.OverlapCircleAll((Vector2)transform.position + bodyOffset, separationRadius);
        Vector2 sep = Vector2.zero;

        foreach (var n in neighbors)
        {
            if (n.gameObject == gameObject) continue;
            if (!n.CompareTag(tag)) continue;
            if (n.isTrigger) continue;

            Vector2 away = (Vector2)transform.position - (Vector2)n.transform.position;
            float dist = away.magnitude;
            if (dist < 0.01f) away = Random.insideUnitCircle.normalized;
            sep += away.normalized * (1f - dist / separationRadius);
        }

        Vector2 perp = new Vector2(-currentDir.y, currentDir.x);
        float side = Mathf.Clamp(Vector2.Dot(sep * separationForce, perp), -1f, 1f);
        return (currentDir + perp * side).normalized;
    }

    void StopMoving()
    {
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        if (currentAnim != "Attack01") PlayAnim("Idle");
    }

    void StartMoving()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        lastAttackTime = Time.time;

        currentAnim = "";
        PlayAnim("Attack01");
        StartCoroutine(DealDamageDelayed());
    }

    IEnumerator DealDamageDelayed()
    {
        yield return new WaitForSeconds(hitDelay);
        if (target == null) yield break;

        AudioManager.Instance?.PlayHitSound();

        TestUnitAI enemyUnit = target.GetComponent<TestUnitAI>();
        if (enemyUnit != null) { enemyUnit.TakeDamage(damage); yield break; }

        TestArcherAI enemyArcher = target.GetComponent<TestArcherAI>();
        if (enemyArcher != null) { enemyArcher.TakeDamage(damage); yield break; }

        BaseController enemyBase = target.GetComponent<BaseController>();
        if (enemyBase != null) enemyBase.TakeDamage((int)damage);
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;
        hp -= dmg;
        if (hp <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        PlayAnim("Death");
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;
        Destroy(gameObject, 1f);
    }

    void PlayAnim(string name)
    {
        if (anim == null || currentAnim == name) return;
        currentAnim = name;
        anim.Play(name);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position + (Vector3)bodyOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, visionRange);
    }
}
