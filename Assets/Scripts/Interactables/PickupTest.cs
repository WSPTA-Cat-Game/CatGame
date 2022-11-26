using UnityEngine;

namespace Interactables
{
    public class PickupTest : PickupBase
    {
        public override void Interact()
        {
            Debug.Log("Pickup used!");
        }
    }
}
