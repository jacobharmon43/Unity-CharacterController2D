using UnityEngine;
using System.Collections.Generic;

public class CollisionData {
	public DirectionalCollision top, bottom, left, right;
	public CollisionData() {top = new(); bottom = new(); left = new(); right = new();}
	public override string ToString() {
    return $"Top: {top}, Bottom: {bottom}, Left: {left}, Right: {right}";
  }
}

public class DirectionalCollision {
	public bool hit;
	public Vector3 location;
	public Transform obj;
	public DirectionalCollision() {hit = false; location = Vector3.zero;}
	public DirectionalCollision(bool hit, Vector3 location, Transform t) { this.hit = hit; this.location = location; this.obj = t; }
	public override string ToString() {
    return $"Hit: {hit}, Location: {location}";
  }
	public static implicit operator bool(DirectionalCollision obj) {
		return obj.hit;
	}
}

public enum Direction {Right, Left, Top, Bottom}

/// <summary>
/// Character controller 2D class currently only supports square colliders.
/// Expansion to more complicated colliders would be possible, but would probably be
/// better with just basic rigidbody physics.
/// </summary>
public class CharacterController2D : MonoBehaviour {

	[SerializeField] private int _numRaycastsPerEdge = 4;
	[SerializeField] private LayerMask _collisionLayer = LayerMask.GetMask();
	[SerializeField] private LayerMask _platformLayer = LayerMask.GetMask();
	[SerializeField] private float _skinWidth = 0.1f;

	private CollisionData _data = new();
	private BoxCollider2D _bc;
	Vector3 halfScaleMinusSkin;


	private void Awake() {
		_bc = GetComponent<BoxCollider2D>();
		halfScaleMinusSkin =  transform.localScale / 2 - Vector3.one * _skinWidth;
	}

	public void Move(Vector3 delta) {
		Vector3 newPos = transform.position + delta;
		_data = CheckCollisions(delta);
		HandleVerticalMovement(ref newPos, delta.y);
		HandleHorizontalMovement(ref newPos, delta.x);
		transform.position = newPos;
	}

	private void HandleHorizontalMovement(ref Vector3 newPos, float delta) {
		if (_data.right.hit && delta > 0) {
			newPos.x = _data.right.location.x - halfScaleMinusSkin.x;
		} else if (_data.left.hit && delta < 0) {
			newPos.x = _data.left.location.x + halfScaleMinusSkin.x;
		}
	}

	private void HandleVerticalMovement(ref Vector3 newPos, float delta) {
		if (_data.top.hit && delta > 0) {
			newPos.y = _data.top.location.y - halfScaleMinusSkin.y;
		} else if (_data.bottom.hit && delta < 0) {
			newPos.y = _data.bottom.location.y + halfScaleMinusSkin.y;
		}
	}

	private CollisionData CheckCollisions(Vector3 delta) {
        CollisionData data = new()
        {
            right = CheckCollision(Direction.Right, delta),
            left = CheckCollision(Direction.Left, delta),
            top = CheckCollision(Direction.Top, delta),
            bottom = CheckCollision(Direction.Bottom, delta)
        };
        return data;
	}

	private DirectionalCollision CheckCollision(Direction direction, Vector3 delta) {
		Vector3 originA, originB;
		Vector3 center = transform.position;
		Vector3 directionToUse = (direction == Direction.Right || direction == Direction.Left) ? Vector3.right : Vector3.up;
		delta.Scale(directionToUse);
		float distX = transform.localScale.x/2;
		float distY = transform.localScale.y/2;
		float inwardOffset = 0.05f; // Arbitrary offset value, squeezes the between points for the raycasts in ever so slightly to avoid corner cases when hugging surfaces.
		LayerMask layers = _collisionLayer;
		if (direction == Direction.Bottom) {
			layers |= _platformLayer;
		}
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
			startingPositions[i] = Vector3.Lerp(originA, originB, t);
		}
		foreach (Vector3 startingPos in startingPositions) {
			RaycastHit2D hit = Physics2D.Linecast(startingPos, startingPos+delta, layers);
			if (hit) {
				return new(true, hit.point, hit.transform);
			}
		}
		return new();
	}

	public DirectionalCollision IsGrounded => _data.bottom;
	public DirectionalCollision TopCollision => _data.top;
	public DirectionalCollision LeftWall => _data.left;
	public DirectionalCollision RightWall => _data.right;
}
