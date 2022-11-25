using System.Collections.Generic;
using UnityEngine;

namespace Pickups
{
    [RequireComponent(typeof(Collider2D))]
    public class PickupHolder : MonoBehaviour
    {
        public PickupBase currentPickup;
        public readonly List<PickupBase> touchingPickups = new();

        private void OnTriggerEnter2D(Collider2D collision)
        {
            PickupBase pickup = collision.GetComponent<PickupBase>();

            if (pickup != null && pickup.gameObject.layer == gameObject.layer)
            {
                touchingPickups.Add(pickup);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            PickupBase pickup = collision.GetComponent<PickupBase>();

            if (pickup != null && pickup.gameObject.layer == gameObject.layer)
            {
                touchingPickups.Remove(pickup);
            }
        }

        private void Update()
        { 
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (currentPickup == null && touchingPickups.Count > 0)
                {
                    currentPickup = touchingPickups[0];
                    touchingPickups[0].gameObject.SetActive(false);
                }
                else if (currentPickup != null)
                {
                    currentPickup.Use();
                }
            } 
            else if (Input.GetKeyDown(KeyCode.F))
            {
                currentPickup.gameObject.SetActive(true);
                currentPickup = null;
            }

            if (currentPickup != null)
            {
                currentPickup.transform.position = transform.position;
            }
        }
    }
}