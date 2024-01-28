using UnityEngine;

/// <summary>
/// Character controller 2D class currently only supports square colliders.
/// Expansion to more complicated colliders would be possible, but would probably be
/// better with just basic rigidbody physics.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController2D : MonoBehaviour {

	[SerializeField] private int _numRaycastsPerEdge = 4;
	[SerializeField] private LayerMask _collisionLayer;
	[SerializeField] private float _skinWidth = 0.1f;

	class CollisionData {
		public DirectionalCollision top, bottom, left, right;
		public CollisionData() {top = new(); bottom = new(); left = new(); right = new();}
	}

	class DirectionalCollision {
		public bool hit;
		public Vector3 location;
		public DirectionalCollision() {hit = false; location = Vector3.zero;}
		public DirectionalCollision(bool hit, Vector3 location) {this.hit = hit; this.location = location;}
	}

	enum Direction {Right, Left, Top, Bottom}

	private CollisionData _data = new();
	private BoxCollider2D _bc;

	private void Awake() {
		_bc = GetComponent<BoxCollider2D>();
	}

	public void Move(Vector3 delta) {
		Vector3 newPos = transform.position + delta;
		_data = CheckCollisions(delta);
		Vector3 halfScaleMinusSkin =  transform.localScale / 2 - Vector3.one * _skinWidth;
		if (_data.right.hit) {
			newPos.x = _data.right.location.x - halfScaleMinusSkin.x;
		} else if (_data.left.hit) {
			newPos.x = _data.left.location.x + halfScaleMinusSkin.x;
		}
		if (_data.top.hit) {
			newPos.y = _data.top.location.y - halfScaleMinusSkin.y;
		} else if (_data.bottom.hit) {
			newPos.y = _data.bottom.location.y + halfScaleMinusSkin.y;
		}
		transform.position = newPos;
	}

	private CollisionData CheckCollisions(Vector3 delta) {
		CollisionData data = new();
		if (delta.x > 0)
			data.right = CheckCollision(Direction.Right, delta);
		if (delta.x < 0)
			data.left = CheckCollision(Direction.Left, delta);
		if (delta.y > 0)
			data.top =  CheckCollision(Direction.Top, delta);
		if (delta.y < 0)
			data.bottom = CheckCollision(Direction.Bottom, delta);
		return data;
	}

	private DirectionalCollision CheckCollision(Direction direction, Vector3 delta) {
		Vector3 originA, originB;
		Vector3 center = transform.position;
		Vector3 directionToUse = (direction == Direction.Right || direction == Direction.Left) ? Vector3.right : Vector3.up;
		delta.Scale(directionToUse);
		float distX = transform.localScale.x/2;
		float distY = transform.localScale.y/2;
		float inwardOffset = 0.01f; // Arbitrary value, avoids detecting other direction collisions when directly pressed into a surface
		switch(direction) {
			case Direction.Right:
				originA = center + new Vector3(distX - _skinWidth, distY - _skinWidth - inwardOffset);
				originB = center + new Vector3(distX - _skinWidth, -distY + _skinWidth + inwardOffset);
				break;
			case Direction.Left:
				originA = center + new Vector3(-distX + _skinWidth, distY - _skinWidth - inwardOffset);
				originB = center + new Vector3(-distX + _skinWidth, -distY + _skinWidth + inwardOffset);
				break;
			case Direction.Top:
				originA = center + new Vector3(distX - _skinWidth - inwardOffset, distY - _skinWidth);
				originB = center + new Vector3(-distX + _skinWidth + inwardOffset, distY - _skinWidth);
				break;
			case Direction.Bottom:
				originA = center + new Vector3(distX - _skinWidth - inwardOffset, -distY + _skinWidth);
				originB = center + new Vector3(-distX + _skinWidth + inwardOffset, -distY + _skinWidth);
				break;
			default:
				originA = Vector3.zero;
				originB = Vector3.zero;
				break;
		}
		Vector3[] startingPositions = new Vector3[_numRaycastsPerEdge];
		for (int i = 0; i < _numRaycastsPerEdge; i++) {
			float t = i / (_numRaycastsPerEdge - 1.0f);
			startingPositions[i] = Vector3.Lerp(originB, originA, t);
		}
		foreach (Vector3 startingPos in startingPositions) {
			RaycastHit2D hit = Physics2D.Linecast(startingPos, startingPos+delta, _collisionLayer);
			if (hit) {
				return new(true, hit.point);
			}
		}
		return new();
	}

	public bool IsGrounded => _data.bottom.hit;
	public bool TopCollision => _data.top.hit;
}