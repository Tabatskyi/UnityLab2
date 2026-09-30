using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class WeaponAim : MonoBehaviour
{
    [Header("Aim Settings")]
    [Tooltip("Обмеження кута стрільби вгору та вниз (у градусах)")]
    [SerializeField] private float minAngle = -75f;
    [SerializeField] private float maxAngle = 75f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.35f; 

    private Camera mainCamera;
    private float nextFireTime;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleAim();
        HandleShooting();
    }

    private void HandleAim()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null) return;
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
#else
        Vector3 mouseScreenPosition = Input.mousePosition;
#endif
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;

        Vector2 aimDirection = (mouseWorldPosition - transform.position).normalized;

        float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        float clampedAngle = Mathf.Clamp(targetAngle, minAngle, maxAngle);

        transform.rotation = Quaternion.Euler(0f, 0f, clampedAngle);
    }

    private void HandleShooting()
    {
#if ENABLE_INPUT_SYSTEM
        bool isShooting = Mouse.current != null && Mouse.current.leftButton.isPressed;
#else
        bool isShooting = Input.GetMouseButton(0);
#endif

        if (isShooting && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;

        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}