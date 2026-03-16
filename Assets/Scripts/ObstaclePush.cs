using UnityEngine;

public class ObstaclePush : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Force")]
    public float forceMagnitude;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rigidbody = hit.collider.attachedRigidbody;
        if (rigidbody != null)
        {
            Vector3 forceDirection = rigidbody.transform.position - transform.position;
            forceDirection.y = 0;
            forceDirection = forceDirection.normalized;
            rigidbody.AddForceAtPosition( forceDirection * forceMagnitude, transform.position, ForceMode.Force );
        }
    }
}

