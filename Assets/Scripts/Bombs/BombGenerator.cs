using System.Collections;
using UnityEngine;

public class BombGenerator : MonoBehaviour
{
    public float bombsFallingInterval = 10f;       // 冰壶炸弹降落间隔时间
    public int bombsFallingNumber = 10;            // 每次掉落的冰壶炸弹数
    public GameObject bombPrefab;                  // 冰壶炸弹预制件

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
            // 生成炸弹
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
