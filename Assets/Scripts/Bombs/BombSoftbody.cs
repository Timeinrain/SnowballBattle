using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.zqc.bombs
{
    [RequireComponent(typeof(ObiActor))]
    public class BombSoftbody : MonoBehaviour
    {
        public float explosionTime;

        ObiActor actor;

        private void Awake()
        {
            actor = GetComponent<ObiActor>();
        }

        private void Start()
        {
            StartCoroutine(ExplosionCountdown());
        }

        private IEnumerator ExplosionCountdown()
        {
            yield return new WaitForSeconds(explosionTime);

            Debug.Log(string.Format("Bomb exploded at {0}", transform.position.ToString()));

            Destroy(gameObject);
        }

        /// <summary>
        /// 这个移动不会改变y坐标，始终在地面上移动
        /// </summary>
        /// <param name="movement"></param>
        public void GroundMove(Vector3 movement)
        {
            actor.AddForce(movement, ForceMode.Acceleration);
        }

        public void Shoot(Vector3 speed)
        {
            speed.y = 0f;
            actor.AddForce(speed, ForceMode.VelocityChange);
        }

        /// <summary>
        /// 角色获得炸弹
        /// </summary>
        public void OnAttached()
        {
        }

        /// <summary>
        /// 角色发射/丢弃炸弹等触发事件
        /// </summary>
        public void OnDetached()
        {
        }
    }
}