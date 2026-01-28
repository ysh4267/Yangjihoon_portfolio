using System.Collections.Generic;

// 플레이어 카드 인터페이스
public interface IBattlePlayerCard : IBattleCard {
	// 사용 가능 여부 (false면 다른 조건과 무관하게 사용 불가)
	bool IsUsable { get; set; }
	// 카드 세력
	ENUM_FACTION FactionEnum { get; set; }
	// 카드 코스트
	BattleCost Cost { get; set; }
	// 카드 수치 데이터
	BattleCardValues battleCardValues { get; set; }
	// 카드 사용 후 목적지
	ENUM_CARD_DESTINATION cardDestination { get; set; }
	// 연계 장비 목록
	List<Equipment> Equipments { get; set; }
	// 최종 코스트 계산
	int CostCalc();
}
