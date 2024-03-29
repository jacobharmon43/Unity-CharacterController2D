# Unity2DCharacterController
A simple rigid character controller for Unity written in C# that doesn't use physics.
Has a very similar API to the CharacterController for Unity3D

Does not handle triggers or anything else, that is on the end user to implement in their own way. Its very easy to do, I just don't want to clutter this code if possible.

Usecase:
Lots of Character Controllers I find online are over the top and complicated. Or rely on built in Unity Physics.
This provides an opportunity for rigid motion, or self created physics.

Example:

For a velocity based approach
```cs
    private void UpdateXVelocity() {
        float xInput = _move.ReadValue<float>();
        float friction = _cc.IsGrounded ? _groundFriction : _airFriction;
        float accelerationUsed = Mathf.Sign(xInput) == Mathf.Sign(_xVelocity) ? _acceleration : _decceleration;
        if (xInput != 0) {
            _xVelocity = Mathf.MoveTowards(_xVelocity, xInput * _maxXVelocity, accelerationUsed * Time.deltaTime);
        } else {
            _xVelocity *= 1-friction*Time.deltaTime;
            _xVelocity = Mathf.Abs(_xVelocity) <= 1f ? 0 : _xVelocity;
        }
        _cc.Move(Time.deltaTime * _xVelocity * Vector3.right);
    }
```

For a non-velocity based approach
```cs
    private void UpdateXVelocity() {
        float xInput = _move.ReadValue<float>();
        _cc.Move(Time.deltaTime * xInput * _movementSpeed * Vector3.right);
    }
```

How to use:

CharacterController2D.Move() is realistically the only function you'll interface with. It decides where you're going to move, how far, and places you there.
So you call _cc.Move( MOVEMENT_VECTOR ) and thats it.

Things Id Like to do but havent because my project didn't call for it
- [ ] Slope support
- [ ] Different Collider Shape support
- [ ] Modular platform / Slope code attachment so theyre a bit less intertwined (ie the Platform code is a part of its own attachable module to the CharacterController2D rather than a part of the base class, or perhaps its some kind of override)

Feel free to use however, change however. I'd enjoy if anyone who uses it also creates a pull request with additional changes or features, just so I can see them. It would be cool.
