using CatGame.CameraControl;
using CatGame.CharacterControl;
using CatGame.Interactables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame.LevelManagement
{
    [RequireComponent(typeof(Collider2D))]
    public class BridgeCutscene : MonoBehaviour
    {
        private const float BridgeMiddle = 8.5f;

        public event Action OnCutsceneFinish;

        private static readonly HashSet<BridgeCutscene> _cutscenes = new();

        public AnimationCurve speedCurve;

        private object _coroutine;

        private FollowCamera _camera;
        private Character _character;

        public static IReadOnlyCollection<BridgeCutscene> Cutscenes => _cutscenes;

        private void Awake()
        {
            _cutscenes.Add(this);

            _camera = FindObjectOfType<FollowCamera>(true);
            _character = FindObjectOfType<Character>(true);
        }

        private void OnDestroy()
        {
            _cutscenes.Remove(this);
        }

        private IEnumerator CutsceneEnumerator()
        {
            _camera.lockX = false;
            _camera.lockY = false;

            InputHandler.IsInputEnabled = false;

            // Set cam bounds to infinite to give free movement
            bool hasFinishedMoving = false;
            _camera.SetBounds(new Bounds(Vector3.zero, Vector3.positiveInfinity), () => hasFinishedMoving = true);
            while (!hasFinishedMoving)
            {
                yield return null;
            }

            // Move until halfway across bridge
            float startZoom = Camera.main.orthographicSize;
            Vector2 charPos;
            do
            {
                charPos = transform.GetChild(0).InverseTransformPoint(_character.transform.position);
                // Slow down based off distance from middle
                _character.Movement.Move(speedCurve.Evaluate(charPos.x / BridgeMiddle));

                // Zoom camera over x sec
                Camera.main.orthographicSize = Mathf.Lerp(startZoom, startZoom * 0.66f, charPos.x / BridgeMiddle);
                
                yield return null;
            }
            while (charPos.x < BridgeMiddle);

            yield return new WaitForSeconds(2);
            
            // Drop cat
            CatPickup pickup = _character.InteractableHandler.CurrentPickup as CatPickup;
            _character.InteractableHandler.DropPickup();
            pickup.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            pickup.Collider.enabled = false;

            yield return new WaitForSeconds(3);

            // Freeze camera and walk away
            _camera.lockedPos = _camera.transform.position;
            _camera.lockedPos.y += 0.25f;
            _camera.lockX = true;
            _camera.lockY = true;

            float startTime = Time.time;
            while (Time.time - startTime < 4)
            {
                _character.Movement.Move(0.7f);
                yield return null;
            }

            InputHandler.IsInputEnabled = true;

            _coroutine = null;
            Camera.main.orthographicSize = startZoom;
            OnCutsceneFinish?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (_character.InteractableHandler.CurrentPickup is not CatPickup || _coroutine != null)
            {
                return;
            }

            _coroutine = StartCoroutine(CutsceneEnumerator());
        }
    }
}
