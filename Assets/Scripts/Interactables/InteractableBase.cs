using UnityEngine;

namespace Interactables
{
    [RequireComponent(typeof(Collider2D))]
    public class InteractableBase : MonoBehaviour
    {
        public virtual void Interact() { }
    }
}
