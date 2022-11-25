using UnityEngine;

namespace Pickups
{
    public class PickupTest : PickupBase
    {
        public override void Use()
        {
            Debug.Log("Pickup used!");
        }
    }
}
