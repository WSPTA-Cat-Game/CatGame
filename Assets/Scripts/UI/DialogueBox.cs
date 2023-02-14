using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CatGame.UI
{
    public class DialogueBox : MonoBehaviour
    {
        public Dialogue[] dialogue;

        public GameObject boxRoot;
        public Image backgroundImage;
        public GameObject speakerGameObject;
        public Text speakerText;
        public Text lineText;

        public bool autoMode;
        public float autoDelaySec = 2;
        public float textDelayMS = 30;

        private Coroutine _fadeCoroutine;

        public void StartDialogue(string id, Action finishCallback = null)
        {
            Dialogue? selectedDialogue = null;
            for (int i = 0; i < dialogue.Length; i++)
            {
                if (dialogue[i].id == id)
                {
                    selectedDialogue = dialogue[i];
                    break;
                }
            }

            if (selectedDialogue == null)
            {
                throw new ArgumentException($"Could not find dialogue with id \"{id}\".");
            }

            StartDialogue(selectedDialogue.Value, finishCallback);
        }

        public void StartDialogue(Dialogue dialogue, Action finishCallback = null)
        {
            StopAllCoroutines();
            _fadeCoroutine = null;
            StartCoroutine(DialogueCoroutine(dialogue, finishCallback));
        }

        private IEnumerator DialogueCoroutine(Dialogue dialogue, Action finishCallback)
        {
            Sprite lastSprite = backgroundImage.sprite;
            foreach (DialogueLine line in dialogue.lines)
            {
                if (string.IsNullOrEmpty(line.speaker))
                {
                    speakerGameObject.SetActive(false);
                }
                else
                {
                    speakerGameObject.SetActive(true);
                    speakerText.text = line.speaker;
                    speakerText.color = line.color;
                }

                if (line.backgroundSprite != null)
                {
                    if (line.fadeToSprite)
                    {
                        // Set to last sprite just to make sure fade isn't
                        // horribly far behind
                        backgroundImage.sprite = lastSprite;
                        backgroundImage.color = Color.white;
                        FadeToSprite(line.backgroundSprite);
                    }
                    else
                    {
                        backgroundImage.sprite = line.backgroundSprite;
                    }

                    lastSprite = line.backgroundSprite;
                }

                yield return DisplayLine(line);


                if (autoMode)
                {
                    float start = Time.unscaledTime;
                    yield return new WaitUntil(() =>
                        InputHandler.DialogueSkip.WasPressedThisFrame()
                        || Time.unscaledTime - start > autoDelaySec);
                }
                else
                {
                    yield return new WaitUntil(InputHandler.DialogueSkip.WasPressedThisFrame);
                }
            }

            finishCallback?.Invoke();
        }

        private IEnumerator DisplayLine(DialogueLine line)
        {
            if (string.IsNullOrEmpty(line.line))
            {
                boxRoot.SetActive(false);
                yield break;
            }

            lineText.text = "";
            foreach (char c in line.line)
            {
                lineText.text += c;

                // Wait for time delay to pass, and also check for dialogue
                // skips
                float start = Time.unscaledTime;
                bool wasSkipPressed = false;
                while (Time.unscaledTime - start < textDelayMS / 1000)
                {
                    yield return null;

                    if (line.skippable && InputHandler.DialogueSkip.WasPressedThisFrame())
                    {
                        wasSkipPressed = true;
                    }
                }

                // Skip if dialogue skip was pressed during delay
                if (wasSkipPressed)
                {
                    lineText.text = line.line;
                    yield return null;
                    break;
                }
            }
        } 

        private void FadeToSprite(Sprite sprite)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeToSpriteCoroutine(sprite));
        }

        private IEnumerator FadeToSpriteCoroutine(Sprite sprite)
        {
            float start = Time.unscaledTime;
            while (Time.unscaledTime - start < 1.2)
            {
                backgroundImage.color = Color.Lerp(Color.white, Color.black, (Time.unscaledTime - start) / 1.2f);
                yield return null;
            }

            backgroundImage.sprite = sprite;
            yield return new WaitForSecondsRealtime(0.5f);

            start = Time.unscaledTime;
            while (Time.unscaledTime - start < 1.2)
            {
                backgroundImage.color = Color.Lerp(Color.black, Color.white, (Time.unscaledTime - start) / 1.2f);
                yield return null;
            }
        }
    }
}
