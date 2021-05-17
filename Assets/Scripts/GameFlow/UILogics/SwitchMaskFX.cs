using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SwitchMaskFX : MonoBehaviour
{
	public float maxZoomSize = 50f;
	public float transitionSpeed = 5;
	public bool zoomOutAllowed = false;
	public float masksDelay = 0.2f;
	public List<GameObject> masks;
	public GameObject loadingPanel;

	/// <summary>
	/// 小熊缩小
	/// </summary>
	/// <param name="position">ZoomOut From some where</param>
	/// <param name="target">To Hide Or Show the target</param>
	public void PlayZoomOut(Vector3 position, GameObject target,bool needLoading = false)
	{
		transform.position = position;
		StartCoroutine(ZoomInOutHandler(false, target, masks,needLoading));
	}

	/// <summary>
	/// 小熊放大
	/// </summary>
	/// <param name="position">ZoomOut From some where</param>
	/// <param name="target">To Hide Or Show the target</param>
	public void PlayZoomIn(Vector3 position, GameObject target,bool needLoading = false)
	{
		transform.position = position;
		StartCoroutine(ZoomInOutHandler(true, target, masks,needLoading));
	}

	IEnumerator ZoomInOutHandler(bool isZoomIn, GameObject target, List<GameObject> panels,bool needLoading = false)
	{
		for (int i = 0; i < panels.Count; i++)
		{
			if (isZoomIn)
			{
				StartCoroutine(ZoomIn(target, panels, i,needLoading));
			}
			else
			{
				StartCoroutine(ZoomOut(target, panels, panels.Count - i - 1,needLoading));
			}
			yield return new WaitForSeconds(masksDelay);
		}
	}

	IEnumerator ZoomIn(GameObject target, List<GameObject> panels, int index,bool needLoading = false)
	{
		while (true)
		{
			panels[index].transform.localScale = Vector3.Lerp(panels[index].transform.localScale, new Vector3(maxZoomSize, maxZoomSize, maxZoomSize), Time.deltaTime * transitionSpeed);
			if (panels[index].transform.localScale.x >= maxZoomSize - 0.5)
			{
				panels[index].transform.localScale = new Vector3(maxZoomSize, maxZoomSize, maxZoomSize);
				break;
			}
			yield return new WaitForEndOfFrame();
		}
		if (index == panels.Count - 1)
		{
			if (needLoading)
			{
				PlayLoadingAnim();
			}
			target.SetActive(false);
		}
	}
	IEnumerator ZoomOut(GameObject target, List<GameObject> panels, int index,bool needLoading = false)
	{
		yield return new WaitForSeconds(0.5f);
		if (index == panels.Count - 1)
			target.SetActive(true);
		while (true)
		{
			panels[index].transform.localScale = Vector3.Lerp(panels[index].transform.localScale, new Vector3(0, 0, 0), Time.deltaTime * transitionSpeed);
			if (panels[index].transform.localScale.x <= 0.5)
			{
				panels[index].transform.localScale = new Vector3(0, 0, 0);
				break;
			}
			yield return new WaitForEndOfFrame();
		}
	}

	void PlayLoadingAnim()
	{

	}
}
