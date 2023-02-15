using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CatGame.UI
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
        private GameObject _settingsMenu;
        private GameObject _settingsButtonsRoot;

        // All of the following methods are used by the unity events attached
        // to various UI elements
        public void ContinueGame()
        {
            HideMenu();

            string highestLayer = PrefsManager.GetHighestAvailableLayer();
            if (highestLayer == null)
            {
                // Play intro then enter layer 1 if player has no data
                gameManager.PlayStartingDialogue(() => gameManager.EnterLayer("Layer 1"));
            }
            else
            {
                gameManager.EnterLayer(highestLayer);
            }
        }

        public void OpenLayerSelect()
        {
            // Lock any layer buttons that haven't yet been completed
            HashSet<string> completedLayers = PrefsManager.GetAvailableLayers().ToHashSet();

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
            _settingsMenu.SetActive(false);
        }

        public void OpenSettings()
        {
            // Set slider values to what the corresponding prefs value is
            foreach (Transform slider in _settingsButtonsRoot.transform)
            {
                Slider sliderComponent = slider.GetComponent<Slider>();
                sliderComponent.SetValueWithoutNotify(PrefsManager.GetGroupVolume(slider.name));

                sliderComponent.onValueChanged.RemoveAllListeners();
                sliderComponent.onValueChanged.AddListener(val => gameManager.SetMixerVolume(slider.name, val));
            }

            _mainMenu.SetActive(false);
            _levelSelectMenu.SetActive(false);
            _settingsMenu.SetActive(true);
        }

        public void OpenMainMenu()
        {
            string highestCompletedLayer = PrefsManager.GetHighestAvailableLayer();
            if (highestCompletedLayer == null)
            {
                _startContinueButton.GetComponent<Button>().image.sprite = startSprite;
            }
            else
            {
                _startContinueButton.GetComponent<Button>().image.sprite = resumeSprite;
            }

            _mainMenu.SetActive(true);
            _levelSelectMenu.SetActive(false);
            _settingsMenu.SetActive(false);
        }

        public void SelectLayer(string layerName)
        {
            HideMenu();
            OpenMainMenu();

            gameManager.EnterLayer(layerName);
        }

        public void HideMenu()
        {
            OpenMainMenu();
            _menuRoot.SetActive(false);
        }

        private void Awake()
        {
            _menuRoot = GameObject.Find("Menu");
            _mainMenu = _menuRoot.transform.Find("Canvas/Main Page").gameObject;
            _startContinueButton = _mainMenu.transform.Find("Central Buttons/Start").gameObject;

            _levelSelectMenu = _menuRoot.transform.Find("Canvas/Level Select").gameObject;
            _levelSelectButtonsRoot = _levelSelectMenu.transform.Find("Levels").gameObject;

            _settingsMenu = _menuRoot.transform.Find("Canvas/Settings").gameObject;
            _settingsButtonsRoot = _settingsMenu.transform.Find("Settings").gameObject;

            OpenMainMenu();
            
            // Set audio mixer values
            foreach (Transform button in _settingsButtonsRoot.transform)
            {
                Slider sliderComponent = button.GetComponent<Slider>();
                gameManager.SetMixerVolume(button.name, sliderComponent.value);
            }
            
        }
    }
}
