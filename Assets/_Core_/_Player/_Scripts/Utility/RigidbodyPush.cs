using UnityEngine;

public class RigidbodyPush : MonoBehaviour
{
	public LayerMask pushLayers;
	public bool canPush;
	[Range(0.5f, 5f)] public float strength = 1.1f;

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (canPush)
		{
			PushRigidBodies(hit);
		}
	}

	private void PushRigidBodies(ControllerColliderHit hit)
	{
		Rigidbody body = hit.collider.attachedRigidbody;
		if (body == null || body.isKinematic) return;

		// Make sure to only push chosen layers
		var bodyLayerMask = 1 << body.gameObject.layer;
		if ((bodyLayerMask & pushLayers.value) == 0) return;
		if (hit.moveDirection.y < -0.3f) return;

        // Calculate push direction from move direction, then apply the push and take strength into account
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);
		body.AddForce(pushDir * strength, ForceMode.Impulse);
	}
}