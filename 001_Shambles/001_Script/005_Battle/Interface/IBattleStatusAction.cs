using UnityEngine;

/// <summary>
/// 전투 개체가 수행할 수 있는 행동을 정의하는 인터페이스
/// 대미지, 회복, 버프 적용 등 전투 중 발생하는 모든 상태 변화 메서드 정의
/// BattlePlayerStatus, BattleEnemyStatus에서 구현됨
/// </summary>
public interface IBattleStatusAction {
    int Damage(IBattleFactor factor, BattleDamage amount);                              // 대미지 처리
    int GainHP(IBattleFactor factor, int amount);                                       // 체력 회복
    void SetHP(int value);                                                              // 체력 설정
    int LoseAP(int amount);                                                             // 행동력 소모
    int GainAP(IBattleFactor factor, int amount);                                       // 행동력 획득
    void GainMaxAP(int amount);                                                         // 최대 행동력 증가
    int LoseShield(int amount);                                                         // 방어도 감소
	int SetShield(int amount);                                                          // 방어도 설정
    void GainShield(IBattleFactor factor, int amount);                                  // 방어도 획득
    void GainBuff(IBattleFactor factor, ENUM_BUFF_INDEX buffIndex, int count, params int[] _params);  // 버프 적용
    int LoseBuffCount(ENUM_BUFF_INDEX buffEnum, int count);                             // 버프 카운트 감소
    void PlayEffect(ENUM_BATTLE_VFX effectEnum);                                        // 이펙트 재생
    void UpdateUI();                                                                    // UI 갱신
}
