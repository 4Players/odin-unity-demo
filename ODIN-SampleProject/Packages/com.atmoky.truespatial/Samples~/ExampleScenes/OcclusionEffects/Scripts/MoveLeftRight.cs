using UnityEngine;

public class MoveLeftRight : MonoBehaviour
{
    public float extent = 5.0f;  // The extent of the movement from the starting position
    public float duration = 2.0f; // Duration of the movement

    private Vector3 startPosition;
    private Vector3 endPosition;
    private float elapsedTime = 0.0f;

    void Start()
    {
        // Initialize the start and end positions
        startPosition = transform.position;
        endPosition = startPosition + new Vector3(extent, 0, 0);
    }

    void Update()
    {
        // Update the elapsed time and calculate the normalized time (0 to 1)
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / duration;

        // Loop the movement by using a PingPong function
        t = Mathf.PingPong(t, 1.0f);

        // Apply ease in and ease out using SmoothStep
        t = Mathf.SmoothStep(0.0f, 1.0f, t);

        // Move the object from start to end position and back
        transform.position = Vector3.Lerp(startPosition, endPosition, t);

        // Reset the elapsed time when a full cycle is completed to prevent overflow
        if (elapsedTime >= duration * 2)
        {
            elapsedTime = 0.0f;
        }
    }
}
