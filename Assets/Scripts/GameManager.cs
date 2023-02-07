using CatGame.Audio;
using CatGame.CameraControl;
using CatGame.CharacterControl;
using CatGame.Interactables;
using CatGame.LevelManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace CatGame
{
    public class GameManager : MonoBehaviour
    {
        private FollowCamera _camera;
        private Character _character;
        private LevelLoader _levelLoader;
        private FadeAudio _audioSource;

        private LayerData _currentLayer;
        private LevelData _currentLevel;
        private Vector2 _currentSpawnPoint;

        public void ContinueGame()
        {
            Debug.Log("Continue");
            // TODO: 
        }

        public void QuitGame()
        {
            Debug.Log("QUIT");
            // TODO:
        }

        public void EnterLayer(string layerName)
            => EnterLevel(layerName, 0, false, false);

        public void EnterLevel(string layerName, int levelIndex, bool cameFromTransition = true, bool pausePhysics = true) 
        {
            // Load level and subscribe to transitions
            _currentLevel = _levelLoader.LoadLevel(layerName, levelIndex);
            foreach (LevelTransition transition in _currentLevel.transitions)
            {
                transition.OnTransitionEntered -= OnLevelTransitionEntered;
                transition.OnTransitionEntered += OnLevelTransitionEntered;
            }

            // Stop physics then restart it once camera is finished moving if 
            // pausePhysics is true
            if (pausePhysics)
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
                ShadowGenerator.GenerateShadowForCollider(
                    _levelLoader.levelParent.GetComponent<CompositeCollider2D>());

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

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _camera = FindObjectOfType<FollowCamera>(true);
            _character = FindObjectOfType<Character>(true);
            _levelLoader = GetComponent<LevelLoader>();
            _audioSource = GetComponent<FadeAudio>();
        }

        private void Start()
        {
            // Not 100% sure why, but this has to go in start
            _character.InteractableHandler.OnPickupChange += OnPickupChange;
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
