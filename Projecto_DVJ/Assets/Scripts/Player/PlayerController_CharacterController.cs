using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_CharacterController : MonoBehaviour
{
    [Header("Audios")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpClip;
    
    [Header("Movement")]
    [SerializeField] private float speedMovement;
    private float actualSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float toRestTime;
    
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform; //Cinemachine
    
    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpGracePeriod;
    [SerializeField] private float jumpHorizontalSpeed;
    private float ySpeed;
    private float originalStepOffset;

    //Nullable variables
    private float? lastGroundedTime;
    private float? jumpPressedTime;
    
    private float isRunning;
    private bool isJumping;
    private bool isGrounded;
    private bool isFalling;
    private bool isResting;
    private float restingTimer;
    
    [Header("Dependencies")]
    private CharacterController characterController;
    private Animator _animator;
    
    [Header("Smooth Input")]
    private PlayerInput _playerControls;
    private InputAction _movementAction;
    private InputAction _runningAction;
    private InputAction _jumpAction;
    private Vector2 currentInput;
    private Vector2 smoothInputVelocity;
    private Vector3 moveInput;
    [SerializeField] private float smoothInputSpeed = .5f;
    
    [Header("Animator Parameters")]
    private readonly int directionX = Animator.StringToHash("DirectionX");
    private readonly int directionY = Animator.StringToHash("DirectionY");
    private readonly int jump = Animator.StringToHash("Jump");
    private readonly int IsGrounded = Animator.StringToHash("isGrounded");
    private readonly int speed = Animator.StringToHash("Speed");
    private readonly int resting = Animator.StringToHash("Rest");
    private readonly int restingLong = Animator.StringToHash("LongRest");
    private readonly int running = Animator.StringToHash("Running");

    [SerializeField] private float animatorSpeed = 1.5f;


    
    
    private void Awake()
    {
        _playerControls = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _movementAction = _playerControls.actions["Movement"];
        _jumpAction = _playerControls.actions["Jump"];
        _runningAction = _playerControls.actions["Running"];
        originalStepOffset = characterController.stepOffset;
    }
    
    
    private void Update()
    {
        OnMovement();
    }

    public void OnMovement()
    {
        //Speed ajustment
        if (isRunning == 1)
        {
            actualSpeed = speedMovement;
        }
        else
        {
            actualSpeed = speedMovement/3;
        }
        
        //Movement
        Vector2 movementInput = _movementAction.ReadValue<Vector2>();
        //currentInput = Vector2.SmoothDamp(currentInput, movementInput, ref smoothInputVelocity, smoothInputSpeed);
        //float inputSpeed = currentInput.sqrMagnitude;
        moveInput = new Vector3(movementInput.x, 0, movementInput.y);
        float inputSpeed = movementInput.sqrMagnitude;
        
        // Camera transform
        Vector3 cameraForward = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;
        Vector3 cameraRight = new Vector3(cameraTransform.right.x, 0f, cameraTransform.right.z).normalized;
        
        ySpeed += Physics.gravity.y * Time.deltaTime;
        
        if(characterController.isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        if (_jumpAction.triggered && (!_animator.IsInTransition(0)))
        {
            jumpPressedTime = Time.time;
        }
        
        //Jumping
        if (Time.time - lastGroundedTime <= jumpGracePeriod)
        {
            characterController.stepOffset = originalStepOffset;
            ySpeed = -0.5f;
            _animator.SetBool(IsGrounded, true);
            isGrounded = true;
            _animator.SetBool(jump, false);
            isJumping = false;
            _animator.SetBool("isFalling", false);
            
            
            if (Time.time - jumpPressedTime <= jumpGracePeriod)
            {
                ySpeed = jumpForce;
                _animator.SetBool(jump, true);
                isJumping = true;
                audioSource.PlayOneShot(jumpClip);
                jumpPressedTime = null;
                lastGroundedTime = null;
            }
        }
        else
        {
            characterController.stepOffset = 0f;
            _animator.SetBool(IsGrounded, false);
            isGrounded = false;
            
            if((isJumping && ySpeed < 0) || ySpeed < -2)
            {
                _animator.SetBool("isFalling", true);
                //isFalling = true;
            }
        }
        
        Vector3 velocity;
        float finalMagnitude = Mathf.Clamp01(moveInput.magnitude)*actualSpeed;
        
      
        // Input + Camera Transform
        moveInput = (cameraForward * movementInput.y + cameraRight * movementInput.x).normalized;
        velocity = moveInput * finalMagnitude;
        velocity.y = ySpeed; 
        characterController.Move(velocity*Time.deltaTime);
        
        // else
        // {
        //     velocity = movementInput * finalMagnitude * jumpHorizontalSpeed;
        //     velocity.y = ySpeed;
        //     characterController.Move(velocity*Time.deltaTime);
        // }
        //Prueba Rootmotion en OnAnimatorMove

        // Asignar la velocidad suavizada al Animator
        if(inputSpeed >= 0)
        {
            _animator.SetFloat(speed, inputSpeed, 0.05f, Time.deltaTime); //Pequeño damping
        }
        else
        {
            _animator.SetFloat(speed, 0,0.05f, Time.deltaTime);
        }
        _animator.speed = animatorSpeed;
        _animator.SetFloat(directionX, movementInput.x);
        _animator.SetFloat(directionY, movementInput.y);
        
        // Rotamos el personaje hacia la dirección de movimiento suavemente
        if (moveInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        
        if (isGrounded == false)
        {
            float inputMagnitude = Mathf.Clamp01(moveInput.magnitude);
            velocity = moveInput * inputMagnitude * jumpHorizontalSpeed;
            Debug.Log(inputMagnitude +" " +  finalMagnitude);
            velocity.y = ySpeed;

            characterController.Move(velocity * Time.deltaTime);
        }
        
        
        //transform.LookAt(transform.position + moveInput);
        
        //Rotacion instantanea
        //if (moveInput != Vector3.zero)
        //{
            //Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            //transform.rotation = targetRotation;
        //}
        
        
        //Rest
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            restingTimer += Time.deltaTime;
            Debug.Log("tiempo de descanso:" + restingTimer);
            if (restingTimer >= toRestTime)
            {
                isResting = true;
                _animator.SetBool(resting, isResting);
            }
        }
        else if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Rest"))
        {
            restingTimer += Time.deltaTime;
            //Debug.Log("tiempo de descanso:" + restingTimer);
            if (restingTimer >= toRestTime * 2)
            {
                _animator.SetBool(restingLong, true);
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
        
        //Running
        if (_runningAction.IsPressed() && isRunning == 0)
        {
            isRunning = 1;
            _animator.SetFloat(running, isRunning);
        }
        
        if (_runningAction.IsPressed() == false && isRunning == 1)
        {
            isRunning = 0;
            _animator.SetFloat(running, isRunning);
        }
    }

    // private void OnAnimatorMove()
    // {
    //     //float finalMagnitude = Mathf.Clamp01(moveInput.magnitude)*actualSpeed;
    //     Vector3 velocity = _animator.deltaPosition;
    //     velocity.y = ySpeed*Time.deltaTime;
    //     characterController.Move(velocity);
    // }
}