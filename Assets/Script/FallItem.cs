﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 落下アイテム
/// </summary>
public class FallItem : Photon.MonoBehaviour {

	/// <summary>
	/// イガグリリスト
	/// </summary>
	public static List<FallItem> fList;

	/// <summary>
	/// 落下スピード
	/// </summary>
	public float fallingSpeed = 0.02f;

	/// <summary>
	/// 回転スピード
	/// </summary>
	public float rotationSpeed = 0.1f;

	/// <summary>
	/// 命の長さ秒
	/// </summary>
	public float lifeTime = 5f;
	float timer = 0;

	/// <summary>
	/// 収穫済みか？
	/// </summary>
	public bool harvested{
		get;
		private set;
	}

	public enum EffectType{
		Catch,
		Damage,
		Happy
	}

	public bool isGoodItem;

	public GameObject[] effects;

	public bool inParentTransform = false;

	/// <summary>
	/// モデル
	/// </summary>
	public GameObject model;

	void Awake()
	{
		if( fList == null)
		{
			fList = new List<FallItem>();
		}

		fList.Add(this);

		Debug.Log("プレイヤーID" + photonView.ownerId.ToString() + "のアイテムを生成");

		if (inParentTransform) {
			// 生成器を親にセット
			transform.SetParent (FallItemGenerator.instance.transform);
		}
	}

	// Update is called once per frame
	void Update () 
	{
		UpdatePosition();
		UpdateRotation();
		UpdateLifeTimer();
	}

	/// <summary>
	/// 位置更新
	/// </summary>
	void UpdatePosition()
	{
		transform.Translate( Vector3.down * fallingSpeed );
	}

	/// <summary>
	/// 回転更新
	/// </summary>
	void UpdateRotation()
	{
		transform.Rotate(Vector3.up, 15f * rotationSpeed );
	}

	/// <summary>
	/// 残り寿命を更新
	/// </summary>
	void UpdateLifeTimer()
	{
		timer += Time.deltaTime;
		if( timer >= lifeTime )
		{
			harvested = true;
			Destroy(gameObject);
			//Debug.Log("プレイヤーID" + photonView.ownerId.ToString() + "のアイテムを削除");
		}
	}

	/// <summary>
	/// 収穫される
	/// </summary>
	/// <param name="byBag">プレイヤーにキャッチされたか？</param>
	[PunRPC]
	public void Harvest(bool caught)
	{
		GameObject effect = null;

		// 良い結果か
		bool positive;

		if(caught)
		{
			// カゴにぶつかった
			if( isGoodItem )
			{
				// 避けるべきアイテムのとき

				positive = false;
			}
			else
			{
				positive = true;

				// キャッチするべきアイテムのとき
				effect = Instantiate( effects[(int)EffectType.Catch] );

				GetComponent<AudioSource>().Play();
			}

			// コントローラを振動させる
		}else{
			// 筋肉にぶつかった
			if( isGoodItem )
			{
				positive = true;

				// 避けるべきアイテム
				effect = Instantiate( effects[(int)EffectType.Happy] );

				GetComponent<AudioSource>().Play();
			}
			else
			{
				positive = false;

				// キャッチするべきアイテム
				effect = Instantiate( effects[(int)EffectType.Damage] );	
			}				
		}

		Muscle.instance.ChangeRate(positive);

		if( effect != null )
		{
			effect.transform.position = transform.position;
		}

		harvested = true;
		model.SetActive(false);
	}
}
