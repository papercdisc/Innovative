using UnityEngine;
using UnityEngine.Events;

public class PlayerKnifeRetrieveHandler : MonoBehaviour
{
    PlayerKnifeCombatManager cm;
    PlayerKnifeInventory inventory;
    [Header("References")]
    [SerializeField] private Transform chestHeight;
    [SerializeField] private LayerMask obstructionMask;
    public GameObject vfxPrefab; // ideally this would be handled in a separate script, but its ok in here for now

    [Header("Knife Retrieval")]
    [SerializeField] private float retrieveRange = 25f;
    [SerializeField] private float closeRetrieveRange = 5f;
    [SerializeField, Range(1f, 30f), Tooltip("Half-angle of FOV cone for retrieving knives.")] private float retrieveAngle = 8f;

    [Header("Finisher Settings")]
    [SerializeField] private float finisherDamage = 60f;
    [SerializeField] private float retrieveFinisherRange = 5f; // range for unique enemy finisher retrieval 
    EnemyHealth pendingFinisherEnemy;
    public UnityEvent OnFinisher;

    private void Awake()
    {
        cm = GetComponent<PlayerKnifeCombatManager>();
        inventory = GetComponent<PlayerKnifeInventory>();
    }

    public void Retrieve()
    {
        // input only goes through if the player is aiming at/near a knife
        KnifeProjectile knifeToRetrieve = FindKnifeInView();
        if (knifeToRetrieve == null) return;

        // depending on proximity to knife + if its in an enemy, do different things
        Vector3 toKnife = knifeToRetrieve.transform.position - Camera.main.transform.position;
        float dist = toKnife.magnitude;

        EnemyHealth enemy = knifeToRetrieve.GetComponentInParent<EnemyHealth>();
        if (enemy)
        {
            // if in an enemy + close to knife, perform special attack (+bonus damage) and retrieve knife
            if (IsInFinisherRange(knifeToRetrieve))
            {
                pendingFinisherEnemy = enemy;
                Vector3 vfxSpawn = knifeToRetrieve.transform.position;

                Instantiate(vfxPrefab, vfxSpawn, Quaternion.LookRotation(transform.forward), this.gameObject.transform);

                knifeToRetrieve.QuickReturnToPlayer();

                OnFinisher?.Invoke(); // start finisher animation
            }
            else // normal retrieve
            {
                enemy.TakeDamage(knifeToRetrieve.projDamage * 0.75f);

                knifeToRetrieve.SetRetrieving(chestHeight);
            }
        }
        else // not in an enemy
        {
            if (dist < closeRetrieveRange) knifeToRetrieve.QuickReturnToPlayer();
            else knifeToRetrieve.SetRetrieving(chestHeight);
        }
    }

    public void ResolveFinisher()
    {
        EnemyHealth enemy = pendingFinisherEnemy;
        //KnifeProjectile knife = pendingFinisherKnife;
        // clear banked knife + enemy
        pendingFinisherEnemy = null;
        //pendingFinisherKnife = null;

        if (enemy != null && !enemy.isDead) enemy.TakeDamage(finisherDamage);
        //if (knife != null) knife.QuickReturnToPlayer();
    }
    public bool IsInFinisherRange(KnifeProjectile knife)
    {
        if (knife == null || knife.State != KnifeProjState.InEnemy) return false; // only continue if stuck to an enemy

        EnemyHealth enemy = knife.GetComponentInParent<EnemyHealth>();
        if (enemy == null || enemy.isDead) return false; // only continue if the enemy actually exists

        float dist = Vector3.Distance(Camera.main.transform.position, knife.transform.position);
        return dist < retrieveFinisherRange;
    }
    private KnifeProjectile FindKnifeInView()
    {
        Transform cam = Camera.main.transform;
        float minDot = Mathf.Cos(retrieveAngle * Mathf.Deg2Rad); // minimum COS for knives to be considered within view

        KnifeProjectile bestMatch = null;
        float bestDot = minDot;

        if(inventory.SpawnedKnives != null)
        foreach (KnifeProjectile knife in inventory.SpawnedKnives)
        {
            if (knife == null) continue;
            bool retrieveable = knife.State == KnifeProjState.CanPickup || knife.State == KnifeProjState.InEnemy;
            if (!retrieveable) continue; // skip this knife if neither are true

            Vector3 toKnife = knife.transform.position - cam.position; // direction + mag to knife
            float dist = toKnife.magnitude; // distance as float
            if (dist > retrieveRange) continue; // ignore if outside of (distance) range

            float dot = Vector3.Dot(cam.forward, toKnife.normalized);
            if (dot < bestDot) continue; // ignore if outside cone, or worse than current best

            // check if in LOS (allow hitting the knife itself, since it may be stuck in a wall)
            if (Physics.Linecast(cam.position, knife.transform.position, out RaycastHit hit, obstructionMask)
                && hit.transform != knife.transform) continue;

            bestMatch = knife;
            bestDot = dot;
        }

        //Debug.Log("Best Match is: " + bestMatch);
        return bestMatch;
    }
}
