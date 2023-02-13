using System.Collections;

namespace CatGame.MovingTiles
{
    internal class ToggleMovingTile : MovingTile
    {
        public void Toggle()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            StopAllCoroutines();
            StartCoroutine(ToggleCoroutine());
        }

        protected override void Update()
        {
            
        }

        private IEnumerator ToggleCoroutine()
        {
            // We have to update once to start the platform again
            base.Update();
            stopTime = 0;
            yield return null;

            while (IsMoving)
            {
                base.Update();
                yield return null;
            }
        }
    }
}
