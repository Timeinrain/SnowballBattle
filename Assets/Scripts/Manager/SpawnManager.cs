using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance = null;
    public Transform[] redTeamSpawnPoints;
    public Transform[] blueTeamSpawnPoints;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    /// <summary>
    /// 每个客户端调用这个方法得到的出生点不一样，基于队伍顺序
    /// </summary>
    /// <returns></returns>
    public Transform GetSpawnPoint(Player player)
    {
        //todo:删除此行(本地测试)
        //return blueTeamSpawnPoints[0];

        List<Player> players = InOutGameRoomInfo.Instance.inRoomPlayerInfos;

        // 返回一个基于队伍顺序的数字
        int index = 0;
        foreach(var pl in players)
        {
            if(player.team == pl.team)
            {
                if (player == pl) break;
                index++;
            }
        }

        if (player.team == Team.Red)
        {
            return redTeamSpawnPoints[index];
        }
        else
        {
            return blueTeamSpawnPoints[index];
        }
    }

    private void OnDrawGizmos()
    {
        foreach(var t in redTeamSpawnPoints)
        {
            Gizmos.DrawIcon(t.position, "spawn_point.png");
        }
        foreach (var t in blueTeamSpawnPoints)
        {
            Gizmos.DrawIcon(t.position, "spawn_point.png");
        }
    }
}
