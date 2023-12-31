using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


/*
 *	プレーヤークラス 基底
 *	Maruchu
 *
 *	キャラクターの移動、メカニム(モーション)の制御など
 */
public class Player_Base : HitObject
{
	GameObject mainCanvas;

	//プレーヤーの操作の種類
	protected enum PlayerInput
	{
		Move_Left       //移動 左
		, Move_Up       //移動 上
		, Move_Right        //移動 右
		, Move_Down     //移動 下
		, Shoot         //射撃
		, Replay        // 재시작
		, EnumMax       //最大数
	}

	private static readonly float MOVE_ROTATION_Y_LEFT = -90f;      //移動方向 左
	private static readonly float MOVE_ROTATION_Y_UP = 0f;      //移動方向 上
	private static readonly float MOVE_ROTATION_Y_RIGHT = 90f;      //移動方向 右
	private static readonly float MOVE_ROTATION_Y_DOWN = 180f;      //移動方向 下

	public float MOVE_SPEED = 5.0f;     //移動の速度

	public GameObject playerObject = null;      //動かす対象のモデル
	public GameObject bulletObject = null;      //弾プレハブ


	public GameObject hitEffectPrefab = null;       //ヒットエフェクトのプレハブ


	private float m_rotationY = 0.0f;       //プレーヤーの回転角度

	protected bool[] m_playerInput = new bool[(int)PlayerInput.EnumMax];        //押されている操作

	public bool m_playerDeadFlag = false;        // 플레이어가 죽은 플래그

	protected int MaxHP = 10;
	protected int HP = 10;


	[SerializeField] GameObject target = null;

	private void Start()
	{
		FindTarget();
	}

	void FindTarget()
	{
		switch (m_hitGroup)
		{
			case HitGroup.Player1:
				target = GameObject.FindGameObjectWithTag("Player2");
				break;
			case HitGroup.Player2:
				target = GameObject.FindGameObjectWithTag("Player1");
				break;
			case HitGroup.Other:
				break;
			default:
				break;
		}

	}

	/*
	 *	毎フレーム呼び出される関数
	 */
	private void Update()
	{
		//if(target.GetComponent<Player_Key>() == null)
		//{
		//	if (m_playerDeadFlag || target.GetComponent<Player_AI>().isDead)
		//		if (m_playerInput[(int)PlayerInput.Replay])
		//			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		//}

		//if(target.GetComponent<Player_AI>() == null)
		//{
		//	if (m_playerDeadFlag || target.GetComponent<Player_Key>().isDead)
		//		if (m_playerInput[(int)PlayerInput.Replay])
		//			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		//}

		// Player 죽음
		if (m_playerDeadFlag)
		{
			// 죽은 오브젝트의 rotation 고정
			gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

			// 모든 처리를 무시한다
			return;
		}

		//フラグ初期化
		ClearInput();
		//入力処理取得
		GetInput();

		//移動処理
		CheckMove();
	}


	/*
	 *	入力処理のチェック
	 */
	private void ClearInput()
	{
		//フラグ初期化
		int i;
		for (i = 0; i < (int)PlayerInput.EnumMax; i++)
		{
			m_playerInput[i] = false;
		}
	}
	/*
	 *	入力処理のチェック
	 */
	protected virtual void GetInput()
	{
	}


	/*
	 *	移動処理のチェック
	 */
	private void CheckMove()
	{

		//アニメーター(メカニム)を取得
		Animator animator = playerObject.GetComponent<Animator>();

		//弾にあたってなければ移動OK
		float moveSpeed = MOVE_SPEED;       //移動速度
		bool shootFlag = false;         //弾を撃つフラグ

		//移動と回転
		{
			// 키 조작에 의한 회전과 이동
			if (m_playerInput[(int)PlayerInput.Move_Left])
			{
				//左
				m_rotationY = MOVE_ROTATION_Y_LEFT;
			}
			else
			if (m_playerInput[(int)PlayerInput.Move_Up])
			{
				//上
				m_rotationY = MOVE_ROTATION_Y_UP;
			}
			else
			if (m_playerInput[(int)PlayerInput.Move_Right])
			{
				//右
				m_rotationY = MOVE_ROTATION_Y_RIGHT;
			}
			else
			if (m_playerInput[(int)PlayerInput.Move_Down])
			{
				//下
				m_rotationY = MOVE_ROTATION_Y_DOWN;
			}
			else
			{
				// 아무것도 누르지 않으면 이동하지 않는
				moveSpeed = 0f;
			}

			// 향하고 있는 방향을 오일러 각으로 넣다
			transform.rotation = Quaternion.Euler(0, m_rotationY, 0);       //Y軸回転でキャラの向きを横に動かせます

			// 이동량을 Transform으로 넘겨 이동시키다
			transform.position += ((transform.rotation * (Vector3.forward * moveSpeed)) * Time.deltaTime);
		}


		// 사격
		{
			// 사격 버튼(클릭) 누르고 있어?
			if (m_playerInput[(int)PlayerInput.Shoot])
			{
				// 쏘았다
				shootFlag = true;

				// 탄을 생성하는 위치
				Vector3 vecBulletPos = transform.position;
				// 진행 방향으로 좀 앞으로
				vecBulletPos += (transform.rotation * Vector3.forward);
				// Y는 높이를 적당히 올린다
				vecBulletPos.y = 2.0f;

				// 탄을 생성
				Instantiate(bulletObject, vecBulletPos, transform.rotation);
			}
			else
			{
				// 쏘지 않았다
				shootFlag = false;
			}
		}


		//メカニム
		{
			//Animatorで設定した値を渡す
			animator.SetFloat("Speed", moveSpeed);      //移動量
			animator.SetBool("Shoot", shootFlag);       //射撃フラグ
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
			// 이 오브젝트에는 닿아서는 안 된다
			return;
		}


		// 총알에 맞았다
		{
			HP--;
			// 애니메이터(메카님) 취득
			Animator animator = playerObject.GetComponent<Animator>();

			if (HP <= 0)
			{
				// 메카님에게 죽었음을 통지
				animator.SetBool("Dead", true);     // 죽은 플래그

				// 이 플레이어는 죽은 상태로 만든다
				m_playerDeadFlag = true;
			}
		}

		// 히트 이펙트 있다？
		if (null != hitEffectPrefab)
		{
			//自分と同じ位置でヒットエフェクトを出す
			Instantiate(hitEffectPrefab, transform.position, transform.rotation);
		}


	}

}
