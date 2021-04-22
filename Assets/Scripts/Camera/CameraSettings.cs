using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName ="CameraInitParams/OffsetSettings",fileName ="OffsetSettings")]
public class CameraSettings : ScriptableObject
{
	[SerializeField]
	public Vector3 offset;
	public TeamFlag team=TeamFlag.A;
}

public enum TeamFlag
{
	A=0,B=1
}
