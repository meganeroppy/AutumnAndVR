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

	public int chestnutCount{ get; private set; }

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
		ResetParametersAndLoadTitleScene();
	}

	/// <summary>
	/// パラメータを初期化してタイトルシーンを読み込む
	/// </summary>
	public void ResetParametersAndLoadTitleScene()
	{
		gameTimer = 0;

		curStatus = Status.BeforeStart;

		running = false;
		inStartingProcess = false;
		chestnutCount = 0;

		if (Muscle.instance != null) {
			Muscle.instance.SetToOrigin ();
		}

		GameObject b = GameObject.Find ("Bag");
		if( b != null )
		{
			b.GetComponent<BagMove>().InitModel();	
		}
			
		//SceneManager.LoadSceneAsync("Title", LoadSceneMode.Additive);
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

		if(!running)
		{
			return;
		}

		UpdateGameTimer();

		CheckInput();
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
		if( MultiPlayerManager.crews == null )
		{
			return;
		}

		if(  MultiPlayerManager.crews.Count < MultiPlayerManager.playerNumNeeded )
		{
			return;
		}

		for( int i=0 ; i< MultiPlayerManager.crews.Count ; i++ )
		{
			if ( !MultiPlayerManager.crews[i].ready )
			{
				return;
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

		if (Chestnut.cList != null) {
			for (int i = Chestnut.cList.Count - 1; i >= 0; i--) {
				var c = Chestnut.cList [i];
				if (c != null) {
					Destroy (c.gameObject);
				}
			}
			Chestnut.cList.Clear ();
		}

		int height = (int)Mathf.Floor( Muscle.height );
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
		ResetParametersAndLoadTitleScene();
	}

	/// <summary>
	/// ゲームクリア演出
	/// </summary>
	public IEnumerator ShowGameClearExpression()
	{
		if (Chestnut.cList != null) {
			for (int i = Chestnut.cList.Count - 1; i >= 0; i--) {
				var c = Chestnut.cList [i];
				if (c != null) {
					Destroy (c.gameObject);
				}
			}
			Chestnut.cList.Clear ();
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
		ResetParametersAndLoadTitleScene();
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
			ResetParametersAndLoadTitleScene();
		}
	}

	[PunRPC]
	void AddChestnutCount()
	{
		chestnutCount++;
	}
}
