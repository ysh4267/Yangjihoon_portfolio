using UnityEngine;
using DG.Tweening;

/// <summary>
/// 전투 씬의 플레이어 개체 클래스
/// 플레이어 상태, UI, 턴 시작/종료 이펙트를 통합 관리
/// </summary>
public class BattlePlayerObject : MonoBehaviour {
	public BattlePlayerStatus battlePlayerStatus = new BattlePlayerStatus();

	[SerializeField] public BattlePlayerStatusUI battlePlayerStatusUI = null;
	[SerializeField] public BattleBuffIconList battlePlayerBuffUI = null;
	[SerializeField] BattlePlayerStart battleStartEffect = null;
	[SerializeField] BattlePlayerTurnStart battlePlayerTurnStartEffect = null;
	[SerializeField] BattlePlayerTurnEnd battlePlayerTurnEndEffect = null;

	public void Initialize() {
		battlePlayerStatus.Initialize(this);
		battlePlayerStatusUI.Initialize(this);
		battleStartEffect.Initialize();
		battlePlayerTurnStartEffect.Initialize();
		battlePlayerTurnEndEffect.Initialize();

		UpdateUI();
	}

	public void UpdateUI() {
		battlePlayerStatusUI.UpdateUI();
		BattleManager.GetInstance().battleSkill.UpdateUI();
	}
}
