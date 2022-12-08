using CatGame.Interactables;
using UnityEngine;

namespace CatGame.CharacterControl
{
    [RequireComponent(typeof(CharacterMovement))]
    public class Character : MonoBehaviour
    {
        public CharacterMovementConfig humanMovement = new();
        public CharacterMovementConfig catMovement = new();

        private CharacterMovement _movement;
        private InteractableHandler _interactableHandler;

        private CharacterMode _mode = CharacterMode.Human;

        private void Start()
        {
            _movement = GetComponent<CharacterMovement>();
            _interactableHandler = GetComponentInChildren<InteractableHandler>();
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
            if (_mode == CharacterMode.Human)
            {
                _mode = CharacterMode.Cat;
                _movement.SetConfig(catMovement);
                _interactableHandler.DropPickup();
                _interactableHandler.enabled = false;
            }
            else
            {
                _mode = CharacterMode.Human;
                _movement.SetConfig(humanMovement);
                _interactableHandler.enabled = true;
            }
        }
    }
}
