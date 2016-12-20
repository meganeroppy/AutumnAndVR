﻿/*
開始前テキスト

string str = “”;

str += 

	"くりあたいむ” + Time + “\n” ;
	str += "ひろった栗の数 = ” + Chestnut.Count + “\n”;
	str += “おとした栗の数 = ” + Chestnut.Count + “\n”;

foreach(Photon.

	str +=
	"あなたのふみつけ数” +  00
	ともだちのふみつけ数 00
	ごうけいふみつけ数 00
	*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// ゲーム情報を表示する
/// </summary>
public class InfoText : Photon.MonoBehaviour {

	public static InfoText instance;

	public TextMesh mesh;

	public MeshRenderer textRenderer;

	bool initialized = false;

	float wait = 0;

	void Awake()
	{
		instance = this;
	}

	// Use this for initialization
	void Start () {
		// プレイヤーのほうを向く
		StartCoroutine( WaitForPlayer() );
	}

	IEnumerator WaitForPlayer()
	{
		while(true)
		{
			if(PhotonNetwork.room == null)
			{
				yield return null;	
			}
			else
			{
				break;
			}
		}

		int playerId = PhotonNetwork.player.ID;

		while(true)
		{
			if(MultiPlayerManager.crews == null || MultiPlayerManager.crews.Count <= 0)
			{
				yield return null;	
			}
			else
			{
				break;
			}
		}

		CrewMove target = null;

		foreach( CrewMove c in MultiPlayerManager.crews)
		{
			if( c.photonView.isMine )
			{
				target = c;
				break;
			}	
		}

		// テキストなのでプレイヤーに背中を向ける
		Vector3 dir = transform.position - target.eye.transform.position;
		Quaternion q = Quaternion.LookRotation( dir );
		transform.rotation = q;

		initialized = true;
	}
	
	// Update is called once per frame
	void Update () 
	{	if(!initialized ){
			return;
		}

		if( wait > 0)
		{
			wait -= Time.deltaTime;
		}

		UpdateText();
	}

	/// <summary>
	/// テキストを更新
	/// </summary>
	void UpdateText()
	{
		if (GameManager.instance.running || wait > 0) {
			textRenderer.enabled = false;
			return;
		}
		textRenderer.enabled = true;

		string str = "";

		switch( GameManager.instance.curStatus )
		{
		case GameManager.Status.BeforeStart:
			int playerNum = PhotonNetwork.room.playerCount;
			if(  playerNum < MultiPlayerManager.playerNumNeeded )
			{
				str = "プレイヤーのさんか\nをたいきちゅう ( " + playerNum.ToString() + " / " + MultiPlayerManager.playerNumNeeded + " )";
			}
			else
			{
				bool ready1p = false;
				bool ready2p = false;
				for( int i=0 ; i< MultiPlayerManager.crews.Count ; i++ )
				{
					CrewMove c = MultiPlayerManager.crews[i];
					if ( c.photonView.ownerId == 1 )
					{
						ready1p = c.ready;
					}
					else
					{
						ready2p = c.ready;
					}
				}
				str = "プレイヤーのじゅんびを\nたいきちゅう ( 1P " + (ready1p ? "◯" : "×") + " / 2P " + (ready2p ? "◯" : "×") + " )"; 
			}
			break;
		case GameManager.Status.GameClear:
			int clearTime = ((int)GameManager.instance.gameTimer);
			int chestnutCount = GameManager.instance.chestnutCount;
			int myStomp = 0;
			int otherStomp = 0;
			for( int i=0 ; i< MultiPlayerManager.crews.Count ; i++ )
			{
				CrewMove c = MultiPlayerManager.crews[i];
				if ( c.photonView.isMine )
				{
					myStomp = c.stompCount;
				}
				else
				{
					otherStomp = c.stompCount;
				}
			}

			int totalStomp = myStomp + otherStomp;

			str = "くりあたいむ\n" + clearTime.ToString () + "びょう" +
			//	"びょう\n\nひろったくりのかず " + chestnutCount.ToString() + "こ" +
				"\n\nあなたは " + myStomp.ToString() + " ふみ" +
				"\nともだちは " + otherStomp.ToString() + " ふみ";
			break;

		case GameManager.Status.GameOver:
			int height = (int)Muscle.height;
			int diff = GameManager.instance._goalHeight - height;

			str = "たいむおーばー\nとうたつこうど " + height.ToString() + "めーとる\n\nごーるまであと" + diff.ToString() + "めーとる";


			break;
		}


		mesh.text = str;
	}

	public void SetWait(float value)
	{
		wait = value;
	}
}
