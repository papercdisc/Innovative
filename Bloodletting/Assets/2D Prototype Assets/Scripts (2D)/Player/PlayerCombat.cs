using System;
using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] PlayerInputSubscription getInput;
    bool basicAttackInput;
    bool abilityInput;

    bool BAIsPressed = false;

    [SerializeField] PlayerAbility equippedAbility;

    bool usedAbility1 = false;

    [SerializeField] GameObject bombPrefab;

    [SerializeField] float ability1Cooldown = 2f;
    [SerializeField] float currentAbility1Cooldown = 0f;

    [SerializeField] Collider2D basicAttackHitbox;
    [SerializeField] float hbActiveDuration = 0.25f;

    [SerializeField] SpriteRenderer basicAttackSprite;
    [SerializeField] Color activeBAColor, inactiveBAColor;

    Coroutine basicAttackCoroutine = null;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PlayerInputSubscription.Instance != null && getInput == null)
        {
            getInput = PlayerInputSubscription.Instance;
        }

        basicAttackHitbox.enabled = false;
        basicAttackSprite.color = inactiveBAColor;
    }

    // Update is called once per frame
    void Update()
    {
        basicAttackInput = getInput.AttackInput;
        abilityInput = getInput.AltAttackInput;

        if (!abilityInput)
        {
            usedAbility1 = false;
        }
        if (!basicAttackInput) { // quick fix for now, but will want to implement a hold down feature (probably with a coroutine that restarts if the key is still held down)
            BAIsPressed = false;
        }

        Vector2 lookPosition = getInput.LookInput;
        Vector2 direction = (lookPosition - (Vector2)transform.position).normalized;
        basicAttackHitbox.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);

        if (basicAttackInput && !BAIsPressed)
        {
            BAIsPressed = true;

            ExecuteBasicAttack();
        }

        if (abilityInput && !usedAbility1)
        {
            usedAbility1 = true;

            switch (equippedAbility)
            {
                case PlayerAbility.Bomb:
                    ExecuteBombAbility();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (currentAbility1Cooldown > 0)
        {
            currentAbility1Cooldown -= Time.deltaTime;
        }
        else
        {
            currentAbility1Cooldown = 0;
        }
    }

    private void ExecuteBasicAttack()
    {
        if (basicAttackCoroutine != null) return;
        basicAttackCoroutine = StartCoroutine(BasicAttackCoroutine());
    }

    IEnumerator BasicAttackCoroutine()
    {
        basicAttackHitbox.enabled = true;
        basicAttackSprite.color = activeBAColor;

        yield return new WaitForSeconds(hbActiveDuration); // Duration of the hitbox being active
        
        basicAttackHitbox.enabled = false;
        basicAttackSprite.color = inactiveBAColor;
        
        basicAttackCoroutine = null;
    }

    private void ExecuteBombAbility()
    {
        //Debug.Log("Bomb ability executed!");
        if (currentAbility1Cooldown > 0)
        {
            //Debug.Log("Ability is on cooldown!");
            return;
        }


        Vector2 lookPosition = getInput.LookInput;
        Vector2 direction = (lookPosition - (Vector2)transform.position).normalized;

        GameObject bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        bomb.GetComponent<BombBehavior>().velocity = direction; // Example velocity, you can set this based on player direction or input

        currentAbility1Cooldown = ability1Cooldown;
    }
}
