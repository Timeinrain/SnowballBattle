using System.Collections;
using UnityEngine;
using core.zqc.bombs;
using Photon.Pun;

public class BombGeneratorOnline : MonoBehaviourPun
{
	public float bombsFallingInterval = 10f;       // 冰壶炸弹降落间隔时间
	public int bombsFallingNumber = 10;            // 每次掉落的冰壶炸弹数（无限弹药情况下）

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
			for (int i = 0; i < bombsFallingNumber; i++)
			{
				GenerateBomb();
			}

			yield return new WaitForSeconds(bombsFallingInterval);
		}
	}

	GameObject GenerateBomb()
	{
		Vector3 position = new Vector3(
			Random.Range(transform.position.x - generatingArea.xWidth / 2, transform.position.x + generatingArea.xWidth / 2),
			transform.position.y,
			Random.Range(transform.position.z - generatingArea.zWidth / 2, transform.position.z + generatingArea.zWidth / 2));
		GameObject bomb
		= PhotonNetwork.Instantiate("DefaultBomb", position, Quaternion.identity);
		bomb.GetComponent<Rigidbody>().velocity = new Vector3(Random.value > 0.5 ? 1 : -1, 0, Random.value > 0.5 ? 1 : -1);
		return bomb;
	}

	public void AddBombAndGenerate(Team owner, int number)
	{
		for (int i = 0; i < number; i++)
		{
			GameObject bombObject = GenerateBomb();
			bombObject.GetComponent<Bomb>().ChangeTeam(owner);
		}
	}

	private void OnDrawGizmosSelected()
	{
		const float height = 30f;
		Vector3 center = transform.position;
		center.y -= height / 2;
		Gizmos.DrawWireCube(center, new Vector3(generatingArea.xWidth, height, generatingArea.zWidth));
	}
}