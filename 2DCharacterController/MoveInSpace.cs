using UnityEngine;

public class MoveInSpace {
	public static void HandleHorizontalMovement(ref Vector2 newPos, float delta, CollisionGetter collisions) {
		if (collisions.Data.right.hit && delta > 0) {
			newPos.x = collisions.Data.right.location.x - collisions.GetHalfScale(Direction.Right);
		} else if (collisions.Data.left.hit && delta < 0) {
			newPos.x = collisions.Data.left.location.x + collisions.GetHalfScale(Direction.Left);
		}
	}
	
	public static void HandleVerticalMovement(ref Vector2 newPos, float delta, CollisionGetter collisions) {
		if (collisions.Data.above.hit && delta > 0) {
			newPos.y = collisions.Data.above.location.y - collisions.GetHalfScale(Direction.Top);
		} else if (collisions.Data.below.hit && delta < 0) {
			newPos.y = collisions.Data.below.location.y + collisions.GetHalfScale(Direction.Bottom);
		}
	}
}