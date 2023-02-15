using CatGame.Audio;
using CatGame.CameraControl;
using CatGame.CharacterControl;
using CatGame.Interactables;
using CatGame.LevelManagement;
using CatGame.MovingTiles;
using CatGame.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;

namespace CatGame
{
    // GameManager is a super generic name, so think of this class as something
    // that handles transitions from one thing to another, which means it
    // provides the needed interface for the menu to function and handles
    // the cohesion between level elements.
    public class GameManager : MonoBehaviour
    {
        public AudioMixer mixer;

        private FollowCamera _camera;
        private Character _character;
        private LevelLoader _levelLoader;
        private FadeAudio _audioSource;

        private LayerData _currentLayer;
        private LevelData _currentLevel;

        private object _coroutine;

        private bool _paused;

        public void Pause()
        {
            Time.timeScale = 0;
            GetComponent<MenuManager>().OpenPause();
            _audioSource.source.Stop();
            _paused = true;
        }

        public void Unpause()
        {
            Time.timeScale = 1;
            GetComponent<MenuManager>().HideMenu();
            _audioSource.source.Play();
            _paused = false;
        }

        public void PlayStartingDialogue(Action finishCallback = null) 
        {
            DialogueBox box = transform.Find("Dialogue").GetComponent<DialogueBox>();
            box.gameObject.SetActive(true);

            finishCallback += () =>
            {
                box.gameObject.SetActive(false);

                PrefsManager.AddAvailableLayer("Layer 1");
            };

            box.StartDialogue("Intro", finishCallback);
        }

        public void PlayEndingDialogue(Action finishCallback = null)
        {
            DialogueBox box = transform.Find("Dialogue").GetComponent<DialogueBox>();
            box.gameObject.SetActive(true);

            finishCallback += () =>
            {
                box.gameObject.SetActive(false);
                // TODO: add credits
            };

            box.StartDialogue("Ending", finishCallback);
        }

        public void SetMixerVolume(string param, float volume)
        {
            // Mixers work in dB, so we need to convert from 0-1 to
            // -20dB - 10dB
            mixer.SetFloat(param, volume * 30 - 20);
            PrefsManager.SetGroupVolume(param, volume);
        }

        public void EnterLayer(string layerName)
            => EnterLevel(layerName, 0, false);

        public void EnterLevel(string layerName, int levelIndex, bool cameFromTransition = true) 
        {
            _character.gameObject.SetActive(true);
            _character.InteractableHandler.OnPickupChange -= OnPickupChange;
            _character.InteractableHandler.OnPickupChange += OnPickupChange;

            // Set caemra to 0,0,0 so the background starts properly
            if (!cameFromTransition)
            {
                if (_coroutine == null)
                {
                    _coroutine = StartCoroutine(LoadScreen(UnityEngine.Random.Range(2, 4)));
                }

                _camera.enabled = false;
                Camera.main.transform.position = Vector3.zero;
                _camera.enabled = true;
            }

            // Load level and subscribe to transitions
            _currentLevel = _levelLoader.LoadLevel(layerName, levelIndex);
            foreach (LevelTransition transition in _currentLevel.transitions)
            {
                transition.OnTransitionEntered -= OnLevelTransitionEntered;
                transition.OnTransitionEntered += OnLevelTransitionEntered;
            }

            // Subscribe to bridge cutscene
            foreach (BridgeCutscene cutscene in BridgeCutscene.Cutscenes)
            {
                cutscene.OnCutsceneFinish -= OnBridgeCutsceneFinish;
                cutscene.OnCutsceneFinish += OnBridgeCutsceneFinish;
            }

            // Stop physics then restart it once camera is finished moving if 
            // pausePhysics is true
            if (cameFromTransition)
            {
                float lastTimeScale = Time.timeScale;
                Time.timeScale = 0;
                _camera.SetBounds(_currentLevel.collideableBounds, () => Time.timeScale = lastTimeScale);
            }
            // Else just set the camera tilemap
            else
            {
                _camera.SetBounds(_currentLevel.collideableBounds);
            }

            // Set camera locks
            _camera.lockX = _currentLevel.lockCameraX;
            _camera.lockY = _currentLevel.lockCameraY;
            _camera.lockedPos = _currentLevel.tilemap.LocalToWorld(_currentLevel.lockedCameraPos);

            // Respawn character if we didn't come from another level; if we 
            // just spawned onto the layer
            if (!cameFromTransition)
            {
                Respawn();
            }

            // Play audio
            _currentLayer = _levelLoader.globalParent.GetComponent<LayerData>();
            if (_currentLayer != null && _audioSource.Clip != _currentLayer.audio)
            {
                _audioSource.Clip = _currentLayer.audio;
                _audioSource.Play();
            }

            // Generate shadows
            if (_currentLayer != null && _currentLayer.generateShadows)
            {
                StartCoroutine(GenerateShadowsForLevel());

                // Also enable or disable the player's light
                foreach (Light2D light in _character.GetComponentsInChildren<Light2D>())
                {
                    light.enabled = true;
                }
            }
            else
            {
                foreach (Light2D light in _character.GetComponentsInChildren<Light2D>())
                {
                    light.enabled = false;
                }
            }

            // Tell LOS moving tiles what the player collider is
            Collider2D characterCollider = _character.GetComponent<Collider2D>();
            foreach (MovingTile tile in MovingTile.Tiles)
            {
                if (tile is LOSMovingTile losTile)
                {
                    losTile.playerCollider = characterCollider;
                }
                if (tile is ToggleMovingTile toggleTile)
                {
                    toggleTile.transform.parent.gameObject.SetActive(
                        toggleTile.transform.IsChildOf(_currentLevel.transform));
                }
            }
            
            // Prevent player from exiting level unless carrying cat
            OnPickupChange(_character.InteractableHandler.CurrentPickup);
        }

        public void Respawn()
        {
            _character.transform.position = _currentLevel.transform
                .TransformPoint(_currentLevel.defaultSpawnPoint);
        }

        private void OnLevelTransitionEntered(LevelTransition transition, Collider2D collision)
        {
            // Only switch level if the character collider hits it
            if (collision.GetComponent<Character>() == null)
            {
                return;
            }

            // Don't listen to it if it tells us to go into the level were
            // already in
            if (transition.nextLevelIndex == _currentLevel.index)
            {
                return;
            }

            // Enter the next level
            EnterLevel(transition.layerName, transition.nextLevelIndex);
        }

        private void OnBridgeCutsceneFinish()
        {
            if (_currentLevel.layerName == "Layer 3")
            {
                // TODO: ????
                return;
            }

            int currentLayerNum = int.Parse(_currentLevel.layerName[6..]);
            string newLayer = "Layer " + (currentLayerNum + 1);

            PrefsManager.AddAvailableLayer(newLayer);
            EnterLayer(newLayer);
        }

        private void OnPickupChange(PickupBase pickup)
        {
            // Only let player exit if the pickup is cat
            bool isHoldingCat = pickup != null && pickup is CatPickup;

            foreach (LevelTransition transition in _currentLevel.transitions)
            {
                if (isHoldingCat || transition.canExitWithoutCat)
                {
                    transition.GetComponent<Collider2D>().isTrigger = true;
                    transition.gameObject.layer = LayerMasks.Default.ToLayer();
                }
                else
                {
                    transition.GetComponent<Collider2D>().isTrigger = false;
                    transition.gameObject.layer = LayerMasks.IgnoreRaycast.ToLayer();
                }
            }

            foreach (BridgeCutscene cutscene in BridgeCutscene.Cutscenes)
            {
                cutscene.GetComponent<Collider2D>().isTrigger = isHoldingCat;
            }
        }

        private IEnumerator GenerateShadowsForLevel()
        {
            // Needed because composite collider has no geometry for one frame
            yield return null;
            ShadowGenerator.GenerateShadowForCollider(_levelLoader.levelParent.GetComponent<CompositeCollider2D>());
        }

        private IEnumerator LoadScreen(float timeSeconds)
        {
            Transform loadScreen = transform.Find("Loading");
            loadScreen.gameObject.SetActive(true);

            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0;

            yield return new WaitForSecondsRealtime(timeSeconds);

            Time.timeScale = originalTimeScale;
            loadScreen.gameObject.SetActive(false);

            _coroutine = null;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _camera = FindObjectOfType<FollowCamera>(true);
            _character = FindObjectOfType<Character>(true);
            _levelLoader = GetComponent<LevelLoader>();
            _audioSource = GetComponent<FadeAudio>();
        }


        private void Update()
        {
            // The editor removes event subscribers on rebuild (aka saving while in
            // play mode). This is a workaround to resubscribe. Terrible but works
#if UNITY_EDITOR
            if (_currentLevel == null)
            {
                return;
            }

            foreach (LevelTransition transition in _currentLevel.transitions)
            {
                transition.OnTransitionEntered -= OnLevelTransitionEntered;
                transition.OnTransitionEntered += OnLevelTransitionEntered;
            }
#endif

            if (InputHandler.Pause.WasPressedThisFrame())
            {
                if (_paused)
                {
                    Unpause();
                }
                else
                {
                    Pause();
                }
            }
        }
    }
}
