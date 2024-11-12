using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/PlayerStats")]
public class PlayerStatsSO : ScriptableObject
{
    public int health;
    public int maxHealth;
    public float speed;
    public float jumpForce;
    public float restTime;
    public float parryTime;
}
