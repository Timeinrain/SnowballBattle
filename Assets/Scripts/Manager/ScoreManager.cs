using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

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
                // 说明该队所有人都被冻住了
                teamInfo.Value.isFailed = true;
            }
            
        }
    }

    public void AddCharacter(Team team, Character character)
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
}
