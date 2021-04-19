using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 时间与昼夜交替控制
/// </summary>
public class TimeLightController : MonoBehaviour
{
	// Start is called before the first frame update
	[Header("Cloud Control")]
	public GameObject clouds;
	public float cloudsFlowingSpeed;
	[Header("Day Night Light Control")]
	public GameObject dayLight;
	public GameObject nightLight;
	public float lightRotateSpeedAtDayTime;
	public float lightRotateSpeedAtNightTime;
	[SerializeField]
	public AnimationCurve dayLightStrength = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField]
	public AnimationCurve nightLightStrength = AnimationCurve.EaseInOut(0, 0, 1, 1);
	public bool atDay = true;
	public bool dayNightSwitchable = true;
	[HideInInspector] [SerializeField] Dictionary<bool, float> rotationSpeedMapping;
	void Start()
	{
		rotationSpeedMapping = new Dictionary<bool, float>();
		rotationSpeedMapping.Add(true, lightRotateSpeedAtDayTime);
		rotationSpeedMapping.Add(false, lightRotateSpeedAtNightTime);
	}
	IEnumerator UpdateDayNightSwitchable()
	{
		yield return new WaitForSeconds(3f);
		dayNightSwitchable = true;
	}
	// Update is called once per frame
	void Update()
	{
		transform.Rotate(Vector3.right, rotationSpeedMapping[atDay] * Time.deltaTime);
		if (transform.rotation.eulerAngles.x > -5 && transform.rotation.eulerAngles.x < 5 && dayNightSwitchable)
		{
			SwitchTimeMode();
			dayNightSwitchable = false;
			StartCoroutine(UpdateDayNightSwitchable());
		}
		UpdateDayNightLightPara();
	}

	void UpdateLightPara()
	{
		//if (atDay)
		//{
		//	nightLight.SetActive(false);
		//	dayLight.SetActive(true);
		//}
		//else
		//{
		//	dayLight.SetActive(false);
		//	nightLight.SetActive(true);
		//}
	}

	void SwitchTimeMode()
	{
		atDay = !atDay;
		UpdateLightPara();
	}

	void UpdateDayNightLightPara()
	{
		switch (atDay)
		{
			case true:
				{
					//todo modify the strength with animationcurve
					//modify the intensity of it.

					break;
				}
			case false:
				{
					break;
				}
		}
	}
}
