using UnityEngine;
using System.Collections;

public class BagMove : Photon.MonoBehaviour 
{
	/// <summary>
	/// カゴのモデルリスト
	/// </summary>
	public GameObject[] models;

	/// <summary>
	/// アイテムを取ったとみなす範囲
	/// </summary>
	float catchRange = 0.95f;

	/// <summary>
	/// モデル差し替えの閾値
	/// </summary>
	int[] modelChangeThreshold = new int[3]{15, 30, 50};

	/// <summary>
	/// キャッチ時効果音
	/// </summary>
	public AudioClip se_catch;

	/// <summary>
	/// キャッチ時筋肉ボイス
	/// </summary>
	public AudioClip vo_catch;

	int catchCount = 0;

	private Muscle myMuscle;

	// Use this for initialization
	void Start ()
	{
		if(!photonView.isMine)
		{
			Destroy(gameObject);
		}

		myMuscle = GameObject.Find("Muscle").GetComponent<Muscle>();
	}

	public void InitModel()
	{
		for( int i=0 ; i < modelChangeThreshold.Length ; i++ )
		{	
			models[i].SetActive( false );
		}
	}


	void Update()
	{
		CheckFallItem();
	}

	void CheckFallItem()
	{
		if(FallItem.fList == null)
		{
			return;
		}

		float dist; 
		for(int i=0 ; i < FallItem.fList.Count ; i++)
		{
			FallItem c = FallItem.fList[i];

			if (c == null) {
				continue;
			}

			// すでに取られたアイテムはスキップ
			if( c.harvested ) continue;

			// アイテムとの距離
			dist =  Mathf.Abs( (transform.position - c.transform.position).magnitude );

			if( dist <= catchRange )
			{
				Debug.Log("アイテムをキャッチ");
				//PhotonNetwork.RPC(c.photonView, "Harvest", PhotonTargets.All, false, true);

				c.Harvest(true);

				// 拾うべきアイテムのときだけ処理
				if( !c.isGoodItem )
				{
					// ローカルのみで良い
					catchCount++;

					// 効果音
					GetComponent<AudioSource>().PlayOneShot(se_catch);

					// 筋肉の喜び時間追加
					myMuscle.Enjoy();

					// ボイス
					if(myMuscle != null)
					{
						myMuscle.PlaySe(vo_catch);
					}

					// モデルの更新
					SetModel();
				}
			}
		}
	}

	/// <summary>
	/// カゴのモデルを更新する
	/// </summary>
	public void SetModel()
	{
		for( int i=0 ; i < modelChangeThreshold.Length ; i++ )
		{
			models[i].SetActive( catchCount >= modelChangeThreshold[i] );
		}
	}
}
