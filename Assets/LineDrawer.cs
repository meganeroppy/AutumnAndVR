using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour 
{
	public static LineDrawer instance;

	void Awake()
	{
		instance = this;
	}


	void Update () 
	{
		var pos = new List<Vector3>();
		GameObject cube_1 = GameObject.Find("Cube1");
		pos.Add( cube_1.transform.position);
		GameObject cube_2 = GameObject.Find("Cube2");
		pos.Add( cube_2.transform.position);
		GameObject cube_3 = GameObject.Find("Cube3");
		//pos.Add( cube_3.transform.position);
		GameObject cube_4 = GameObject.Find("Cube4");
		//pos.Add( cube_4.transform.position);

		var lineY = GameObject.Find ("LineRenderer").GetComponent<LineRenderer> ();

		lineY.SetPositions( pos.ToArray() );
		lineY.SetColors( Color.red, Color.red);
		lineY.SetWidth( 1, 1 );

	//	lineY.SetPosition(0, cube_1.transform.position);
	//	lineY.SetPosition(1, cube_2.transform.position);

	//	lineY.SetPosition(2, cube_3.transform.position);
	//	lineY.SetPosition(3, cube_4.transform.position);
	}
}
