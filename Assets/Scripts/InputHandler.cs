using UnityEngine;
using UnityEngine.InputSystem;

namespace CatGame
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputHandler : MonoBehaviour
    {
        private static InputHandler instance;
        private static InputHandler Instance
        {
            get
            {
                if (instance == null)
                {
                   instance = FindObjectOfType<InputHandler>();
                }
                return instance;
            }
        }

        private PlayerInput playerInput;

        public static InputAction Move 
            => Instance.playerInput.actions.FindAction("Move");
        public static InputAction Interact 
            => Instance.playerInput.actions.FindAction("Interact");
        public static InputAction Drop 
            => Instance.playerInput.actions.FindAction("Drop");
        public static InputAction Jump 
            => Instance.playerInput.actions.FindAction("Jump");
        public static InputAction Transition 
            => Instance.playerInput.actions.FindAction("Transition");

        private void Start()
        {
            playerInput = GetComponent<PlayerInput>();
        }
    }
}
