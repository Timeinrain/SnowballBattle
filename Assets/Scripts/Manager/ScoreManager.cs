using Photon.Pun;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// ������������Ϸʱ��ͳ�ƣ����з���ͳ�ƣ����������ͻ��˹㲥�������ṩ������ʾ
/// ���������ͻ��ˣ����ܷ����㲥���ṩ������ʾ
/// </summary>
public class ScoreManager : MonoBehaviourPun
{
    public Text redTeamKillCount;
    public Text greenTeamKillCount;

    public static ScoreManager Instance;

    public bool IsGameStart { get; private set; } = false;

    class TeamScoreInfo
    {
        public int killCount = 0;
    }
    class PlayerScoreInfo
    {
        public int fillCannonCount = 0;
        public int getBombCount = 0;
        public int deathCount = 0;
        public int hurtCount = 0;
    }

    private Player currentPlayer;
    private Dictionary<string, Team> idTeamTable = new Dictionary<string, Team>();
    private Dictionary<Team, TeamScoreInfo> teamScores = new Dictionary<Team, TeamScoreInfo>();
    private Dictionary<string, PlayerScoreInfo> playerScores = new Dictionary<string, PlayerScoreInfo>();

    private void Start()
    {
        Instance = this;
        currentPlayer = InOutGameRoomInfo.Instance.GetPlayerByName(PhotonNetwork.LocalPlayer.NickName);

        // ��ʼ��UI
        redTeamKillCount.text = "0";
        greenTeamKillCount.text = "0";
    }

    /// <summary>
    /// ��ʼ��Ϸ����ʼ����������
    /// </summary>
    public void StartScoreCount()
    {
        IsGameStart = true;

        foreach(var player in InOutGameRoomInfo.Instance.inRoomPlayerInfos)
        {
            AddCharacter(player.team, player.Instance.GetComponent<Character>());
        }
    }

    /// <summary>
    /// ������Ϸ����λ��������
    /// </summary>
    public void EndScoreCount()
    {
        IsGameStart = false;

        idTeamTable = new Dictionary<string, Team>();
        teamScores = new Dictionary<Team, TeamScoreInfo>();
        playerScores = new Dictionary<string, PlayerScoreInfo>();
}

    private void AddCharacter(Team team, Character character)
    {
        idTeamTable.Add(character.id, team);
        if (!teamScores.ContainsKey(team))
            teamScores.Add(team, new TeamScoreInfo());
        playerScores.Add(character.id, new PlayerScoreInfo());
        RegisterEventsForCharacter(character);
    }

    private void RegisterEventsForCharacter(Character character)
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)  // �����¼�ֻ�������Ͻ�������
            return;

        character.died += AddDeathCount;
        character.damaged += AddHurtCount;
        character.filledCannon += AddFillCannonCount;
        character.gotBomb += AddGetBombCount;
    }

    private void AddDeathCount(string id)
    {
        Team team = idTeamTable[id];
        Team hostile = GetHostileTeam(team);
        if (!playerScores.ContainsKey(id))
        {
            playerScores.Add(id, new PlayerScoreInfo());
        }
        playerScores[id].deathCount++;
        if (!teamScores.ContainsKey(hostile))
        {
            teamScores.Add(hostile, new TeamScoreInfo());
        }
        teamScores[hostile].killCount++;  // ��������жԵĶ������ӻ�ɱ��
        UpdateScoreUI();

        // ��ͻ��˷���ͬ��
        photonView.RPC("SyncKillCount", RpcTarget.Others, hostile, teamScores[hostile].killCount);
        RequesetPlayerInfoSync(id);
    }

    private void AddFillCannonCount(string id)
    {
        playerScores[id].fillCannonCount++;
        RequesetPlayerInfoSync(id);
    }

    private void AddGetBombCount(string id)
    {
        playerScores[id].getBombCount++;
        RequesetPlayerInfoSync(id);
    }

    private void AddHurtCount(string id, int damage)
    {
        playerScores[id].hurtCount++;
        RequesetPlayerInfoSync(id);
    }

    [PunRPC]
    private void SyncKillCount(Team team, int value)
    {
        if (!teamScores.ContainsKey(team))
        {
            teamScores.Add(team, new TeamScoreInfo());
        }
        teamScores[team].killCount = value;
        UpdateScoreUI();
    }

    /// <summary>
    /// ��ͻ��˷���ͬ��
    /// </summary>
    private void RequesetPlayerInfoSync(string id)
    {
        photonView.RPC("SyncPlayerInfo", RpcTarget.Others,
            id,
            playerScores[id].fillCannonCount,
            playerScores[id].getBombCount,
            playerScores[id].deathCount,
            playerScores[id].hurtCount);
    }

    [PunRPC]
    private void SyncPlayerInfo(string id, int fillCannonCount, int getBombCount, int deathCount, int hurtCount)
    {
        if (!playerScores.ContainsKey(id))
        {
            playerScores.Add(id, new PlayerScoreInfo());
        }
        playerScores[id].fillCannonCount = fillCannonCount;
        playerScores[id].getBombCount = getBombCount;
        playerScores[id].deathCount = deathCount;
        playerScores[id].hurtCount = hurtCount;
    }

    private static Team GetHostileTeam(Team team)
    {
        if (team == Team.Red) return Team.Green;
        if (team == Team.Green) return Team.Red;
        else return Team.Null;
    }

    /// <summary>
    /// ��ȡ�����ɱ����
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public int GetTeamKillCount(Team team)
    {
        if (!teamScores.ContainsKey(team))
        {
            return 0;
        }
        return teamScores[team].killCount;
    }

    public int GetPlayerDeathCount(string id)
    {
        if (!playerScores.ContainsKey(id))
        {
            return 0;
        }
        return playerScores[id].deathCount;
    }

    public int GetPlayerFillCannonCount(string id)
    {
        if (!playerScores.ContainsKey(id))
        {
            return 0;
        }
        return playerScores[id].fillCannonCount;
    }

    public int GetPlayerGetBombCount(string id)
    {
        if (!playerScores.ContainsKey(id))
        {
            return 0;
        }
        return playerScores[id].getBombCount;
    }

    public int GetPlayerHurtCount(string id)
    {
        if (!playerScores.ContainsKey(id))
        {
            return 0;
        }
        return playerScores[id].hurtCount;
    }

    /// <summary>
    /// ��ȡ���˷��ܷ֣���ȥ�Ŷӷ֣�
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public int GetPlayerTotalScore(string id)
    {
        return
            GetPlayerGetBombCount(id) * 3 +
            GetPlayerFillCannonCount(id) * 10 -
            GetPlayerHurtCount(id) * 3 -
            GetPlayerDeathCount(id) * 5;
    }

    public Team GetTeam(bool isWinner)
	{
		if (GetTeamKillCount(Team.Red) > GetTeamKillCount(Team.Green))
		{
            return Team.Red;
		}
		else
		{
            return Team.Green;
		}
	}

    public void UpdateScoreUI()
    {
        redTeamKillCount.text = GetTeamKillCount(Team.Red).ToString();
        greenTeamKillCount.text = GetTeamKillCount(Team.Green).ToString();
    }

}
