using UnityEngine;
using System.Collections;


/// <summary>
/// 필드 생성 프로그램
/// </summary>
public class Field : MonoBehaviour
{
	public GameObject m_blockObject = null; // 블록의 조립식

	public GameObject m_player1Object = null; // 플레이어 1의 조립식
	public GameObject m_player2Object = null; // 플레이어 2의 조립식

	public static readonly int FIELD_GRID_X = 9; // 필드의 X 그리드 수
	public static readonly int FIELD_GRID_Y = 9; // 필드의 Y 그리드 수

	public static readonly float BLOCK_SCALE = 2.0f; // 블록 스케일(블록 1개 크기)
	public static readonly Vector3 BLOCK_OFFSET = new Vector3(1, 1, 1); // 블록의 배치 오프셋


	// 배치물의 종류
	public enum ObjectKind
	{
		Empty       //	0 공백
		, Block     //	1 블록
		, Player1   //	2 플레이어1
		, Player2   //	3 플레이어2
	}

	public static readonly int[] GRID_OBJECT_DATA = new int[] {	// 배치 데이터
		// 0 공란, 1 블록
		1,  1,  1,  1,  1,  1,  1,  1,  1,
		1,  2,  0,  0,  0,  0,  0,  0,  1,
		1,  0,  1,  1,  1,  0,  1,  0,  1,
		1,  0,  0,  0,  0,  0,  0,  0,  1,
		1,  0,  1,  0,  1,  1,  1,  0,  1,
		1,  0,  1,  0,  1,  0,  0,  0,  1,
		1,  0,  1,  0,  0,  0,  1,  0,  1,
		1,  0,  0,  0,  1,  0,  0,  3,  1,
		1,  1,  1,  1,  1,  1,  1,  1,  1,

		// 배치할 때 위아래가 뒤집히므로 주의
	};



	private GameObject m_blockParent = null; // 생성한 블록의 부모용 오브젝트

	/// <summary>
	/// 부팅 시 호출되는 함수
	/// </summary>
	private void Awake()
	{
		// 필드의 초기화
		InitializeField();
	}



	/// <summary>
	/// 필드의 초기화
	/// 배열 변수를 초기화하여 외벽과 기둥을 만들다
	/// </summary>
	private void InitializeField()
	{

		// 블록 부모를 만들다
		m_blockParent = new GameObject();
		m_blockParent.name = "BlockParent";
		m_blockParent.transform.parent = transform;


		// 블록을 만들다
		GameObject originalObject; // 생성하는 블록의 원래 객체
		GameObject instanceObject; // 블록을 일단 넣어두는 변수
		Vector3 position;           //ブロックの生成位置


		//外枠と中に柱を立てていく
		int gridX;
		int gridY;
		for (gridX = 0; gridX < FIELD_GRID_X; gridX++)
		{
			for (gridY = 0; gridY < FIELD_GRID_Y; gridY++)
			{

				//この位置には何を置くか
				switch ((ObjectKind)GRID_OBJECT_DATA[gridX + (gridY * FIELD_GRID_X)])
				{
					case ObjectKind.Block:
						//壁
						originalObject = m_blockObject;
						break;
					case ObjectKind.Player1:
						//プレーヤー
						originalObject = m_player1Object;
						break;
					case ObjectKind.Player2:
						//プレーヤー
						originalObject = m_player2Object;
						break;
					default:
						//それ以外は空欄
						originalObject = null;
						break;
				}

				//空欄ならここまで
				if (null == originalObject)
				{
					continue;
				}


				//ブロックの生成位置
				position = new Vector3(gridX * BLOCK_SCALE, 0, gridY * BLOCK_SCALE) + BLOCK_OFFSET;             //UnityではXZ平面が地平線

				//ブロック生成							複製する対象		生成位置		回転
				instanceObject = Instantiate(originalObject, position, originalObject.transform.rotation) as GameObject;
				//名前を変更
				instanceObject.name = "" + originalObject.name + "(" + gridX + "," + gridY + ")";       //グリッドの位置を書いておく

				//ローカルスケール(大きさ)を変更
				instanceObject.transform.localScale = (Vector3.one * BLOCK_SCALE);              //Vector3.one は new Vector3( 1f, 1f, 1f) と同じ

				//前述の親の下につける
				instanceObject.transform.parent = m_blockParent.transform;
			}
		}
	}

}
