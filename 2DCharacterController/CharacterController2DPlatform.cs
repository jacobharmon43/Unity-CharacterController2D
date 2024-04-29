using UnityEngine;

public class CharacterController2DPlatform : MonoBehaviour, ICharacterControllerExtension {
    [SerializeField] private LayerMask _platformMask = default;
    private CollisionGetter _collisions;
    
    private void Awake() {
        _collisions = GetComponent<CollisionGetter>();
    }

    public void AdjustMovement(ref Vector2 newPos, Vector2 delta) {
        _collisions.Recaclulate(delta, _platformMask);
        HandleVerticalMovement(ref newPos, delta.y);
    }

    private void HandleVerticalMovement(ref Vector2 newPos, float delta) {
        if (_collisions.Data.below && delta < 0) {
			newPos.y = _collisions.Data.below.location.y + _collisions.GetHalfScale(Direction.Bottom);
        }
    }
}