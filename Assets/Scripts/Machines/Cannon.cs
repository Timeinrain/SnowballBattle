using core.zqc.bombs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class Cannon : MonoBehaviour
{
    public BombGeneratorOnline linkedGenerator;
    public Animator cannonAnimator;
    public Animator porterAnimator;        // �ڵ����˹�
    public Team owner;

    [Header("Shoot Settings")]
    public float fillDelay = 5f;           // ���ڵ����뵽���ڲ��Ŷ�����ʱ�䣨���˹�����ʱ�䣩
    public float fireDelay = 1.1f;         // ���ڴ���䵽�����ڵ��Ķ������ʱ��
    public float fireTime = 2.15f;         // ���ڷ��䶯��������ʱ��
    public Transform bombShootPosition;    // ����ĳ�ʼλ��
    public float bombShootSpeed;           // ������ٶ�
    public float falseBombLifespan;        // ����ը������������

    private Queue<GameObject> bombsPool = new Queue<GameObject>();          // �ȴ������ը���ض���
    private List<GameObject> falseBombsWatchList = new List<GameObject>();  // ���ӵķ���ļ�ը��
    private float maxFalseBombHeight;

    private void Start()
    {
        // ���㵽0�ٶȵ�ʱ��
        float upperBound = bombShootSpeed / Physics.gravity.magnitude;
        if (falseBombLifespan > upperBound)
            falseBombLifespan = upperBound;
        maxFalseBombHeight = bombShootSpeed * falseBombLifespan - 0.5f * Physics.gravity.magnitude * falseBombLifespan * falseBombLifespan;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bomb"))
        {
            if (falseBombsWatchList.Contains(other.gameObject))
                return;

            int generateNum = 3;
            for(int i = 0; i < generateNum; i++)
            {
                // ��¡ԭ�е�ը��
                GameObject bomb = Instantiate(other.gameObject);
                bomb.GetComponent<Bomb>().SetPushable(false);
                bomb.GetComponent<Bomb>().StopExplosionCountdown();
                bomb.GetComponent<Bomb>().StopCarrierPushing();
                bomb.SetActive(false);     // ����ը��
                bombsPool.Enqueue(bomb.gameObject);
            }
            Destroy(other.gameObject);
        }
    }

    private bool isLoaded = false;  // �Ѿ������ڵ�
    private void Update()
    {
        if (bombsPool.Count > 0 && !isLoaded)
        {
            isLoaded = true;
            StartCoroutine(ShootAllBombs());
        }

        HandleBombsLifespan();
    }

    private IEnumerator ShootAllBombs()
    {
        // ���Ű��˹�����
        porterAnimator.SetTrigger("FillCannon");    
        yield return new WaitForSeconds(fillDelay);

        GameObject bomb = bombsPool.Count > 0 ? bombsPool.Dequeue() : null;
        while(bomb != null)
        {
            // ���Ŵ��ڶ���
            cannonAnimator.SetTrigger("Fire");
            yield return new WaitForSeconds(fireDelay);

            // �����ڵ�������
            falseBombsWatchList.Add(bomb);
            bomb.SetActive(true);
            bomb.transform.position = bombShootPosition.position;
            bomb.GetComponent<Rigidbody>().velocity = new Vector3(0f, bombShootSpeed, 0f);
            yield return new WaitForSeconds(fireTime - fireDelay);

            // ��ȡ�ڵ������¸��ڵ�
            bomb = bombsPool.Count > 0 ? bombsPool.Dequeue() : null;
        }

        isLoaded = false;
    }

    /// <summary>
    /// ������ļ�ը������������
    /// </summary>
    private void HandleBombsLifespan()
    {
        for(int i = falseBombsWatchList.Count - 1; i >= 0; i--)
        {
            GameObject bomb = falseBombsWatchList[i];
            if (bomb.transform.position.y > maxFalseBombHeight)
            {
                // �����߶Ⱥ����٣�������������BombGenerator
                falseBombsWatchList.RemoveAt(i);
                Destroy(bomb);
                linkedGenerator.AddBombAndGenerate(owner, 1);
            }
        }
    }

    private void OnValidate()
    {
        if(fireTime < fireDelay)
        {
            fireTime = fireDelay;
        }
    }
}
