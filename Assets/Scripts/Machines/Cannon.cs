using core.zqc.bombs;
using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class Cannon : MonoBehaviour
{
    public BombGenerator linkedGenerator;
    public Team owner;
    public float fireDelay;   // 大炮从填充到发射炮弹的动画间隔时间
    public Animator cannonAnimator;

    private void Start()
    {
    }

    public void FillBomb(Bomb bomb, float delay)
    {
        StartCoroutine(FillBombDelay(bomb, delay, fireDelay));
    }

    private IEnumerator FillBombDelay(Bomb bomb, float fillDelay, float fireDelay)
    {
        yield return new WaitForSeconds(fillDelay);
        Destroy(bomb.gameObject);
        cannonAnimator.SetTrigger("Fire");
        yield return new WaitForSeconds(fireDelay);
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
