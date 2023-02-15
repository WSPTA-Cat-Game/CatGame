using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace CatGame.Interactables
{
    public class InteractablePicture : InteractableBase
    {
        public GameObject picture;

        public override void Interact()
        {
            picture.SetActive(true);

            // Disable picture if any button is pressed
            InputSystem.onAnyButtonPress.
                CallOnce((_) => picture.SetActive(false));
        }
    }
}
