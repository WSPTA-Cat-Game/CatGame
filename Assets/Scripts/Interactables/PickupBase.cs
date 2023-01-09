using UnityEngine;

namespace CatGame.Interactables
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PickupBase : InteractableBase
    {
        protected bool isHeld = false;
        protected Transform holder = null;


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
    }
}
