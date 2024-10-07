using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speedMovement;
    [SerializeField] private float rotationSpeed;
    
    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private Transform groundCheck; 
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;
    private bool isGrounded; 
    
    [Header("Dependencies")]
    private Rigidbody rigidbody;
    private Animator _animator;
    
    [Header("Smooth Input")]
    private PlayerInput _playerControls;
    private InputAction _movementAction;
    private InputAction _jumpAction;
    private Vector2 currentInput;
    private Vector2 smoothInputVelocity;
    private Vector3 moveInput;
    [SerializeField] private float smoothInputSpeed = .5f;
    
    [Header("Animator Parameters")]
    private readonly int directionX = Animator.StringToHash("DirectionX");
    private readonly int directionY = Animator.StringToHash("DirectionY");
    private readonly int jump = Animator.StringToHash("Jump");
    private readonly int speed = Animator.StringToHash("Speed");

    private void Awake()
    {
        _playerControls = GetComponent<PlayerInput>();
        rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _movementAction = _playerControls.actions["Movement"];
        _jumpAction = _playerControls.actions["Jump"];
    }
    

    private void Update()
    {
        OnMovement();
    }
    
    private void FixedUpdate()
    {
        MovePlayer();
        CheckGroundStatus();
    }
    
    private void MovePlayer()
    {
        //Moving
        if(moveInput != Vector3.zero)
        {
            rigidbody.MovePosition(rigidbody.position + moveInput * speedMovement * Time.fixedDeltaTime);
            
            // Rotar hacia la dirección del movimiento
            Quaternion targetRotation = Quaternion.LookRotation(moveInput, Vector3.up);
            Quaternion smoothRotation = Quaternion.Slerp(rigidbody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            rigidbody.MoveRotation(smoothRotation);
        }
    }

    public void OnMovement()
    {
        //Movement
        Vector2 movementInput = _movementAction.ReadValue<Vector2>();
        currentInput = Vector2.SmoothDamp(currentInput, movementInput, ref smoothInputVelocity, smoothInputSpeed);
        float inputSpeed = currentInput.sqrMagnitude;
        moveInput = new Vector3(currentInput.x, 0, currentInput.y);
        

        // Asignar la velocidad suavizada al Animator
        if(inputSpeed >= 0)
        {
            _animator.SetFloat(speed, inputSpeed);
        }
        else
        {
            _animator.SetFloat(speed, 0);
        }
        _animator.SetFloat(directionX, movementInput.x);
        _animator.SetFloat(directionY, movementInput.y);
        
        //Jumping
        if (_jumpAction.triggered && isGrounded)
        {
            rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            _animator.SetBool(jump, true);
            isGrounded = false;
        }
    }
    
    private void CheckGroundStatus()
    {
        // Raycast
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // Está en el suelo
        if (isGrounded)
        {
            _animator.SetBool(jump, false);
        }
    }
}