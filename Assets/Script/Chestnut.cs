using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// イガグリ
/// </summary>
public class Chestnut : Photon.MonoBehaviour {

	/// <summary>
	/// イガグリリスト
	/// </summary>
	public static List<Chestnut> cList;

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
		Good,
		bad,
	}

	public GameObject[] effects;

	/// <summary>
	/// モデル
	/// </summary>
	public GameObject model;

	void Awake()
	{
		if( cList == null)
		{
			cList = new List<Chestnut>();
		}

		cList.Add(this);

		Debug.Log("プレイヤーID" + photonView.ownerId.ToString() + "の栗を生成");


	}

	// Update is called once per frame
	void Update () 
	{
		if( !PhotonNetwork.isMasterClient )
		{
		//	return;
		}

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
			//if( PhotonNetwork.isMasterClient )
			//{
			//	PhotonNetwork.Destroy(gameObject);
			//}
			Destroy(gameObject);

			Debug.Log("プレイヤーID" + photonView.ownerId.ToString() + "の栗を削除");
		}
	}

	/// <summary>
	/// 収穫される
	/// </summary>
	/// <param name="byBag">カゴによる収穫か？</param>
	[PunRPC]
	public void Harvest(bool byBag)
	{
		if( !GameManager.instance.running )
		{
		//	return;
		}

		GameObject effect;

		if(byBag){
			// 良いエフェクト
			effect = Instantiate( effects[(int)EffectType.Good] );
		}else{
			// ダメージエフェクト
			effect = Instantiate( effects[(int)EffectType.bad] );
		}

		effect.transform.position = transform.position;

		harvested = true;
		model.SetActive(false);
	}
}
