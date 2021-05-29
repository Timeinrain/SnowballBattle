using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayer : MonoBehaviour
{
    private RawImage rawImage;//rawImage组件
    private VideoPlayer videoPlayer;//视频播放组件

    private void Start()
    {
        rawImage = GetComponent<RawImage>();
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Update()
    {
       
    }
}
