using UnityEngine;

public class StormMover : MonoBehaviour
{
    public Transform startPoint;
    public Transform targetPoint;

    public float moveSpeed = 5f;
    public float shrinkSpeed = 2f;
    public float stopDistance = 0.1f;
    public float targetRadius = 5f;

	[SerializeField]
    private bool moving = false;

    void Start()
    {
        transform.position = startPoint.position;
    }
	
    void Update()
    {
        if (moving)
        {
            // Move toward the target point
            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

   
            // Stop moving when close enough
            if (Vector3.Distance(transform.position, targetPoint.position) <= stopDistance)
            {
                moving = false;
            }
        }
		
		if(Input.GetKeyDown(KeyCode.M)){
			moving = true;
		}
    }

    // Call this from another script or button to start the next phase
    public void BeginMovement(Transform newTargetPoint, float newTargetRadius)
    {
        targetPoint = newTargetPoint;
        targetRadius = newTargetRadius;
        moving = true;
    }
}
