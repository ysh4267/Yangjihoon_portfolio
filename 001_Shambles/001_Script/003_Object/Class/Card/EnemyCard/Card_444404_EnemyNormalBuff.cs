using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card_444404_EnemyNormalBuff : IBattleEnemyCard, IBattleEnemyBuffCard {
	// 적용할 버프 인덱스
    public ENUM_BUFF_INDEX BuffEnum { get; set; }
	// 카드 소유자 상태
    public IBattleStatus OwnerStatus { get; set; }
	// 카드 대상 상태
    public BattleStatus TargetStatus { get; set; }

	// 버프 종류 설정
    public void BuffInitialize(ENUM_BUFF_INDEX _buffEnum) {
        BuffEnum = _buffEnum;
    }

	// 카드 초기화
    public void InitializeCardAction(IBattleStatus cardOwnerStatus) {
        OwnerStatus = cardOwnerStatus;
    }

	// 카드 사용 시 대상에게 버프 적용
    public void ProceedCardAction(BattleStatus cardTargetStatus, int count) {
        TargetStatus = cardTargetStatus;

        if (TargetStatus.IsNoneTarget) return;
        TargetStatus.GainBuff(this, BuffEnum, count);
    }

	// 단일 대상용 오버로드
    public void ProceedCardAction(IBattleStatus cardTargetStatus, int count) {
        ProceedCardAction(BattleManager.GetInstance().GetBattleStatus(cardTargetStatus), count);
    }
}
