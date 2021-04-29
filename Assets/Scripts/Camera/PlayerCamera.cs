using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCamera : MonoBehaviourPun
{
    GameObject mainCam;
    public CameraSettings cameraSettings;

    private void Awake()
    {
        if (photonView.IsMine)
        {
            mainCam = GameObject.FindGameObjectWithTag("MainCamera");
            mainCam.transform.position = cameraSettings.offset + transform.position;
            mainCam.GetComponent<CameraFollow>().lookAt = transform;
            mainCam.GetComponent<CameraFollow>().follow = transform;
            mainCam.GetComponent<CameraFollow>().offset = cameraSettings.offset;
        }
        else if (!PhotonNetwork.IsConnected)
		{
            mainCam = GameObject.FindGameObjectWithTag("MainCamera");
            mainCam.transform.position = cameraSettings.offset + transform.position;
            mainCam.GetComponent<CameraFollow>().lookAt = transform;
            mainCam.GetComponent<CameraFollow>().follow = transform;
            mainCam.GetComponent<CameraFollow>().offset = cameraSettings.offset;
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            mainCam.GetComponent<CameraFollow>().UpdatePosition(transform.position);
        }
        else if (!PhotonNetwork.IsConnected)
		{
            mainCam.GetComponent<CameraFollow>().UpdatePosition(transform.position);
        }

    }
}
