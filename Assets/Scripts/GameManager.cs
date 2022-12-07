using CatGame.CameraControl;
using CatGame.CharacterControl;
using CatGame.LevelManagement;
using UnityEngine;

namespace CatGame
{
    public class GameManager : MonoBehaviour
    {
        private new FollowCamera camera;
        private Character character;
        private LevelLoader levelLoader;

        private LevelData currentLevel;
        private Vector2 currentSpawnPoint;

        public void EnterLayer(string layerName)
            => EnterLevel(layerName, 0, false, false);

        public void EnterLevel(string layerName, int levelIndex, bool cameFromTransition = true, bool pausePhysics = true) 
        {
            // Load level and subscribe to transitions
            currentLevel = levelLoader.LoadLevel(layerName, levelIndex);
            foreach (LevelTransition transition in currentLevel.transitions)
            {
                transition.OnTransitionEntered -= OnLevelTransitionEntered;
                transition.OnTransitionEntered += OnLevelTransitionEntered;
            }

            // Stop physics then restart it once camera is finished moving if 
            // pausePhysics is true
            if (pausePhysics)
            {
                Time.timeScale = 0;
                camera.SetTilemap(currentLevel.tilemap, () => Time.timeScale = 1);
            }
            // Else just set the camera tilemap
            else
            {
                camera.SetTilemap(currentLevel.tilemap);
            }

            // Respawn character if we didn't come from another level; if we 
            // just spawned onto the layer
            if (!cameFromTransition)
            {

                // TODO: Write a proper respawn
                character.transform.position = currentLevel.defaultSpawnPoint;
                currentSpawnPoint = currentLevel.defaultSpawnPoint;
            }
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
            if (transition.nextLevelIndex == currentLevel.index)
            {
                return;
            }

            // Enter the next level and set current spawn point
            EnterLevel(transition.layerName, transition.nextLevelIndex);
            if (transition.associatedSpawnPoint.HasValue)
            {
                currentSpawnPoint = transition.associatedSpawnPoint.Value;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            camera = FindObjectOfType<FollowCamera>(true);
            character = FindObjectOfType<Character>(true);
            levelLoader = GetComponent<LevelLoader>();
            EnterLayer("Test");
        }

        // The editor removes event subscribers on rebuild (aka saving while in
        // play mode). This is a workaround to resubscribe. Terrible but works
#if UNITY_EDITOR
        private void Update()
        {
            foreach (LevelTransition transition in currentLevel.transitions)
            {
                transition.OnTransitionEntered -= OnLevelTransitionEntered;
                transition.OnTransitionEntered += OnLevelTransitionEntered;
            }
        }
#endif
    }
}
