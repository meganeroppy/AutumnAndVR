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

	/// <summary>
	/// ふみつけカウント
	/// </summary>
	public int stompCount { private set; get;}

	// Use this for initialization
	void Start () 
	{
		MultiPlayerManager.crews.Add(this);

		if( !photonView.isMine || ( GameManager.instance.singleMode && !MultiPlayerManager.crews.IndexOf(this).Equals(0) ))
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

		if( GameManager.instance.singleMode && !MultiPlayerManager.crews.IndexOf(this).Equals(0) )
		{
			t = myMuscle.pos2;
		}

		transform.SetParent(t);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;

		// 有効になっている手が存在するか
		bool isActiveHands = hands.Find( h => { return h.gameObject.activeInHierarchy; } ) != null;

		// 右手が無効だったらダミーハンドを使用
		dummyHands.ForEach( h => { h.gameObject.SetActive( !isActiveHands ); } );

		photonView.RPC ("SetReady", PhotonTargets.All, false);
	}

	// Update is called once per frame
	void Update () {

		if( !photonView.isMine )
		{
			return;
		}

		// シングルモードの２PはInput無視
		if( GameManager.instance.singleMode && !MultiPlayerManager.crews.IndexOf(this).Equals(0) )
		{
			return;
		}

		GetInput();

//		CheckItemCatchBySquare();
	}
		
	void GetInput()
	{
		if( Input.GetKeyDown(KeyCode.Space)){
			photonView.RPC ("SetReady", PhotonTargets.All, true);
		}

		var moveX = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
		var moveZ = Input.GetAxisRaw("Vertical") * speed * Time.deltaTime;

		// 左Shiftお押しながらの操作あ左手
		int handIdx = Input.GetKey( KeyCode.LeftShift ) ? 1 : 0;

		Transform targetHand = (hands[handIdx].gameObject.activeInHierarchy ? hands[handIdx].transform : dummyHands[handIdx] );
			
		targetHand.Translate(moveX, 0, moveZ);

		if( Input.GetKeyDown(KeyCode.U) )
		{
			Debug.Log("Uが押された プレイヤーID = " + PhotonNetwork.player.ID.ToString());
			myMuscle.joy_rate *= 1.1f;
			U_count++;
		//	PhotonNetwork.RPC(photonView, "AddStompCount", PhotonTargets.All, false);
		}

		var device = SteamVR_Controller.Input((int) hands[ handIdx ].index);

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

	/// <summary>
	/// 踏みつけ回数を加算
	/// </summary>
	[PunRPC]
	public void AddStompCount()
	{
		if( !GameManager.instance.running )
		{
			Debug.Log("ゲームは終了済み。カウント追加は無効");
		}

		stompCount++;
		Debug.Log( "クライアント" + PhotonNetwork.player.ID.ToString() + "上の オーナーID" + photonView.ownerId.ToString() + "のカウント=" + stompCount.ToString());

		if( myMuscle != null ){
			myMuscle.AddEnergy(1, this);
		}else{
			Debug.Log( "myMuscleがnull" );
		}

		int sum = 0;
		MultiPlayerManager.crews.ForEach( c => sum += c.stompCount );

		Debug.Log( "全プレイヤーの合計カウント = " + sum.ToString());
	}

	[PunRPC]
	void SetReady(bool value)
	{
		Debug.Log ("PhotonViewID[ " + photonView.viewID.ToString () + " ]のreadyを" + value.ToString ());
		ready = value;
	}

	/// <summary>
	/// 手のトランスフォームを返す
	/// </summary>
	/// <value>The hand position.</value>
	public Transform GetHand( int handIdx )
	{	
		return (hands[ handIdx ].gameObject.activeInHierarchy ? hands[ handIdx ].transform : dummyHands[ handIdx ] );
	}

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
