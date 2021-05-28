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
        public int playerNum = 0;
        public int frozenNum = 0;
        public int deathCount = 0;
        public int killCount = 0;
    }

    private Player currentPlayer;
    private Dictionary<string, Team> idTeamTable = new Dictionary<string, Team>();
    private Dictionary<Team, TeamScoreInfo> teamScores = new Dictionary<Team, TeamScoreInfo>();

    private void Start()
    {
        Instance = this;
        currentPlayer = InOutGameRoomInfo.Instance.GetPlayerByName(PhotonNetwork.LocalPlayer.NickName);

        // 初始化UI
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            // 非主机情况
            // 这里直接视为只有红队和蓝队
            teamScores.Add(Team.Blue, new TeamScoreInfo());
            teamScores.Add(Team.Red, new TeamScoreInfo());
        }
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

        foreach(var key in teamScores.Keys)
        {
            teamScores[key] = new TeamScoreInfo();
        }
    }

    private void AddCharacter(Team team, Character character)
    {
        idTeamTable.Add(character.id, team);
        RegisterEventsForCharacter(character);

        if (!teamScores.ContainsKey(team))
            teamScores.Add(team, new TeamScoreInfo());

        teamScores[team].playerNum++;
    }

    private void RegisterEventsForCharacter(Character character)
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)  // 人物事件只在主机上进行侦听
            return;

        character.died += AddDeathCount;
        character.frozen += AddFrozen;
        character.unfrozen += AddUnfrozen;
    }

    private void AddDeathCount(string id)
    {
        Team team = idTeamTable[id];
        Team hostile = GetHostileTeam(team);
        teamScores[team].deathCount++;
        teamScores[hostile].killCount++;  // 死亡会给敌对的队伍增加击杀数
        UpdateScore();

        // 向客户端发起同步
        photonView.RPC("SyncKillCount", RpcTarget.Others, hostile, teamScores[hostile].killCount);
    }

    [PunRPC]
    private void SyncKillCount(Team team, int value)
    {
        teamScores[team].killCount = value;
        UpdateScore();
    }

    private void AddFrozen(string id)
    {
        Team team = idTeamTable[id];
        teamScores[team].frozenNum++;

        UpdateScore();
    }

    private void AddUnfrozen(string id)
    {
        Team team = idTeamTable[id];
        teamScores[team].frozenNum--;

        UpdateScore();
    }

    private static Team GetHostileTeam(Team team)
    {
        if (team == Team.Red) return Team.Blue;
        if (team == Team.Blue) return Team.Red;
        else return Team.Null;
    }

    /// <summary>
    /// 获取队伍总人数
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public int GetTeamPlayerNum(Team team)
    {
        return teamScores[team].playerNum;
    }

    /// <summary>
    /// 获取队伍被冰冻住人员的数量
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public int GetTeamFrozenNum(Team team)
    {
        return teamScores[team].frozenNum;
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

    public void UpdateScore()
    {
        textKillCount.text = "Team Kill Count:" + GetTeamKillCount(currentPlayer.team).ToString();
    }

}
