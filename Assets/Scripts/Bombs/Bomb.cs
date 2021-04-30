using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.zqc.bombs
{
    public class Bomb : MonoBehaviour
    {
        public float explosionTime;
        public float explosionRange;
        public GameObject snowPathFX;

        Rigidbody bombRigidbody;
        BombController carrier = null;
        bool freezeCountdown = false;

        public GameObject ExplosionFx;

        List<Team> friendlyList = new List<Team>();

        private void Awake()
        {
            bombRigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            StartCoroutine(ExplosionCountdown());
        }

        private IEnumerator ExplosionCountdown()
        {
            yield return new WaitForSeconds(explosionTime);
            if (freezeCountdown) yield break;
            Explode();
        }

        private void Explode()
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, explosionRange, LayerMask.GetMask("Player"));
            foreach(var col in cols)
            {
                Character character = col.GetComponent<Character>();
                if (character != null)
                {
                    if (friendlyList.Contains(character.GetTeam())) return;
                    character.DealDamage();
                    Debug.Log(string.Format("{0}'s health was reduced to {1}", character.ToString(), character.Health));
                }
            }
            StartCoroutine(ExplosionFxPlay());
        }

        /// <summary>
        /// Explosion FX
        /// </summary>
        /// 
        IEnumerator ExplosionFxPlay()
		{
            ExplosionFx.SetActive(true);
            yield return new WaitForSeconds(0.95f);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (carrier != null)
                carrier.DetachCurrentBomb();
        }


        public void UpdateTransform(Vector3 position, Quaternion rotation)
        {
            position.y = transform.position.y;
            bombRigidbody.MovePosition(position);
            bombRigidbody.MoveRotation(rotation);
        }

        public void DelayShoot(Vector3 speed, float delay)
        {
            StartCoroutine(Shoot(speed, delay));
        }

        IEnumerator Shoot(Vector3 speed, float delay)
        {
            SetPositionLock(true);             // 防止动画期间炸弹发生位移
            yield return new WaitForSeconds(delay);
            SetPositionLock(false);
            bombRigidbody.AddForce(speed, ForceMode.VelocityChange);
        }

        /// <summary>
        /// 角色获得炸弹
        /// </summary>
        public void OnAttached(BombController bombControl)
        {
            carrier = bombControl;
        }

        /// <summary>
        /// 角色发射/丢弃炸弹等触发事件
        /// </summary>
        public void OnDetached()
        {
            carrier = null;
        }

        public void StopExplosionCountdown()
        {
            freezeCountdown = true;
        }

        /// <summary>
        /// 锁定/解锁炸弹位置，控制其是否能发生位移
        /// </summary>
        /// <param name="flag"></param>
        public void SetPositionLock(bool flag)
        {
            if (flag)
            {
                bombRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                bombRigidbody.constraints = RigidbodyConstraints.None;
            }
        }

        public void AddAlly(Team team)
        {
            if (!friendlyList.Contains(team))
                friendlyList.Add(team);
        }

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.gameObject.CompareTag("Ground"))
			{
                snowPathFX.SetActive(true);
			}
		}
	}
}