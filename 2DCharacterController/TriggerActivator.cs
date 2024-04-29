using UnityEngine;
using System.Linq;



public class TriggerActivator : MonoBehaviour {
    [HideInInspector] public const int MAX_TRIGGER_COUNT = 10;
    private BoxCollider2D _bc;

    private Collider2D[] _thisFrameTriggers;
    private Collider2D[] _lastFrameTriggers;
    private int _lastFrameTriggersCount;

    private void Awake() {
        _bc = GetComponent<BoxCollider2D>();
    }

    private void Update() {
        if (!enabled) return;
        _thisFrameTriggers = new Collider2D[MAX_TRIGGER_COUNT];
        int thisFrameTriggersCount = Physics2D.OverlapBoxNonAlloc(transform.position, new Vector2(transform.localScale.x * _bc.size.x, transform.localScale.y * _bc.size.y), 0, _thisFrameTriggers);

        for (int i = 0; i < thisFrameTriggersCount; i++) {
            Collider2D collision = _thisFrameTriggers[i];
            if (!collision.isTrigger) continue;
            if (_lastFrameTriggers.Contains(collision)) {
                collision.gameObject.SendMessage("OnTriggerStay", gameObject, SendMessageOptions.DontRequireReceiver);
            } else {
                collision.gameObject.SendMessage("OnTriggerEnter", gameObject, SendMessageOptions.DontRequireReceiver);
            }
        }
        for (int i = 0; i < _lastFrameTriggersCount; i++) {
            Collider2D collisionLastFrame = _lastFrameTriggers[i];
            if (collisionLastFrame == null) continue;
            if (!_thisFrameTriggers.Contains(collisionLastFrame)) {
                collisionLastFrame.gameObject.SendMessage("OnTriggerExit", gameObject, SendMessageOptions.DontRequireReceiver);
            }
        }
        _lastFrameTriggers = new Collider2D[MAX_TRIGGER_COUNT];
        _thisFrameTriggers.CopyTo(_lastFrameTriggers, 0);
        _lastFrameTriggersCount = thisFrameTriggersCount;
    }
}