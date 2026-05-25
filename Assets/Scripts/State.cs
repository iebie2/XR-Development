using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class State 
{

    public EnemyBaseAI enemyAI;
    
    //protected Animator anim;
    protected Transform enemy;
    protected Transform player;
    
    protected NavMeshAgent agent;
    



    public State(EnemyBaseAI _enemyAI, Transform _player, NavMeshAgent _agent/*, Animator _anim*/)
    {
        enemyAI = _enemyAI;
        enemy = _enemyAI.transform;
        player = _player;
        agent = _agent;
        //anim = _anim;
        
    }


    //Progress
    public virtual void Enter() { }

    public virtual void Updating() { }
    public virtual void Exit() { }
   

    
}
    

//States
public class Patrol : State
    {
    public Patrol(EnemyBaseAI _enemy, Transform _player, NavMeshAgent _agent) 
        : base(_enemy, _player, _agent)
        {
        agent.speed = enemyAI.patrolSpeed;
        agent.isStopped = false;
    }
    public override void Updating()
    {
        if (enemyAI.pathPoints.Length == 0) 
            return;
        if (Vector3.Distance(enemy.position, enemyAI.pathPoints[enemyAI.currentPathPointID].position) < enemyAI.accuracyDistance)
        {
            enemyAI.currentPathPointID = Random.Range(0, enemyAI.pathPoints.Length);
        }

        agent.SetDestination(enemyAI.pathPoints[enemyAI.currentPathPointID].position);
        
    }

}
public class Chase : State
{
    public Chase(EnemyBaseAI _enemy, Transform _player, NavMeshAgent _agent)
        : base(_enemy, _player, _agent)
    {
        agent.speed = enemyAI.chaseSpeed;
        agent.isStopped = false;
    }


    public override void Updating()
    {
        Debug.Log("enters");
        agent.SetDestination(player.position);
       
    }
}
public class Run : State
{
    public Run(EnemyBaseAI _enemy, Transform _player, NavMeshAgent _agent)
       : base(_enemy, _player, _agent)
    { 
        agent.isStopped = false;
        agent.speed = enemyAI.runSpeed;
    }

    public override void Enter()
    {
        
        base.Enter();
    }

    public override void Updating()
    {
        
        agent.SetDestination(enemyAI.safePlace.transform.position);

        }
  
}
public class Caught : State {
    public Caught(EnemyBaseAI _enemy, Transform _player, NavMeshAgent _agent)
            : base(_enemy, _player, _agent)
    {
        agent.isStopped = true;
    }
    public override void Updating()

    {
        Vector3 direction = player.position - enemy.position;

        direction.y = 0f;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        enemy.rotation = Quaternion.Slerp(
            enemy.rotation,
            targetRotation,
            Time.deltaTime * 5f);

    }

}


