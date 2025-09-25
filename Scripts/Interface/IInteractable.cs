using UnityEngine;

namespace _Lofty.Hidden.Interface
{
    /// <summary>
    /// Interface for interactable objects in the game.
    /// </summary>
    /// <remarks>
    /// This interface is used to define the contract for objects that can be interacted with by the player.
    /// </remarks>
    public interface IInteractable
    {
        public GameObject PopupUI { get; set; } // UI element that appears when the player can interact with the object
        public bool IsActive { get; set; } // for UI visibility
        public NewPlayer Player { get; set; } // reference to the player interacting with the object

        public void Interact(); // Method to perform the interaction logic
        public void TriggerEnter(NewPlayer _player); // Method to handle trigger events when the player enters the interactable area
        public void TriggerExit(); // Methods to handle trigger events when the player enters or exits the interactable area
    }
}
