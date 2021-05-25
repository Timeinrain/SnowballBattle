using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���ƴ��ڵĶ����������¼��ص�
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
