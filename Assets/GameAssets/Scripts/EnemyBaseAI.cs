
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

    [Header("Spotted Indicator")]
    public bool showSpottedBeam = true;
    public Color spottedBeamColor = Color.red;
    public float spottedBeamWidth = 0.08f;

    
    public Transform[] pathPoints;
    public float accuracyDistance = 1f;
    public int currentPathPointID;

    public Transform safePlace;
    public NavMeshAgent agent;
    public Transform player;

    private LineRenderer spottedBeam;
    private Vector3 lastPlayerPosition;
    private float movementTimer;

    //Conditions
    public bool CanSeePlayer()
    {
        Vector3 direction = (player.position - this.transform.position).normalized;
        float distance = Vector3.Distance(this.transform.position, player.position);
        float angle = Vector3.Angle(this.transform.forward, direction);

        RaycastHit hit;
        Vector3 eyePosition = transform.position + Vector3.up;
        if (distance < visionDistance && angle < visionAngle)
        {
            
           
            if (Physics.Raycast(eyePosition, direction, out hit, visionDistance))
            {
               Debug.DrawRay(eyePosition, direction * visionDistance, Color.red);
                if (hit.transform.CompareTag("Player"))
                {  
                    SetSpottedIndicator(true, eyePosition, hit.point);
                    return true;
                }
                else
                {
                    SetSpottedIndicator(false, eyePosition, eyePosition);
                    return false;
                }
            } 
       }
        SetSpottedIndicator(false, eyePosition, eyePosition);
        return false;
    }

    private void SetSpottedIndicator(bool isVisible, Vector3 startPosition, Vector3 endPosition)
    {
        if (!showSpottedBeam)
        {
            if (spottedBeam != null)
            {
                spottedBeam.enabled = false;
            }

            return;
        }

        if (spottedBeam == null)
        {
            spottedBeam = gameObject.AddComponent<LineRenderer>();
            spottedBeam.positionCount = 2;
            spottedBeam.useWorldSpace = true;
            spottedBeam.material = new Material(Shader.Find("Sprites/Default"));
        }

        spottedBeam.enabled = isVisible;

        if (!isVisible)
            return;

        spottedBeam.startColor = spottedBeamColor;
        spottedBeam.endColor = spottedBeamColor;
        spottedBeam.startWidth = spottedBeamWidth;
        spottedBeam.endWidth = spottedBeamWidth * 0.35f;
        spottedBeam.SetPosition(0, startPosition);
        spottedBeam.SetPosition(1, endPosition);
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

    protected void TriggerGameOver(string reason)
    {
        Vector3 direction = transform.position - player.position;
        direction.y = 0f;
        player.rotation = Quaternion.LookRotation(direction);

        GameManager.Instance.EndGame(reason);
    }

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentPathPointID = 0;
        safePlace = GameObject.FindGameObjectWithTag("Safe").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

   
}
