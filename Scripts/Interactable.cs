using _Lofty.Hidden.Interface;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    private IInteractable interactableObject;
    public IInteractable InteractableObject { get => interactableObject; set => interactableObject = value; }
    public virtual void Interact()
    {
        InteractableObject?.Interact();
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.TryGetComponent<NewPlayer>(out var _player))
        {
            InteractableObject?.TriggerEnter(_player);
            // Additional logic for when the player enters the interactable area
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.CompareTag("Player"))
        {
            InteractableObject?.TriggerExit();
            // Additional logic for when the player exits the interactable area
        }
    }
}
