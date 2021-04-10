using System.Collections;
using UnityEngine;

public class BombGenerator : MonoBehaviour
{
    public float bombsFallingInterval = 10f;       // ����ը��������ʱ��
    public int bombsFallingNumber = 10;            // ÿ�ε���ı���ը����
    public GameObject bombPrefab;                  // ����ը��Ԥ�Ƽ�

    [System.Serializable]
    public class GeneratingArea
    {
        public float minX;
        public float maxX;
        public float minZ;
        public float maxZ;
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
            for (int i = 0; i < bombsFallingNumber; i++)
            {
                Vector3 position = new Vector3(
                    Random.Range(generatingArea.minX, generatingArea.maxX),
                    transform.position.y,
                    Random.Range(generatingArea.minZ, generatingArea.maxZ));
                GameObject.Instantiate(bombPrefab, position, Quaternion.identity);
            }

            yield return new WaitForSeconds(bombsFallingInterval);
        }
    }
}
