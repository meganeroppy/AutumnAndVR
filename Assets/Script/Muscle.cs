using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

/// <summary>
/// 上昇する筋肉
/// </summary>
public class Muscle : Photon.MonoBehaviour 
{
	public static Muscle instance;

	/// <summary>
	/// 筋肉の現在高度
	/// </summary>
	public static float height;

	/// <summary>
	/// 一回の上昇で 何M 上昇するか
	/// </summary>
	public float ascend_value = 1f; 

	/// <summary>
	/// 何ポイントで１回上昇するか？
	/// </summary>
	const int ascend_cost = 1;

	/// <summary>
	/// 集まったエネルギー
	/// </summary>
	int energy = 0; 

	/// <summary>
	/// アイテムがぶつかったとみなす範囲
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

	/// <summary>
	/// クリア時のテキスト
	/// </summary>
	public TextMesh clearText;

	/// <summary>
	/// 筋肉の初期位置
	/// </summary>
	public Transform originPos;

	/// <summary>
	/// キャッチ時効果音
	/// </summary>
	public AudioClip se_catch;

	/// <summary>
	/// キャッチ時筋肉ボイス
	/// </summary>
	public AudioClip vo_catch;


	/// <summary>
	/// 筋肉喜び中時間の残り秒
	/// </summary>
	[HideInInspector]
	private float joyTimer =0;
	public void Enjoy()
	{
		joy_rate *= 1.35f;
		//Debug.Log(joy_rate);
		joyTimer = 5;
	}

	/// <summary>
	/// 到達した高さの区切り
	/// </summary>
	private int reachedHeightUnit = 0;

	/// <summary>
	/// 上昇率
	/// </summary>
	/// <value>The ascend rate.</value>
	public float joy_rate{ get; set; }

	public GameManager gm;

	/// <summary>
	/// プレイヤーポジション
	/// </summary>
	public Transform pos1;

	/// <summary>
	/// プレイヤーポジション２
	/// </summary>
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

		joy_rate = 1f;
	}

	void Update()
	{
		// 上昇
		Ascend();

		// アイテムとのあたり判定をチェック
		CheckCollisionFallItem();

		ReduceJoyTimer ();
	}

	bool reduceOnce = false;

	/// <summary>
	/// 喜び時間減少
	/// </summary>
	void ReduceJoyTimer()
	{
		if (joyTimer >= 0) {
			reduceOnce = false;
			joyTimer -= Time.deltaTime;
		}
		else{
			if( !reduceOnce )
			{
				joy_rate *= 0.5f;
				reduceOnce = true;
			}
		}
	}
		
	/// <summary>
	/// 毎フレームの上昇
	/// </summary>
	void Ascend()
	{
		if (!GameManager.instance.running) {
			return;
		}

		var val = ascend_value;
		if( joyTimer > 0 )
		{
			val *= 	joy_rate;
		}

		height += val * Time.deltaTime;

		var originPos = transform.position;
		transform.position = new Vector3(originPos.x, height, originPos.z);

//		Debug.Log(height.ToString() + "まで上昇");

		if( height >= GameManager.instance.goalHeight )
		{
			StartCoroutine( gm.ShowGameClearExpression());
			clearText.gameObject.SetActive (true);
			clearText.text = "くりあたいむ\n" + ((int)GameManager.instance.gameTimer).ToString () + "びょう";
		}
		else
		{
			int reachUnit = (int)(height / GameManager.instance.measureExpInterval);
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
	public void AddEnergy(int val, CrewMove sender)
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
	/// アイテムとの衝突を検出
	/// </summary>
	void CheckCollisionFallItem()
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

			// アイテムとの高度比較
			dist =  Mathf.Abs( (transform.position.y - c.transform.position.y) );

			if( dist <= hitRange )
			{
				Debug.Log("アイテムが筋肉にヒット！");

				c.Harvest(false);
			
				if( c.isGoodItem )
				{
					PlaySe(se_catch);

					// 筋肉の喜び時間追加
					Enjoy();

					// ボイス
					PlaySe(vo_catch);

				}
				else
				{
					Roar();
				}
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

	/// <summary>
	/// 到達高度を3Dテキストで表示
	/// </summary>
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
		if(myAudio.isPlaying && !GameManager.instance.running)
		{
			SetBGM(false);
		}
	}
}