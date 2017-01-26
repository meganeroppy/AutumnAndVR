using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Measure : MonoBehaviour {

	public TextMesh textMesh;

	public MeshRenderer meshRenderer;

	bool enablePrev;

	public void SetText(string value){
		textMesh.text = value;
	}

	public SpriteRenderer flag;

	// Use this for initialization
	void Start () {
	//	meshRenderer = GetComponent<MeshRenderer> ();
		enablePrev = false;
	}

	// Update is called once per frame
	void Update () {
		int unitVal = GameManager.instance.measureExpInterval;

		// 近くだけ表示
		meshRenderer.enabled = Muscle.instance.height > transform.position.y - unitVal && Muscle.instance.height < transform.position.y + unitVal;

		if (flag.gameObject.activeInHierarchy) {
			flag.enabled = meshRenderer.enabled;
		}

		if (!enablePrev && meshRenderer.enabled && GameManager.instance.running) {
			FrameIn ();
		}
		enablePrev = meshRenderer.enabled;

		// 到達したら黄色
		textMesh.color = Muscle.instance.height >= transform.position.y ? Color.yellow : Color.red;
	}

	void FrameIn()
	{
		Vector3 target = transform.position;
		transform.Translate(Vector3.right * 50f);
		transform.DOMove (target, 1f).SetEase(Ease.OutBack);
	}

	public void EnableFlag(bool key)
	{
		flag.gameObject.SetActive (key);
	}
}
