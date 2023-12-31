using UnityEngine;
using System.Collections;

/*
 *	弾クラス
 *	Maruchu
 *
 *	何でもいいので接触したらエフェクトを出して消える
 */
public class Bullet : HitObject
{

	private static readonly float bulletMoveSpeed = 10.0f;                  //1秒間に弾が進む距離

	public GameObject hitEffectPrefab = null;                       //ヒットエフェクトのプレハブ


	/*
	 *	毎フレーム呼び出される関数
	 */
	private void Update()
	{

		//移動
		{
			//1秒間の移動量
			Vector3 vecAddPos = (Vector3.forward * bulletMoveSpeed);
			/*
					Vector3.forward は new Vector3( 0f, 0f, 1f) と同じです

					他にも色々あるので↓のページを参照してみてください
					http://docs.unity3d.com/ScriptReference/Vector3.html

					そして Vector3 に transform.rotation をかけると、その方向へ曲げてくれます
					この時、Vector3 は Z+ の方向を正面として考えます
			 */

			//移動量、回転量には Time.deltaTime をかけて実行環境(フレーム数の差)による違いが出ないようにします
			transform.position += ((transform.rotation * vecAddPos) * Time.deltaTime);
		}
	}



	/*
	 *	Collider が何かにヒットしたら呼ばれる関数
	 *
	 *	自分の GameObject に Collider(IsTriggerをつける) と Rigidbody をつけると呼ばれるようになります
	 */
	private void OnTriggerEnter(Collider hitCollider)
	{

		// 히트해도 되는지 확인
		if (false == IsHitOK(hitCollider.gameObject))
		{
			// このオブジェクトにはあたってはいけない
			return;
		}

		// ヒットエフェクトある？
		if (null != hitEffectPrefab)
		{
			// 자신과 같은 위치에서 히트 이펙트를 내다
			Instantiate(hitEffectPrefab, transform.position, transform.rotation);
		}

		// 이 GameObject를 히에랄키에서 삭제
		Destroy(gameObject);
	}

}
