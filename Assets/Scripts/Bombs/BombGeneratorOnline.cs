using System.Collections;
using UnityEngine;
using core.zqc.bombs;
using Photon.Pun;

public class BombGeneratorOnline : MonoBehaviourPun
{
	[Tooltip("����Ϊfalse���޵�ҩ,true�����ĵ�ҩ���")]
	public bool useAmmunition = false;

	public float bombsFallingInterval = 10f;       // ����ը��������ʱ��
	public int bombsFallingNumber = 10;            // ÿ�ε���ı���ը���������޵�ҩ����£�

	public GameObject bombPrefab;                  // ����ը��Ԥ�Ƽ�

	int ammunitionStock = 0;
	bool hasOwner = false;
	Team owner;

	[System.Serializable]
	public class GeneratingArea
	{
		public float xWidth;
		public float zWidth;
	}

	public GeneratingArea generatingArea = new GeneratingArea();

	bool inGame = true;

	public void StartGenerateBomb()
	{
		if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
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
		GameObject bomb
		= PhotonNetwork.Instantiate("DefaultBomb", position, Quaternion.identity);
		bomb.GetComponent<Rigidbody>().velocity = new Vector3(Random.value > 0.5 ? 1 : -1, 0, Random.value > 0.5 ? 1 : -1);
		if (hasOwner)
		{
			bomb.GetComponent<Bomb>().AddAlly(owner);
		}
	}

	public void AddBomb(int num = 1)
	{
		ammunitionStock += num;
	}

	/// <summary>
	/// Set owner Team
	/// </summary>
	/// <param name="team"></param>
	public void SetOwner(Team team)
	{
		hasOwner = true;
		owner = team;
	}

	private void OnDrawGizmosSelected()
	{
		const float height = 30f;
		Vector3 center = transform.position;
		center.y -= height / 2;
		Gizmos.DrawWireCube(center, new Vector3(generatingArea.xWidth, height, generatingArea.zWidth));
	}
}