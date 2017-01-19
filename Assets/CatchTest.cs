using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CatchTest : MonoBehaviour 
{
	[SerializeField]
	List<Transform> points;

	[SerializeField]
	Transform target;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if( target == null )
		{
			return;
		}


		if( SquareDetector.IsInside( points[0].position, points[1].position, points[2].position, points[3].position, target.position) )
		{
			target.GetComponent<Rigidbody>().AddForce(Vector3.up * 300);
		}
	}
}
