using UnityEngine;
using System.Collections;

/// <summary>
/// イガグリ生成装置
/// </summary>
public class FallItemGenerator : Photon.MonoBehaviour {

	public static FallItemGenerator instance;
	/// <summary>
	/// 生成頻度
	/// </summary>
	public float interval = 1f;
	float timer = 0;

	/// <summary>
	/// 生成位置の誤差
	/// </summary>
	public Vector2 diffRange;

	AudioSource myAudio;

	void Awake()
	{
		instance = this;	
	}

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
	/// 落下アイテムを生成
	/// </summary>
	void Generate()
	{
		Debug.Log("マスタープレイヤー[ " + PhotonNetwork.player.ID + " ]がアイテムを生成");

		//　生成位置を決定
		Vector3 pos = new Vector3(transform.position.x + Random.Range( -diffRange.x, diffRange.x ), transform.position.y, transform.position.z + Random.Range( -diffRange.y, diffRange.y ));

		bool isGoodItem = Random.Range(0, 4).Equals(0);

		// photn上に生成
		if( isGoodItem )
		{
			var g = PhotonNetwork.Instantiate("Banana", pos, Quaternion.identity, 0);
		}
		else
		{
			var g =PhotonNetwork.Instantiate("Chestnut", pos, Quaternion.identity, 0);
		}

		// SEを再生
		myAudio.Play();
	}
}
