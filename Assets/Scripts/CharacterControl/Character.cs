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
                .GetComponent<SpriteRenderer>().sprite=cat;
                .GetComponent<BoxCollider2D>().size=new vector2(0.3f, 0.3f);
            }
            else
            {
                mode = CharacterMode.Human;
                movement.SetConfig(humanMovement);
                .GetComponent<SpriteRenderer>().sprite=human;
                .GetComponent<BoxCollider2D>().size=new vector2(0.475f, 0.475f);
            }
        }
    }
}
