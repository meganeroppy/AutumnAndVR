﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

/// <summary>
/// 上昇する筋肉
/// </summary>
public class MuscleTest : Photon.MonoBehaviour 
{
	public static MuscleTest instance;
	public static float height; //高度

	const float ascend_value = 1f; // 一回の上昇で 何M 上昇するか
	const int ascend_cost = 1; // 何ポイントで１回上昇するか？
	int energy = 0; // 集まったエネルギー

	/// <summary>
	/// 栗がぶつかったとみなす範囲
	/// </summary>
	float hitRange = 0.1f;

	public AudioSource myAudio;

	/// <summary>
	/// ダメージを食らったときのボイス
	/// </summary>
	public AudioClip[] se_roar;

	/// <summary>
	/// ゲームクリア時のボイス
	/// </summary>
	public AudioClip se_clear;

	/// <summary>
	/// 特定高度に達したときの効果音
	/// </summary>
	public AudioClip se_reachUnitHeight;

	/// <summary>
	/// 特定高度に達したときのボイス
	/// </summary>
	public AudioClip vo_reachUnitHeight;

	public TextMesh clearText;

	/// <summary>
	/// 筋肉の初期位置
	/// </summary>
	public Transform originPos;

	float joyTimer =0;

	/// <summary>
	/// 到達した高さの区切り
	/// </summary>
	private int reachedHeightUnit = 0;

	public GameManagerTest gm;

	public Transform pos1;

	public Transform pos2;

	enum VoicePat{
		Delight,	// 歓喜
		Painful,	// 苦痛
	}

	void Start()
	{
		instance = this;

		height = transform.position.y;
		Debug.Log("スタート時点での高度は " + height.ToString() );
	}

	void Update()
	{
		// 栗とのあたり判定をチェック
		CheckCollisionChestnut();

		ReduceJoyTimer ();

	//	CheckBGMPlay();
	}

	void ReduceJoyTimer()
	{
		if (joyTimer >= 0) {
			joyTimer -= Time.deltaTime;
		}
	}

	/// <summary>
	/// 上昇する
	/// </summary>
	void Ascend()
	{
		if (!GameManagerTest.running) {
			return;
		}

		float val = ascend_value;
		if( joyTimer > 0 )
		{
			val *= 3f;
		}

		height += ascend_value;

		transform.DOMoveY( height, 0.5f);

		Debug.Log(height.ToString() + "まで上昇");

		if( height >= GameManagerTest._goalHeight )
		{
			StartCoroutine( gm.ShowGameClearExpression());

			clearText.gameObject.SetActive (true);
			clearText.text = "くりあたいむ\n" + ((int)GameManagerTest.gameTimer).ToString () + "びょう";
		}
		else
		{
			int reachUnit = (int)(height / GameManagerTest.measureExpInterval);

			if( reachUnit > reachedHeightUnit )
			{
				// ボイスと効果音再生
				StartCoroutine( PlayReachingSe() );

				reachedHeightUnit = reachUnit;
			}
		}

	}

	/// <summary>
	/// エネルギーを加算
	/// </summary>
	public void AddEnergy(int val, CrewMoveTest sender)
	{
		Debug.Log("プレイヤー[ " + sender.photonView.ownerId.ToString() + " ]がエネルギーを加算");

		energy += val ;
		if( energy >= ascend_cost ){
			Ascend();
			energy -= ascend_cost;
		}
	}

	/// <summary>
	/// 苦痛のうめき声
	/// </summary>
	/// <param name="pat">ボイスタイプ</param>
	private void Roar()
	{
		int key = Random.Range (0, se_roar.Length - 1);

		// うめき声を再生
		myAudio.PlayOneShot( se_roar[key] );
	}

	/// <summary>
	/// 特定の高さに達したときのSEを再生する
	/// </summary>
	IEnumerator PlayReachingSe()
	{
		// SE
		myAudio.PlayOneShot(se_reachUnitHeight);

		// SEの長さ分待機
		yield return new WaitForSeconds(1.5f);

		// ボイス
		myAudio.PlayOneShot(vo_reachUnitHeight);
	}

	/// <summary>
	/// 栗との衝突を検出
	/// </summary>
	void CheckCollisionChestnut()
	{
		if(Chestnut.cList == null)
		{
			return;
		}

		float dist; 
		for(int i=0 ; i < Chestnut.cList.Count ; i++)
		{
			Chestnut c = Chestnut.cList[i];

			if (c == null) {
				continue;
			}

			// すでに取られた栗はスキップ
			if( c.harvested ) continue;

			// 栗との高度比較
			dist =  Mathf.Abs( (transform.position.y - c.transform.position.y) );

			if( dist <= hitRange )
			{
				Debug.Log("栗が筋肉にヒット！");

				joyTimer += 1f;

				c.Harvest(false);
			
				Roar();
			}
		}
	}

	/// <summary>
	/// 初期位置に戻す
	/// </summary>
	public void SetToOrigin()
	{
		transform.position = originPos.position;
		height = transform.position.y;
		energy = 0;
		clearText.gameObject.SetActive (false);
	}


	/// <summary>
	/// 外部から
	/// 効果音とボイスを再生
	/// </summary>
	public void PlaySe(AudioClip clip)
	{
		myAudio.PlayOneShot(clip);
	}

	public void DisplayReachedHeight()
	{
		clearText.gameObject.SetActive (true);
		clearText.text = "たいむおーばー\n\nとうたつこうど\n" + ((int)height).ToString () + "めーとる";
	}

	/// <summary>
	/// BGMを再生
	/// </summary>
	public void SetBGM(bool play)
	{
		if(play)
		{
			myAudio.Play();
		}
		else
		{
			myAudio.Stop();
		}
	}

	/// <summary>
	/// ゲーム中でなければBGMを停止
	/// </summary>
	private void CheckBGMPlay()
	{
		if(myAudio.isPlaying && !GameManagerTest.running)
		{
			SetBGM(false);
		}
	}
}