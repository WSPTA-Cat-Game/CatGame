using System;
using UnityEngine;

namespace CatGame.UI
{
    [Serializable]
    public struct DialogueLine
    {
        public string speaker;
        public Color color;
        public string line;
        public Sprite backgroundSprite;
        public bool fadeToSprite;
    }
}
