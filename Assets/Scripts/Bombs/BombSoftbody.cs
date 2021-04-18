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
        /// ����ƶ�����ı�y���꣬ʼ���ڵ������ƶ�
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
        /// ��ɫ���ը��
        /// </summary>
        public void OnAttached()
        {
        }

        /// <summary>
        /// ��ɫ����/����ը���ȴ����¼�
        /// </summary>
        public void OnDetached()
        {
        }
    }
}