using UnityEngine;
using System.Collections;

public class BagMove : Photon.MonoBehaviour 
{
	/// <summary>
	/// カゴのモデルリスト
	/// </summary>
	public GameObject[] models;

	/// <summary>
	/// 栗を取ったとみなす範囲
	/// </summary>
	float catchRange = 0.75f;

	/// <summary>
	/// モデル差し替えの閾値
	/// </summary>
	int[] modelChangeThreshold = new int[3]{10, 20, 40};

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
		CheckChestnut();
	}

	void CheckChestnut()
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

			// 栗との距離
			dist =  Mathf.Abs( (transform.position - c.transform.position).magnitude );

			if( dist <= catchRange )
			{
				Debug.Log("栗をキャッチ");
				//PhotonNetwork.RPC(c.photonView, "Harvest", PhotonTargets.All, false, true);

				c.Harvest(true);

				// ローカルのみで良い
				catchCount++;

				//PhotonNetwork.RPC( GameManagerTest.instance.photonView, "AddChestnutCount", PhotonTargets.All, false, 1);

				// 効果音
				GetComponent<AudioSource>().PlayOneShot(se_catch);

				// 筋肉の喜び時間追加
				myMuscle.joyTimer += 1f;

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
