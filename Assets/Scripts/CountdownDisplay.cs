using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownDisplay : MonoBehaviour
{
    public Text num0;
    public Text num1;
    public Text num2;
    public Text num3;

    public void SetTime(int time)
    {
        num0.text = (time / 600).ToString();
        time -= time / 600 * 600;
        num1.text = (time / 60).ToString();
        time -= time / 60 * 60;
        num2.text = (time / 10).ToString();
        time -= time / 10 * 10;
        num3.text = time.ToString();
    }
}
