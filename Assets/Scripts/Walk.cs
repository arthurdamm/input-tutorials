using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class Walk : MonoBehaviour
{
    [SerializeField] private InputActionAsset  actionAsset;
    private InputActionMap playerMap;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float rotateSpeed = 200f;
    [SerializeField] private Vector2 moveAmt;
    [SerializeField] private Vector2 lookAmt;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        playerMap = actionAsset.FindActionMap("Player");
        playerMap.Enable();
    }

    private void OnDisable()
    {
        playerMap.Disable();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        moveAmt = playerMap.FindAction("Move").ReadValue<Vector2>();
        lookAmt = playerMap.FindAction("Look").ReadValue<Vector2>();
        if (playerMap.FindAction("Jump").WasPressedThisFrame())
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
        float rotateAmount = lookAmt.x * rotateSpeed * Time.deltaTime;
        Quaternion rotation = Quaternion.Euler(0, rotateAmount, 0);
        _rigidbody.MoveRotation(_rigidbody.rotation * rotation);
    }

    private void Walking()
    {
        float moveAmount = moveAmt.y * moveSpeed * Time.deltaTime;
        _rigidbody.MovePosition(_rigidbody.position + transform.forward * moveAmount);
        animator.SetFloat("Speed", moveAmt.y);
    }

    private void Jump()
    {
        _rigidbody.AddForceAtPosition(new Vector3(0f, 5f, 0f), _rigidbody.position,  ForceMode.Impulse);
        animator.SetTrigger("Jump");
    }
}
