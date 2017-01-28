using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 乗組員クラス
/// 自分の場合と相手の場合がある
/// </summary>
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

	[HideInInspector]
	public bool handInitialized = false;

	private bool isActiveViveControllers = false;

	[HideInInspector]
	public bool playerSettingDefined = false;

	public int handCount{
		get{
			return hands.Count > 1 ? hands.Count : dummyHands.Count;
		}
	}

	void Start()
	{
		handInitialized = false;
		StartCoroutine ( StartLoad() );
	}

	// Use this for initialization
	IEnumerator StartLoad () 
	{
		// 配列に自身を追加
		MultiPlayerManager.cList.Add(this);

		// 自分でなかったら相手の自分の表示専用のオブジェクトを無効化
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
			// 自分だったら相手表示用のオブジェクトを無効化
			notNeededObjForMe.ForEach( g => g.SetActive(false) );
		}

		myMuscle = GameObject.Find("Muscle").GetComponent<Muscle>();

		int photonViewId = photonView.ownerId;
		Debug.LogError(( photonView.isMine ? "あなた" : "相手") + "のプレイヤーIDは[ " + photonViewId.ToString() + " ]");

		Transform t = photonViewId == 1 ? myMuscle.pos1 : myMuscle.pos2;

		if( GameManager.instance.singleMode && !MultiPlayerManager.cList.IndexOf(this).Equals(0) )
		{
			t = myMuscle.pos2;
		}

		transform.SetParent(t);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;

		// Viveコントローラを使用するかキーボード操作か場合分け

		if (photonView.isMine) {

			// 自分の場合
			var waitCount = 0;
			var maxWait = 10;
			bool allHandActive = true;

			while (true) {
				if (waitCount >= maxWait || Input.GetKey (KeyCode.D)) {
					break;
				}

				allHandActive = true;

				for (int i = 0; i < hands.Count; i++) {
					var h = hands [i];
					if (!h.gameObject.activeInHierarchy) {
						allHandActive = false;
						Debug.LogWarning ("[YOU]" + h.gameObject.name + "が非アクティブ状態");
					}
				}

				if (allHandActive) {
					Debug.Log ("[YOU]すべてのViveコントローラーを検出");
					break;
				}

				Debug.LogWarning ("[YOU]有効になっているViveコントローラの数が足りないので待機します 有効コントローラ数 -> [ " + hands.Count.ToString() + " ] [ D ]キーでデバッグ用キーボード操作に移行します");
				yield return new WaitForSeconds (1);
				waitCount++;
			}
				
			// Viveコントローラが無効だったらダミーハンドを使用
			dummyHands.ForEach( h => { h.gameObject.SetActive( !allHandActive ); } );
			Debug.LogWarning ("[YOU]" + (allHandActive ? "Viveコントローラ" : "キーボード") + "で操作します");

			photonView.RPC ("SetReady", PhotonTargets.AllBuffered, false);
		//	SetReady( false );
			photonView.RPC ("SetViveControllerStatus", PhotonTargets.AllBuffered, allHandActive);
		//	SetViveControllerStatus( allHandActive );
		} else {
			// 相手の場合
			while (!GameManager.instance.singleMode && !playerSettingDefined) {
				Debug.LogWarning ("相手のプレイヤー設定確認中");
				yield return new WaitForSeconds (1);
				continue;
			}

			// Viveコントローラが無効だったらダミーハンドを使用
			dummyHands.ForEach( h => { h.gameObject.SetActive( !isActiveViveControllers ); } );
			Debug.LogWarning ("[OTHER]" + (isActiveViveControllers ? "Viveコントローラ" : "キーボード") + "で操作します");
		}

		handInitialized = true;
	}

	// Update is called once per frame
	void Update () {

		if( !photonView.isMine )
		{
			return;
		}

		// シングルモードの2PはInput無視
		if( GameManager.instance.singleMode && !MultiPlayerManager.cList.IndexOf(this).Equals(0) )
		{
			return;
		}

		GetInput();
	}
		
	/// <summary>
	/// プレイヤー入力処理
	/// マルチの相手のときとシングルモードのダミー相手のときは呼ばれないはず
	/// </summary>
	private void GetInput()
	{
		if( Input.GetKeyDown(KeyCode.Space)){
			photonView.RPC ("SetReady", PhotonTargets.AllBuffered, true);
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
		}
			
		for( int i=0 ; i< hands.Count ; i++ )
		{
			var device = SteamVR_Controller.Input((int) hands[ i ].index);

			if( device != null )
			{			
				if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger)) {
					photonView.RPC ("SetReady", PhotonTargets.AllBuffered, true);
				}
				if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
					photonView.RPC ("SetReady", PhotonTargets.AllBuffered, true);
				}
			}
		}
	}

	/// <summary>
	/// コントローラを振動させる
	/// </summary>
	public void VibrateController()
	{
		// 自分にのみ有効
		if (!photonView.isMine) {
			return;
		}

		if( hands == null )
		{
			return;
		}

		// 両手を振動させる
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
		Debug.Log ("RPC(SetReady) " + (photonView.isMine ? "あなた" : "相手") + " ID[ " + photonView.viewID.ToString () + " ] の呼び出し" + value.ToString());
		playerSettingDefined = true;
		ready = value;
	}

	[PunRPC]
	void SetViveControllerStatus( bool key )
	{
		Debug.Log ("RPC(SetViveControllerStatus) " + (photonView.isMine ? "あなた" : "相手") + "ID[ " + photonView.viewID.ToString () + " ] の呼び出し" + key.ToString() );
		isActiveViveControllers = key;
		if (isActiveViveControllers) {
			Debug.Log ("PhotonViewID[ " + photonView.viewID.ToString () + " ]のプレイヤーはViveを使用します");
		} else {
			Debug.Log ("PhotonViewID[ " + photonView.viewID.ToString () + " ]のプレイヤーはキーボードを使用します");
		}
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
	public Transform GetHand( int handIdx )
	{	
		return (hands[ handIdx ].gameObject.activeInHierarchy ? hands[ handIdx ].transform : dummyHands[ handIdx ] );
	}



	/// <summary>
	/// 値の同期
	/// </summary>
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if( myMuscle == null )
		{
			return;
		}

		if (stream.isWriting) {
			//データの送信
			stream.SendNext(myMuscle.joy_rate);
			stream.SendNext (ready);
			stream.SendNext (isActiveViveControllers);
		} else {
			//データの受信
			myMuscle.joy_rate = (float)stream.ReceiveNext();
			ready = (bool)stream.ReceiveNext ();
			isActiveViveControllers = (bool)stream.ReceiveNext ();
		}

		string str = photonView.isMine ? "[YOU]" : "[OTHER]";
		str += "ID[ " + PhotonNetwork.player.ID.ToString() + " ] ";
		str += " ready=[ " + ready.ToString() + " ] ";
		str += " isActiveViveControllers=[" + isActiveViveControllers.ToString()  + " ]";

		Debug.Log(str);
	}
}
