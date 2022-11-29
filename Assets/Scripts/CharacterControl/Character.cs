using CatGame.Interactables;
using UnityEngine;

namespace CatGame.CharacterControl
{
    [RequireComponent(typeof(CharacterMovement))]
    public class Character : MonoBehaviour
    {
        public CharacterMovementConfig humanMovement = new();
        public CharacterMovementConfig catMovement = new();
        public Sprite cat;
        public Sprite human;
        private CharacterMode mode = CharacterMode.Human;

        private CharacterMovement movement;
        private BoxCollider2D square;
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
                this.GetComponent<SpriteRenderer>().sprite=human;
            }
            else
            {
                mode = CharacterMode.Human;
                movement.SetConfig(humanMovement);
                this.GetComponent<SpriteRenderer>().sprite=cat;
            }
        }
    }
}
