using System.Collections;
using UnityEngine;
using core.zqc.bombs;
using Photon.Pun;
using System.Collections.Generic;

public class BombGeneratorOnline : MonoBehaviourPun
{
	public float bombsFallingInterval = 10f;       // 冰壶炸弹降落间隔时间
	public int bombsFallingNumber = 10;            // 每次掉落的冰壶炸弹数（无限弹药情况下）
	public bool autoGenerate = true;               // 是否自动生成炸弹，取消选择后只会被动接受大炮传入的炸弹
	public GameObject bombString;

	public static BombGeneratorOnline Instance;

	[System.Serializable]
	public class GeneratingArea
	{
		public float xOffset;
		public float zOffset;
		public float xWidth;
		public float zWidth;
	}

	public void Awake()
	{
		if (Instance == null)
			Instance = this;
	}

	/// <summary>
	/// 最终会在所有生成区域中按照区域面积分摊概率进行炸弹生成
	/// </summary>
	public GeneratingArea[] generatingArea;
	/// <summary>
	/// 每个区域可能生成炸弹的概率，在Start的时候计算
	/// </summary>
	private List<float> areaChance = new List<float>();

	bool inGame = true;

    private void Start()
    {
		float totalArea = 0f;
        foreach(var area in generatingArea)
        {
			totalArea += area.xWidth * area.zWidth;
        }
		for(int i = 0; i < generatingArea.Length; i++)
        {
			areaChance.Add(generatingArea[i].xWidth * generatingArea[i].zWidth / totalArea);
        }
    }

    public void StartGenerateBomb()
	{
		if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
			StartCoroutine(BombFallingCycle());
	}

	IEnumerator BombFallingCycle()
	{
		while (inGame && autoGenerate)
		{
			for (int i = 0; i < bombsFallingNumber; i++)
			{
				GenerateBomb();
			}

			yield return new WaitForSeconds(bombsFallingInterval);
		}
	}

	public void GenerateBomb(Team owner = Team.Null, int number = 1)
	{
		if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
		{
			for (int i = 0; i < number; i++)
			{
				GeneratingArea area = generatingArea[GetGenerateAreaIndex()];
				Vector3 position = new Vector3(
					Random.Range(transform.position.x + area.xOffset - area.xWidth / 2, transform.position.x + area.xOffset + area.xWidth / 2),
					transform.position.y,
					Random.Range(transform.position.z + area.zOffset - area.zWidth / 2, transform.position.z + area.zOffset + area.zWidth / 2));

				Vector3 initialVelocity = new Vector3(Random.value > 0.5 ? 1 : -1, 0, Random.value > 0.5 ? 1 : -1);
				PhotonNetwork.Instantiate(bombString.name, position, Quaternion.identity, 0, new object[] { initialVelocity, owner });
			}
		}
	}

	private int GetGenerateAreaIndex()
    {
		float rand = Random.value;
		float totalChance = 0f;
		for(int i = 0; i < areaChance.Count; i++)
        {
			totalChance += areaChance[i];
			if (rand < totalChance)
            {
				return i;
            }
        }
		return (areaChance.Count - 1);
	}

	private void OnDrawGizmosSelected()
	{
		const float height = 30f;

		foreach(var area in generatingArea)
        {
			Vector3 center = transform.position;
			center.x += area.xOffset;
			center.y -= height / 2;
			center.z += area.zOffset;
			Gizmos.DrawWireCube(center, new Vector3(area.xWidth, height, area.zWidth));
		}
	}

    private void OnValidate()
    {
		foreach (var area in generatingArea)
		{
			if (area.xWidth < 0)
				area.xWidth = 0f;
			if (area.zWidth < 0)
				area.zWidth = 0f;
		}
	}
}