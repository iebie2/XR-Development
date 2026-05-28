using UnityEngine;
using static State;

public class SkeletonAI : EnemyBaseAI
{
    //Variables
    State currentState;
    private float lostSightTimer = 0f;
    public float lostSightDuration = 5f;
    
    //Methods
    void ChangeState(State newState)
    {
        //currentState.Exit();

        currentState = newState;

        //currentState.Enter();
    }

    //Conditions
    public bool LostPlayer()
    {
        if (CanSeePlayer())
        {
            lostSightTimer = 0f;
            return false;
        }
        else
        {
            lostSightTimer += Time.deltaTime;

            if (lostSightTimer >= lostSightDuration)
            {
                lostSightTimer = 0f;
                return true;
            }
        }

        return false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void  Start()
    {
        base.Start();
        currentState = new Patrol(this, player, agent);
        //currentState.Enter();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(currentState);
        currentState.Updating();

        if(currentState is Patrol)
        {
           
            if (CanSeePlayer() && IsPlayerMoving())
            {
                ChangeState(new Chase(this, player, agent));
            }
            else if (IsPlayerBehind())
            {
                ChangeState(new Run(this, player, agent));
            }
        } 
        else if (currentState is Chase)
        {
            if (LostPlayer()){
                ChangeState(new Patrol(this, player, agent));
            }
            else if (CaughtPlayer())
            {
                ChangeState(new Caught(this, player, agent));

                Vector3 direction =transform.position - player.position;
                direction.y = 0f;
                player.rotation =Quaternion.LookRotation(direction);
                
                Debug.Log("Game over");
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
