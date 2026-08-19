using UnityEngine;

public class Enums : MonoBehaviour
{

}

public enum KnifeState
{
    Melee, // while adopting a melee stance
    Aiming, // while RMB (or equivalent) is held down
    Empty // when the player has no knives left
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