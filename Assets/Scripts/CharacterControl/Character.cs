using UnityEngine;

namespace CharacterControl
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
        private void Start()
        {
            movement = GetComponent<CharacterMovement>();
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
                this.GetComponent<SpriteRenderer>().sprite=cat;
                square.GetComponent<BoxCollider2D>().size=new vector2(0.3f,0.3f);
            }
            else
            {
                mode = CharacterMode.Human;
                movement.SetConfig(humanMovement);
                this.GetComponent<SpriteRenderer>().sprite=human;
                square.GetComponent<BoxCollider2D>().size = new vector2(0.475f,0.475f);
            }
        }
    }
}
