using UnityEngine;

namespace CatGame.RuleTiles
{
    // For some reason, when built, rule tiles with a gameobject set will have
    // their gameobject duplicated at 0, 0, 0, so let's just delete any 
    // gameobject at 0, 0, 0
    // https://issuetracker.unity3d.com/issues/instantiating-tilemap-that-has-rule-tile-with-default-gameobject-set-creates-extra-copy-of-gameobject-at-00-0-in-builds
    public class RuleTileFix : MonoBehaviour
    {
        private void Awake()
        {
            DestroyImmediate(gameObject);
        }
    }
}
