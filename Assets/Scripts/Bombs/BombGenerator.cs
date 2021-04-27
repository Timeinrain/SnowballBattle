using System.Collections;
using UnityEngine;

namespace core.zqc.bombs
{
    public class BombGenerator : MonoBehaviour
    {
        [Tooltip("����Ϊfalse���޵�ҩ,true�����ĵ�ҩ���")]
        public bool useAmmunition = false;

        public float bombsFallingInterval = 10f;       // ����ը��������ʱ��
        public int bombsFallingNumber = 10;            // ÿ�ε���ı���ը���������޵�ҩ����£�
        //public GameObject obiSolver;                 // �����solver��������ֻ��Ҫ����һ��
        public GameObject bombPrefab;                  // ����ը��Ԥ�Ƽ�

        int ammunitionStock = 0;

        [System.Serializable]
        public class GeneratingArea
        {
            public float xWidth;
            public float zWidth;
        }

        public GeneratingArea generatingArea = new GeneratingArea();

        bool inGame = true;

        void Start()
        {
            StartCoroutine(BombFallingCycle());
        }

        IEnumerator BombFallingCycle()
        {
            while (inGame)
            {
                // ����ը��
                int generateNum;
                if (useAmmunition)
                {
                    generateNum = ammunitionStock;
                    ammunitionStock = 0;
                }
                else
                {
                    generateNum = bombsFallingNumber;
                }

                for (int i = 0; i < generateNum; i++)
                {
                    GenerateBomb();
                }

                yield return new WaitForSeconds(bombsFallingInterval);
            }
        }

        void GenerateBomb()
        {
            Vector3 position = new Vector3(
                Random.Range(transform.position.x - generatingArea.xWidth / 2, transform.position.x + generatingArea.xWidth / 2),
                transform.position.y,
                Random.Range(transform.position.z - generatingArea.zWidth / 2, transform.position.z + generatingArea.zWidth / 2));
            GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);
            //bomb.transform.parent = obiSolver.transform;
        }

        public void AddBomb()
        {
            ammunitionStock++;
        }
    }
}