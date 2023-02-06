using UnityEngine;

namespace CatGame.Interactables
{
    public class CatPickup : PickupBase
    {
        public override void Pickup(Transform holder)
        {
            base.Pickup(holder);
            Transform child = transform.GetChild(0);
            child.GetComponent<Animator>().enabled = false;
            child.localPosition = Vector3.zero;
        }

        public override void Drop()
        {
            base.Drop();
            transform.GetChild(0).GetComponent<Animator>().enabled = true;
        }
    }
}
