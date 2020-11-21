using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public enum Faction { Player, Enemy };
    public enum Direction { North = 0, East = 1, South = 2, West = 3};

    public Faction faction = Faction.Player;
    public Vector2 moveSpeed = new Vector2(100.0f, 100.0f);
    public float   attackHoldTime = 0.5f;
    public float   hitHoldTime = 0.5f;
    public int     maxHealth = 3;
    public Weapon  weapon;
    public Armour  armour;
    public float   invulnerabilityDuration = 1.0f;

    [Header("Colliders")]
    public Collider2D[]         hitColliders;

    protected Rigidbody2D       rigidBody;
    protected SpriteRenderer    sprite;
    protected Animator          animator;

    protected Vector2           visualDir;
    protected float             holdPosTimer;
    protected Collider2D[]      collisionResults;
    protected ContactFilter2D   hitFilter;
    protected int               hitPoints;
    protected float             invulnerabilityTimer;
    protected bool              isDead;
    protected bool              stopChar;

    public bool isInvulnerable
    {
        get
        {
            if (isDead) return true;

            return invulnerabilityTimer > 0.0f;
        }
        set
        {
            if (value) invulnerabilityTimer = invulnerabilityDuration;
            else invulnerabilityTimer = 0.0f;
        }
    }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        visualDir = Vector2.zero;
        collisionResults = new Collider2D[32];
        hitFilter = new ContactFilter2D();
        hitFilter.SetLayerMask(LayerMask.GetMask("Character"));
        hitPoints = maxHealth;

        stopChar = true;
    }

    protected void Update()
    {
        if (isDead) return;

        if (stopChar)
        {
            animator.SetFloat("VelocityX", 0.0f);
            animator.SetFloat("VelocityY", 0.0f);
        }
        else
        {
            animator.SetFloat("VelocityX", Mathf.Abs(visualDir.x));
            animator.SetFloat("VelocityY", visualDir.y);
        }

        if (visualDir.x != 0.0f)
        {
            sprite.flipX = visualDir.x < 0.0f;
        }

        if (holdPosTimer > 0.0f)
        {
            holdPosTimer -= Time.deltaTime;

            if (holdPosTimer <= 0.0f) holdPosTimer = 0.0f;
        }

        if (invulnerabilityTimer > 0.0f)
        {
            invulnerabilityTimer -= Time.deltaTime;

            if (invulnerabilityTimer <= 0.0f)
            {
                sprite.enabled = true;
            }
            else
            {
                sprite.enabled = (Mathf.FloorToInt(invulnerabilityTimer * 10.0f) % 2) == 0;
            }
        }
    }

    protected void MoveTo(Vector2 direction)
    {
        if (holdPosTimer > 0.0f) return;
        if (isDead) return;

        rigidBody.velocity = direction * moveSpeed;

        if (direction.magnitude > 0.05f)
        {
            visualDir = new Vector2(direction.x * 2.0f, direction.y);
            visualDir.Normalize();
            stopChar = false;
        }
        else
        {
            stopChar = true;
        }
    }

    protected void Attack()
    {
        if (isDead) return;

        animator.SetTrigger("Attack");

        rigidBody.velocity = Vector2.zero;

        holdPosTimer = attackHoldTime;
    }

    protected void Hit()
    {
        if (isDead) return;

        animator.SetTrigger("Hit");

        rigidBody.velocity = Vector2.zero;

        holdPosTimer = hitHoldTime;
    }

    void DisableAllColliders()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders)
        {
            c.enabled = false;
        }
    }

    public Direction GetDirection()
    {
        if (Mathf.Abs(visualDir.x) > Mathf.Abs(visualDir.y))
        {
            // Right or Left collider
            if (visualDir.x > 0.0f) return Direction.East;

            return Direction.West;
        }
        else
        {
            // Up or down collider
            if (visualDir.y > 0.0f) return Direction.North;

            return Direction.South;
        }
    }

    Collider2D GetHitCollider()
    {
        if (visualDir.sqrMagnitude < 0.1f)
        {
            return hitColliders[(int)Direction.East];
        }

        var dir = GetDirection();

        return hitColliders[(int)dir];
    }

    public void AttackFrame()
    {
        Collider2D collider = GetHitCollider();

        int nCollisions = Physics2D.OverlapCollider(collider, hitFilter, collisionResults);
        if (nCollisions > 0)
        {
            for (int i = 0; i < nCollisions; i++)
            {
                Character character = collisionResults[i].GetComponent<Character>();
                if (character)
                {
                    if (character.faction != faction)
                    {
                        Vector2 impactDir = (transform.position - character.transform.position).normalized;

                        // Hit detected
                        int damage = 1;
                        if (weapon)
                        {
                            damage = weapon.damage;
                            if (armour)
                            {
                                if ((armour.armourSetId != -1) && (armour.armourSetId == weapon.armourSetId))
                                {
                                    damage *= 2;
                                }
                            }
                        }

                        character.DealDamage(damage, impactDir);
                    }
                }
            }
        }
    }

    protected void DealDamage(int nPoints, Vector2 impactDir)
    {
        if (isInvulnerable) return;

        visualDir = impactDir;

        Hit();

        int actualDamage = nPoints;
        if (armour)
        {
            if (weapon)
            {
                if ((weapon.armourSetId != -1) && (weapon.armourSetId == armour.armourSetId))
                {
                    actualDamage = (int)(actualDamage * Mathf.Clamp01((1.0f - armour.mitigation * 1.5f)));
                }
            }
            else
            {
                actualDamage = (int)(actualDamage * (1.0f - armour.mitigation));
            }
        }

        hitPoints -= actualDamage;
        if (hitPoints <= 0)
        {
            Die();
        }
        else
        {
            isInvulnerable = true;
        }
    }

    protected void Die()
    {
        if (isDead) return;

        isDead = true;

        StartCoroutine(DieCR());        
    }

    IEnumerator DieCR()
    {
        animator.SetTrigger("Death");

        DisableAllColliders();

        float elapsed = 0.0f;
        float totalTime = 0.5f;
        float totalRotation = 400.0f;
        float totalScale = 2.0f;

        while (elapsed < totalTime)
        {
            float t = elapsed / totalTime;
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, t * totalRotation);
            transform.localScale = new Vector3(1.0f + t * totalScale, 1.0f + t * totalScale, 1.0f);
            sprite.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Clamp01(1.0f - (elapsed / totalTime)));

            yield return null;

            elapsed += Time.deltaTime;
        }

        Destroy(gameObject);
    }

    public Vector2 GetVisualDir()
    {
        return visualDir;
    }
}
