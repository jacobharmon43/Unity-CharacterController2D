using UnityEngine;
using System.Collections.Generic;
using System;

public class CollisionData {
	public Collision above, below, left, right;
	public CollisionData() {above = new(); below = new(); left = new(); right = new();}
	public override string ToString() {
    return $"Top: {above}, Bottom: {below}, Left: {left}, Right: {right}";
  }
}

public class Collision {
	public bool hit;
	public Vector3 location;
	public Transform obj;
  public Vector2 normal;
	public Collision() {hit = false; location = Vector3.zero; normal = Vector2.zero;}
	public Collision(bool hit, Vector3 location, Transform t, Vector2 slope) { this.hit = hit; this.location = location; this.obj = t; this.normal = slope; }
	public override string ToString() {
    return $"Hit: {hit}, Location: {location}, obj: {(obj ? obj.name : "None")}, normal: {normal}";
  }
	public static implicit operator bool(Collision obj) {
		return obj.hit;
	}
}

public enum Origin {TopRight = 0, BottomRight = 1, TopLeft = 2, BottomLeft = 3}
public enum Direction {Right, Left, Top, Bottom}

[RequireComponent(typeof(BoxCollider2D))]
public class CollisionGetter : MonoBehaviour {
  public CollisionData Data { get; private set; }
	
  [SerializeField] private float _skinWidth = 0.01f;
  [SerializeField] private int _rayCountHorizontal = 4;
  [SerializeField] private int _rayCountVertical = 4;
  private Vector2[] _origins = new Vector2[4];
  private BoxCollider2D _bc;
  private const float ARBITRARY_INSET = 0.01f;

  private Dictionary<Direction, Tuple<Origin, Origin>> _directionToOrigins = new(){
    {Direction.Right, Tuple.Create(Origin.BottomRight, Origin.TopRight)},
    {Direction.Left, Tuple.Create(Origin.BottomLeft, Origin.TopLeft)},
    {Direction.Top, Tuple.Create(Origin.TopLeft, Origin.TopRight)},
    {Direction.Bottom, Tuple.Create(Origin.BottomLeft,Origin.BottomRight)}
  };

  private Dictionary<Direction, Vector2> _directionToVector2 =  new() {
    {Direction.Right, Vector2.right},
    {Direction.Left, Vector2.left},
    {Direction.Top, Vector2.up},
    {Direction.Bottom, Vector2.down},
  };

  private int RayCount(Direction direction) => (direction == Direction.Left || direction == Direction.Right) ? _rayCountHorizontal : _rayCountVertical;

  public float GetHalfScale(Direction direction) {
    return direction switch {
      Direction.Left or Direction.Right => _bc.offset.x + _bc.size.x * transform.localScale.x / 2 - _skinWidth,
      Direction.Top or Direction.Bottom => _bc.offset.y + _bc.size.y * transform.localScale.y / 2 - _skinWidth,
      _ => 0,
    };
  } 

  public void Recalculate(Vector2 delta, LayerMask mask) {
    Data = new CollisionData(){
      above = CheckCollision(Direction.Top, delta, mask),
      below = CheckCollision(Direction.Bottom, delta, mask),
      left = CheckCollision(Direction.Left, delta, mask),
      right = CheckCollision(Direction.Right, delta, mask)
    };
  }

  public Collision CheckDirection(Direction direction, float deltaInDirection, LayerMask mask) => CheckCollision(Direction.Bottom, _directionToVector2[direction] * deltaInDirection, mask);

  private void Awake() {
    _bc = GetComponent<BoxCollider2D>();
  }

  private void OnValidate() {
    CalculateOrigins();
  }

	private void CalculateOrigins() {
    if (!_bc) {
      _bc = GetComponent<BoxCollider2D>();
    }
    Vector2 halfSize = 0.5f * _bc.size * transform.localScale - _skinWidth * Vector2.one;
    Vector2 scaledOffset = new Vector2(_bc.offset.x * transform.localScale.x, _bc.offset.y * transform.localScale.y);
		_origins[(int)Origin.TopRight] = halfSize + scaledOffset;
		_origins[(int)Origin.TopLeft] = new Vector2(-halfSize.x, halfSize.y) + scaledOffset;
		_origins[(int)Origin.BottomRight] = new Vector2(halfSize.x, -halfSize.y) + scaledOffset;
		_origins[(int)Origin.BottomLeft] = -halfSize + scaledOffset;
	}
	
	private Collision CheckCollision(Direction direction, Vector2 delta, LayerMask collisionLayer) {
    Tuple<Origin,Origin> starts = _directionToOrigins[direction];
    Vector2 originStart = (Vector2)transform.position + _origins[(int)starts.Item1] + ARBITRARY_INSET * ((direction == Direction.Left || direction == Direction.Right) ? Vector2.up : Vector2.right);
    Vector2 originEnd = (Vector2)transform.position + _origins[(int)starts.Item2] + ARBITRARY_INSET * ((direction == Direction.Left || direction == Direction.Right) ? Vector2.down : Vector2.left);
    delta.Scale(_directionToVector2[direction]);
		LayerMask layers = collisionLayer;

    int rayCount = RayCount(direction);
    Vector2[] startingPositions = new Vector2[rayCount];
		for (int i = 0; i < rayCount; i++) {
			float t = i / (rayCount - 1.0f);
			startingPositions[i] = Vector3.Lerp(originStart, originEnd, t);
		}

		foreach (Vector2 startingPos in startingPositions) {
			RaycastHit2D hit = Physics2D.Linecast(startingPos, startingPos + delta, layers);
			if (hit) {
        Vector2 normal = Physics2D.Raycast(startingPos - _directionToVector2[direction] * ARBITRARY_INSET, _directionToVector2[direction], ARBITRARY_INSET * 2, layers).normal;
				return new(true, hit.point, hit.transform, normal);
      }
		}
		return new();
	}
#region GIZMO
  private void OnDrawGizmos() {
    if (!_bc) {
      _bc = GetComponent<BoxCollider2D>();
    }
    float largeCircle = 0.005f;
    // Output positions
    Gizmos.color = Color.green;
    Gizmos.DrawSphere((Vector2)transform.position + _origins[(int)Origin.TopRight], largeCircle);
    Gizmos.DrawSphere((Vector2)transform.position + _origins[(int)Origin.TopLeft], largeCircle);
    Gizmos.DrawSphere((Vector2)transform.position + _origins[(int)Origin.BottomRight], largeCircle);
    Gizmos.DrawSphere((Vector2)transform.position + _origins[(int)Origin.BottomLeft], largeCircle);

    // Draw Inner Collider Considering Skin Width
    Gizmos.color = Color.red;
    Gizmos.DrawLine((Vector2)transform.position + _origins[(int)Origin.TopLeft], (Vector2)transform.position + _origins[(int)Origin.TopRight]);
    Gizmos.DrawLine((Vector2)transform.position + _origins[(int)Origin.BottomLeft], (Vector2)transform.position + _origins[(int)Origin.TopLeft]);
    Gizmos.DrawLine((Vector2)transform.position + _origins[(int)Origin.BottomLeft], (Vector2)transform.position + _origins[(int)Origin.BottomRight]);
    Gizmos.DrawLine((Vector2)transform.position + _origins[(int)Origin.BottomRight], (Vector2)transform.position + _origins[(int)Origin.TopRight]);

    // Draw default ray size
    Gizmos.color = Color.cyan;
    float defaultRaySize = 2 * _skinWidth;
    void DrawLinesInDirection(Direction direction, Vector2 delta) {
      Tuple<Origin,Origin> starts = _directionToOrigins[direction];
      Vector2 originStart = (Vector2)transform.position + _origins[(int)starts.Item1] + ARBITRARY_INSET * ((direction == Direction.Left || direction == Direction.Right) ? Vector2.up : Vector2.right);
      Vector2 originEnd = (Vector2)transform.position + _origins[(int)starts.Item2] + ARBITRARY_INSET * ((direction == Direction.Left || direction == Direction.Right) ? Vector2.down : Vector2.left);
      delta.Scale(_directionToVector2[direction]);
      int rayCount = RayCount(direction);

      Vector2[] startingPositions = new Vector2[rayCount];
      for (int i = 0; i < rayCount; i++) {
        float t = i / (rayCount - 1.0f);
        startingPositions[i] = Vector3.Lerp(originStart, originEnd, t);
      }

      foreach (Vector2 startingPos in startingPositions) {
        Gizmos.DrawLine(startingPos, startingPos + delta * (1+_skinWidth));
      }
    }
    DrawLinesInDirection(Direction.Right, Vector2.one * defaultRaySize);
    DrawLinesInDirection(Direction.Left, Vector2.one * defaultRaySize);
    DrawLinesInDirection(Direction.Top, Vector2.one * defaultRaySize);
    DrawLinesInDirection(Direction.Bottom, Vector2.one * defaultRaySize);
  }
#endregion
}