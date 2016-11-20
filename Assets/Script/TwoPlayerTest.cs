﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ２人プレイ用にPhoton上で扱うもの全般を制御
/// </summary>
public class TwoPlayerTest : Photon.MonoBehaviour 
{
	/// <summary>
	/// ルーム名
	/// </summary>
	string roomName = "autumnvr";

	public Transform pos1;

	public Transform pos2;

	CrewMoveTest myCrew;

	public static List<CrewMoveTest> crews = new List<CrewMoveTest>();

	GameObject bag;

	GameObject chestnutGenerator;

	/// <summary>
	/// ゲームを開始するのに必要なプレイヤー数
	/// </summary>
	public static int playerNumNeeded = 2;

	void Start ()
	{
		PhotonNetwork.ConnectUsingSettings ("0.1");
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

		RoomOptions options = new RoomOptions();
		options.MaxPlayers = (byte)playerNumNeeded;
		TypedLobby lobby = new TypedLobby();

		PhotonNetwork.CreateRoom (roomName, options, lobby);
	}

	//  ルームに入れた時に呼ばれる（自分の作ったルームでも）
	void OnJoinedRoom()
	{
		myCrew = PhotonNetwork.Instantiate("CrewMoveTest", Vector3.zero, Quaternion.identity, 0).GetComponent<CrewMoveTest>();

		bag = PhotonNetwork.Instantiate("BagTest", Vector3.zero, Quaternion.identity, 0);

		if(PhotonNetwork.isMasterClient )
		{
			chestnutGenerator = PhotonNetwork.Instantiate("ChestnutGenerator", Vector3.zero, Quaternion.identity, 0);
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
			if( crews[i] == null )
			{
				Debug.Log("プレイヤーがnull");
				continue;
			}

			bagPos += crews[i].handPos;
		}
		bagPos /= crews.Count;

		bagPos += Vector3.down * 1f;

		bag.transform.position = bagPos;
	}
}

