using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Combat")]
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private LoadoutController loadoutController;
    [SerializeField] private GrenadeSlotController grenadeSlotController;

    private Rigidbody rb;
    private Animator animator;
    private Camera mainCamera;
    private Vector3 movement;
    private Vector3 aimWorldPoint;

    private InputAction moveAction;
    private InputAction pointAction;
    private InputAction fireAction;
    private InputAction switchPrimaryAction;
    private InputAction switchSecondaryAction;
    private InputAction cycleWeaponAction;
    private InputAction cycleGrenadeAction;
    private InputAction throwGrenadeAction;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        if (weaponController == null) weaponController = GetComponent<WeaponController>();
        if (loadoutController == null) loadoutController = GetComponent<LoadoutController>();
        if (grenadeSlotController == null) grenadeSlotController = GetComponent<GrenadeSlotController>();

        moveAction = InputSystem.actions.FindAction("Move");
        pointAction = InputSystem.actions.FindAction("Point");
        fireAction = InputSystem.actions.FindAction("Fire");
        switchPrimaryAction = InputSystem.actions.FindAction("SwitchPrimary");
        switchSecondaryAction = InputSystem.actions.FindAction("SwitchSecondary");
        cycleWeaponAction = InputSystem.actions.FindAction("CycleWeapon");
        cycleGrenadeAction = InputSystem.actions.FindAction("CycleGrenade");
        throwGrenadeAction = InputSystem.actions.FindAction("ThrowGrenade");

        if (switchPrimaryAction != null) switchPrimaryAction.performed += OnSwitchPrimary;
        if (switchSecondaryAction != null) switchSecondaryAction.performed += OnSwitchSecondary;
        if (cycleWeaponAction != null) cycleWeaponAction.performed += OnCycleWeapon;
        if (cycleGrenadeAction != null) cycleGrenadeAction.performed += OnCycleGrenade;
        if (throwGrenadeAction != null) throwGrenadeAction.performed += OnThrowGrenade;
    }

    private void OnDestroy()
    {
        if (switchPrimaryAction != null) switchPrimaryAction.performed -= OnSwitchPrimary;
        if (switchSecondaryAction != null) switchSecondaryAction.performed -= OnSwitchSecondary;
        if (cycleWeaponAction != null) cycleWeaponAction.performed -= OnCycleWeapon;
        if (cycleGrenadeAction != null) cycleGrenadeAction.performed -= OnCycleGrenade;
        if (throwGrenadeAction != null) throwGrenadeAction.performed -= OnThrowGrenade;
    }

    void Update()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        movement.x = moveInput.x;
        movement.z = moveInput.y;
        movement.y = 0f;
        movement = movement.normalized;

        animator.SetFloat("Speed", movement.magnitude);

        AimAtMouse();

        if (fireAction != null && fireAction.IsPressed() && weaponController != null)
        {
            weaponController.TryFire(transform.forward);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void AimAtMouse()
    {
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        Vector2 mousePosition = pointAction.ReadValue<Vector2>();
        Ray cameraRay = mainCamera.ScreenPointToRay(mousePosition);

        if (groundPlane.Raycast(cameraRay, out float hitDistance))
        {
            aimWorldPoint = cameraRay.GetPoint(hitDistance);
            transform.LookAt(aimWorldPoint);
        }
    }

    private void OnSwitchPrimary(InputAction.CallbackContext _)
    {
        if (loadoutController != null) loadoutController.SetActiveSlot(WeaponSlot.Primary);
    }

    private void OnSwitchSecondary(InputAction.CallbackContext _)
    {
        if (loadoutController != null) loadoutController.SetActiveSlot(WeaponSlot.Secondary);
    }

    private void OnCycleWeapon(InputAction.CallbackContext context)
    {
        if (loadoutController == null) return;

        float scrollValue = context.ReadValue<Vector2>().y;
        if (Mathf.Abs(scrollValue) < 0.1f) return;

        // Cycle between Primary and Secondary
        WeaponSlot nextSlot = loadoutController.ActiveSlot == WeaponSlot.Primary ? WeaponSlot.Secondary : WeaponSlot.Primary;
        loadoutController.SetActiveSlot(nextSlot);
    }

    private void OnCycleGrenade(InputAction.CallbackContext _)
    {
        if (grenadeSlotController != null) grenadeSlotController.CycleGrenadeType();
    }

    private void OnThrowGrenade(InputAction.CallbackContext _)
    {
        if (grenadeSlotController != null) grenadeSlotController.TryThrowSelected(aimWorldPoint);
    }
    }
