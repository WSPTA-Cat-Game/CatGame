using System;
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
        public static InputAction ToggleTentacles 
            => Instance._playerInput.actions.FindAction("Toggle Tentacles");
        public static InputAction Jump 
            => Instance._playerInput.actions.FindAction("Jump");
        public static InputAction Transition 
            => Instance._playerInput.actions.FindAction("Transition");
        public static InputAction DialogueSkip
            => Instance._playerInput.actions.FindAction("Dialogue Skip");
        public static InputAction Pause
            => Instance._playerInput.actions.FindAction("Pause");

        public static bool IsInputEnabled
        {
            get => Instance._playerInput.inputIsActive;
            set => (value ? (Action)Instance._playerInput.ActivateInput : Instance._playerInput.DeactivateInput)();
        }

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }
    }
}
