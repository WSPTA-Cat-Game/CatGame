using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CatGame
{
    public class MenuManager : MonoBehaviour
    {
        public GameManager gameManager;

        public Sprite lockedSprite;
        public Sprite unlockedSprite;

        public Sprite startSprite;
        public Sprite resumeSprite;

        private GameObject _menuRoot;
        private GameObject _mainMenu;
        private GameObject _startContinueButton;
        private GameObject _levelSelectMenu;
        private GameObject _levelSelectButtonsRoot;

        public void ContinueGame()
        {
            HideMenu();
            gameManager.EnterLayer(SaveManager.GetHighestCompletedLayer());
        }

        public void OpenLayerSelect()
        {
            HashSet<string> completedLayers = SaveManager.GetCompletedLayersEnumerator().ToHashSet();

            foreach (Transform button in _levelSelectButtonsRoot.transform)
            {
                Button buttonComponent = button.GetComponent<Button>();
                Image image = button.transform.Find("Outline").GetComponent<Image>();
                if (completedLayers.Contains(button.name))
                {
                    image.sprite = unlockedSprite;
                    buttonComponent.interactable = true;
                }
                else
                {
                    image.sprite = lockedSprite;
                    buttonComponent.interactable = false;
                }
            }

            _mainMenu.SetActive(false);
            _levelSelectMenu.SetActive(true);
        }

        public void OpenMainMenu()
        {
            string highestCompletedLayer = SaveManager.GetHighestCompletedLayer();
            if (highestCompletedLayer == "Layer 1")
            {
                _startContinueButton.GetComponent<Button>().image.sprite = startSprite;
            }
            else
            {
                _startContinueButton.GetComponent<Button>().image.sprite = resumeSprite;
            }

            _mainMenu.SetActive(true);
            _levelSelectMenu.SetActive(false);
        }

        public void SelectLayer(string layerName)
        {
            HideMenu();
            OpenMainMenu();

            gameManager.EnterLayer(layerName);
        }

        public void HideMenu()
        {
            _menuRoot.SetActive(false);
        }

        private void Awake()
        {
            _menuRoot = GameObject.Find("Menu/Canvas");
            _mainMenu = _menuRoot.transform.Find("Main Page").gameObject;
            _startContinueButton = _mainMenu.transform.Find("Central Buttons/Start").gameObject;

            _levelSelectMenu = _menuRoot.transform.Find("Level Select").gameObject;
            _levelSelectButtonsRoot = _levelSelectMenu.transform.Find("Levels").gameObject;

            OpenMainMenu();
        }
    }
}
