using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 特定のオブジェクトが指定の３つの動点からできる三角形の内側にあるかどうか調べる
/// </summary>
public class TriangleDetect : MonoBehaviour 
{
	[SerializeField]
	Transform[] points = new Transform[3];

	[SerializeField]
	Transform target;

	void Update()
	{
		var inside = IsInside( points[0].position,  points[1].position,  points[2].position, target.position);
		Debug.Log( inside ? "外側" : "内側");
	}
		
	// 三角形と点の当たり判定(３Dの場合)
	// 戻り値    0:三角形の内側に点がある    1:三角形の外側に点がある
	public static bool IsInside( Vector3 A, Vector3 B, Vector3 C, Vector3 P ) {

		//点と三角形は同一平面上にあるものとしています。同一平面上に無い場合は正しい結果になりません
		//線上は外とみなします。
		//ABCが三角形かどうかのチェックは省略...

		Vector3 AB = B - A;

		Vector3 BP = P - B;

		Vector3 BC = C - B;
		Vector3 CP = P - C;

		Vector3 CA = A - C;
		Vector3 AP = P - A;

		Vector3 c1 = Vector3.Cross( AB, BP );
		Vector3 c2 = Vector3.Cross( BC, CP );
		Vector3 c3 = Vector3.Cross( CA, AP );

		//内積で順方向か逆方向か調べる
		double dot_12 = Vector3.Dot(c1, c2);
		double dot_13 = Vector3.Dot(c1, c3);

		if( dot_12 > 0 && dot_13 > 0 ) {
			//三角形の内側に点がある

			var averageHeight = (A.y + B.y + C.y) / 3;
			if (Mathf.Abs (averageHeight - P.y) < 0.25f ){
				return true;
			}
		}

		//三角形の外側に点がある
		return false;
	}
}
