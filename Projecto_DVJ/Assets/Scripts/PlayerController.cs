using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Audios")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpClip;
    
    [Header("Movement")]
    [SerializeField] private float speedMovement;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float toRestTime;
    
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform; //Cinemachine
    
    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private Transform groundCheck; 
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;
    
    private bool isGrounded;
    private bool isResting;
    private float restingTimer;
    
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
    private readonly int resting = Animator.StringToHash("Rest");

    [SerializeField] private float animatorSpeed = 1.5f;

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
            // Movemos al personaje con el Rigidbody
            rigidbody.MovePosition(rigidbody.position + moveInput * speedMovement * Time.fixedDeltaTime);
        }
    }

    public void OnMovement()
    {
        //Movement
        Vector2 movementInput = _movementAction.ReadValue<Vector2>();
        //currentInput = Vector2.SmoothDamp(currentInput, movementInput, ref smoothInputVelocity, smoothInputSpeed);
        //float inputSpeed = currentInput.sqrMagnitude;
        moveInput = new Vector3(movementInput.x, 0, movementInput.y);
        float inputSpeed = movementInput.sqrMagnitude;
        
        // Camera transform
        Vector3 cameraForward = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;
        Vector3 cameraRight = new Vector3(cameraTransform.right.x, 0f, cameraTransform.right.z).normalized;

        // Input + Camera Transform
        moveInput = (cameraForward * movementInput.y + cameraRight * movementInput.x).normalized;

        // Asignar la velocidad suavizada al Animator
        if(inputSpeed >= 0)
        {
            _animator.SetFloat(speed, inputSpeed);
        }
        else
        {
            _animator.SetFloat(speed, 0);
        }
        _animator.speed = animatorSpeed;
        _animator.SetFloat(directionX, movementInput.x);
        _animator.SetFloat(directionY, movementInput.y);
        
        
        // Rotamos el personaje hacia la dirección de movimiento suavemente
        if (moveInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(rigidbody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
            
        
        //transform.LookAt(transform.position + moveInput);
        
        //Rotacion instantanea
        //if (moveInput != Vector3.zero)
        //{
            //Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            //transform.rotation = targetRotation;
        //}
        
        
        //Jumping
        if (_jumpAction.triggered && isGrounded && (!_animator.IsInTransition(0)))
        {
            rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            _animator.SetBool(jump, true);
            isGrounded = false;
            audioSource.PlayOneShot(jumpClip);
        }
        
        //Rest
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            restingTimer += Time.deltaTime;
            //Debug.Log("tiempo de descanso:" + restingTimer);
            if (restingTimer >= toRestTime)
            {
                isResting = true;
                _animator.SetBool(resting, isResting);
            }
        }
        else
        {
            restingTimer = 0f;
            if (isResting)
            {
                isResting = false;
                _animator.SetBool(resting, isResting);
            }
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