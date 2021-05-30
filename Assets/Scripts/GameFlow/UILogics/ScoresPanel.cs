using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class ScoresPanel : MonoBehaviour
{
	public List<GameObject> posLeft;
	public List<GameObject> posRight;

	public List<bool> empty;

	private void OnEnable()
	{
		empty = new List<bool> { true, true, true, true, true, true, true, true };
	}

	public GameObject scorePrefabL;
	public GameObject scorePrefabR;

	public Text totalScoreLeft;
	public Text totalScoreRight;

	public int FindNextEmpty(bool red)
	{
		if (red)
		{
			for (int i = 0; i < 4; i++)
			{
				if (empty[i] == true)
				{
					empty[i] = false;
					return i;
				}
			}
			return -1;
		}
		else
		{
			for (int i = 4; i < 8; i++)
			{
				if (empty[i] == true)
				{
					empty[i] = false;
					return i;
				}
			}
			return -1;
		}
	}

	[Button]
	public void Insert(bool isRed, string name, string total, string fill, string attacked, string touch, bool isMvp = false)
	{
		if (isRed)
		{
			GameObject go = Instantiate(scorePrefabL, posLeft[FindNextEmpty(true)].transform);
			go.GetComponent<SettlementObject>().Init(name, total, fill, attacked, touch, isMvp);
		}
		else
		{
			GameObject go = Instantiate(scorePrefabR, posRight[FindNextEmpty(false) - 4].transform);
			go.GetComponent<SettlementObject>().Init(name, total, fill, attacked, touch, isMvp);
		}
	}

	[Button]
	public void StartSettle()
	{
		StartCoroutine(DisplayCount());
	}

	public IEnumerator DisplayCount()
	{
		int maxScoreR = 0;
		string maxScorePlayerIdR = "-1";
		int maxScoreG = 0;
		string maxScorePlayerIdG = "-1";

		foreach (var player in InOutGameRoomInfo.Instance.inRoomPlayerInfos)
		{
			if (player != null)
			{
				if (player.team == Team.Green)
				{
					if (ScoreManager.Instance.GetPlayerTotalScore(player.playerId) >= maxScoreG)
					{
						maxScoreG = ScoreManager.Instance.GetPlayerTotalScore(player.playerId);
						maxScorePlayerIdG = player.playerId;
					}
				}
				else
				{
					if (ScoreManager.Instance.GetPlayerTotalScore(player.playerId) >= maxScoreR)
					{
						maxScoreR = ScoreManager.Instance.GetPlayerTotalScore(player.playerId);
						maxScorePlayerIdR = player.playerId;
					}
				}
			}
		}
		foreach (var player in InOutGameRoomInfo.Instance.inRoomPlayerInfos)
		{
			Insert(player.team == Team.Red,
					player.playerId,
					ScoreManager.Instance.GetPlayerTotalScore(player.playerId).ToString(),
					ScoreManager.Instance.GetPlayerFillCannonCount(player.playerId).ToString(),
					ScoreManager.Instance.GetPlayerHurtCount(player.playerId).ToString(),
					ScoreManager.Instance.GetPlayerGetBombCount(player.playerId).ToString(),
					player.playerId == (player.team == Team.Red ? maxScorePlayerIdR : maxScorePlayerIdG)
					);
			yield return new WaitForSeconds(0.2f);
		}
		StartCoroutine(DisplayTotal());
	}

	IEnumerator DisplayTotal()
	{
		int left = 0;
		int right = 0;

		while (true)
		{
			if (left != int.Parse(ScoreManager.Instance.redTeamKillCount.text))
			{
				left++;
			}
			if (right != int.Parse(ScoreManager.Instance.greenTeamKillCount.text))
			{
				right++;
			}
			totalScoreLeft.text = left.ToString();
			totalScoreRight.text = right.ToString();
			if (left == int.Parse(ScoreManager.Instance.redTeamKillCount.text) && right == int.Parse(ScoreManager.Instance.greenTeamKillCount.text))
			{
				break;
			}
			yield return new WaitForSeconds(0.03f);
		}
	}
}
