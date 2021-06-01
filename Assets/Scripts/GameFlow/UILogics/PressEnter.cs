using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PressEnter : MonoBehaviour
{
    public AnimationCurve curve;

	public Text pressEnter;
	private void Update()
	{
		pressEnter.GetComponent<Text>().color = new Color(pressEnter.GetComponent<Text>().color.r, pressEnter.GetComponent<Text>().color.g, pressEnter.GetComponent<Text>().color.b, curve.Evaluate(Time.time));
	}
}
