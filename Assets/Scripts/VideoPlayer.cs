using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayer : MonoBehaviour
{
    private RawImage rawImage;//rawImage���
    private VideoPlayer videoPlayer;//��Ƶ�������

    private void Start()
    {
        rawImage = GetComponent<RawImage>();
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Update()
    {
       
    }
}
