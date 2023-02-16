using UnityEngine;

namespace CatGame
{
    public class CreditsFinisher : MonoBehaviour
    {
        MenuManager _menuManager;

        public void FinishCredits()
        {
            _menuManager.OpenMainMenu();
            Destroy(gameObject);
        }

        private void Awake()
        {
            _menuManager = FindObjectOfType<MenuManager>();
        }

        private void Update()
        {
            if (InputHandler.Pause.WasPressedThisFrame())
            {
                FinishCredits();
            }
        }
    }
}
