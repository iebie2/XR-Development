using UnityEngine;
using static State;

public class EyeAI : EnemyBaseAI
{
    //Variables
    State currentState;
    private float seenTimer = 0f;

    public float seenDuration = 5f;

    //Methods
    void ChangeState(State newState)
    {

        //currentState.Exit();

        currentState = newState;

        //currentState.Enter();
    }
    public bool HasDirectSight()
    {
        Vector3 direction =
            (player.position - transform.position).normalized;

        RaycastHit hit;

        Vector3 eyePosition =
            transform.position + Vector3.up * 1.5f;

        if (Physics.Raycast(
            eyePosition,
            direction,
            out hit))
        {
            return hit.transform.CompareTag("Player");
        }

        return false;
    }
    public bool SeenPlayerTooLong()
    {
        if (HasDirectSight())
        {
            seenTimer += Time.deltaTime;

            if (seenTimer >= seenDuration)
            {
                return true;
            }
        }
        else
        {
            seenTimer = 0f;
        }

        return false;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
        currentState = new Patrol(this, player, agent);
        //currentState.Enter();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(currentState);
        currentState.Updating();
        if (currentState is Patrol)
        {
            if (CanSeePlayer())
            {
                ChangeState(new Caught(this, player, agent));
            }
            else if (IsPlayerBehind())
            {
                ChangeState(new Run(this, player, agent));
            }
        }
      
        else if (currentState is Run)
        {
            if (IsSafe())
            {
                ChangeState(new Patrol(this, player, agent));
            }
        }
        else if (currentState is Caught)
        {
            if (SeenPlayerTooLong())
            {
                ChangeState(new Caught(this, player, agent));

                Vector3 direction = transform.position - player.position;
                direction.y = 0f;
                player.rotation = Quaternion.LookRotation(direction);

                Debug.Log("Game over");
            }
            else if (!CanSeePlayer())
            {
                ChangeState(new Patrol(this, player, agent));

            }
        }
    }
}
