using core.zqc.bombs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.zqc.players
{

    public class PlayerAnimator : MonoBehaviour
    {
	    Rigidbody rb;
	    Animator animator;

	    [Range(1, 100)]
	    public float speed;
	    [Range(10, 1000)]
	    public float rotationSpeed = 10;

	    bool inAnimation = false;
	    bool isKick = false;
	    bool isPushing = false;
		bool isFilling = false;

        float inputHorizontal = 0f;
        float inputVertical = 0f;

        void Start()
	    {
		    rb = GetComponent<Rigidbody>();
		    animator = GetComponentInChildren<Animator>();
	    }

	    private void FixedUpdate()
	    {
		    if (inAnimation) return;

		    rb.velocity = new Vector3(inputHorizontal, 0, inputVertical).normalized * speed + new Vector3(0, rb.velocity.y, 0);

		    Vector3 dir = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
		    if (dir.magnitude != 0)
		    {
			    Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
			    Quaternion lerp = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
			    rb.MoveRotation(lerp);
		    }
	    }

	    private void Update()
	    {
			// 处理动画结束回调
			AnimatorStateInfo animatorInfo;
			animatorInfo = animator.GetCurrentAnimatorStateInfo(0);
			if (animatorInfo.normalizedTime > 1.0f)
			{
                if (animatorInfo.IsName("Kick"))
                {
					OnKickExit();
                }
			}

			// 处理animator参数
		    animator.SetFloat("MovingSpeed", (new Vector2(rb.velocity.x, rb.velocity.z)).magnitude);
		    animator.SetBool("IsKick", isKick);
		    animator.SetBool("IsPushing", isPushing);
			animator.SetBool("IsFiring", isFilling);
	    }

		void OnKickExit()
        {

        }

		/// <summary>
		/// 设定前摇/后摇动画，inAnimation期间角色无法被控制
		/// </summary>
		/// <param name="flag"></param>
	    public void SetAnimationFlag(bool flag)
	    {
		    inAnimation = flag;
	    }

		public void SetInput(float horizontal, float vertical)
		{
			inputHorizontal = horizontal;
			inputVertical = vertical;
		}

		public void SetPushing()
        {
			isPushing = true;
			isKick = false;
        }

		public void SetKick()
        {
			isKick = true;
			isPushing = false;
        }

		public void SetFilling()
        {
			isFilling = true;
        }

		public void SetRotation(Quaternion rotation)
        {
			rb.transform.rotation = rotation;
        }
	}
}