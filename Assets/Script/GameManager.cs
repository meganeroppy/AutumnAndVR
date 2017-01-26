using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ゲーム全体の変数を管理
/// </summary>
public class GameManager : Photon.MonoBehaviour 
{
	public static GameManager instance;

	public enum Status{
		BeforeStart,
		GameClear,
		GameOver
	}

	/// <summary>
	/// 現在のステータス
	/// </summary>
	public Status curStatus{ get; private set; }

	/// <summary>
	/// シングルモード
	/// </summary>
	public bool singleMode = false;

	/// <summary>
	/// 制限時間
	/// </summary>
	public int timeLimitSec = 120;

	/// <summary>
	/// ゴールとみなす高度
	/// </summary>
	public int goalHeight = 200;

	/// <summary>
	/// 筋肉
	/// </summary>
	public Muscle muscle;

	/// <summary>
	/// 開始時の筋肉ボイス
	/// </summary>
	public AudioClip vo_start;

	/// <summary>
	/// クリア時の効果音
	/// </summary>
	public AudioClip se_clear;

	/// <summary>
	/// クリア時の筋肉ボイス
	/// </summary>
	public AudioClip vo_clear;

	/// <summary>
	/// 失敗時の効果音
	/// </summary>
	public AudioClip se_failed;

	/// <summary>
	/// 失敗時のボイス
	/// </summary>
	public AudioClip vo_failed;

	/// <summary>
	/// 経過時間
	/// </summary>
	public float gameTimer{ get; private set; }

	/// <summary>
	/// ゲームの最中か？
	/// </summary>
	public bool running{ get; private set; }

	/// <summary>
	/// ゲーム開始処理中か？
	/// </summary>
	public bool inStartingProcess{ get; private set; }

	public int catchCount{ get; private set; }

	public GameObject cityObjectSet;
	public GameObject spaceObjectSet;

	public Material citySkybox;
	public Material spaceSkybox;

	public Transform cloud;

	/// <summary>
	/// 高度到達演出が発生する頻度
	/// </summary>
	public int measureExpInterval = 25;

	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		Reset();
	}

	/// <summary>
	/// パラメータを初期化してタイトルシーンを読み込む
	/// </summary>
	private void Reset()
	{

			
		MultiPlayerManager.cList.ForEach( c => {
			c.photonView.RPC ("Reset", PhotonTargets.All);
		});
		//SceneManager.LoadSceneAsync("Title", LoadSceneMode.Additive);
	}

	public void ResetPrameter()
	{
		gameTimer = 0;

		curStatus = Status.BeforeStart;

		running = false;
		inStartingProcess = false;
		catchCount = 0;

		GameObject b = GameObject.Find ("Bag");
		if( b != null )
		{
			b.GetComponent<BagMove>().InitModel();	
		}
	}

	void Update () 
	{
		if( PhotonNetwork.room == null )
		{
			return;
		}

		if( !inStartingProcess )
		{
			CheckPlayersReady();
		}

		SwitchBg ();

		if(!running)
		{
			return;
		}

		UpdateGameTimer();

		CheckInput();

	}

	void SwitchBg()
	{
		// いずれかのプレイヤーが雲の高さに到達したら拝啓差し替え
		bool overSky = false;
		for (int i = 0; i < MultiPlayerManager.cList.Count; i++) 
		{
			if (MultiPlayerManager.cList [i].transform.position.y > cloud.position.y)
			{
				overSky = true;
				break;
			}
		}

		cityObjectSet.SetActive (!overSky);
		spaceObjectSet.SetActive (overSky);
		RenderSettings.skybox = overSky ? spaceSkybox : citySkybox;
	}

	/// <summary>
	/// 経過時間を更新
	/// 制限時間を超えたらタイムオーバー
	/// </summary>
	private void UpdateGameTimer()
	{
		gameTimer += Time.deltaTime;

		if( gameTimer >= timeLimitSec )
		{
			if(!running)
			{
				return;
			}
			StartCoroutine( ShowTimeOverExpression() );
		}
	}

	void CheckPlayersReady()
	{
		if( MultiPlayerManager.cList == null )
		{
			return;
		}

		if(  MultiPlayerManager.cList.Count < MultiPlayerManager.playerNumNeeded )
		{
			return;
		}

		if( singleMode && MultiPlayerManager.cList[0].ready )
		{
		}
		else
		{
			for( int i=0 ; i< MultiPlayerManager.cList.Count ; i++ )
			{
				if ( !MultiPlayerManager.cList[i].ready )
				{
					return;
				}
			}
		}
		inStartingProcess = true;
		ShowGameStartExpression();
	}

	/// <summary>
	/// /ゲーム開始演出
	/// </summary>
	void ShowGameStartExpression()
	{
		StartCoroutine (_ShowGameStartExpression ());
	}
	IEnumerator _ShowGameStartExpression()
	{	
		// 筋肉ボイス
		muscle.PlaySe(vo_start);

		yield return new WaitForSeconds(3f);

		// 文字演出

		// BGM再生
		muscle.SetBGM(true);

		running = true;
	}

	/// <summary>
	/// タイムオーバー演出
	/// </summary>
	private IEnumerator ShowTimeOverExpression()
	{
		float wait = 3f;

		InfoText.instance.SetWait(wait);
		curStatus = Status.GameOver;
		running = false;

		if (FallItem.fList != null) {
			for (int i = FallItem.fList.Count - 1; i >= 0; i--) {
				var c = FallItem.fList [i];
				if (c != null) {
					Destroy (c.gameObject);
				}
			}
			FallItem.fList.Clear ();
		}

		int height = (int)Mathf.Floor( Muscle.instance.height );
		Debug.LogErrorFormat("時間切れ！あなたが到達した高度は{0}", height);

		// BGM停止
		muscle.SetBGM(false);

		// 筋肉ボイス
		muscle.PlaySe(vo_failed);


		// 筋肉の額に文字演出
		muscle.DisplayReachedHeight();

		// 一泊待機
		yield return new WaitForSeconds(wait);

		// 効果音
		muscle.PlaySe(se_failed);

		yield return new WaitForSeconds(5f);

		// 入力を待機
		yield return WaitInput();

		// タイトルに戻る
		Reset();
	}

	/// <summary>
	/// ゲームクリア演出
	/// </summary>
	public IEnumerator ShowGameClearExpression()
	{
		if (FallItem.fList != null) {
			for (int i = FallItem.fList.Count - 1; i >= 0; i--) {
				var c = FallItem.fList [i];
				if (c != null) {
					Destroy (c.gameObject);
				}
			}
			FallItem.fList.Clear ();
		}

		Debug.LogError("ゴール！！");

		float wait = 3f;

		InfoText.instance.SetWait(wait);
		curStatus = Status.GameClear;
		running = false;

		// BGM停止
		muscle.SetBGM(false);

		// 効果音
		muscle.PlaySe(se_clear);

		yield return new WaitForSeconds(wait);

		// 筋肉ボイス
		muscle.PlaySe(vo_clear);

		yield return WaitInput();

		// タイトルに戻る
		Reset();
	}

	/// <summary>
	/// 入力を待機
	/// </summary>
	IEnumerator WaitInput()
	{
		do {
			if(Input.anyKeyDown)
			{
				yield break;
			}

			Debug.Log("入力待機中");
			yield return null;
		} while(true);
	}

	/// <summary>
	/// 入力チェックを行う
	/// 主にデバッグ用
	/// </summary>
	void CheckInput()
	{
		if( ( Input.GetKey( KeyCode.LeftControl ) 
			|| Input.GetKey( KeyCode.RightControl ) 
			|| Input.GetKey( KeyCode.LeftAlt ) 
			|| Input.GetKey( KeyCode.RightAlt ) ) 
			&& Input.GetKeyDown( KeyCode.Q ) )
		{
			// ctrl + Q で強制タイトル遷移
			Reset();
		}
	}

	[PunRPC]
	void AddCatchCount()
	{
		catchCount++;
	}
}
