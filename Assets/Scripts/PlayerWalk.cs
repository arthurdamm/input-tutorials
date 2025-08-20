using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWalk : MonoBehaviour
{
    public InputActionAsset InputActions;

    private InputAction m_moveAction;
    private InputAction m_lookAction;
    private InputAction m_jumpAction;

    private Vector2 m_moveAmt;
    private Vector2 m_lookAmt;
    private Animator m_animator;
    private Rigidbody m_rigidbody;

    public float WalkSpeed = 5;
    public float RotateSpeed = 200;
    public float JumpSpeed = 5;
    [SerializeField] float MouseXSens = 0.15f;   // scales pixels/frame
    [SerializeField] float PadXSens   = 1.0f;    // scales [-1..1]
    private float CurrentXSens;

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        // m_moveAction = InputSystem.actions.FindAction("Move");
        // m_lookAction = InputSystem.actions.FindAction("Look");
        // m_jumpAction = InputSystem.actions.FindAction("Jump");

        var playerMap = InputActions.FindActionMap("Player", throwIfNotFound: true);
        m_moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
        m_lookAction = playerMap.FindAction("Look", throwIfNotFound: true);
        m_jumpAction = playerMap.FindAction("Jump", throwIfNotFound: true);

        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody>();

    // this can also be done with a processor in the input action
        var dev = m_lookAction.activeControl?.device;
        if (dev is Mouse) CurrentXSens = MouseXSens;
        else CurrentXSens = PadXSens;
    
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        m_moveAmt = m_moveAction.ReadValue<Vector2>();
        m_lookAmt = m_lookAction.ReadValue<Vector2>();
        m_lookAmt.x *= CurrentXSens;

        if (m_jumpAction.WasPressedThisFrame())
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        Walking();
        Rotating();
    }

    private void Rotating()
    {
        // small deadzone so tiny noise doesn't accumulate
        if (Mathf.Abs(m_lookAmt.x) < 0.01f)
            return;

        float rotationAmount = m_lookAmt.x * RotateSpeed * Time.fixedDeltaTime; // physics timestep
        Quaternion deltaRotation = Quaternion.Euler(0f, rotationAmount, 0f);
        m_rigidbody.MoveRotation(deltaRotation * m_rigidbody.rotation);
        Debug.Log($"Rotating {deltaRotation}");
    }

    private void Walking()
    {
        m_animator.SetFloat("Speed", m_moveAmt.y);
        m_rigidbody.MovePosition(m_rigidbody.position + transform.forward * m_moveAmt.y * WalkSpeed * Time.deltaTime);
    }

    private void Jump()
    {
        m_rigidbody.AddForceAtPosition(new Vector3(0, 5f, 0), Vector3.up, ForceMode.Impulse);
        m_animator.SetTrigger("Jump");
    }
}
