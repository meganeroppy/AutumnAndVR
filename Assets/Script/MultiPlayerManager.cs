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

	public static List<CrewMove> crews = new List<CrewMove>();

	GameObject bag;

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
		if(  bag != null )
		{
			UpdateBagPos();
		}
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

		bag = PhotonNetwork.Instantiate("Bag", Vector3.zero, Quaternion.identity, 0);

		if(PhotonNetwork.isMasterClient )
		{
			PhotonNetwork.Instantiate("FallItemGenerator", Vector3.zero, Quaternion.identity, 0);
		}
	}

	/// <summary>
	/// かごの位置を更新
	/// </summary>
	void UpdateBagPos()
	{
		Vector3 bagPos = Vector3.zero;

		for( int i=0 ; i< crews.Count ; i++)
		{
			var crew = crews[i];
			if( crew == null )
			{
				Debug.Log("プレイヤーがnull");
				continue;			
			}

			for( int h=0 ; h < crew.handCount ; h++ )
			{
				bagPos += crew.GetHandPos( h );
			}
		}

		bagPos /= crews.Count;

		bagPos += Vector3.down * 1f;

		bag.transform.position = bagPos;
	}
}