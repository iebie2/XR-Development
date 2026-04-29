using UnityEngine;

public class State : MonoBehaviour
{
    
    [SerializeField]
    float chaseSpeed;
    [SerializeField]
    float runSpeed;
    [SerializeField]
    float patrolSpeed;
    [SerializeField]
    Transform[] pathPoints;
    [SerializeField]
    float accuracyDistance;
    int currentPathPointID;
    public enum STATE
    {
        CHASE, RUN, PATROL
    }
    public STATE currentState;
    public Transform safePlace;
    protected Transform enemy;
    protected Transform player;


    public void CanSeePlayer()
    {
        Vector3 direction = (player.position - enemy.position).normalized;
        float distance = Vector3.Distance(enemy.position, player.position);
        float angle = Vector3.Angle(enemy.forward, direction);

        if (distance < 10f && angle < 60f)
        {
            RaycastHit hit;

            if (Physics.Raycast(enemy.position + Vector3.up, direction, out hit, 10f))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    currentState = STATE.CHASE;
                }
            }
        }
    }
    public void IsPlayerBehind()
    {
        Vector3 direction = enemy.position - player.position;
        float angle = Vector3.Angle(direction, enemy.forward);
        if (direction.magnitude < 2 && angle < 30)
        {
            currentState = STATE.RUN;
            return;
        }
        else
        {

            return;
        }
    }
    public void Patrol()
    {
        if (Vector3.Distance(transform.position, pathPoints[currentPathPointID].position) < accuracyDistance)
        {
            currentPathPointID = (currentPathPointID + 1) % pathPoints.Length;
        }

        Vector3 direction = pathPoints[currentPathPointID].position - transform.position;
        direction.y = 0;
        direction = direction.normalized;

        transform.position += direction * patrolSpeed * Time.deltaTime;
        transform.LookAt(pathPoints[currentPathPointID]);
    }
    public void RunAway()
    {
        Vector3 direction = safePlace.position - transform.position;
        direction.y = 0;
        direction = direction.normalized;

        transform.position += direction * runSpeed * Time.deltaTime;
        transform.LookAt(safePlace);

        if (Vector3.Distance(transform.position, safePlace.position) < 1f)
        {
            if (Vector3.Distance(player.position, transform.position) > 5f)
            {
                currentState = STATE.PATROL;
            }
        }
    }
    public void Chase()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        direction = direction.normalized;

        transform.position += direction * chaseSpeed * Time.deltaTime;
        transform.LookAt(player);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentState = STATE.PATROL;
        enemy = transform;
        safePlace = GameObject.FindGameObjectWithTag("Safe").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentPathPointID = 0;
    }

    // Update is called once per frame
    void Update()
    {
        CanSeePlayer();
        IsPlayerBehind();

        // Execute behavior
        switch (currentState)
        {
            case STATE.PATROL:
                Patrol();
                break;

            case STATE.CHASE:
                Chase();
                break;

            case STATE.RUN:
                RunAway();
                break;
        }
    }
}
