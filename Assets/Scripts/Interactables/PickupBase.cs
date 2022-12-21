using UnityEngine;

namespace CatGame.Interactables
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PickupBase : InteractableBase
    {
        protected bool isHeld = false;
        protected Transform holder = null;

        public Collider2D Collider { get; private set; }

        public virtual void Pickup(Transform holder) 
        {
            this.holder = holder;
            isHeld = true;
        }

        public virtual void Drop()
        {
            holder = null;
            isHeld = false;
        }

        private void Start()
        {
            Collider = GetComponent<Collider2D>();
        }
    }
}
