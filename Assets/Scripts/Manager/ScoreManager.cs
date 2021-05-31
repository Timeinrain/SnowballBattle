using Photon.Pun;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 对于主机：游戏时间统计，进行分数统计，并向其他客户端广播分数，提供分数显示
/// 对于其他客户端：接受分数广播，提供分数显示
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

        // 初始化UI
        redTeamKillCount.text = "0";
        greenTeamKillCount.text = "0";
    }

    /// <summary>
    /// 开始游戏并初始化队伍数据
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
    /// 结束游戏并复位所有数据
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
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)  // 人物事件只在主机上进行侦听
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
        teamScores[hostile].killCount++;  // 死亡会给敌对的队伍增加击杀数
        UpdateScoreUI();

        // 向客户端发起同步
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
    /// 向客户端发起同步
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
    /// 获取队伍击杀数量
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
    /// 获取个人分总分（除去团队分）
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
