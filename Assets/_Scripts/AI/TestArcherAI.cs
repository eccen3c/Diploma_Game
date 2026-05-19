using System.Collections;
using UnityEngine;

public class TestArcherAI : MonoBehaviour
{
    [Header("Stats")]
    public float hp = 100f;
    public float damage = 10f;
    public float speed = 2f;
    public float attackRange = 5f;
    public float minRange = 2f;
    public float visionRange = 10f;
    public float attackCooldown = 1.5f;
    public float arrowDelay = 0.3f;

    [Header("Arrow")]
    public GameObject arrowPrefab;
    public Vector2 arrowSpawnOffset = new Vector2(0.5f, 0.3f);
    public bool isFireball = false;

    [Header("Body")]
    public Vector2 bodyOffset = new Vector2(0f, 0.5f);

    [Header("Spreading")]
    public float laneY = 0f;
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

    public void SetupUnit(UnitData data)
    {
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

        if (!isPlayer)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        lastAttackTime = -Random.Range(0f, attackCooldown);
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
            Vector2 targetCenter = (Vector2)target.position + GetTargetBodyOffset();
            float dist = Vector2.Distance(myCenter, targetCenter);

            if (dist <= attackRange)
            {
                StopMoving();
                TryAttack();
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

    bool AnyEnemyWithinMinRange(Vector2 myCenter)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(myCenter, minRange);
        foreach (var hit in hits)
        {
            if (hit.isTrigger) continue;
            if (hit.CompareTag(enemyTag)) return true;
        }
        return false;
    }

    Vector2 GetTargetBodyOffset()
    {
        TestUnitAI unit = target.GetComponent<TestUnitAI>();
        if (unit != null) return unit.bodyOffset;
        TestArcherAI archer = target.GetComponent<TestArcherAI>();
        if (archer != null) return archer.bodyOffset;
        return Vector2.zero;
    }

    void FindTarget()
    {
        if (target != null && target.gameObject.activeInHierarchy) return;
        target = null;

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

            Vector2 hitCenter = (Vector2)hit.transform.position;
            float dist = Vector2.Distance(center, hitCenter);

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
        Vector2 dir = ((Vector2)destination - rb.position).normalized;
        dir = ApplySeparation(dir);
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
    }

    void MoveForward()
    {
        PlayAnim("Walk");
        float forwardX = isPlayer ? 1f : -1f;
        float yDiff = laneY - transform.position.y;
        Vector2 dir = new Vector2(forwardX, Mathf.Clamp(yDiff, -1f, 1f)).normalized;
        dir = ApplySeparation(dir);
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
    }

    void Retreat()
    {
        PlayAnim("Walk");
        float backX = isPlayer ? -1f : 1f;
        Vector2 dir = new Vector2(backX, 0f);
        dir = ApplySeparation(dir);
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
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
        StartCoroutine(ShootArrowDelayed());
    }

    IEnumerator ShootArrowDelayed()
    {
        yield return new WaitForSeconds(arrowDelay);
        if (target == null || arrowPrefab == null) yield break;

        if (isFireball) AudioManager.Instance?.PlayFireball();
        else AudioManager.Instance?.PlayArrow();

        float spawnX = isPlayer ? arrowSpawnOffset.x : -arrowSpawnOffset.x;
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(spawnX, arrowSpawnOffset.y);
        GameObject arrowGO = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

        Arrow arrow = arrowGO.GetComponent<Arrow>();
        if (arrow != null)
        {
            Vector2 dir = ((Vector2)target.position + GetTargetBodyOffset()) - spawnPos;
            arrow.damage = damage;
            arrow.targetTag = enemyTag;
            arrow.Init(dir, damage, enemyTag);
        }
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, minRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, visionRange);
    }
}
