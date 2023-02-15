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
        public new AudioSource audio;

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
            bool originalInputEnabled = InputHandler.IsInputEnabled;
            bool originalAutoMode = autoMode;
            float originalAutoModeDelay = autoDelaySec;
            float originalTextDelay = textDelayMS;

            autoMode = dialogue.forceAutoMode;
            InputHandler.IsInputEnabled = dialogue.enableInput;
            if (dialogue.autoDelaySec >= 0)
            {
                autoDelaySec = dialogue.autoDelaySec;
            }
            if (dialogue.textDelayMS >= 0)
            {
                textDelayMS = dialogue.textDelayMS;
            }

            Sprite lastSprite = backgroundImage.sprite;

            if (dialogue.audio != null)
            {
                audio.clip = dialogue.audio;
                audio.Play();
            }

            foreach (DialogueLine line in dialogue.lines)
            {
                if (line.audioOverride != null)
                {
                    audio.clip = line.audioOverride;
                    audio.Play();
                }

                if (string.IsNullOrEmpty(line.speaker))
                {
                    // Disable speaker is there is none
                    speakerGameObject.SetActive(false);
                }
                else
                {
                    // Set speaker data
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

                // Wait until the skip button is pressed, or if in auto mode, 
                // once autoDelaySec has passed
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

            audio.clip = null;
            audio.Stop();

            autoMode = originalAutoMode;
            InputHandler.IsInputEnabled = originalInputEnabled;
            autoDelaySec = originalAutoModeDelay;
            textDelayMS = originalTextDelay;

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
            // Fade to black
            float start = Time.unscaledTime;
            while (Time.unscaledTime - start < 1.2)
            {
                backgroundImage.color = Color.Lerp(Color.white, Color.black, (Time.unscaledTime - start) / 1.2f);
                yield return null;
            }

            // Wait for a bit
            backgroundImage.color = Color.black;
            backgroundImage.sprite = sprite;
            yield return new WaitForSecondsRealtime(0.5f);

            // Fade to white
            start = Time.unscaledTime;
            while (Time.unscaledTime - start < 1.2)
            {
                backgroundImage.color = Color.Lerp(Color.black, Color.white, (Time.unscaledTime - start) / 1.2f);
                yield return null;
            }
            backgroundImage.color = Color.white;
        }
    }
}
