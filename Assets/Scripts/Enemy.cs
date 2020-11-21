using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    public enum State { Patrol, Follow };

    [Header("Enemy AI")]
    public State       state = State.Patrol;
    public float       detectionRadius = 50.0f;
    public float       attackDistance = 10.0f;
    public float       timeBeforeAttack = 0.5f;
    public Transform[] waypoints;

    Vector2 targetPoint;

    // Patrol mode
    int waypointIndex = 0;

    // Follow mode
    Character currentTarget;
    bool      hasLOS;
    Vector3   lastPos;
    float     timeStuck;
    Vector3   initialSpawnPosition;
    float     timerBeforeAttack;

    void Start()
    {
        targetPoint = waypoints[waypointIndex].position;
        lastPos = transform.position;
        initialSpawnPosition = transform.position;
    }

    void FixedUpdate()
    {
        Vector2 currentPos = transform.position;

        if (state == State.Patrol)
        {
            float distanceToTarget = Vector2.Distance(currentPos, targetPoint);
            if (distanceToTarget < 5.0f)
            {
                NextWaypoint();
            }

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, LayerMask.GetMask("Character"));
            Character    target = null;
            float        minDist = float.MaxValue;

            foreach (var collider in colliders)
            {
                Character character = collider.GetComponent<Character>();
                if (character)
                {
                    if ((character.faction != faction) && (HasLOS(character)))
                    {
                        float dist = Vector3.Distance(character.transform.position, transform.position);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            target = character;
                        }
                    }
                }
            }

            if (target)
            {
                state = State.Follow;
                currentTarget = target;
            }
        }
        else if (state == State.Follow)
        {
            if (currentTarget == null)
            {
                state = State.Patrol;
            }
            else
            {
                if (HasLOS(currentTarget))
                {
                    targetPoint = currentTarget.transform.position;
                }
                else
                {
                    if (Vector3.Distance(targetPoint, currentPos) < 5.0f)
                    {
                        state = State.Patrol;
                    }
                }

                float distanceToOpponent = Vector3.Distance(currentTarget.transform.position, transform.position);
                if (distanceToOpponent < attackDistance)
                {
                    if (timerBeforeAttack > timeBeforeAttack)
                    {
                        visualDir = (currentTarget.transform.position - transform.position).normalized;
                        Attack();
                        timerBeforeAttack = -1.0f;
                    }
                    else
                    {
                        timerBeforeAttack += Time.fixedDeltaTime;
                    }
                }
                else if (distanceToOpponent > attackDistance * 2.0f)
                {
                    timerBeforeAttack = 0.0f;
                }
            }
        }

        Vector2 moveDir = (targetPoint - currentPos).normalized;
        MoveTo(moveDir);

        if (Vector3.Distance(lastPos, transform.position) < 1.0f)
        {
            timeStuck += Time.fixedDeltaTime;

            if (timeStuck > 5.0f)
            {
                if (state == State.Patrol)
                {
                    // Find visible waypoint
                    bool foundViableWaypoint = false;
                    for (int i = 0; i < waypoints.Length; i++)
                    {
                        if (HasLOS(waypoints[i].position))
                        {
                            NextWaypoint(i);
                            foundViableWaypoint = true;
                            break;
                        }
                    }                    
                    if (!foundViableWaypoint)
                    {
                        // Move back to initial position
                        transform.position = initialSpawnPosition;
                    }
                }
                else Die();
            }
        }
        else
        {
            timeStuck = 0.0f;
            lastPos = transform.position;
        }
    }

    void NextWaypoint()
    {
        NextWaypoint((waypointIndex + 1) % (waypoints.Length));
    }

    void NextWaypoint(int i)
    {
        waypointIndex = i;
        targetPoint = waypoints[waypointIndex].position;
    }

    bool HasLOS(Vector2 targetPosition)
    {
        Vector2 currentPos = transform.position;
        Vector2 dir = targetPosition - currentPos;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir.normalized, dir.magnitude, LayerMask.GetMask("Default", "Character"));

        foreach (var hit in hits)
        {
            Character character = hit.collider.GetComponentInParent<Character>();

            if (character == null)
            {
                return false;
            }
        }

        return true;
    }

    bool HasLOS(Character target)
    {
        Vector2 dir = target.transform.position - transform.position;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir.normalized, dir.magnitude, LayerMask.GetMask("Default", "Character"));

        foreach (var hit in hits)
        {
            Character character = hit.collider.GetComponentInParent<Character>();

            if (character == null)
            {
                return false;
            }
            if (character == target)
            {
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPoint, 2.0f);

        if (state == State.Patrol)
        {
            Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 0.25f);
        }
        else if (state == State.Follow)
        {
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.25f);
        }
        Gizmos.DrawSphere(transform.position, detectionRadius);

        Gizmos.color = new Color(1.0f, 0.5f, 0.0f, 0.25f);
        Gizmos.DrawSphere(transform.position, attackDistance);
    }
}
