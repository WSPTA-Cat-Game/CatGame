using UnityEngine;
using UnityEngine.InputSystem;

namespace CatGame
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputHandler : MonoBehaviour
    {
        private static InputHandler _instance;
        private static InputHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                   _instance = FindObjectOfType<InputHandler>();
                }
                return _instance;
            }
        }

        private PlayerInput _playerInput;

        public static InputAction Move 
            => Instance._playerInput.actions.FindAction("Move");
        public static InputAction Interact 
            => Instance._playerInput.actions.FindAction("Interact");
        public static InputAction Drop 
            => Instance._playerInput.actions.FindAction("Drop");
        public static InputAction Jump 
            => Instance._playerInput.actions.FindAction("Jump");
        public static InputAction Transition 
            => Instance._playerInput.actions.FindAction("Transition");

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }
    }
}
