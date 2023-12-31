using UnityEngine;
using System.Collections;

/// <summary>
/// 레이어 클래스 AI가 조작
/// </summary>
public class Player_AI : Player_Base
{
	//체크 방향
	private enum CheckDir
	{
		// ← ↑ → ↓ 순서
		Left        // 좌
		, Up        // 상
		, Right     // 우
		, Down      // 하
		, EnumMax   // 최대수
	}

	// 체크 정보
	private enum CheckData
	{
		X           // X축
		, Y         // Y축
		, EnumMax   // 최대수
	}

	private static readonly int[][] CHECK_DIR_LIST = new int[(int)CheckDir.EnumMax][] {		// 체크 방향
	//										 X		 Y
	 new int[ (int)CheckData.EnumMax] {     -1,      0      }
	,new int[ (int)CheckData.EnumMax] {      0,      1      }
	,new int[ (int)CheckData.EnumMax] {      1,      0      }
	,new int[ (int)CheckData.EnumMax] {      0,     -1      }
};

	private static readonly int AI_PRIO_MIN = 99; // AI우선 순위에서 가장 낮은 값


	private static readonly float AI_INTERVAL_MIN = 0.5f; // AI 사고 간격 최단
	private static readonly float AI_INTERVAL_MAX = 0.8f; // AI사고의 간격 최장

	private static readonly float AI_IGNORE_DISTANCE = 2.0f; // 플레이어에게 더 이상 접근하지 않는다

	private static readonly float SHOOT_INTERVAL = 1.0f; // 사격 간격



	private float m_aiInterval = 0f; // AI의 사고를 갱신하기까지의 시간
	private float m_shootInterval = 0f; // 사격 간격


	private PlayerInput m_pressInput = PlayerInput.Move_Left; // AI가 실시하는 입력의 종류

	bool isTargetDead = false;

	// 현재 오브젝트에서 앞 방향으로 Ray 생성
	Ray ray;

	/// <summary>
	/// 입력 처리 체크
	/// </summary>
	protected override void GetInput()
	{
		// 사용자가 움직이고 있는 플레이어의 객체를 가져옵니다.
		GameObject mainObject = Player_Key.m_mainPlayer;
		if (null == mainObject)
		{
			// 선수가 없었다면 사고를 중단했을 것이다.
			return;
		}


		// AI의 사고를 갱신하기까지의 시간
		m_aiInterval -= Time.deltaTime;

		// 사격 사고를 갱신하기까지의 시간
		m_shootInterval -= Time.deltaTime;

		// 플레이어와 자신의 거리 산출
		Vector3 aiSubPosition = (transform.position - mainObject.transform.position);
		aiSubPosition.y = 0f;

		if (isTargetDead) return;
		// 거리가 떨어져 있으면 움직인다
		if (aiSubPosition.magnitude > AI_IGNORE_DISTANCE)
		{

			// 일정 시간마다 AI 갱신
			if (m_aiInterval < 0f)
			{
				// 다음 사고까지 이 시간 기다린다
				m_aiInterval = Random.Range(AI_INTERVAL_MIN, AI_INTERVAL_MAX);      // 랜덤으로 시간을 결정


				// AI가 있는 위치에서 상하 좌우 우선도를 취득하다
				int[] prioTable = GetMovePrioTable();

				// 가장 우선순위가 높은 장소의 숫자를 산출하다
				int highest = AI_PRIO_MIN;
				int i;
				for (i = 0; i < (int)CheckDir.EnumMax; i++)
				{
					// 수치가 낮은 쪽이 우선순위가 높다
					if (highest > prioTable[i])
					{
						// 우선도 갱신
						highest = prioTable[i];
					}
				}

				// 어느 방향이 우선순위가 높아?
				// 이 입력을 하다
				PlayerInput pressInput = PlayerInput.Move_Left;
				if (highest == prioTable[(int)CheckDir.Left])
				{
					// 왼쪽으로 이동
					pressInput = PlayerInput.Move_Left;
				}
				else
				if (highest == prioTable[(int)CheckDir.Right])
				{
					// 오른쪽으로 이동
					pressInput = PlayerInput.Move_Right;
				}
				else
				if (highest == prioTable[(int)CheckDir.Up])
				{
					//위로 이동
					pressInput = PlayerInput.Move_Up;
				}
				else
				if (highest == prioTable[(int)CheckDir.Down])
				{
					// 아래로 이동
					pressInput = PlayerInput.Move_Down;
				}
				m_pressInput = pressInput;
			}

			// 입력
			m_playerInput[(int)m_pressInput] = true;
		}


		ray = new Ray(transform.position, transform.forward);

		// 객체와 Ray의 충돌에 대한 결과 정보를 저장하는 구조체
		RaycastHit hitData;
		float maxDis = 20f;
		Debug.DrawRay(transform.position + new Vector3(0, 2, 0), transform.forward * maxDis, Color.blue);

		if (Physics.Raycast(ray, out hitData))
		{
			GameObject hitObject = hitData.transform.gameObject;
			// AI의 Ray가 Player와 부딪힘
			if (hitObject.tag == "Player1")
			{
				isTargetDead = hitObject.GetComponent<Player_Key>().m_playerDeadFlag;

				// 사격의 사고를 하는가 (수업내용)
				if (m_shootInterval < 0f)
				{
					Debug.Log(hitObject.tag);
					if (!hitObject.GetComponent<Player_Key>().m_playerDeadFlag)
					{
						Debug.Log("사격");
						// 사격 조작
						m_playerInput[(int)PlayerInput.Shoot] = true;

						// 다음 사격은 이 시간이 경과할 때까지 기다린다(연사 억제)
						m_shootInterval = SHOOT_INTERVAL;
					}
				}
			}

			if (hitObject.GetComponent<Player_Key>().m_playerDeadFlag) return;


		}
	}


	/// <summary>
	/// 위치에서 그리드로 변환 그리드X
	/// </summary>
	/// <param name="posX"></param>
	/// <returns></returns>
	private int GetGridX(float posX)
	{
		// 그리드 범위 내에 들어가도록 Mathf.Clamp에서 제한을 가하다
		return Mathf.Clamp((int)((posX) / Field.BLOCK_SCALE), 0, (Field.FIELD_GRID_X - 1));
	}

	/// <summary>
	/// 위치에서 그리드로 변환 그리드Y
	/// </summary>
	/// <param name="posZ"></param>
	/// <returns></returns>
	private int GetGridY(float posZ)
	{
		//UnityではXZ平面が地平線
		return Mathf.Clamp((int)((posZ) / Field.BLOCK_SCALE), 0, (Field.FIELD_GRID_Y - 1));
	}



	/// <summary>
	/// AI가 이동할 때 우선순위 산출
	/// </summary>
	/// <returns></returns>
	private int[] GetMovePrioTable()
	{

		int i, j;

		//自分自身(AI)の位置
		Vector3 aiPosition = transform.position;
		//グリッドに変換
		int aiX = GetGridX(aiPosition.x);
		int aiY = GetGridY(aiPosition.z);

		//ユーザーが動かしているプレーヤーのオブジェクトを取得
		GameObject mainObject = Player_Key.m_mainPlayer;
		//攻撃目標の位置を取得
		Vector3 playerPosition = mainObject.transform.position;
		//グリッドに変換
		int playerX = GetGridX(playerPosition.x);
		int playerY = GetGridY(playerPosition.z);
		int playerGrid = playerX + (playerY * Field.FIELD_GRID_X);


		//グリッドの各位置の優先度を格納する配列
		int[] calcGrid = new int[(Field.FIELD_GRID_X * Field.FIELD_GRID_Y)];
		//初期化
		for (i = 0; i < (Field.FIELD_GRID_X * Field.FIELD_GRID_Y); i++)
		{
			//優先度を最低にする
			calcGrid[i] = AI_PRIO_MIN;
		}



		//プレーヤーが現在いる場所にまず 1 を入れる
		calcGrid[playerGrid] = 1;


		//チェックする優先度はまず 1 から
		int checkPrio = 1;
		//チェック用変数
		int checkX;
		int checkY;
		int tempX;
		int tempY;
		int tempGrid;
		//何かチェックしたら true
		bool update;
		do
		{
			//初期化
			update = false;

			//チェック開始
			for (i = 0; i < (Field.FIELD_GRID_X * Field.FIELD_GRID_Y); i++)
			{
				//チェックする優先度でないなら無視
				if (checkPrio != calcGrid[i])
				{
					continue;
				}

				//このグリッドがチェックする優先度の場所
				checkX = (i % Field.FIELD_GRID_X);
				checkY = (i / Field.FIELD_GRID_X);

				//そこから上下左右の場所をチェック
				for (j = 0; j < (int)CheckDir.EnumMax; j++)
				{
					//調べる場所の隣
					tempX = (checkX + CHECK_DIR_LIST[j][(int)CheckData.X]);
					tempY = (checkY + CHECK_DIR_LIST[j][(int)CheckData.Y]);
					//グリッドの外？
					if ((tempX < 0) || (tempX >= Field.FIELD_GRID_X) || (tempY < 0) || (tempY >= Field.FIELD_GRID_Y))
					{
						//場外なので無視
						continue;
					}
					//ここを調べる
					tempGrid = (tempX + (tempY * Field.FIELD_GRID_X));

					//隣が壁かチェック
					if (Field.ObjectKind.Block == (Field.ObjectKind)Field.GRID_OBJECT_DATA[tempGrid])
					{
						//壁なら無視
						continue;
					}

					//この場所の優先度の数字が現在チェックしている優先度より大きければ更新
					if (calcGrid[tempGrid] > (checkPrio + 1))
					{
						//値を更新
						calcGrid[tempGrid] = (checkPrio + 1);   //この数字が次にチェックするときの優先度
																//フラグを立てる
						update = true;
					}
				}
			}

			//チェックする優先度を +1 する
			checkPrio++;

			//何か更新があればもう一回 回す
		} while (update);


		// AI 주변 우선 순위표
		int[] prioTable = new int[(int)CheckDir.EnumMax];

		// 우선도 테이블이 생성되면 AI 주변 우선도 취득
		for (i = 0; i < (int)CheckDir.EnumMax; i++)
		{

			// 조사 장소 옆
			tempX = (aiX + CHECK_DIR_LIST[i][(int)CheckData.X]);
			tempY = (aiY + CHECK_DIR_LIST[i][(int)CheckData.Y]);
			// 그리드 밖?
			if ((tempX < 0) || (tempX >= Field.FIELD_GRID_X) || (tempY < 0) || (tempY >= Field.FIELD_GRID_Y))
			{
				// 장외이므로 우선도를 최저로 하다
				prioTable[i] = AI_PRIO_MIN;
				continue;
			}

			// 이 장소의 우선도를 대입
			tempGrid = (tempX + (tempY * Field.FIELD_GRID_X));
			prioTable[i] = calcGrid[tempGrid];
		}


		//// 우선순위 테이블을 디버깅 출력
		//{
		//	//디버깅용 문자열
		//	string temp = "";

		//	// 우선도 테이블이 생성되면 AI 주변 우선도 취득
		//	temp += "PRIO TABLE\n";
		//	for (tempY = 0; tempY < Field.FIELD_GRID_Y; tempY++)
		//	{
		//		for (tempX = 0; tempX < Field.FIELD_GRID_X; tempX++)
		//		{

		//			// Y축은 상하 반대로 출력되어 버리기 때문에 거꾸로 한다
		//			temp += "\t\t" + calcGrid[tempX + ((Field.FIELD_GRID_Y - 1 - tempY) * Field.FIELD_GRID_X)] + "";

		//			// 자기 위치
		//			if ((aiX == tempX) && (aiY == (Field.FIELD_GRID_Y - 1 - tempY)))
		//			{
		//				temp += "*";
		//			}
		//		}
		//		temp += "\n";
		//	}
		//	temp += "\n";

		//	// 이동 방향별 우선순위 정보
		//	temp += "RESULT\n";
		//	for (i = 0; i < (int)CheckDir.EnumMax; i++)
		//	{
		//		// 이 장소의 우선도를 대입
		//		temp += "\t" + prioTable[i] + "\t" + (CheckDir)i + "\n";
		//	}

		//	// 출력
		//	Debug.Log("" + temp);
		//}


		// 4방향 우선순위 정보를 반환하다
		return prioTable;
	}

}
