using UnityEngine;
using UnityEngine.AI;

public class StalkerAI : EnemyBaseAI
{
    //Variables
    State currentState;
    private bool hasSpottedPlayer = false;

    //Methods
    void ChangeState(State newState)
    {
        //currentState.Exit();

        currentState = newState;

        //currentState.Enter();
    }

    //Conditions
    public bool IsPlayerLookingAtMe()
    {
        Vector3 directionToEnemy =
            transform.position - player.position;

        float angle =
            Vector3.Angle(player.forward, directionToEnemy);

        return angle < 40f;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
        
        currentState = new Patrol(this, player, agent);
        currentState.Enter();
    }

    // Update is called once per frame
    void Update()
    {
        
        
        currentState.Updating();
        if (currentState is Patrol)
        {
            if (CanSeePlayer() && !IsPlayerLookingAtMe())
            {
                ChangeState(new Chase(this, player, agent));
            }
        }
        else if (currentState is Freeze)
        {
            if (CanSeePlayer() && !IsPlayerLookingAtMe())
            {
                ChangeState(new Chase(this, player, agent));
            }
            else if (IsPlayerBehind())
            {
                ChangeState(new Run(this, player, agent));
            }else if (!CanSeePlayer())
            {
                ChangeState(new Patrol(this, player, agent));
            }

        }
        else if (currentState is Chase)
        {
            if (CaughtPlayer())
            {
                ChangeState(new Caught(this, player, agent));
                TriggerGameOver("Stalker caught the player");
            }
            else if (!CanSeePlayer())
            {

                ChangeState(new Patrol(this, player, agent));
            }
            else if (IsPlayerLookingAtMe())
            {
                ChangeState(new Freeze(this, player, agent));

            }

        }
        else if (currentState is Run)
        {
            if (IsSafe())
            {
                ChangeState(new Patrol(this, player, agent));
            }
        }
        
    }
}
