using UnityEngine;

namespace CatGame.Interactables
{
    [RequireComponent(typeof(Collider2D))]
    public class InteractableBase : MonoBehaviour
    {
        public virtual void Interact() { }
    }
}
