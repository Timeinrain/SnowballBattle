using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class SettlementObject : MonoBehaviour
{
    public Text playerName;
    public Text totalScore;
    public Text FAT;//Fill Attacked Touch
    public GameObject mvp;

    [Button]
    public void Init(string name, string score,string fill,string attacked,string touch,bool isMvp)
	{
        playerName.text = name;
        totalScore.text = score;
        FAT.text = fill + "/" + attacked + "/" + touch;
		if (isMvp)
		{
            mvp.SetActive(true);
		}
		else
		{
            mvp.SetActive(false);
		}
        GetComponent<Animator>().SetTrigger("Flash");
	}

}
