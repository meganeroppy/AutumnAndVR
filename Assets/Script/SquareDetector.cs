using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SquareDetector : MonoBehaviour {

	[SerializeField]
	Transform[] points = new Transform[4];

	[SerializeField]
	Transform target;

	// Update is called once per frame
	void Update () {
		var inside = IsInside(points[0].position, points[1].position, points[2].position, points[3].position, target.position);
		Debug.Log( inside ? "内側" : "外側");
	}

	public static bool IsInside( Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 pos4, Vector3 target )
	{
		return TriangleDetect.IsInside(pos1, pos2, pos3, target) || TriangleDetect.IsInside(pos1, pos4, pos3, target);
	}
}
