using UnityEngine;

public enum CharacterControllerMovementType {MoveInSpace, MoveOnSlope}

[RequireComponent(typeof(CollisionGetter), typeof(BoxCollider2D))]
public class CharacterController2D : MonoBehaviour {
	[SerializeField] private LayerMask _collisionLayer = default;

	private CollisionGetter _collisions;

	public CollisionData Collisions => _collisions.Data;
	

	private void Awake() {
		_collisions = GetComponent<CollisionGetter>();
	}
	
	public void Move(Vector2 delta) {
		Vector2 newPos = (Vector2)transform.position + delta;
		_collisions.Recaclulate(delta, _collisionLayer);
		MoveInSpace.HandleHorizontalMovement(ref newPos, delta.x, _collisions);
		MoveInSpace.HandleVerticalMovement(ref newPos, delta.y, _collisions);
		foreach (var controllerExtension in GetComponents<ICharacterControllerExtension>()) {
			controllerExtension.AdjustMovement(ref newPos, delta);
		}
		transform.position = newPos;
	}
}