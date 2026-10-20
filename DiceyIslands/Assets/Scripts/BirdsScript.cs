using UnityEngine;

public class BirdFlight : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed at which the bird flies.")]
    public float flightSpeed = 5f;
    
    [Tooltip("How close the bird needs to get to the target before picking a new one.")]
    public float waypointThreshold = 0.5f;

    [Header("Square Boundaries")]
    [Tooltip("Center point of the square area.")]
    public Vector3 areaCenter = Vector3.zero;
    
    [Tooltip("The size of the square (width/length).")]
    public float squareSize = 120f;
    
    [Tooltip("The height (Y axis) at which the bird should fly.")]
    public float flightHeight;

    private Vector3 targetPosition;

    void Start()
    {
        // Pick the first random destination when the game starts
        GetNewRandomDestination();
    }

    void Update()
    {
        // Move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, flightSpeed * Time.deltaTime);

        // Rotate smoothly to face the target
        if (targetPosition != transform.position)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            }
        }

        // Check if the bird has reached the target; if so, pick a new one
        if (Vector3.Distance(transform.position, targetPosition) < waypointThreshold)
        {
            GetNewRandomDestination();
        }
    }

    void GetNewRandomDestination()
    {
        // Calculate random X and Z coordinates within the square bounds
        float halfSize = squareSize / 2f;
        float randomX = Random.Range(areaCenter.x - halfSize, areaCenter.x + halfSize);
        float randomZ = Random.Range(areaCenter.z - halfSize, areaCenter.z + halfSize);
        flightHeight = Random.Range(25f, 35f);

        // Set the target position (keeping Y at the designated flight height)
        targetPosition = new Vector3(randomX, flightHeight, randomZ);
    }

    // Optional: Draw the square area in the Scene view for easy visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 centerWithHeight = new Vector3(areaCenter.x, flightHeight, areaCenter.z);
        Gizmos.DrawWireCube(centerWithHeight, new Vector3(squareSize, 0.1f, squareSize));
    }
}