using UnityEngine;
using System.Collections;

public class Planet : MonoBehaviour {

	public float rotSpeed;
	public Vector3 rotDigree;
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (rotDigree * rotSpeed * Time.deltaTime );
	}
}
