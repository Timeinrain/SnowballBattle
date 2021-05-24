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
    public float shootCycleTime;           // �ڵ����䴦��������̼��ʱ�䣬���������������֮��
    public float fillDelay = 5f;           // ���ڵ����뵽���ڲ��Ŷ�����ʱ�䣨���˹�����ʱ�䣩
    public float fireDelay = 1.1f;         // ���ڴ���䵽�����ڵ��Ķ������ʱ��
    public Transform bombShootPosition;    // ����ĳ�ʼλ��
    public float bombShootSpeed;           // ������ٶ�
    public float maxFalseBombHeight;       // ����ը�������˸߶�ʱ������

    private Queue<GameObject> bombsPool = new Queue<GameObject>();          // �ȴ������ը���ض���
    private List<GameObject> falseBombsWatchList = new List<GameObject>();  // ���ӵķ���ļ�ը��

    private void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bomb"))
        {
            if (falseBombsWatchList.Contains(other.gameObject))
                return;
            bombsPool.Enqueue(other.gameObject);
            other.GetComponent<Bomb>().StopCarrierPushing();
            other.gameObject.gameObject.SetActive(false);  // ����ը��
        }
    }

    private void Fire()
    {
        Debug.Log("fire!!!!!!");
    }

    private float shootTimer = 0f;
    private bool canFire = true;
    private void Update()
    {
        // ��ʱ������ȴ
        if (!canFire)
        {
            if (shootTimer >= shootCycleTime)
            {
                shootTimer = 0f;
                canFire = true;
            }
            else
            {
                shootTimer += Time.deltaTime;
            }            
        }

        if (canFire && bombsPool.Count > 0)
        {
            // ���µ��ڵ�ִ��һϵ�ж���
            GameObject bomb = bombsPool.Dequeue();
            StartCoroutine(ShootFalseBomb(bomb));
            canFire = false;
        }

        HandleBombsLifespan();
    }

    private IEnumerator ShootFalseBomb(GameObject bomb)
    {
        // ���Ű��˹�����
        porterAnimator.SetTrigger("FillCannon");    
        yield return new WaitForSeconds(fillDelay);

        // ���Ŵ��ڶ���
        cannonAnimator.SetTrigger("Fire");          
        yield return new WaitForSeconds(fireDelay);

        // �����ڵ�������
        falseBombsWatchList.Add(bomb);
        bomb.SetActive(true);
        bomb.transform.position = bombShootPosition.position;
        bomb.GetComponent<Rigidbody>().velocity = new Vector3(0f, bombShootSpeed, 0f);
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
                linkedGenerator.AddBombAndGenerate(owner, 3);
            }
        }
    }

    private void OnValidate()
    {
        if (shootCycleTime < fillDelay + fireDelay)
        {
            shootCycleTime = fillDelay + fireDelay;
        }
    }
}
