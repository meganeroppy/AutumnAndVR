using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ２人プレイ用にPhoton上で扱うもの全般を制御
/// </summary>
public class MultiPlayerManager : Photon.MonoBehaviour 
{
	/// <summary>
	/// ルーム名
	/// </summary>
	string roomName = "autumnvr";

	public Transform pos1;

	public Transform pos2;

	public static List<CrewMove> cList = new List<CrewMove>();

	GameObject bag;

	public Material safeLineMat;

	/// <summary>
	/// ゲームを開始するのに必要なプレイヤー数
	/// </summary>
	public static int playerNumNeeded = 2;

	void Start ()
	{
		PhotonNetwork.ConnectUsingSettings ("0.1");
		if( GameManager.instance.singleMode )
		{
			playerNumNeeded = 1;
		}
	}

	void Update()
	{
		UpdateCatchArea();
	}

	//  ランダムでルームを選び入る
	void OnJoinedLobby()
	{
		PhotonNetwork.JoinRandomRoom();
	}

	//  JoinRandomRoom()が失敗した(false)時に呼ばれる
	void OnPhotonRandomJoinFailed(){
		//  部屋に入れなかったので自分で作る

		var options = new RoomOptions();
		options.MaxPlayers = (byte)playerNumNeeded;
		var lobby = new TypedLobby();

		PhotonNetwork.CreateRoom (roomName, options, lobby);
	}

	//  ルームに入れた時に呼ばれる（自分の作ったルームでも）
	void OnJoinedRoom()
	{
		PhotonNetwork.Instantiate("CrewMove", Vector3.zero, Quaternion.identity, 0);

		// シングルモードだったらもう一体
		if( GameManager.instance.singleMode )
		{
			PhotonNetwork.Instantiate("CrewMove", Vector3.zero, Quaternion.identity, 0);
		}

		if( bagMode )
		{
			bag = PhotonNetwork.Instantiate("Bag", Vector3.zero, Quaternion.identity, 0);
		}

		if(PhotonNetwork.isMasterClient )
		{
			PhotonNetwork.Instantiate("FallItemGenerator", Vector3.zero, Quaternion.identity, 0);
		}
	}

	bool defineHandObjects = false;
	List<Transform> hands = new List<Transform>();

	public bool bagMode = true;

	/// <summary>
	/// アイテム取得範囲を更新する
	/// </summary>
	void UpdateCatchArea()
	{
		if( bagMode )
		{
			if( bag == null )
			{
				return;
			}

			Vector3 bagPos = Vector3.zero;
			var handCount = 0;
			for( int i=0 ; i< cList.Count ; i++)
			{
				var crew = cList[i];
				if( crew == null )
				{
					Debug.Log("プレイヤーがnull");
					continue;			
				}

				for( int h=0 ; h < crew.handCount ; h++ )
				{
					bagPos += crew.GetHand( h ).position;
					handCount++;
				}
			}

			bagPos /= handCount;
			bagPos += Vector3.down * 1f;
			bag.transform.position = bagPos;
		}
		else
		{
			if( !defineHandObjects )
			{
				// 手が定義されていなければ取得する
				hands.Clear();

				if (cList.Count < playerNumNeeded) {
				//	Debug.LogWarning ("プレイヤー数不足[ " + cList.Count.ToString()  + " / " + playerNumNeeded.ToString() + " ]");
					return;
				}

				for( int i=0 ; i< cList.Count ; i++)
				{
					var crew = cList[i];
					if( crew == null )
					{
						Debug.Log("プレイヤーがnull");
						continue;			
					}

					for( int j=0 ; j < crew.handCount ; j++ )
					{
						if (!crew.handInitialized) {
							return;
						}
						var h = crew.GetHand( j );
						if( h == null )
						{
							return;
						}

						// ラインレンダラー付与
						if (h.GetComponent<LineRenderer> () == null) {
							var lr = h.gameObject.AddComponent<LineRenderer> ();
							lr.SetWidth (0.2f, 0.2f);
							lr.material = safeLineMat;
						}
						hands.Add( h );
					}
				}
				if( hands.Count < 4 )
				{
					Debug.LogError("手の数が不足しているのでアイテム取得判定が不可 手の数 -> [ " + hands.Count.ToString() + " ]");
					return;
				}
				defineHandObjects = true;

				Debug.LogError("手の数が揃いました！ 手の数 -> [ " + hands.Count.ToString() + " ]");
			}
				
			// 光線描画処理
			DrawLine();

			if(FallItem.fList == null)
			{
				return;
			}

			// ここからキャッチ判定と光線描画
			for(int i=0 ; i < FallItem.fList.Count ; i++)
			{
				var item = FallItem.fList[i];
				if( item == null )
				{
					continue;
				}
				var inside = SquareDetector.IsInside( hands[0].position, hands[1].position, hands[2].position, hands[3].position, item.transform.position );
				if( inside )
				{
					if( !item.harvested )
					{
						Debug.Log( item.name + "をキャッチ！");
						item.Harvest(true);

						// コントローラを振動
						cList.ForEach( c => c.VibrateController() );
					}
				}
			}
				

		}




	}

	/// <summary>
	/// 光線を描画
	/// </summary>
	private void DrawLine()
	{
		if( hands == null)
		{
			return;
		}

		for (int idx = 0; idx < hands.Count; idx++) {
			var hand = hands [idx];
			if (hand == null) {
				continue;
			}
			var lr = hand.GetComponent<LineRenderer> ();
			if (lr == null) {
				Debug.LogError( hand.gameObject.name + "にLineRendererがアタッチされていない");
				continue;
			}

			var targetIdx = idx >= hands.Count - 1 ? 0 : idx + 1;
			var targetPos = hands [targetIdx].position;
		
			lr.SetPosition (0, hand.position);
			lr.SetPosition (1, targetPos);
		}
	}
}