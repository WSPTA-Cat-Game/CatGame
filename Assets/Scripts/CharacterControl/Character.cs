using UnityEngine;

namespace CharacterControl
{
    [RequireComponent(typeof(CharacterMovement))]
    public class Character : MonoBehaviour
    {
        public CharacterMovementConfig humanMovement = new();
        public CharacterMovementConfig catMovement = new();

        private CharacterMode mode = CharacterMode.Human;

        private CharacterMovement movement;

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
            }
            else
            {
                mode = CharacterMode.Human;
                movement.SetConfig(humanMovement);
            }
        }
    }
}
