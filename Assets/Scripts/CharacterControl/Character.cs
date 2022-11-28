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
        public BoxCollider2D square;
        public Sprite cat;
        public Sprite human;
        private InteractableHandler interactableHandler;

        private void Start()
        {
            movement = GetComponent<CharacterMovement>();
            interactableHandler = GetComponentInChildren<InteractableHandler>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
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
                this.GetComponent<SpriteRenderer>().sprite=human;
            }
            else
            {
                mode = CharacterMode.Human;
                movement.SetConfig(humanMovement);
                interactableHandler.enabled = true;
                this.GetComponent<SpriteRenderer>().sprite=cat;
            }
        }
    }
}
