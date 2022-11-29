using CatGame.Interactables;
using UnityEngine;

namespace CatGame.CharacterControl
{
    [RequireComponent(typeof(CharacterMovement))]
    public class Character : MonoBehaviour
    {
        public CharacterMovementConfig humanMovement = new();
        public CharacterMovementConfig catMovement = new();

        private CharacterMode mode = CharacterMode.Human;

        private CharacterMovement movement;
        private InteractableHandler interactableHandler;

        private void Start()
        {
            movement = GetComponent<CharacterMovement>();
            interactableHandler = GetComponentInChildren<InteractableHandler>();
        }

        private void Update()
        {
            if (InputHandler.Transition.WasPressedThisFrame())
            {
                ToggleMode();
            }
        }

        private void ToggleMode()
        {
            if (mode == CharacterMode.Human)
            {
                mode = CharacterMode.Cat;
                movement.SetConfig(catMovement);
                interactableHandler.DropPickup();
                interactableHandler.enabled = false;
            }
            else
            {
                mode = CharacterMode.Human;
                movement.SetConfig(humanMovement);
                interactableHandler.enabled = true;
            }
        }
    }
}
