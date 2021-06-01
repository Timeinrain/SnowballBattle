using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LoadingPanel : MonoBehaviour
{
	public List<GameObject> tips;
	public GameObject loadingPanel;


	private void Start()
	{
		DontDestroyOnLoad(gameObject);
	}

	[Button]
	public void StartLoading()
	{
		loadingPanel.SetActive(true);
		SelectRandomToDisplay();
		StartCoroutine(AutoInActive());
	}

	IEnumerator AutoInActive()
	{
		yield return new WaitForSeconds(10f);
		EndLoading();
	}

	[Button]
	public void EndLoading()
	{
		loadingPanel.GetComponent<ActiveCtrler>().SetActiveFalse();
	}

	public void SelectRandomToDisplay()
	{
		GameObject go = tips[Random.Range(0, tips.Count)];
		go.SetActive(true);
		go.GetComponent<Animator>().SetTrigger("In");
	}

}
