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
        playerScores[id].deathCount++;
        teamScores[hostile].killCount++;  // ��������жԵĶ������ӻ�ɱ��
        UpdateScoreUI();

        // ��ͻ��˷���ͬ��
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
    /// ��ȡ�����ɱ����
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
