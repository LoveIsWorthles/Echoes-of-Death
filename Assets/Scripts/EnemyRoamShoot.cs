using UnityEngine;

public class EnemyRoamShoot : MonoBehaviour
{
    public Transform player;

    public float roamSpeed = 2f;
    public float chaseSpeed = 3f;
    public float spotDistance = 12f;
    public float shootingDistance = 8f;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    public float fireRate = 1.5f;

    private Vector3 roamTarget;
    private float nextFireTime;

    void Start()
    {
        PickNewRoamTarget();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= spotDistance)
        {
            LookAtPlayer();

            if (distanceToPlayer > shootingDistance)
            {
                MoveTowardPlayer();
            }
            else
            {
                ShootAtPlayer();
            }
        }
        else
        {
            Roam();
        }
    }

    void Roam()
    {
        transform.position = Vector3.MoveTowards(transform.position, roamTarget, roamSpeed * Time.deltaTime);

        Vector3 direction = roamTarget - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (Vector3.Distance(transform.position, roamTarget) < 0.5f)
        {
            PickNewRoamTarget();
        }
    }

    void PickNewRoamTarget()
    {
        float randomX = Random.Range(-8f, 8f);
        float randomZ = Random.Range(-8f, 8f);

        roamTarget = new Vector3(randomX, transform.position.y, randomZ);
    }

    void MoveTowardPlayer()
    {
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, chaseSpeed * Time.deltaTime);
    }

    void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void ShootAtPlayer()
    {
        if (Time.time >= nextFireTime)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            Vector3 direction = (player.position - firePoint.position).normalized;
            rb.linearVelocity = direction * bulletSpeed;

            nextFireTime = Time.time + fireRate;
        }
    }
}