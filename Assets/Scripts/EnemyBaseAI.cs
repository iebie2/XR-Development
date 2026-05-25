
using UnityEngine;
using UnityEngine.AI;

public class EnemyBaseAI : MonoBehaviour
{
    public float catchDistance = 1.5f;
    public float catchTime = 2f;
    public float catchTimer = 0f;


    public float patrolSpeed;
    public float chaseSpeed;
    public float runSpeed;

    
    public float visionDistance;
    public float visionAngle;

    
    public Transform[] pathPoints;
    public float accuracyDistance = 1f;
    public int currentPathPointID;

    public Transform safePlace;
    public NavMeshAgent agent;
    public Transform player;

    private Vector3 lastPlayerPosition;
    private float movementTimer;

    //Conditions
    public bool CanSeePlayer()
    {
        Vector3 direction = (player.position - this.transform.position).normalized;
        float distance = Vector3.Distance(this.transform.position, player.position);
        float angle = Vector3.Angle(this.transform.forward, direction);
        
        if (distance < visionDistance && angle < visionAngle)
        {
            RaycastHit hit;
            //Vector3 eyePosition = transform.position + Vector3.up * 1.5f;

            if (Physics.Raycast(transform.position, direction, out hit, visionDistance))
            {
               
                if (hit.transform.CompareTag("Player"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }
    public bool IsPlayerBehind()
    {
        Vector3 direction = this.transform.position - player.position;
        float angle = Vector3.Angle(direction, this.transform.forward);
        if (direction.magnitude < 2 && angle < 30)
        {
            return true;
        }
        else
        {

            return false;
        }
    }
    public bool IsSafe()
    {
        if (Vector3.Distance(this.transform.position, safePlace.position) < 1f)
        {
            if (Vector3.Distance(player.position, this.transform.position) > 5f)
            {
                return true;
            }
        } return false;
    }
    public bool IsPlayerMoving()
    {
        float distanceMoved =
       Vector3.Distance(player.position, lastPlayerPosition);

        if (distanceMoved > 0.001f)
        {
            movementTimer = 0.2f; // movement grace period
        }
        else
        {
            movementTimer -= Time.deltaTime;
        }

        lastPlayerPosition = player.position;

        return movementTimer > 0f;

        /*bool moving = Vector3.Distance(player.position, lastPlayerPosition) > 0.001f;

        lastPlayerPosition = player.position;

        return moving;*/
        /*Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("Player has no Rigidbody!");
            return false;
        }

        return rb.velocity.sqrMagnitude > 0.01f;*/
        // return player.GetComponent<Rigidbody>().linearVelocity.sqrMagnitude > 0.01f;
    }
    public bool CaughtPlayer()
    {
        float distance =
        Vector3.Distance(transform.position,
                         player.position);

        if (distance <= catchDistance)
        {
            catchTimer += Time.deltaTime;

            if (catchTimer >= catchTime)
            {
                return true;
            }
        }
        else
        {
            catchTimer = 0f;
        }

        return false;
    }

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentPathPointID = 0;
        safePlace = GameObject.FindGameObjectWithTag("Safe").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

   
}
