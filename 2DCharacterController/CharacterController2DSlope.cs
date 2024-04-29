using UnityEngine;

public class CharacterController2DSlope : MonoBehaviour, ICharacterControllerExtension {
    [SerializeField] private LayerMask _slopeLayer = default;
    private CollisionGetter _collisions;
    [SerializeField] private float _slopeMax = 45f;

    private void Awake() {
        _collisions = GetComponent<CollisionGetter>();
    }

    public void AdjustMovement(ref Vector2 newPos, Vector2 delta) {
        if (delta.x == 0) return;
        _collisions.Recalculate(delta, _slopeLayer);
        Collision slope =  delta.x > 0 ? _collisions.Data.right : _collisions.Data.left;
        if (!slope) return;
        float angle = Vector2.Angle(slope.normal.normalized, Vector2.up);
        if (angle > _slopeMax) return;
        float verticalOffset = Mathf.Tan(Mathf.Deg2Rad * angle) * Mathf.Abs(delta.x);
        newPos += new Vector2(delta.x, verticalOffset);
    }
}