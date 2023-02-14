using System;
using UnityEngine;

namespace CatGame.UI
{
    [Serializable]
    public struct Dialogue
    {
        public string id;
        public DialogueLine[] lines;
    }
}
