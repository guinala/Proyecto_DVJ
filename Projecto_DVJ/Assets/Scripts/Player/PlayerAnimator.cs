using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;
    private bool _isWalking;
    
    [Header("Animator Parameters")]
    private readonly int direction = Animator.StringToHash("Direction");
    private readonly int speed = Animator.StringToHash("Speed");
    private readonly int walking = Animator.StringToHash("Walking");
    
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    
}
