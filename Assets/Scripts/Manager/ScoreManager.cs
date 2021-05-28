using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ������������Ϸʱ��ͳ�ƣ����з���ͳ�ƣ����������ͻ��˹㲥�������ṩ������ʾ
/// ���������ͻ��ˣ����ܷ����㲥���ṩ������ʾ
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

        // ��ʼ��UI
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            // ���������
            // ����ֱ����Ϊֻ�к�Ӻ�����
            teamScores.Add(Team.Blue, new TeamScoreInfo());
            teamScores.Add(Team.Red, new TeamScoreInfo());
        }
        textKillCount.text = "Team Kill Count: 0";
    }

    private void Update()
    {
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
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)  // �����¼�ֻ�������Ͻ�������
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
        teamScores[hostile].killCount++;  // ��������жԵĶ������ӻ�ɱ��
        UpdateScore();

        // ��ͻ��˷���ͬ��
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
    /// ��ȡ����������
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public int GetTeamPlayerNum(Team team)
    {
        return teamScores[team].playerNum;
    }

    /// <summary>
    /// ��ȡ���鱻����ס��Ա������
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public int GetTeamFrozenNum(Team team)
    {
        return teamScores[team].frozenNum;
    }

    /// <summary>
    /// ��ȡ�����ɱ����
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
