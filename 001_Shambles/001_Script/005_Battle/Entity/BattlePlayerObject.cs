using UnityEngine;
using DG.Tweening;

/// <summary>
/// 전투 씬의 플레이어 개체 클래스
/// 플레이어 상태, UI, 턴 시작/종료 이펙트를 통합 관리
/// </summary>
public class BattlePlayerObject : MonoBehaviour {
	// Data
	public BattlePlayerStatus battlePlayerStatus = new BattlePlayerStatus();       // 플레이어의 상태를 제어하는 클래스 (상태 데이터 포함)

	// UI
	[SerializeField] public BattlePlayerStatusUI battlePlayerStatusUI = null;      // HP/AP 표시 UI
	[SerializeField] public BattleBuffIconList battlePlayerBuffUI = null;          // 버프/디버프 아이콘 리스트 UI

	// 연출 효과
	[SerializeField] BattlePlayerStart battleStartEffect = null;                   // 전투 시작 시
	[SerializeField] BattlePlayerTurnStart battlePlayerTurnStartEffect = null;     // 턴 시작 시
	[SerializeField] BattlePlayerTurnEnd battlePlayerTurnEndEffect = null;         // 턴 종료 시

	// 초기화 메서드
	public void Initialize() {
		// 내부 데이터 초기화
		battlePlayerStatus.Initialize(this);

		// UI 초기화
		battlePlayerStatusUI.Initialize(this);
		battleStartEffect.Initialize();
		battlePlayerTurnStartEffect.Initialize();
		battlePlayerTurnEndEffect.Initialize();

		UpdateUI();
	}

	// UI 갱신
	public void UpdateUI() {
		battlePlayerStatusUI.UpdateUI();
		BattleManager.GetInstance().battleSkill.UpdateUI();
	}
}
