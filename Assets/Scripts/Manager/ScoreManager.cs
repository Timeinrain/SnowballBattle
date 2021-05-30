using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 对于主机：游戏时间统计，进行分数统计，并向其他客户端广播分数，提供分数显示
/// 对于其他客户端：接受分数广播，提供分数显示
/// </summary>
public class ScoreManager : MonoBehaviourPun
{
    public Text textKillCount;

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
        textKillCount.text = "Team Kill Count: 0";
    }

    private void Update()
    {
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
        playerScores[id].deathCount++;
        teamScores[hostile].killCount++;  // 死亡会给敌对的队伍增加击杀数
        UpdateScoreUI();

        // 向客户端发起同步
        photonView.RPC("SyncKillCount", RpcTarget.Others, hostile, teamScores[hostile].killCount);
    }

    private void AddFillCannonCount(string id)
    {
        playerScores[id].fillCannonCount++;
    }

    private void AddGetBombCount(string id)
    {
        playerScores[id].getBombCount++;
    }

    private void AddHurtCount(string id, int damage)
    {
        playerScores[id].hurtCount++;
    }

    [PunRPC]
    private void SyncKillCount(Team team, int value)
    {
        teamScores[team].killCount = value;
        UpdateScoreUI();
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
        return teamScores[team].killCount;
    }

    public void UpdateScoreUI()
    {
        textKillCount.text = "Team Kill Count:" + GetTeamKillCount(currentPlayer.team).ToString();
    }

}
