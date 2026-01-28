using System.Collections.Generic;

/// <summary>
/// 예제 장비 클래스
/// 전투 이벤트에 반응하는 장비 효과를 구현한 템플릿
/// </summary>
public class ExampleEquipment : IBattleEquipment {
	#region 기본 속성
	// 장비 데이터 객체
	public Equipment ThisEquipment { get; set; }
	// 플레이어 상태 참조
	BattlePlayerStatus playerStatus;
	// 활성화 상태 플래그
	bool isActivated = false;
	#endregion

	#region 장비 효과
	// 장비 효과 활성화 및 이벤트 등록
	public void ActivateEquipmentEffect(BattlePlayerStatus playerStatus) {
		this.playerStatus = playerStatus;

		// 이벤트 등록 예시
		BattleManager.GetInstance().battleEventManager.OnTurnStart += OnTurnStart;
		BattleManager.GetInstance().battleEventManager.OnTurnEnd += OnTurnEnd;
		BattleManager.GetInstance().battleEventManager.OnUseCard += OnUseCard;
		BattleManager.GetInstance().battleEventManager.OnDamaged += OnDamaged;
	}
	#endregion

	#region 이벤트 핸들러
	// 턴 시작 시 처리
	void OnTurnStart() {
		isActivated = false;
		// 턴 시작 효과 예시: 방어력 획득
		playerStatus.GainShield(null, 3);
	}

	// 턴 종료 시 처리
	void OnTurnEnd() {
		// 턴 종료 효과 예시: 체력 회복
		playerStatus.GainHP(null, 2);
	}

	// 카드 사용 시 처리
	void OnUseCard(Card card) {
		if (isActivated) return;
		// 카드 사용 효과 예시: 공격 카드 사용 시 날카로움 버프
		if (card.enumCardType == ENUM_CARD_TYPE.ATTACK) {
			playerStatus.GainBuff(null, ENUM_BUFF_INDEX.SHARPNESS, 1);
			isActivated = true;
		}
	}

	// 피해 받을 시 처리
	void OnDamaged(int damage) {
		// 피해 시 효과 예시: 10 이상 피해 시 방어력 획득
		if (damage >= 10) {
			playerStatus.GainShield(null, 5);
		}
	}
	#endregion
}
