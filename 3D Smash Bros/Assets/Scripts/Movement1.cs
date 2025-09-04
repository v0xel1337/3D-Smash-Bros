using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class Movement1 : NetworkBehaviour
{
    [SerializeField]
    private float defaultSpeed = 5.0f;
    private float currentSpeed = 5.0f;

    [SerializeField] private float jumpForce = 7;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 1.1f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded = false;
    Camera _camera;
    [SerializeField] private Transform cameraTransform; // A főkamera Transformja

    public Image cooldownImage; // ide húzod be az UI Image-t
    public Image QcooldownImage;
    public Image EcooldownImage;
    public Image cooldownGreenImage;
    public GameObject CylinderObject;
    public LayerMask terrainLayer; // Csak Terrain-hez

    private Queue<GameObject> spawnedObjects = new Queue<GameObject>();

    [SerializeField] private float dashForce = 50f;
    [SerializeField] private float dashDuration = 0.2f;
    private bool isDashing = false;
    private float lastDashTime = -999f;
    private float qCooldownTimer = 0f;
    public float qCooldownDelay = 5f;


    private float eCooldownTimer = 0f;
    public float eCooldownDelay = 5f;
    public LineRenderer lineRenderer;  // a lézersugár vizuálja
    public float maxDistance = 50f;
    public float damagePerSecond = 20f;


    public GameObject projectilePrefab; // A lövedék prefab (pl. egy kis gömb rigidbody-val)
    public Transform firePoint; // Innen lövi ki (pl. a kamera vagy fegyver eleje)
    public float shootForce = 20f;
    private Rigidbody lastProjectileRb;

    private IEnumerator WaitForGameUI()
    {
        // Várd meg, amíg a GameUI.Instance nem null
        while (GameUI.Instance == null)
        {
            yield return null;
        }

        cooldownImage = GameUI.Instance.clickCooldownImage;
        QcooldownImage = GameUI.Instance.QCooldownImage;
        EcooldownImage = GameUI.Instance.ECooldownImage;
        cooldownGreenImage = GameUI.Instance.clickGreenCooldownImage;
        pc.ui = GameUI.Instance.healthUI;

        if (pc.ui != null)
            pc.ui.SetPlayerHealth(pc);
    }

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        pc = GetComponent<PlayerCombat>();

        _camera = GetComponentInChildren<Camera>();
        cameraTransform = _camera.transform;

        if (IsServer)
        {
            GameManager.Instance.RegisterPlayer(OwnerClientId);
        }

        if (!IsOwner)
        {
            _camera.enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(WaitForGameUI());
    }

    private void Start()
    {
        qCooldownTimer = qCooldownDelay;
        eCooldownTimer = eCooldownDelay;
    }


    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (!pc.animator.GetBool("inSubStateMachine"))
        {
            pc.playersInside.Clear();
        }
        
        if (pc.isStunned)
        {
            if (Time.time >= pc.stunEndTime)
            {
                pc.isStunned = false;
                // Esetleg animáció visszaállítása itt
            }
            else
            {
                return; // ne csináljon semmit, ha stunban van
            }
        }

        AnimatorStateInfo stateInfo = pc.animator.GetCurrentAnimatorStateInfo(0); // 0 = base layer

        if (rb.linearVelocity.magnitude < 2f){
			rb.linearVelocity = Vector3.zero;
		}
		
		if(Input.GetKeyDown(KeyCode.Escape)){
			Application.Quit();	
		}
		

        // Capture movement input relative to current orientation
        moveInput = Vector3.zero;



        if (pc.animator.GetBool("inSubStateMachine") && !stateInfo.IsName("Roll"))
        {
            moveInput = Vector3.zero;
        }
        else
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            // Csak vízszintes komponensek
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            moveInput = Vector3.zero;

            if (Input.GetKey(KeyCode.W)) moveInput += forward;
            if (Input.GetKey(KeyCode.S)) moveInput -= forward;
            if (Input.GetKey(KeyCode.D)) moveInput += right;
            if (Input.GetKey(KeyCode.A)) moveInput -= right;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = 10.0f;
        }
        else
        {
            currentSpeed = defaultSpeed;
        }


        // ?? Animáció vezérlése
        bool isWalking = moveInput.magnitude > 0.1f;
        bool isRunning = isWalking && Input.GetKey(KeyCode.LeftShift);

        pc.animator.SetBool("IsWalking", isWalking);
        pc.animator.SetBool("IsRunning", isRunning);

        // Grounded check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);

        // Jump
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // reset vertical velocity
            rb.AddForce(Vector3.up * jumpForce * 10, ForceMode.Impulse);
        } else if (!isGrounded){
		   	rb.AddForce(Vector3.up * -0.1f * jumpForce, ForceMode.Impulse);
		}
		
        moveInput = moveInput.normalized;

        if (moveInput.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }


        if (Input.GetKeyDown(KeyCode.V))
        {
            pc.animator.SetTrigger("CylinderLodged");
        }

        if (Input.GetMouseButtonDown(0))
        {
            FaceCameraDirection();
            pc.animator.SetTrigger("Shoot");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            EnableGravityOnLastProjectile();
        }

        if (qCooldownTimer < qCooldownDelay)
        {
            qCooldownTimer += Time.deltaTime;
            QcooldownImage.fillAmount = qCooldownTimer / qCooldownDelay;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                DashForward();
                qCooldownTimer = 0;
            }
        }

        if (eCooldownTimer < eCooldownDelay)
        {
            eCooldownTimer += Time.deltaTime;
            EcooldownImage.fillAmount = eCooldownTimer / eCooldownDelay;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                FaceCameraDirection();
                pc.animator.SetTrigger("ShootE");
                eCooldownTimer = 0;
            }
        }
    }


    public void AttackE()
    {
        // képernyő közepéből induló ray
        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, terrainLayer))
        {
            // ha ütközött valamihez, akkor a találati pont
            targetPoint = hit.point;
        }
        else
        {
            // ha nincs ütközés, akkor a ray maxDistance távolságán lévő pont
            targetPoint = ray.origin + ray.direction * maxDistance;
        }

        // LineRenderer kezdőpont = fegyver vége, végpont = targetPoint
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, targetPoint);
    }

    public void DashForward()
    {
        if (!IsOwner || isDashing)
            return;
        pc.animator.SetTrigger("Dash");

        StartCoroutine(DashCoroutine());
        
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        Vector3 dashDirection = cameraTransform.forward;
        dashDirection.Normalize();

        if (dashDirection.y < 0f)
            dashDirection.y = 0f;

        float startTime = Time.time;

        rb.useGravity = false;

        // ide tesszük: mely ellenfelek kapták már meg a hitet
        HashSet<PlayerCombat> hitEnemies = new HashSet<PlayerCombat>();

        while (Time.time < startTime + dashDuration)
        {
            FaceCameraDirection();

            foreach (PlayerCombat enemy in pc.playersInside)
            {
                if (enemy != null && !hitEnemies.Contains(enemy))
                {
                    enemy.PlayGetHitAnimationServerRpc(10, 12, transform.position, OwnerClientId);
                    hitEnemies.Add(enemy); // most már nem fogja többször meghívni
                }
            }

            rb.linearVelocity = dashDirection * dashForce;
            yield return null;
        }

        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;

        isDashing = false;
    }



    void FixedUpdate()
    {
        // Apply movement using Rigidbody
        Vector3 targetPosition = rb.position + moveInput * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }



    public void PerformPunchHit(float punchRange)
    {
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out RaycastHit hit, punchRange))
        {
            var enemy = hit.collider.GetComponent<PlayerCombat>();
            if (enemy != null)
            {
                enemy.PlayGetHitAnimationServerRpc(10, 12, transform.position, OwnerClientId);
            }
        }
    }
    public void OnClick()
    {
        if (!IsOwner) return;

        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
        Vector3 shootDirection;

        // Összes találat
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);

        // Rendezés távolság szerint (RaycastAll nem mindig garantálja a sorrendet)
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        RaycastHit? validHit = null;
        foreach (var h in hits)
        {
            if (h.distance > 5f) // csak ha 5 méternél messzebb van
            {
                validHit = h;
                break;
            }
        }

        if (validHit.HasValue)
        {
            shootDirection = (validHit.Value.point - firePoint.position).normalized;
        }
        else
        {
            shootDirection = _camera.transform.forward;
        }

        ShootProjectileServerRpc(firePoint.position, shootDirection);
    }

    [ServerRpc]
    private void ShootProjectileServerRpc(Vector3 spawnPosition, Vector3 shootDirection)
    {
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        Bullet bullet = projectile.GetComponent<Bullet>();

        bullet.bulletShooter = pc;
        rb.useGravity = false;
        rb.linearVelocity = shootDirection.normalized * shootForce;

        projectile.GetComponent<NetworkObject>().Spawn(true);

        lastProjectileRb = rb;
    }

    void EnableGravityOnLastProjectile()
    {
        if (lastProjectileRb != null)
        {
            lastProjectileRb.useGravity = true;
        }
    }

    public void CylinderLodged()
    {
        if (!IsOwner)
            return;

        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, terrainLayer))
        {
            // Csak a szervert kérjük meg a spawnra
            SpawnCylinderServerRpc(hit.point, hit.normal);
        }
    }

    [ServerRpc]
    private void SpawnCylinderServerRpc(Vector3 position, Vector3 normal)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
        GameObject newObj = Instantiate(CylinderObject, position, rotation);

        // Fontos: NetworkObject.Spawn()
        newObj.GetComponent<NetworkObject>().Spawn();

        spawnedObjects.Enqueue(newObj);

        if (spawnedObjects.Count > 3)
        {
            GameObject oldest = spawnedObjects.Dequeue();
            if (oldest != null && oldest.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Despawn();
            }
            else
            {
                Destroy(oldest);
            }
        }
    }

    public PlayerCombat pc;

    void FaceCameraDirection()
    {
        Vector3 cameraForward = cameraTransform.forward;

        // Csak a horizontális irányt vegyük, de úgy hogy tényleg számítson a hátrafelé nézés
        Vector3 flatForward = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;

        if (flatForward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatForward);
            transform.rotation = targetRotation;
        }
    }
}
