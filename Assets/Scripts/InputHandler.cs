using UnityEngine;
using UnityEngine.InputSystem;

namespace CatGame
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputHandler : MonoBehaviour
    {
        private static InputHandler instance;
        private PlayerInput playerInput;

        public static InputAction Move 
            => instance.playerInput.actions.FindAction("Move");
        public static InputAction Interact 
            => instance.playerInput.actions.FindAction("Interact");
        public static InputAction Drop 
            => instance.playerInput.actions.FindAction("Drop");
        public static InputAction Jump 
            => instance.playerInput.actions.FindAction("Jump");
        public static InputAction Transition 
            => instance.playerInput.actions.FindAction("Transition");

        private void Start()
        {
            instance = this;
            playerInput = GetComponent<PlayerInput>();
        }
    }
}
