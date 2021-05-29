using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FrozenCountdown : MonoBehaviour
{
    public Character character;
    private Text textCountdown;

    private bool isDisplay = false;

    private void Start()
    {
        textCountdown = GetComponent<Text>();
        textCountdown.text = "";

        character.frozen += OnFrozen;
        character.unfrozen += OnUnfrozen;
        character.died += OnDie;
    }

    private void OnDestroy()
    {
        character.frozen -= OnFrozen;
        character.unfrozen -= OnUnfrozen;
        character.died -= OnDie;
    }

    void OnFrozen(string id)
    {
        isDisplay = true;
    }

    void OnUnfrozen(string id)
    {
        isDisplay = false;
        textCountdown.text = "";
    }

    void OnDie(string id)
    {
        isDisplay = false;
        textCountdown.text = "";
    }

    void Update()
    {
        if (isDisplay)
        {
            int displayNum = (int)(character.GetFrozenCountdown() + 0.99f);
            textCountdown.text = (displayNum.ToString());
        }
        
    }
}
