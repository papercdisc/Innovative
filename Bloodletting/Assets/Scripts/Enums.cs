using UnityEngine;

public class Enums : MonoBehaviour
{

}

public enum HeldKnifeState
{
    Melee, // while adopting a melee stance
    Aiming // while RMB (or equivalent) is held down
}
public enum  KnifeProjState
{
    InFlight, // initial state (constantly in movement)
    InEnemy, // after hitting an enemy
    Dropped, // while falling to the ground
    CanPickup // after hitting a wall or if enemy is dead and the knife is on the ground
}

#region Old (2D) Enums
public enum PlayerAbility
{
    Bomb,
    Dash
}
public enum EnemyType
{
    Chaser,
    Coward,
    Saboteur,
    Healer
}
public enum EnemyState
{
    Idle,
    Aggro,
    Flee
}
#endregion