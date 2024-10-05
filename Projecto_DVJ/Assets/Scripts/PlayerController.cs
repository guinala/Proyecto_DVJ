using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Speed")]
    public float speedMovement;
    
    private Rigidbody rigidbody;
    
    [Header("Smooth Input")]
    private PlayerInput _playerControls;
    private InputAction _movementAction;
    private Vector2 currentInput;
    private Vector2 smoothInputVelocity;
    private Vector3 moveInput;
    [SerializeField] private float smoothInputSpeed = .5f;
    
    private Vector2 _movement;
    private float _movementMagnitude;
    public Vector2 Movement => _movement;
    
    private Animator _animator;
    private bool _isWalking;
    
    [Header("Animator Parameters")]
    private readonly int directionX = Animator.StringToHash("DirectionX");
    private readonly int directionY = Animator.StringToHash("DirectionY");
    private readonly int speed = Animator.StringToHash("Speed");

    private void Awake()
    {
        _playerControls = GetComponent<PlayerInput>();
        rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _movementAction = _playerControls.actions["Movement"];
    }
    

    private void Update()
    {
        OnMovement();
    }
    
    private void FixedUpdate()
    {
        MovePlayer();
    }
    
    private void MovePlayer()
    {
        if(moveInput != Vector3.zero)
        {
            rigidbody.MovePosition(rigidbody.position + moveInput * speedMovement * Time.fixedDeltaTime);
        }
    }

    public void OnMovement()
    {
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
        _animator.SetFloat("DirectionX", movementInput.x);
        _animator.SetFloat("DirectionY", movementInput.y);
    }
    

    private void StopMovement(InputAction.CallbackContext value)
    {
        _movement = Vector2.zero;
    }
}