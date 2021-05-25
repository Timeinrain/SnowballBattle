using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制大炮的动画并接受事件回调
/// </summary>
public class CannonAnimator : MonoBehaviour
{
    private Animator animator;
    public System.Action shootBombEvent;
    public System.Action animationEndEvent;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void StartTripleShot()
    {
        animator.SetTrigger("Fire");
    }

    public void Fire()
    {
        if(shootBombEvent != null)
        {
            shootBombEvent();
        }
    }

    public void EndAnimation()
    {
        if (animationEndEvent != null)
        {
            animationEndEvent();
        }
    }
}
