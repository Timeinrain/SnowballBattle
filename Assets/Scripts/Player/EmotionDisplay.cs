using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EmotionDisplay : MonoBehaviour
{
    private Image image;

    [Tooltip("小于等于这个血量显示为受伤")]
    public int hurtThreshold = 1;
    [Tooltip("得分表情显示时间")]
    public float scoreEmotionDisplayTime;

    [Header("Red Team")]
    public Sprite redDie;
    public Sprite redHurt;
    public Sprite redFrozen;
    public Sprite redNormal;
    public Sprite redScore;

    [Header("Green Team")]
    public Sprite greenDie;
    public Sprite greenHurt;
    public Sprite greenFrozen;
    public Sprite greenNormal;
    public Sprite greenScore;

    [Header("Health Bar")]
    public Text healthText;

    private Character monitorCharacter;
    private Team team;

    private bool initialized = false;

    private enum EmotionState
    {
        Die,
        Hurt,
        Frozen,
        Normal,
        Score,
    }
    private EmotionState state = EmotionState.Normal;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    private void OnDestroy()
    {
        monitorCharacter.died -= OnDie;
        monitorCharacter.respawned -= OnRespawn;
        monitorCharacter.frozen -= OnFrozen;
        monitorCharacter.unfrozen -= OnUnfrozen;
        //monitorCharacter.gotBomb -= OnScore;
        monitorCharacter.filledCannon -= OnScore;
    }

    private void Update()
    {
        if (!initialized)
        {
            monitorCharacter = InOutGameRoomInfo.Instance.GetPlayerByName(PhotonNetwork.LocalPlayer.NickName).Instance.GetComponent<Character>();
            team = InOutGameRoomInfo.Instance.GetPlayerByName(PhotonNetwork.LocalPlayer.NickName).team;

            // 初始表情
            if (team == Team.Red)
                image.sprite = redNormal;
            else if (team == Team.Green)
                image.sprite = greenNormal;

            monitorCharacter.died += OnDie;
            monitorCharacter.respawned += OnRespawn;
            monitorCharacter.frozen += OnFrozen;
            monitorCharacter.unfrozen += OnUnfrozen;
            //monitorCharacter.gotBomb += OnScore;
            monitorCharacter.filledCannon += OnScore;

            initialized = true;
            return;
        }

        if (monitorCharacter.Health <= hurtThreshold && state == EmotionState.Normal)
        {
            state = EmotionState.Hurt;
        }

        if (team == Team.Red)
        {
            switch (state)
            {
                case EmotionState.Die:
                    image.sprite = redDie;
                    break;
                case EmotionState.Hurt:
                    image.sprite = redHurt;
                    break;
                case EmotionState.Frozen:
                    image.sprite = redFrozen;
                    break;
                case EmotionState.Normal:
                    image.sprite = redNormal;
                    break;
                case EmotionState.Score:
                    image.sprite = redScore;
                    break;
            }
        }
        else if (team == Team.Green)
            {
                switch (state)
                {
                    case EmotionState.Die:
                        image.sprite = greenDie;
                        break;
                    case EmotionState.Hurt:
                        image.sprite = greenHurt;
                        break;
                    case EmotionState.Frozen:
                        image.sprite = greenFrozen;
                        break;
                    case EmotionState.Normal:
                        image.sprite = greenNormal;
                        break;
                    case EmotionState.Score:
                        image.sprite = greenScore;
                        break;
                }
            }

        // 血条
        healthText.text = monitorCharacter.Health.ToString();
    }

    private void OnDie(string id)
    {
        state = EmotionState.Die;
    }

    private void OnRespawn(string id)
    {
        state = EmotionState.Normal;
    }

    private void OnFrozen(string id)
    {
        state = EmotionState.Frozen;
    }

    private void OnUnfrozen(string id)
    {
        state = EmotionState.Normal;
    }

    private void OnScore(string id)
    {
        StartCoroutine(DisplayScoreEmotion());
    }

    private IEnumerator DisplayScoreEmotion()
    {
        state = EmotionState.Score;
        yield return new WaitForSeconds(scoreEmotionDisplayTime);
        if (state != EmotionState.Frozen || state != EmotionState.Die)
            state = EmotionState.Normal;
    }
}
