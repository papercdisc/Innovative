using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// handles tracking of knives
public class PlayerKnifeInventory : MonoBehaviour
{
    [field: SerializeField] public int MaxKnives { get; private set; } = 3;
    [field: SerializeField] public int HeldKnives { get; private set; } = 0;
    [field: SerializeField] public List<KnifeProjectile> SpawnedKnives { get; private set; } = new List<KnifeProjectile>(); // might want to change to hash set but idk

    public UnityEvent<int> OnKnifeCountChanged;

    public void AddKnifeToInventory(int amount)
    {
        HeldKnives = Mathf.Clamp(HeldKnives + amount, 0, MaxKnives);
        OnKnifeCountChanged?.Invoke(HeldKnives); // invoke event to update UI
    }

    public void RegisterThrownKnife(KnifeProjectile knife) => SpawnedKnives.Add(knife);
    public void RemoveThrownKnife(KnifeProjectile knife) => SpawnedKnives.Remove(knife);
}
