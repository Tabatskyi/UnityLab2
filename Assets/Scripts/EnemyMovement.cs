using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 1.8f;

    [Tooltip("Optional fallback distance if no collider is attached to the base.")]
    public float stopDistance = 0.0f;

    public string baseTag = "Base";

    [Header("Combat Settings")]
    public float damage = 10f;
    public float attackInterval = 1f;
    public bool destroyOnHit = false;

    private Transform targetBase;
    private Collider2D targetCollider;
    private BaseHealth targetBaseHealth;
    private Rigidbody2D enemyRigidbody;
    private Collider2D enemyCollider;
    private Coroutine attackCoroutine;
    private bool canMove = true;

    public bool CanMove => canMove;

    private void Awake()
    {
        enemyRigidbody = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        FindTargetBase();
    }

    private void OnDisable()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    private void FindTargetBase()
    {
        GameObject baseObject = GameObject.FindGameObjectWithTag(baseTag);
        if (baseObject != null)
        {
            targetBase = baseObject.transform;
            targetCollider = baseObject.GetComponent<Collider2D>() ?? baseObject.GetComponentInChildren<Collider2D>();
            targetBaseHealth = baseObject.GetComponent<BaseHealth>() ?? baseObject.GetComponentInChildren<BaseHealth>();
        }
        else
        {
            Debug.LogWarning($"Base not found on the scene! Check the '{baseTag}' tag.", this);
        }
    }

    private void FixedUpdate()
    {
        if (!canMove) return;

        if (targetBase == null)
        {
            FindTargetBase();
            if (targetBase == null) return;
        }

        if (targetCollider != null && enemyCollider != null && enemyCollider.IsTouching(targetCollider))
        {
            if (canMove)
            {
                Debug.Log("Enemy reached the base and stopped moving.");
                StopMoving();
            }
            StartAttackingBase(targetBaseHealth);
            return;
        }

        Vector2 targetPoint = new(targetBase.position.x, enemyRigidbody.position.y);

        if (targetCollider == null && stopDistance > 0f)
        {
            float distance = Vector2.Distance(enemyRigidbody.position, targetPoint);
            if (distance <= stopDistance)
            {
                Debug.Log("Enemy reached the base and stopped moving.");
                StopMoving();
                StartAttackingBase(targetBaseHealth);
                return;
            }
        }

        Vector2 nextPosition = Vector2.MoveTowards(
            enemyRigidbody.position,
            targetPoint,
            speed * Time.fixedDeltaTime
        );
        enemyRigidbody.MovePosition(nextPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckBaseCollision(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        CheckBaseCollision(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckBaseCollision(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckBaseCollision(collision.gameObject);
    }

    private void CheckBaseCollision(GameObject hitObject)
    {
        if (hitObject.CompareTag(baseTag) || hitObject.transform.root.CompareTag(baseTag) ||
            (targetBase != null && (hitObject.transform == targetBase || hitObject.transform.IsChildOf(targetBase))))
        {
            if (canMove)
            {
                Debug.Log("Enemy reached the base and stopped moving.");
                StopMoving();
            }

            BaseHealth baseHealth = hitObject.GetComponentInParent<BaseHealth>() ?? hitObject.GetComponentInChildren<BaseHealth>();
            StartAttackingBase(baseHealth);
        }
    }

    private void StartAttackingBase(BaseHealth baseHealth)
    {
        if (baseHealth == null)
        {
            baseHealth = targetBaseHealth;
        }

        if (baseHealth == null && targetBase != null)
        {
            baseHealth = targetBase.GetComponentInParent<BaseHealth>() ?? targetBase.GetComponentInChildren<BaseHealth>();
        }

        if (baseHealth == null) return;

        if (destroyOnHit)
        {
            baseHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        attackCoroutine ??= StartCoroutine(AttackRoutine(baseHealth));
    }

    private IEnumerator AttackRoutine(BaseHealth baseHealth)
    {
        while (baseHealth != null && baseHealth.CurrentHealth > 0f)
        {
            baseHealth.TakeDamage(damage);

            if (baseHealth.CurrentHealth <= 0f)
            {
                break;
            }

            yield return new WaitForSeconds(attackInterval);
        }

        attackCoroutine = null;
    }

    public void StopMoving()
    {
        canMove = false;
        if (enemyRigidbody != null)
        {
            enemyRigidbody.linearVelocity = Vector2.zero;
        }
    }

    public void ResumeMoving()
    {
        canMove = true;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }
}