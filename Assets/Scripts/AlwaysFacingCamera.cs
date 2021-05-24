using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysFacingCamera : MonoBehaviour
{
    private Camera playerViewCamera;

	private void Awake()
	{
        playerViewCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        GetComponent<Canvas>().worldCamera = playerViewCamera;
	}

	// Update is called once per frame
	void Update()
    {
        LookAtCam();
    }

    public void LookAtCam()
    {
        transform.forward = new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(playerViewCamera.transform.position.x, 0, playerViewCamera.transform.position.z);
    }
}
