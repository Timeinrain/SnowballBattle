using System.Collections;
using UnityEngine;
using core.zqc.bombs;
using Photon.Pun;

public class BombGeneratorOnline : MonoBehaviourPun
{
	public float bombsFallingInterval = 10f;       // ����ը��������ʱ��
	public int bombsFallingNumber = 10;            // ÿ�ε���ı���ը���������޵�ҩ����£�

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

	GameObject GenerateBomb(string resourceName = "DefaultBomb")
	{
		Vector3 position = new Vector3(
			Random.Range(transform.position.x - generatingArea.xWidth / 2, transform.position.x + generatingArea.xWidth / 2),
			transform.position.y,
			Random.Range(transform.position.z - generatingArea.zWidth / 2, transform.position.z + generatingArea.zWidth / 2));
		GameObject bomb
		= PhotonNetwork.Instantiate(resourceName, position, Quaternion.identity);
		bomb.GetComponent<Rigidbody>().velocity = new Vector3(Random.value > 0.5 ? 1 : -1, 0, Random.value > 0.5 ? 1 : -1);
		return bomb;
	}

	public void AddBombAndGenerate(Team owner, int number)
	{
		string resouceName;
		switch (owner)
		{
			case Team.Blue:
				resouceName = "BlueBomb";
				break;
			case Team.Red:
				resouceName = "RedBomb";
				break;
			case Team.Yellow:
				resouceName = "YellowBomb";
				break;
			case Team.Green:
				resouceName = "GreenBomb";
				break;
			default:
				resouceName = "";
				break;
		}
		for (int i = 0; i < number; i++)
		{
			GameObject bombObject = GenerateBomb(resouceName);
			bombObject.GetComponent<Bomb>().AddAlly(owner);
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