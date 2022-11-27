using UnityEngine;

namespace Interactables
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PickupBase : InteractableBase
    {
        protected bool isHeld = false;
        protected Transform holder;

        public virtual void Pickup(Transform holder) 
        {
            this.holder = holder;
            gameObject.SetActive(false);
            isHeld = true;
        }

        public virtual void Drop()
        {
            transform.position = holder.position;
            gameObject.SetActive(true);
            isHeld = false;
        }
    }
}
