using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using static ENUM_BATTLE_PHASE_TARGET;
using static ENUM_BATTLE_PHASE_ACTION;

public class BattleEnemyStatus : IBattleStatus {
    //enemy 게임 내 오브젝트 객체
    BattleEnemyObject enemyObject;
    //실제 값을 담기 위한 데이터
    CharacterBattleStatus enemyStatus;
    int currentShield;
    //버프 지속시간과 추가 삭제를 관리하기 위한 카운터
    CharacterBuffCounter enemyBuffCounter;
    //전투 진행중 적용될 값
    BattleDynamicValues enemyDynamicValues;

    //외부 참조를 위한 프로퍼티 모음
    public ENUM_BATTLE_PHASE_TARGET TargetEnum => enemyObject.enemyPhaseTargetEnum;

    public int[] Status => enemyStatus.status;

    public int MaxHp => enemyStatus.max_hp;

    public int CurrentHp => enemyStatus.current_hp;

    public int MaxAp => enemyStatus.max_ap;

    public int CurrentAp => enemyStatus.current_ap;

    public int CurrentShield => currentShield;

    public Transform ObjectTransform => enemyObject.gameObject.transform;

    public CharacterBuffCounter BuffCounter => enemyBuffCounter;
    
    public BattleDynamicValues DynamicValues => enemyDynamicValues;

    //enemy 전용 변수
    BattlePlayerStatus playerStatus => BattleManager.GetInstance().battlePlayerObject.battlePlayerStatus;
    Color damageTextColor = new Color(202f / 255, 0, 0);
    Color healTextColor = new Color(61f / 255, 239f / 255, 53f / 255);
    // 데미지를 받은 적 오브젝트를 추적하기 위한 변수
    public BattleEnemyObject LastDamagedByTarget { get; set; }


    // 적 전투 데이터 초기화, 난이도에 따른 스테이터스 보정 및 버프 카운터 생성
    public void Initialize(BattleEnemyObject enemy) {
        enemyObject = enemy;
        BattleManager.GetInstance().AddBattleStatus(this);

        // 적 데이터에서 기본 체력을 설정
        enemyStatus = new PlayerBattleStatus {
            current_hp = enemyObject.enemyData.hp,
            max_hp = enemyObject.enemyData.hp
        };

		// Reality 이상 난이도일 경우 체력 2배 적용
		if (SettingManager.GetSettingData().difficulty >= ENUM_DIFFICULTY.Reality) {
			enemyStatus = new PlayerBattleStatus {
				current_hp = enemyObject.enemyData.hp * 2,
				max_hp = enemyObject.enemyData.hp * 2
			};
		}

		enemyBuffCounter = new CharacterBuffCounter(enemy.enemyBuffUI, this, TURN_END);
        enemyDynamicValues = new BattleDynamicValues(this);
    }

    // 적에게 대미지를 적용하여 명중률 계산 후 방어도와 체력을 차감하고 사망/페이즈 전환을 처리
    public int Damage(IBattleFactor factor, BattleDamage amount) {
        // 대미지 계산 후 명중률을 적용한 최종 대미지 산출
        BattleDamage finalDamage = BattleCardStaticMethod.BasicDamageAccuracyCalc(DamageCalc(factor, amount));

        if (finalDamage.damageSourceType == ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK) {
            DynamicValues.lastDamagedByTarget = finalDamage.attacker;
            if (factor is IBattleCard) { // 스킬 또는 카드에 의한 효과만 적용
                BattleManager.GetInstance().battlePhaseManager.ProceedPhase(TargetEnum, DIRECT_DAMAGED_STANDBY);
            }
			if (factor is IBattlePlayerCard) { 
				BattleManager.GetInstance().battleArchive.PlayerAct.Add((ENUM_BATTLE_PLAYER_ACT_TYPE.Attack, finalDamage.damage));
			}
		}
        BattleManager.GetInstance().battlePhaseManager.ProceedPhase(TargetEnum, DAMAGE_STANDBY);

        int prevHp = enemyStatus.current_hp;
        // 대미지가 0 이하이거나 무적/회피 버프가 활성화된 경우 대미지 적용을 건너뜀
        if (!(finalDamage.damage <= 0 || DynamicValues.isBuffFlag_014 || DynamicValues.isBuffFlag_006)) {
            // 관통 대미지: 방어도를 무시하고 체력에 직접 적용
            if (finalDamage.damageType == ENUM_DAMAGE_TYPE.PENETRATE) {
                enemyStatus.current_hp -= finalDamage.damage;
            }
            // 일반 대미지: 방어도를 먼저 차감한 후 잔여 대미지를 체력에 적용
            else {
                int remainDamage = finalDamage.damage;
                // 방어도 없음
                if (currentShield <= 0) {
                    currentShield = 0;
                }
                // 방어도가 대미지보다 큰 경우
                else if (currentShield > finalDamage.damage) {
                    currentShield -= finalDamage.damage;
                    BattleManager.GetInstance().battleEventManager.OnTargetShieldDamaged?.Invoke(this, factor, remainDamage);
                    remainDamage = 0;
                }
                // 방어도 <= 대미지
                else {
                    remainDamage = finalDamage.damage - currentShield;
					int prevShield = currentShield;
					currentShield = 0;
                    BattleManager.GetInstance().battlePhaseManager.ProceedPhase(TargetEnum, BROKE_SHIELD);
					BattleManager.GetInstance().battleEventManager.OnTargetHpDamaged?.Invoke(this, factor, prevShield);
                    SoundManager.GetInstance().PlayEffectSound(ENUM_EFFECT_SOUND.Player_shield_break, noOverlap: true);
				}

                if (remainDamage > 0) {
                    enemyStatus.current_hp -= remainDamage;
                }
            }
            //Camera Effect
            enemyObject.enemySprite.transform.DOComplete();
            enemyObject.enemySprite.transform.DOShakePosition(0.5f, 0.1f);

            if (enemyStatus.current_hp < prevHp) {
                BattleManager.GetInstance().battlePhaseManager.ProceedPhase(TargetEnum, HP_DAMAGED);
                if (finalDamage.damageSourceType == ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK && factor != null) {
                    BattleManager.GetInstance().battleEventManager.OnTargetHpDamaged?.Invoke(this, factor, prevHp - enemyStatus.current_hp);
                    // 데미지를 받은 적 오브젝트를 추가 :: 업적 조건을 확인하기 위함
                    BattleManager.GetInstance().battleArchive.DamagedByTarget.Add(enemyObject);
                }
            }

            //Phase Change Check
            if (enemyObject.enemyScript is IBattleEnemyPhase) {
                (enemyObject.enemyScript as IBattleEnemyPhase).SwitchPhase();
            }

            // 사망 처리: 불사 상태이면 체력 1로 유지, 아니면 사망 지연 처리
            if (enemyStatus.current_hp <= 0) {
                if (DynamicValues.isImmortal) {
                    enemyStatus.current_hp = 1;
                    BattleManager.GetInstance().battleEventManager.OnEnemyDead?.Invoke(enemyObject);
                    BattleManager.GetInstance().battlePhaseManager.ProceedPhase(TargetEnum, DEAD);
                }
                else {
                    enemyStatus.current_hp = 0;
                    BattleManager.GetInstance().battleEnemyManager.KillEnemyDelay(enemyObject);
                }
            }

            //Damage Text
            GameObject text = BattleManager.GetInstance().battleObjectPool.damageTextPool.GetPooledObject();
            text.GetComponent<TextMeshPro>().text = finalDamage.damage.ToString();
            text.GetComponent<TextMeshPro>().color = damageTextColor;
            BattleTextAnimator.TextFadeOutUpside(text, ObjectTransform);
            BattleManager.GetInstance().battleStatisticsManager.AddTotalDamagedValue(finalDamage.damage);

            BattleManager.GetInstance().battlePhaseManager.ProceedPhase(TargetEnum, DAMAGED);

            // 직접 공격 시 피격 애니메이션 및 카메라 이펙트 재생
            if (finalDamage.damageSourceType == ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK) {
                enemyObject.enemySprite.GetComponent<Animator>().PlayDamaged();
                Camera.main.BlurCamera(BattleCameraEffectDefine.blur_duration, finalDamage.damage, true);
                Camera.main.ShakeCamera(BattleCameraEffectDefine.shake_duration, finalDamage.damage, true);

                if (factor is IBattleCard) { // 스킬 또는 카드에 의한 효과만 적용
                    // 흡혈 버프 활성화 시 대미지의 30%를 공격자가 회복
                    if (finalDamage.attacker != null) {
                        if (finalDamage.attacker.DynamicValues.isBuffFlag_004) finalDamage.attacker.GainHP(finalDamage.attacker.BuffCounter.GetBuff(ENUM_BUFF_INDEX.EXAMPLE_BUFF_004).battleBuffScript, (int)(finalDamage.damage * 0.3f));
                    }

                    BattleManager.GetInstance().battlePhaseManager.ProceedPhase(TargetEnum, DIRECT_DAMAGED);
                }
            }
        }

        if (finalDamage.damageSourceType == ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK && factor != null)
            BattleManager.GetInstance().battleEventManager.OnTargetDamaged?.Invoke(this, factor, finalDamage.damage);

        UpdateUI();

        return finalDamage.damage;
    }

    // 적에게 카드 또는 스킬을 갖다댔을 때 예상되는 수치를 체력바에 나타내기 위한 계산식 분리
    public BattleDamage DamageCalc(IBattleFactor factor, BattleDamage damage) {
        BattleDamage battleDamage = new BattleDamage(damage);

        float m_damage = battleDamage.damage;

        //대미지 타입 변경
        if (DynamicValues.isBuffFlag_012) battleDamage.damageType = ENUM_DAMAGE_TYPE.PENETRATE;

        if (battleDamage.damageSourceType == ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK) {
            if (battleDamage.attacker?.DynamicValues.isBuffFlag_001 ?? false) battleDamage.damageType = ENUM_DAMAGE_TYPE.PENETRATE;
            //가감 연산
            if (DynamicValues.isBuffFlag_002) m_damage -= 2f;
            if (DynamicValues.isBuffFlag_002_2) m_damage -= 5f;
            if (playerStatus.DynamicValues.hasEquipment_Example_006) m_damage += BuffCounter.numberOfDebuffs;

            //승제 연산
            if (DynamicValues.isBuffFlag_011) {
                if (damage.attacker?.DynamicValues.isCardEffect_004 ?? false) m_damage *= 1.5f;
                else m_damage *= 1.2f;
            }
            if (DynamicValues.isBuffFlag_008 && CurrentShield > 0) m_damage *= 1.5f;
            if (DynamicValues.isCardEffect_001) m_damage *= 0.5f;
            if (playerStatus.DynamicValues.isBuffFlag_005 && CurrentShield > 0) m_damage *= 1.5f;
            if (playerStatus.DynamicValues.hasEquipment_Example_007 && CurrentShield > 0) m_damage *= 1.2f;
        }

        // 이벤트에 등록된 추가 대미지를 합산
        if (BattleManager.GetInstance().battleEventManager.ExtraDamageOnTargetDamaged != null && factor != null)
            foreach (Func<IBattleStatus, IBattleFactor, int> func in BattleManager.GetInstance().battleEventManager.ExtraDamageOnTargetDamaged?.GetInvocationList())
                m_damage += func(this, factor);

        battleDamage.SetDamageValue(Mathf.FloorToInt(m_damage));
        return battleDamage;
    }

    // 적은 AP를 사용하지 않으므로 미구현
    public int GainAP(IBattleFactor factor, int amount) {
        return 0;
    }

    // 적은 AP를 사용하지 않으므로 미구현
    public void GainMaxAP(int amount) {
        return;
    }

    // 방어도를 획득하여 버프에 따른 보정을 적용하고 최대치(MaxHp / 2)를 초과하지 않도록 제한
    public void GainShield(IBattleFactor factor, int amount) {
        int change = 0;
		if (DynamicValues.isBuffFlag_018) return;
		if (DynamicValues.isBuffFlag_010) amount -= (int)(BuffCounter.GetBuffCount(ENUM_BUFF_INDEX.EXAMPLE_BUFF_005) / 3f * amount);

        if (amount > 0) {
            int previousShield = currentShield;
            currentShield += amount;
            if (currentShield > MaxHp / 2) currentShield = MaxHp / 2;

            change = currentShield - previousShield;
            if (change != 0) {
                SoundManager.GetInstance().PlayEffectSound(ENUM_EFFECT_SOUND.Player_shield_up, noOverlap: true);
                BattleManager.GetInstance().battlePhaseManager.ProceedPhase(TargetEnum, GAIN_SHIELD);
            }
        }

        if (factor != null)
            BattleManager.GetInstance().battleEventManager.OnTargetGainShield?.Invoke(this, factor, change);

        UpdateUI();
    }

    // 체력을 회복하여 버프에 따른 회복 제한을 적용하고 회복 텍스트를 출력 후 변동량을 반환
    public int GainHP(IBattleFactor factor, int amount) {
        int change = 0;

		if (DynamicValues.isBuffFlag_009) return change;
		if (DynamicValues.isBuffFlag_018) return change;

		if (amount > 0) {
            int previousHp = CurrentHp;
            enemyStatus.current_hp += amount;
            if (enemyStatus.current_hp > MaxHp) enemyStatus.current_hp = MaxHp;
            change = CurrentHp - previousHp;

            if (change != 0) {
                //Heal Text
                GameObject text = BattleManager.GetInstance().battleObjectPool.damageTextPool.GetPooledObject();
                text.GetComponent<TextMeshPro>().text = change.ToString();
                text.GetComponent<TextMeshPro>().color = healTextColor;
                BattleTextAnimator.TextFadeOutUpside(text, ObjectTransform);

                SoundManager.GetInstance().PlayEffectSound(ENUM_EFFECT_SOUND.Player_hp_heal, noOverlap: true);
                BattleManager.GetInstance().battlePhaseManager.ProceedPhase(TargetEnum, HEALED);
            }
        }

        // 적 카드에 의한 회복일 경우 UI에서 회복 카드를 제거
        if (factor is IBattleEnemyCard)
            enemyObject.enemyStatusUI.RemoveHealCard((factor as IBattleEnemyCard).OwnerStatus.TargetEnum);

        if (factor != null)
            BattleManager.GetInstance().battleEventManager.OnTargetGainHp?.Invoke(this, factor);

        UpdateUI();
        return change;
    }

    // 현재 체력을 지정된 값으로 설정하고 0~MaxHp 범위로 보정
    public void SetHP(int value) {
        enemyStatus.current_hp = value;
        if (enemyStatus.current_hp < 0) enemyStatus.current_hp = 0;
        else if (enemyStatus.current_hp > MaxHp) enemyStatus.current_hp = MaxHp;

        UpdateUI();
    }

    // 적은 AP를 사용하지 않으므로 미구현
    public int LoseAP(int amount) {
        return 0;
    }

    // 버프를 획득하여 초기화 및 효과를 적용하고 업적 조건을 확인
    public void GainBuff(IBattleFactor factor, ENUM_BUFF_INDEX buffIndex, int count, params int[] _params) {
        if (count > 0) {
            Buff buff = BuffDao.GetBuff((int)buffIndex);
            IBattleBuff buffScript = buff.battleBuffScript;
            buffScript.InitializeBuff(this, buff, count, _params);
            buffScript.ActivateBuffEffect();
            #region 버프 업적 처리
            //모든 적과 자신에게 실명을 부여하십시오.
            int BlindObjectCount = 0;
            foreach (var Target in BattleManager.GetInstance().GetBattleStatus(ALL).TargestList) {
                if (Target.BuffCounter.HasEnoughBuffCount(ENUM_BUFF_INDEX.EXAMPLE_BUFF_006, 1))
                    BlindObjectCount++;
            }
            if (BlindObjectCount == BattleManager.GetInstance().GetBattleStatus(ALL).TargestList.Count)
                AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.City_Of_The_Blind);
            #endregion
            if (factor != null)
                BattleManager.GetInstance().battleEventManager.OnTargetGainBuff?.Invoke(this, factor, buff, count);
        }
    }

    // 지정된 버프의 스택을 감소시키고 실제 감소된 수치를 반환
    public int LoseBuffCount(ENUM_BUFF_INDEX buffEnum, int count) {
        return BuffCounter.SubtractBuffCount(buffEnum, count);
    }

    // 방어도를 감소시키고 변동량을 반환
    public int LoseShield(int amount) {
        int change = 0;

        if (amount <= 0) return 0;
        int previousShield = currentShield;
        currentShield -= amount;
        if (currentShield <= 0) currentShield = 0;
        if (currentShield.Equals(previousShield)) return 0;
        change = previousShield - currentShield;

        BattleManager.GetInstance().battlePhaseManager.ProceedPhase(TargetEnum, LOSE_SHIELD);
        UpdateUI();

        return change;
    }

    // 방어도를 지정된 값으로 설정하고 0~(MaxHp / 2) 범위로 보정
    public int SetShield(int amount) {
        currentShield = amount;
        if (amount < 0 || currentShield < 0) currentShield = 0;
        if (amount > MaxHp / 2) currentShield = MaxHp / 2;

        //SyncUI();
        UpdateUI();

        return currentShield;
    }

    // 적 오브젝트 위치에 이펙트를 재생
    public void PlayEffect(ENUM_BATTLE_VFX effectEnum) {
        BattleEffectManager.PlayEffect(effectEnum, ObjectTransform);
    }

    // 적 UI를 갱신
    public void UpdateUI() {
        enemyObject.UpdateUI();
    }
}
