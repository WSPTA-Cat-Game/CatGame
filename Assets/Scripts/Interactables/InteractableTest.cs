using UnityEngine;

namespace Interactables
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
