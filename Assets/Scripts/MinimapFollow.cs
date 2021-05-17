using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
	public Transform player;

	void Update()
	{
		transform.position = new Vector3(player.position.x, player.position.y + 15.0f, player.position.z);
	}
}
