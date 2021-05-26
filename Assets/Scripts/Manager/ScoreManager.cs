using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviourPun
{
    public static ScoreManager Instance;

    public bool IsGameStart { get; private set; } = false;

    class TeamScoreInfo
    {
        public int playerNum = 0;
        public int frozenNum = 0;
        public int killCount = 0;
        public int deathCount = 0;
        public bool isFailed = false;
    }

    private Dictionary<string, Team> idTeamTable = new Dictionary<string, Team>();
    private Dictionary<Team, TeamScoreInfo> teamScores = new Dictionary<Team, TeamScoreInfo>();

    private void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        foreach(var teamInfo in teamScores)
        {
            if(teamInfo.Value.playerNum == teamInfo.Value.frozenNum)
            {
                // ˵���ö������˶�����ס��
                teamInfo.Value.isFailed = true;
            }
            
        }
    }

    /// <summary>
    /// ��ʼ��Ϸ����ʼ����������
    /// </summary>
    [PunRPC]
    public void StartScoreCount()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;

        IsGameStart = true;

        foreach(var player in InOutGameRoomInfo.Instance.inRoomPlayerInfos)
        {
            AddCharacter(player.team, player.Instance.GetComponent<Character>());
        }
        
    }

    /// <summary>
    /// ������Ϸ����λ��������
    /// </summary>
    [PunRPC]
    public void EndScoreCount()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;

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
        character.died += AddDeathCount;
        character.frozen += AddFrozen;
        character.unfrozen += AddUnfrozen;
    }

    private void AddDeathCount(string id)
    {
        if (!idTeamTable.ContainsKey(id))
        {
            return;
        }
        Team team = idTeamTable[id];

        if (!teamScores.ContainsKey(team))
        {
            return;
        }
        teamScores[team].deathCount++;
    }

    private void AddFrozen(string id)
    {
        if (!idTeamTable.ContainsKey(id))
        {
            return;
        }
        Team team = idTeamTable[id];

        if (!teamScores.ContainsKey(team))
        {
            return;
        }
        teamScores[team].frozenNum++;
    }

    private void AddUnfrozen(string id)
    {
        if (!idTeamTable.ContainsKey(id))
        {
            return;
        }
        Team team = idTeamTable[id];

        if (!teamScores.ContainsKey(team))
        {
            return;
        }
        teamScores[team].frozenNum--;
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
    /// (��ѩ������)��������������˶�������������true
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public bool CheckTeamFail(Team team)
    {
        return teamScores[team].isFailed;
    }
}
