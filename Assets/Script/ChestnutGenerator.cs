using UnityEngine;
using System.Collections;

/// <summary>
/// イガグリ生成装置
/// </summary>
public class ChestnutGenerator : Photon.MonoBehaviour {

	/// <summary>
	/// 生成頻度
	/// </summary>
	public float interval = 1f;
	float timer = 0;

	/// <summary>
	/// 生成位置の誤差
	/// </summary>
	public Vector2 diffRange;

	/// <summary>
	/// イガグリ
	/// </summary>
	public GameObject chestnut;

	AudioSource myAudio;

	void Start()
	{
		myAudio = GetComponent<AudioSource>();

		Transform t = GameObject.Find("GeneratorPos").transform;
		transform.SetParent(t);
		transform.localPosition = Vector3.zero;
	}

	// Update is called once per frame
	void Update () 
	{
		if( !GameManager.instance.running)
		{
			return;
		}

		if( PhotonNetwork.room.playerCount < MultiPlayerManager.playerNumNeeded )
		{
			return;
		}
		else if( !PhotonNetwork.isMasterClient )
		{
			return;
		}

		if( CheckTimer() )
		{
		//	PhotonNetwork.RPC(photonView, "Generate", PhotonTargets.All, false);
			Generate();
		}
	}

	/// <summary>
	/// 一定間隔でtrueを返す
	/// </summary>
	bool CheckTimer()
	{
		timer += Time.deltaTime;
		if( timer >= interval )
		{
			timer = 0;
			return true;
		}
		return false;
	}

	/// <summary>
	/// イガグリを生成
	/// </summary>
	[PunRPC]
	void Generate()
	{
		Debug.Log("マスタープレイヤー[ " + PhotonNetwork.player.ID + " ]が栗を生成");

		//　生成位置を決定
		Vector3 pos = new Vector3(transform.position.x + Random.Range( -diffRange.x, diffRange.x ), transform.position.y, transform.position.z + Random.Range( -diffRange.y, diffRange.y ));

		// photn上に生成
		var c = PhotonNetwork.Instantiate("Chestnut", pos, Quaternion.identity, 0);
		c.transform.SetParent( transform );
		//Instantiate(chestnut, pos, Quaternion.identity);

		// SEを再生
		myAudio.Play();
	}
}
