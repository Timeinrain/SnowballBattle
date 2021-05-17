using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class NumberGroup : MonoBehaviour
{
	public Button left;
	public Button right;
	public Text number;
	[Range(2, 10)]
	[OnValueChanged("CheckIfEven")]
	public int maximumNumber;


	public void CheckIfEven()
	{
		if (maximumNumber % 2 != 0)
		{
			maximumNumber--;
		}
	}
	public void Decrease()
	{
		int num;
		int.TryParse(number.text, out num);
		if (num == 2)
		{
			return;
		}
		else
		{
			num -= 2;
		}
		number.text = num.ToString();
	}

	public void Increase()
	{
		int num;
		int.TryParse(number.text, out num);
		if (num == maximumNumber)
		{
			return;
		}
		else
		{
			num += 2;
		}
		number.text = num.ToString();
	}

}
