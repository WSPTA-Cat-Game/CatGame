using UnityEngine;

namespace CatGame.Interactables
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class InteractableTest : InteractableBase
    {
        public override void Interact()
        {
            GetComponent<SpriteRenderer>().color = Random.ColorHSV();
        }
    }
}
