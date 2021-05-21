using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace core.zqc.bombs
{
    [RequireComponent(typeof(Rigidbody))]
    public class Bomb : PushableObject
    {
        public float explosionTime;
        public float explosionRange;

        bool freezeCountdown = false;

        public GameObject ExplosionFx;

        List<Team> friendlyList = new List<Team>();

        protected override void Awake()
        {
            base.Awake();

            type = CarryType.Bomb;
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
        IEnumerator ExplosionFxPlay()
		{
            ExplosionFx.SetActive(true);
            yield return new WaitForSeconds(0.95f);
            Destroy(gameObject);
        }

        [PunRPC]
        public void DelayShoot(Vector3 speed, float delay)
        {
            StartCoroutine(Shoot(speed, delay));
        }

        [PunRPC]
        //todo:
        /// <summary>
        /// Set Lock Sync For online 
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        IEnumerator Shoot(Vector3 speed, float delay)
        {
            SetPositionLock(true);             // 防止动画期间炸弹发生位移
            yield return new WaitForSeconds(delay);
            SetPositionLock(false);
            GetComponent<Rigidbody>().AddForce(speed, ForceMode.VelocityChange);
        }

        [PunRPC]
        public void StopExplosionCountdown()
        {
            freezeCountdown = true;
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