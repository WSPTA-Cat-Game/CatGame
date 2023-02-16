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

        private GameObject _currentMenu;
        private GameObject _lastMenu;

        private GameObject _menuRoot;
        private GameObject _mainMenu;
        private GameObject _startContinueButton;
        private GameObject _levelSelectMenu;
        private GameObject _levelSelectButtonsRoot;
        private GameObject _settingsMenu;
        private GameObject _settingsButtonsRoot;
        private GameObject _pauseMenu;

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

            ShowPage(_levelSelectMenu);
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

            ShowPage(_settingsMenu);
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

            ShowPage(_mainMenu);
        }

        public void OpenPause()
        {
            ShowPage(_pauseMenu);
        }

        public void SelectLayer(string layerName)
        {
            HideMenu();

            gameManager.EnterLayer(layerName);
        }

        public void ShowPage(GameObject page)
        {
            ShowMenu();

            if (page == _currentMenu)
            {
                return;
            }

            _lastMenu = _currentMenu;
            _currentMenu = page;

            if (_lastMenu != null)
            {
                _lastMenu.SetActive(false);
            }
            _currentMenu.SetActive(true);
        }

        public void Back()
        {
            ShowPage(_lastMenu);
        }

        public void HideMenu()
        {
            _menuRoot.SetActive(false);
        }

        public void ShowMenu()
        {
            _menuRoot.SetActive(true);
        }

        public void PlayCredits()
        {
            HideMenu();
            gameManager.PlayCredits();
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

            _pauseMenu = _menuRoot.transform.Find("Canvas/Pause").gameObject;

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
