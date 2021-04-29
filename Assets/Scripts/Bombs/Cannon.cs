using System.Collections;
using UnityEngine;

namespace core.zqc.bombs
{
    [RequireComponent(typeof(Collider))]
    public class Cannon : MonoBehaviour
    {
        public BombGenerator linkedGenerator;
        public Team owner;

        public void FillBomb(Bomb bomb, float delay)
        {
            StartCoroutine(FillBombDelay(bomb, delay));
        }

        private IEnumerator FillBombDelay(Bomb bomb, float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(bomb.gameObject);
            linkedGenerator.AddBomb();
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.AddNearbyCannon(this);
            }

        }

        private void OnTriggerExit(Collider other)
        {

            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.RemoveNearbyCannon();
            }

        }
    }
}

