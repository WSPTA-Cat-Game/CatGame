using CatGame.Audio;
using CatGame.CameraControl;
using CatGame.CharacterControl;
using CatGame.Interactables;
using CatGame.LevelManagement;
using CatGame.MovingTiles;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;

namespace CatGame
{
    public class GameManager : MonoBehaviour
    {
        public AudioMixer mixer;

        private FollowCamera _camera;
        private Character _character;
        private LevelLoader _levelLoader;
        private FadeAudio _audioSource;

        private LayerData _currentLayer;
        private LevelData _currentLevel;
        private Vector2 _currentSpawnPoint;

        private object _coroutine;

        public void SetMixerVolume(string param, float volume)
        {
            mixer.SetFloat(param, volume);
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
                    _coroutine = StartCoroutine(LoadScreen(Random.Range(2, 4)));
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
                // TODO: Write a proper respawn
                _character.transform.position = _currentLevel.transform
                    .TransformPoint(_currentLevel.defaultSpawnPoint);
                _currentSpawnPoint = _currentLevel.defaultSpawnPoint;
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

            // Enter the next level and set current spawn point
            EnterLevel(transition.layerName, transition.nextLevelIndex);
            if (transition.hasAssociatedSpawnPoint)
            {
                _currentSpawnPoint = transition.associatedSpawnPoint;
            }
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
            PrefsManager.AddCompletedLayer(newLayer);
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

        // The editor removes event subscribers on rebuild (aka saving while in
        // play mode). This is a workaround to resubscribe. Terrible but works
#if UNITY_EDITOR
        private void Update()
        {
            if (_currentLevel == null)
            {
                return;
            }

            foreach (LevelTransition transition in _currentLevel.transitions)
            {
                transition.OnTransitionEntered -= OnLevelTransitionEntered;
                transition.OnTransitionEntered += OnLevelTransitionEntered;
            }
        }
#endif
    }
}
