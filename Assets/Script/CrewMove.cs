using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrewMove : Photon.MonoBehaviour {

	float speed = 2f;

	public List<SteamVR_TrackedObject> hands;
	public Transform eye;

	public AudioListener listener;

	public bool ready { get; private set; }

	/// <summary>
	/// 自分自身のときに不要
	/// </summary>
	public List<GameObject> notNeededObjForMe;

	/// <summary>
	/// 相手のときに不要
	/// </summary>
	public List<GameObject> notNeededObjForOther;
	public SteamVR_PlayArea pa;
	public SteamVR_ControllerManager cm;
	public SteamVR_Camera sc;
	public SteamVR_Ears se;
	public List<SteamVR_TrackedObject> to;
	public List<Camera> c;

	/// <summary>
	/// VRコントローラが無効の時はこれが手
	/// </summary>
	public List<Transform> dummyHands;

	Muscle myMuscle;

	private Vector3 offsetHeightFromMuscle;

	void Start()
	{
		handInitialized = false;
		StartCoroutine ( StartLoad() );
	}

	// Use this for initialization
	IEnumerator StartLoad () 
	{
		MultiPlayerManager.cList.Add(this);

		if( !photonView.isMine || ( GameManager.instance.singleMode && !MultiPlayerManager.cList.IndexOf(this).Equals(0) ))
		{
			notNeededObjForOther.ForEach( g => g.SetActive(false) );
			to.ForEach( g => g.enabled = false );
			c.ForEach( g => g.enabled = false );
			pa.enabled = false;
			cm.enabled = false;
			sc.enabled = false;
			se.enabled = false;

			listener.enabled = false;
		}
		else
		{
			notNeededObjForMe.ForEach( g => g.SetActive(false) );
		}

		myMuscle = GameObject.Find("Muscle").GetComponent<Muscle>();

		int photonViewId = photonView.ownerId;
		Debug.LogError("あなたのプレイヤーIDは[ " + photonViewId.ToString() + " ]");

		Transform t = photonViewId == 1 ? myMuscle.pos1 : myMuscle.pos2;

		if( GameManager.instance.singleMode && !MultiPlayerManager.cList.IndexOf(this).Equals(0) )
		{
			t = myMuscle.pos2;
		}

		transform.SetParent(t);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;

		bool isActiveViveControllers = false;

		var waitCount = 0;
		var maxWait = 10;
		while (true ) 
		{
			if ( waitCount >= maxWait || Input.GetKey (KeyCode.D)) 
			{
				break;
			}

			bool allHandActive = true;
			for (int i = 0; i < hands.Count; i++) 
			{
				var h = hands [i];
				if (!h.gameObject.activeInHierarchy) {
					allHandActive = false;
					Debug.LogWarning (h.gameObject.name + "が非アクティブ状態");
				}
			}

			if (allHandActive) 
			{
				Debug.Log ("すべてのViveコントローラーを検出");
				isActiveViveControllers = true;
				break;
			}

			Debug.LogWarning ("有効になっているViveコントローラの数が足りないので待機します [ D ]キーでデバッグ用キーボード操作に移行します");
			yield return new WaitForSeconds(1);
			waitCount++;
		}

		// Vが無効だったらダミーハンドを使用
		dummyHands.ForEach( h => { h.gameObject.SetActive( !isActiveViveControllers ); } );
		handInitialized = true;

		photonView.RPC ("SetReady", PhotonTargets.All, false);
	}

	// Update is called once per frame
	void Update () {

		if( !photonView.isMine )
		{
			return;
		}

		// シングルモードの２PはInput無視
		if( GameManager.instance.singleMode && !MultiPlayerManager.cList.IndexOf(this).Equals(0) )
		{
			return;
		}

		GetInput();
	}
		
	void GetInput()
	{
		if( Input.GetKeyDown(KeyCode.Space)){
			photonView.RPC ("SetReady", PhotonTargets.All, true);
		}

		var moveX = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
		var moveZ = Input.GetAxisRaw("Vertical") * speed * Time.deltaTime;

		// 左Shiftお押しながらの操作は左手
		int handIdx = Input.GetKey( KeyCode.LeftShift ) ? 1 : 0;

		Transform targetHand = (hands[handIdx].gameObject.activeInHierarchy ? hands[handIdx].transform : dummyHands[handIdx] );
			
		targetHand.Translate(moveX, 0, moveZ);

		if( Input.GetKeyDown(KeyCode.U) )
		{
			Debug.Log("Uが押された プレイヤーID = " + PhotonNetwork.player.ID.ToString());
			myMuscle.joy_rate *= 1.1f;
			U_count++;
		}
			
		for( int i=0 ; i< hands.Count ; i++ )
		{
			var device = SteamVR_Controller.Input((int) hands[ i ].index);

			if( device != null )
			{			
				if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger)) {
					photonView.RPC ("SetReady", PhotonTargets.All, true);
				}
				if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
					photonView.RPC ("SetReady", PhotonTargets.All, true);
				}
			}
		}
	}

	/// <summary>
	/// コントローラを振動させる
	/// </summary>
	public void VibrateController()
	{
		if( hands == null )
		{
			return;
		}

		for( int i=0 ; i< hands.Count ; i++ )
		{
			var device = SteamVR_Controller.Input((int) hands[ i ].index);

			if( device != null )
			{			
				device.TriggerHapticPulse();
			}
		}
	}

	/// <summary>
	/// 準備完了状態かのフラグをセット
	/// </summary>
	[PunRPC]
	void SetReady(bool value)
	{
		Debug.Log ("PhotonViewID[ " + photonView.viewID.ToString () + " ]のreadyを" + value.ToString ());
		ready = value;
	}

	/// <summary>
	/// マッチョの位置をリセット
	/// </summary>
	[PunRPC]
	void Reset()
	{
		Muscle.instance.SetToOrigin();
		ready = false;
		GameManager.instance.ResetPrameter();
	}

	/// <summary>
	/// 手のトランスフォームを返す
	/// </summary>
	/// <value>The hand position.</value>
	public Transform GetHand( int handIdx )
	{	
		return (hands[ handIdx ].gameObject.activeInHierarchy ? hands[ handIdx ].transform : dummyHands[ handIdx ] );
	}

	public bool handInitialized = false;

	public int handCount{
		get{
			return hands.Count > 1 ? hands.Count : dummyHands.Count;
		}
	}

	int U_count = 0;

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if( myMuscle == null )
		{
			return;
		}

		if (stream.isWriting) {
			//データの送信
			stream.SendNext(U_count);
			stream.SendNext(myMuscle.joy_rate);
		} else {
			//データの受信
			U_count = (int)stream.ReceiveNext();
			myMuscle.joy_rate = (float)stream.ReceiveNext();
		}
		Debug.Log( "クライアント" + PhotonNetwork.player.ID.ToString() + "上の オーナーID" + photonView.ownerId.ToString() + "の上昇率=" + myMuscle.joy_rate.ToString());
	}
}
