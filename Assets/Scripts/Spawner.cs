using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public enum SpawnMode
    {
        WeightedRandom,
        Sequential
    }

    [System.Serializable]
    public class EnemyTypeConfig
    {
        public string typeName = "Basic";
        public Color color = Color.green;
        public float speed = 1.8f;
        public float maxHealth = 30f;
        [Tooltip("Relative probability weight (e.g. 60 for Basic, 30 for Fast, 10 for Tank)")]
        [Min(0f)] public float spawnWeight = 50f;
        public GameObject prefabOverride;
    }

    [Header("Enemy Prefab")]
    [Tooltip("Default enemy prefab used if no specific prefab override is provided.")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Selection Mode")]
    [Tooltip("WeightedRandom selects enemies based on weights; Sequential cycles in order.")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.WeightedRandom;

    [Header("Enemy Types Configuration & Weights")]
    [SerializeField] private EnemyTypeConfig[] enemyTypes = new EnemyTypeConfig[]
    {
        new EnemyTypeConfig { typeName = "Basic", color = new Color(0.057f, 1f, 0f, 1f), speed = 1.8f, maxHealth = 30f, spawnWeight = 60f },
        new EnemyTypeConfig { typeName = "Fast", color = Color.yellow, speed = 3.2f, maxHealth = 15f, spawnWeight = 30f },
        new EnemyTypeConfig { typeName = "Tank", color = Color.black, speed = 0.9f, maxHealth = 80f, spawnWeight = 10f }
    };

    [Header("Spawn Timing (Cooldown)")]
    [Tooltip("Time before the first spawn in seconds.")]
    [SerializeField] private float initialDelay = 1.0f;

    [Tooltip("Cooldown interval between enemy spawns in seconds.")]
    [SerializeField] private float cooldown = 2.5f;

    private int currentTypeIndex = 0;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    private BaseHealth baseHealth;

    public SpawnMode Mode
    {
        get => spawnMode;
        set => spawnMode = value;
    }

    public float Cooldown
    {
        get => cooldown;
        set => cooldown = value;
    }

    public bool IsSpawning => isSpawning;
    public int CurrentTypeIndex => currentTypeIndex;

    private void Awake()
    {
        EnsureEnemyPrefabAssigned();
        EnsureEnemyTypesConfigured();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EnsureEnemyPrefabAssigned();
        EnsureEnemyTypesConfigured();
    }
#endif

    private void Start()
    {
        EnsureEnemyPrefabAssigned();
        EnsureEnemyTypesConfigured();

        GameObject baseObject = GameObject.FindGameObjectWithTag("Base");
        if (baseObject != null)
        {
            baseHealth = baseObject.GetComponent<BaseHealth>() ?? baseObject.GetComponentInChildren<BaseHealth>();
            if (baseHealth != null)
            {
                baseHealth.OnDestroyed += StopSpawning;
            }
        }

        StartSpawning();
    }

    private void OnDestroy()
    {
        if (baseHealth != null)
        {
            baseHealth.OnDestroyed -= StopSpawning;
        }
    }

    public void EnsureEnemyPrefabAssigned()
    {
        if (enemyPrefab != null) return;

#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("Enemy t:Prefab");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            GameObject asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset != null && asset.GetComponent<EnemyMovement>() != null)
            {
                enemyPrefab = asset;
                if (!Application.isPlaying)
                {
                    UnityEditor.EditorUtility.SetDirty(this);
                }
                return;
            }
        }
#endif

        // Runtime fallback: find an existing enemy in the scene to clone as template
        GameObject sceneEnemy = GameObject.FindGameObjectWithTag("Enemy");
        if (sceneEnemy != null)
        {
            enemyPrefab = sceneEnemy;
            return;
        }

#if UNITY_2023_1_OR_NEWER
        EnemyMovement movement = FindFirstObjectByType<EnemyMovement>();
#else
        EnemyMovement movement = FindObjectOfType<EnemyMovement>();
#endif
        if (movement != null)
        {
            enemyPrefab = movement.gameObject;
        }
    }

    public void EnsureEnemyTypesConfigured()
    {
        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            enemyTypes = GetDefaultEnemyTypes();
        }
    }

    public void StartSpawning()
    {
        if (isSpawning) return;

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        if (initialDelay > 0f)
        {
            yield return new WaitForSeconds(initialDelay);
        }

        while (isSpawning)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(cooldown);
        }
    }

    public GameObject SpawnEnemy()
    {
        EnsureEnemyPrefabAssigned();
        EnsureEnemyTypesConfigured();

        EnemyTypeConfig config = GetNextEnemyConfig();

        GameObject prefab = (config != null && config.prefabOverride != null) ? config.prefabOverride : enemyPrefab;
        if (prefab == null)
        {
            Debug.LogWarning("Spawner: No enemy prefab assigned to spawn! Please assign Enemy.prefab in the Spawner Inspector.", this);
            return null;
        }

        GameObject enemyInstance = Instantiate(prefab, transform.position, Quaternion.identity);

        // Apply color coding: basic (green), fast (yellow), tank (black)
        SpriteRenderer sr = enemyInstance.GetComponent<SpriteRenderer>() ?? enemyInstance.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = config.color;
        }

        // Apply movement speed
        EnemyMovement movement = enemyInstance.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.speed = config.speed;
        }

        // Apply health
        EnemyHealth health = enemyInstance.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.SetMaxHealth(config.maxHealth);
        }

        return enemyInstance;
    }

    private EnemyTypeConfig GetNextEnemyConfig()
    {
        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            enemyTypes = GetDefaultEnemyTypes();
        }

        if (spawnMode == SpawnMode.Sequential)
        {
            EnemyTypeConfig seqConfig = enemyTypes[currentTypeIndex];
            currentTypeIndex = (currentTypeIndex + 1) % enemyTypes.Length;
            return seqConfig;
        }

        // Weighted Random Selection
        float totalWeight = 0f;
        for (int i = 0; i < enemyTypes.Length; i++)
        {
            if (enemyTypes[i] != null && enemyTypes[i].spawnWeight > 0f)
            {
                totalWeight += enemyTypes[i].spawnWeight;
            }
        }

        if (totalWeight <= 0f)
        {
            return enemyTypes[Random.Range(0, enemyTypes.Length)];
        }

        float randomRoll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < enemyTypes.Length; i++)
        {
            if (enemyTypes[i] != null && enemyTypes[i].spawnWeight > 0f)
            {
                cumulative += enemyTypes[i].spawnWeight;
                if (randomRoll <= cumulative)
                {
                    return enemyTypes[i];
                }
            }
        }

        return enemyTypes[enemyTypes.Length - 1];
    }

    private EnemyTypeConfig[] GetDefaultEnemyTypes()
    {
        return new EnemyTypeConfig[]
        {
            new EnemyTypeConfig { typeName = "Basic", color = new Color(0.057f, 1f, 0f, 1f), speed = 1.8f, maxHealth = 30f, spawnWeight = 60f },
            new EnemyTypeConfig { typeName = "Fast", color = Color.yellow, speed = 3.2f, maxHealth = 15f, spawnWeight = 30f },
            new EnemyTypeConfig { typeName = "Tank", color = Color.black, speed = 0.9f, maxHealth = 80f, spawnWeight = 10f }
        };
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
