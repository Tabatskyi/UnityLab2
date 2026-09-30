using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProjectileMovement : MonoBehaviour
{
    [Header("Projectile Stats")]
    public float speed = 15f;
    public float damage = 10f;
    public float lifeTime = 3f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        if (rb != null)
        {
            rb.linearVelocity = transform.right * speed;
        }
    }

    private void Update()
    {
        if (rb == null || rb.bodyType == RigidbodyType2D.Kinematic)
        {
            transform.position += transform.right * (speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Base") || collision.CompareTag("Projectile"))
        {
            return;
        }

        EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>() ?? collision.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Base") || collision.gameObject.CompareTag("Projectile"))
        {
            return;
        }

        EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>() ?? collision.gameObject.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}