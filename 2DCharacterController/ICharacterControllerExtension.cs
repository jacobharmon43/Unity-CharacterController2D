using UnityEngine;

public interface ICharacterControllerExtension {
    public void AdjustMovement(ref Vector2 newPos, Vector2 delta); 
}