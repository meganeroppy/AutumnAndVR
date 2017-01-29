using UnityEngine;
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

	private GameObject effectOngoing;

	public bool inParentTransform = false;

	public Material fallLine_good;

	public Material fallLine_bad;

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


	}

	void Start()
	{
		if (inParentTransform) {
			// 生成器を親にセット(移動目的)
			transform.SetParent (FallItemGenerator.instance.transform);
			// 親なしにする
			transform.parent = null;
		}

		DrawFallLine ();
	}

	void DrawFallLine()
	{
		var lr = gameObject.AddComponent<LineRenderer> ();
		lr.SetPosition (0, transform.position + Vector3.down * 10f);
		lr.SetPosition (1, transform.position + Vector3.down * 20f);
		lr.material = isGoodItem ? fallLine_good : fallLine_bad;
		var width = isGoodItem ? 0.4f : 0.2f;
		lr.SetWidth (width, 0f);
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
		transform.Translate( Vector3.down * fallingSpeed * Time.deltaTime);
	}

	/// <summary>
	/// 回転更新
	/// </summary>
	void UpdateRotation()
	{
		transform.Rotate(Vector3.up, 15f * rotationSpeed * Time.deltaTime);
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
		// 良い結果か
		bool positive;

		GameObject effect = null;

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
				effect = effects [(int)EffectType.Catch];

				GetComponent<AudioSource>().Play();
			}

			// コントローラを振動させる
		}else{
			// 筋肉にぶつかった
			if( isGoodItem )
			{
				positive = true;

				// 避けるべきアイテム
				effect = effects [(int)EffectType.Happy];

				GetComponent<AudioSource>().Play();
			}
			else
			{
				positive = false;

				// キャッチするべきアイテム
				effect = effects[(int)EffectType.Damage];	
			}				
		}

		Muscle.instance.ChangeRate(positive);

		if (effect != null && effectOngoing == null) {
			effectOngoing = Instantiate (effect);
			effectOngoing.transform.position = transform.position;
			effectOngoing.transform.SetParent( Muscle.instance.transform, true );
		}

		harvested = true;
		model.SetActive(false);
	}

	void OnDestroy()
	{
		if (effectOngoing != null) {
			Destroy (effectOngoing);
		}
	}
}
