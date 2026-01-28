using UnityEngine;

public interface IBattleStatusAction {
    int Damage(IBattleFactor factor, BattleDamage amount);
    int GainHP(IBattleFactor factor, int amount);
    void SetHP(int value);
    int LoseAP(int amount);
    int GainAP(IBattleFactor factor, int amount);
    void GainMaxAP(int amount);
    int LoseShield(int amount);
	int SetShield(int amount);
    void GainShield(IBattleFactor factor, int amount);
    void GainBuff(IBattleFactor factor, ENUM_BUFF_INDEX buffIndex, int count, params int[] _params);
    int LoseBuffCount(ENUM_BUFF_INDEX buffEnum, int count);
    void PlayEffect(ENUM_BATTLE_VFX effectEnum);
    void UpdateUI();
}
