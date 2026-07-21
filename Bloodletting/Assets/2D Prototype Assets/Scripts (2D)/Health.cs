using UnityEngine;

public abstract class Health : MonoBehaviour
{
    public abstract void TakeDamage(float dmg);
    public abstract void Die();
}
