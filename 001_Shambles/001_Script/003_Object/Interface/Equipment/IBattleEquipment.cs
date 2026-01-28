// 전투 장비 인터페이스
public interface IBattleEquipment {
	// 장비 데이터 객체
	Equipment ThisEquipment { get; set; }
	// 장비 효과 활성화
	void ActivateEquipmentEffect(BattlePlayerStatus playerStatus);
}
