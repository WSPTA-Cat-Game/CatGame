using UnityEngine;

namespace Pickups
{
    [RequireComponent(typeof(Collider2D))]
    public class PickupBase : MonoBehaviour
    {
        public virtual void Use() { }
    }
}
