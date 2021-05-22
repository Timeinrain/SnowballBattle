using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class CameraMapSelecting : MonoBehaviour
{
	public Camera _MainCam;
	#region Transform Infos
	public Vector3 initPos;
	public Quaternion initRot;
	#endregion

	[Header("LookAt Parameters")]
	public float lookAtSmoothness = 5;
	public float lookAdjustThreshold = 0.02f;
	public byte currentIndex = 0;
	[ReadOnly] public Map currentMap;
	public MapSelectionUI mapSelectionUI;

	[System.Serializable]
	public struct Mapping
	{
		public GameObject prefab;
		public Transform camTransform;
		public Map mapInfo;
	}

	[SerializeField]
	[ShowInInspector]
	public List<Mapping> mappingInfos;

	private void Start()
	{
		initPos = _MainCam.transform.position;
		initRot = _MainCam.transform.rotation;
	}

	[Button("Test Selecting Map")]
	public void StartSelecting()
	{
		StopAllCoroutines();
		StartCoroutine(SmoothLookAt(mappingInfos[currentIndex].camTransform.localPosition, mappingInfos[currentIndex].camTransform.rotation));
		currentMap = mappingInfos[currentIndex].mapInfo;
		mapSelectionUI.SetCurrentMap(currentMap);
	}

	public void LookAtSpecificMap(Map map)
	{
		foreach (var mapInfo in mappingInfos)
		{
			if (mapInfo.mapInfo == map)
			{
				StopAllCoroutines();
				StartCoroutine(SmoothLookAt(mapInfo.camTransform.localPosition, mapInfo.camTransform.rotation));
				break;
			}
		}
	}

	[Button("Test_SwitchLeft")]
	public void SwitchToLeft()
	{
		StopAllCoroutines();
		if (currentIndex == 0)
		{
			currentIndex = (byte)(mappingInfos.Count - 1);
		}
		else
		{
			currentIndex--;
		}
		StartCoroutine(SmoothLookAt(mappingInfos[currentIndex].camTransform.localPosition, mappingInfos[currentIndex].camTransform.rotation));
		currentMap = mappingInfos[currentIndex].mapInfo;
		mapSelectionUI.SetCurrentMap(currentMap);
	}

	[Button("Test_SwitchRight")]
	public void SwitchToRight()
	{
		StopAllCoroutines();
		if (currentIndex == mappingInfos.Count - 1)
		{
			currentIndex = 0;
		}
		else
		{
			currentIndex++;
		}
		StartCoroutine(SmoothLookAt(mappingInfos[currentIndex].camTransform.localPosition, mappingInfos[currentIndex].camTransform.rotation));
		currentMap = mappingInfos[currentIndex].mapInfo;
		mapSelectionUI.SetCurrentMap(currentMap);
	}

	/// <summary>
	/// Leaner Interpolation.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="rotaion"></param>
	/// <returns></returns>
	public IEnumerator SmoothLookAt(Vector3 position, Quaternion rotaion)
	{
		while (Vector3.Distance(_MainCam.transform.position, position) > lookAdjustThreshold || Quaternion.Angle(_MainCam.transform.rotation, rotaion) > lookAdjustThreshold)
		{
			_MainCam.transform.position = Vector3.Lerp(_MainCam.transform.position, position, Time.deltaTime * lookAtSmoothness);
			_MainCam.transform.rotation = Quaternion.Lerp(_MainCam.transform.rotation, rotaion, Time.deltaTime * lookAtSmoothness);
			yield return new WaitForEndOfFrame();
		}
		_MainCam.transform.position = position;
		_MainCam.transform.rotation = rotaion;
	}

	/// <summary>
	/// Reset the camera Transform to normal
	/// </summary>
	[Button("Test_Reset")]
	public void SwitchToNormal()
	{
		StopAllCoroutines();
		_MainCam.transform.position = initPos;
		_MainCam.transform.rotation = initRot;
	}
}
