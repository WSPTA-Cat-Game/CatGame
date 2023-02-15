using System;
using UnityEngine;

namespace CatGame.UI
{
    [Serializable]
    public struct Dialogue
    {
        public string id;
        public AudioClip audio;
        public DialogueLine[] lines;

        public bool enableInput;
        public bool forceAutoMode;
        public float autoDelaySec;
        public float textDelayMS;
    }
}
