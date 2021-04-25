using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.zqc.bombs
{
    public class Bomb : MonoBehaviour
    {
        public float explosionTime;

        Rigidbody bombRigidbody;
        BombControl carrier = null;

        private void Awake()
        {
            bombRigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            StartCoroutine(ExplosionCountdown());
        }

        private void OnTriggerEnter(Collider other)
        {
            AmmunitionDepot depot = other.GetComponent<AmmunitionDepot>();
            if (depot != null)
            {
                if (carrier != null) carrier.DetachCurrentBomb();
                depot.FillBomb(this);
            }
        }

        private IEnumerator ExplosionCountdown()
        {
            yield return new WaitForSeconds(explosionTime);

            Debug.Log(string.Format("Bomb exploded at {0}", transform.position.ToString()));

            carrier.DetachCurrentBomb();
            Destroy(gameObject);
        }


        public void UpdateTransform(Vector3 position, Quaternion rotation)
        {
            position.y = transform.position.y;
            bombRigidbody.MovePosition(position);
            bombRigidbody.MoveRotation(rotation);
        }

        public void Shoot(Vector3 speed)
        {
            bombRigidbody.AddForce(speed, ForceMode.VelocityChange);
        }

        /// <summary>
        /// ��ɫ���ը��
        /// </summary>
        public void OnAttached(BombControl bombControl)
        {
            carrier = bombControl;
        }

        /// <summary>
        /// ��ɫ����/����ը���ȴ����¼�
        /// </summary>
        public void OnDetached()
        {
            carrier = null;
        }
    }
}