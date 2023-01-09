using UnityEngine;

namespace CatGame.Interactables
{
    [RequireComponent(typeof(Collider2D))]
    public class InteractableBase : MonoBehaviour
    {
        public Collider2D Collider { get; private set; }

        public virtual void Interact() { }

        private void Start()
        {
            Collider = GetComponent<Collider2D>();
        }
    }
}
