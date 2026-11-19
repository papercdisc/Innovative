using UnityEngine;
using UnityEngine.Events;

// executes attacks & spawns knives
public class PlayerKnifeAttackHandler : MonoBehaviour
{
    PlayerKnifeCombatManager cm;
    PlayerKnifeInventory inventory;
    [SerializeField] float atkCD = 0.7f;
    [Header("Knife Spawning")]
    [SerializeField] private Transform knifeSpawnPoint;
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private float maxKnifeLifetime;
    [Header("Events")]
    public UnityEvent OnKnifeThrown;

    public bool CanAttack { get; private set; } = true;

    private void Awake()
    {
        cm = GetComponent<PlayerKnifeCombatManager>();
        inventory = GetComponent<PlayerKnifeInventory>();
    }

    public void Attack(HeldKnifeState knifeState) // actually execute attack if conditions are met (called in FixedUpdate to avoid frame rate issues)
    {
        CanAttack = false;

        if (knifeState == HeldKnifeState.Aiming)
        {
            // assuming all conditions are met, throw the knife
            // 1. Instantiate the knife prefab at the spawn point
            // 2. Set velocity of the knife to the player's orientation (child object) * throw force
            // 3. Decrease heldKnives and store the thrown knife in a list for potential pickup later

            // find trajectory
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // shoot from center of screen
            RaycastHit hit;

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out hit))
            {
                targetPoint = hit.point;
            }
            else targetPoint = ray.GetPoint(75);

            if(inventory != null) inventory.AddKnifeToInventory(-1);

            SpawnKnife(targetPoint);

            OnKnifeThrown?.Invoke(); // invoke event to trigger HUD animation, etc.
        }
        else if (knifeState == HeldKnifeState.Melee)
        {
            // assuming all conditions are met, perform melee attack
            // 1. Play melee animation
            // 2. Check for enemies in range and apply damage
        }

        Invoke(nameof(ResetAtk), atkCD); // reset attack after cooldown)
    }

    public void SpawnKnife(Vector3 hitTargetPoint)
    {
        Vector3 dir = hitTargetPoint - knifeSpawnPoint.position; // find direction from given target point & spawn position

        KnifeProjectile knife = Instantiate(knifePrefab, knifeSpawnPoint.position, Quaternion.identity).GetComponent<KnifeProjectile>();
        //Debug.Log($"Assigning lifetime: {maxKnifeLifetime}");
        knife.lifetime = maxKnifeLifetime;
        knife.moveDir = dir;
        knife.PlayerInventory = inventory;

        if(inventory != null) inventory.RegisterThrownKnife(knife);
    }

    void ResetAtk()
    {
        CanAttack = true;
    }
}
