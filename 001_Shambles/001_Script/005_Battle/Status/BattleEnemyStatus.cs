using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using static ENUM_BATTLE_PHASE_TARGET;
using static ENUM_BATTLE_PHASE_ACTION;

public class BattleEnemyStatus : IBattleStatus {
    //original values
    BattleEnemyObject enemyObject;

    int currentShield;
    PlayerBattleStatus enemyStatus;
    CharacterBuffCounter enemyBuffCounter;
    BattleDynamicValues enemyDynamicValues;

    public ENUM_BATTLE_PHASE_TARGET TargetEnum => enemyObject.enemyPhaseTargetEnum;

    public int[] Status => enemyStatus.status;

    public int MaxHp => enemyStatus.max_hp;

    public int CurrentHp => enemyStatus.current_hp;

    public int MaxAp => enemyStatus.max_ap;

    public int CurrentAp => enemyStatus.current_ap;

    public int CurrentShield => currentShield;

    public BattleDynamicValues DynamicValues => enemyDynamicValues;

    public CharacterBuffCounter BuffCounter => enemyBuffCounter;

    public Transform ObjectTransform => enemyObject.gameObject.transform;

    BattlePlayerStatus playerStatus => BattleManager.GetInstance().battlePlayerObject.battlePlayerStatus;
    Color damageTextColor = new Color(202f / 255, 0, 0);
    Color healTextColor = new Color(61f / 255, 239f / 255, 53f / 255);
    // 데미지를 받은 적 오브젝트를 추적하기 위한 변수
    public BattleEnemyObject LastDamagedByTarget { get; set; }


    public void Initialize(BattleEnemyObject enemy) {
        enemyObject = enemy;
        BattleManager.GetInstance().AddBattleStatus(this);

        enemyStatus = new PlayerBattleStatus {
            current_hp = enemyObject.enemyData.hp,
            max_hp = enemyObject.enemyData.hp
        };

		if (SettingManager.GetSettingData().difficulty >= ENUM_DIFFICULTY.Reality) {
			enemyStatus = new PlayerBattleStatus {
				current_hp = enemyObject.enemyData.hp * 2,
				max_hp = enemyObject.enemyData.hp * 2
			};
		}

		enemyBuffCounter = new CharacterBuffCounter(enemy.enemyBuffUI, this, TURN_END);
        enemyDynamicValues = new BattleDynamicValues(this);
    }

    public int Damage(IBattleFactor factor, BattleDamage amount) {
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
        if (!(finalDamage.damage <= 0 || DynamicValues.isStopped || DynamicValues.isInvincible)) {
            if (finalDamage.damageType == ENUM_DAMAGE_TYPE.PENETRATE) {
                enemyStatus.current_hp -= finalDamage.damage;
            }
            else {
                int remainDamage = finalDamage.damage;
                if (currentShield <= 0) {
                    currentShield = 0;
                }
                else if (currentShield > finalDamage.damage) {
                    currentShield -= finalDamage.damage;
                    BattleManager.GetInstance().battleEventManager.OnTargetShieldDamaged?.Invoke(this, factor, remainDamage);
                    remainDamage = 0;
                }
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

            //Ready to Death
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

            if (finalDamage.damageSourceType == ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK) {
                enemyObject.enemySprite.GetComponent<Animator>().PlayDamaged();
                Camera.main.BlurCamera(BattleCameraEffectDefine.blur_duration, finalDamage.damage, true);
                Camera.main.ShakeCamera(BattleCameraEffectDefine.shake_duration, finalDamage.damage, true);

                if (factor is IBattleCard) { // 스킬 또는 카드에 의한 효과만 적용
                    if (finalDamage.attacker != null) {
                        if (finalDamage.attacker.DynamicValues.isBloodSucking) finalDamage.attacker.GainHP(finalDamage.attacker.BuffCounter.GetBuff(ENUM_BUFF_INDEX.BLOOD_SUCKING).battleBuffScript, (int)(finalDamage.damage * 0.3f));
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
        if (DynamicValues.isDisarmed) battleDamage.damageType = ENUM_DAMAGE_TYPE.PENETRATE;

        if (battleDamage.damageSourceType == ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK) {
            if (battleDamage.attacker?.DynamicValues.isPenetrating ?? false) battleDamage.damageType = ENUM_DAMAGE_TYPE.PENETRATE;
            //가감 연산
            if (DynamicValues.isHarden) m_damage -= 2f;
            if (DynamicValues.isHarden_2) m_damage -= 5f;
            if (playerStatus.DynamicValues.hasEquipment_318) m_damage += BuffCounter.numberOfDebuffs;

            //승제 연산
            if (DynamicValues.isMarked) {
                if (damage.attacker?.DynamicValues.isWeaknessDetection ?? false) m_damage *= 1.5f;
                else m_damage *= 1.2f;
            }
            if (DynamicValues.isCorrosion && CurrentShield > 0) m_damage *= 1.5f;
            if (DynamicValues.isTargetFixed) m_damage *= 0.5f;
            if (playerStatus.DynamicValues.isDefenseDestruction && CurrentShield > 0) m_damage *= 1.5f;
            if (playerStatus.DynamicValues.hasEquipment_313 && CurrentShield > 0) m_damage *= 1.2f;
        }

        if (BattleManager.GetInstance().battleEventManager.ExtraDamageOnTargetDamaged != null && factor != null)
            foreach (Func<IBattleStatus, IBattleFactor, int> func in BattleManager.GetInstance().battleEventManager.ExtraDamageOnTargetDamaged?.GetInvocationList())
                m_damage += func(this, factor);

        battleDamage.SetDamageValue(Mathf.FloorToInt(m_damage));
        return battleDamage;
    }

    public int GainAP(IBattleFactor factor, int amount) {
        return 0;
    }

    public void GainMaxAP(int amount) {
        return;
    }

    public void GainShield(IBattleFactor factor, int amount) {
        int change = 0;
		if (DynamicValues.isRampaged) return;
		if (DynamicValues.isFrenzy) amount -= (int)(BuffCounter.GetBuffCount(ENUM_BUFF_INDEX.FRENZY) / 3f * amount);

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

    public int GainHP(IBattleFactor factor, int amount) {
        int change = 0;

		if (DynamicValues.isCorruption) return change;
		if (DynamicValues.isRampaged) return change;

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

        if (factor is IBattleEnemyCard)
            enemyObject.enemyStatusUI.RemoveHealCard((factor as IBattleEnemyCard).OwnerStatus.TargetEnum);

        if (factor != null)
            BattleManager.GetInstance().battleEventManager.OnTargetGainHp?.Invoke(this, factor);

        UpdateUI();
        return change;
    }

    public void SetHP(int value) {
        enemyStatus.current_hp = value;
        if (enemyStatus.current_hp < 0) enemyStatus.current_hp = 0;
        else if (enemyStatus.current_hp > MaxHp) enemyStatus.current_hp = MaxHp;

        UpdateUI();
    }

    public int LoseAP(int amount) {
        return 0;
    }

    public void GainBuff(IBattleFactor factor, ENUM_BUFF_INDEX buffIndex, int count, params int[] _params) {
        if (count > 0) {
            Buff buff = BuffDao.GetBuff((int)buffIndex);
            IBattleBuff buffScript = buff.battleBuffScript;
            buffScript.InitializeBuff(this, buff, count, _params);
            buffScript.ActivateBuffEffect();
            #region 버프 업적 처리
            //눈먼자들의 도시: 모든 적과 자신에게 실명을 부여하십시오.
            int BlindObjectCount = 0;
            foreach (var Target in BattleManager.GetInstance().GetBattleStatus(ALL).TargestList) {
                if (Target.BuffCounter.HasEnoughBuffCount(ENUM_BUFF_INDEX.BLIND, 1))
                    BlindObjectCount++;
            }
            if (BlindObjectCount == BattleManager.GetInstance().GetBattleStatus(ALL).TargestList.Count)
                AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.City_Of_The_Blind);
            #endregion
            if (factor != null)
                BattleManager.GetInstance().battleEventManager.OnTargetGainBuff?.Invoke(this, factor, buff, count);
        }
    }

    public int LoseBuffCount(ENUM_BUFF_INDEX buffEnum, int count) {
        return BuffCounter.SubtractBuffCount(buffEnum, count);
    }

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

    public int SetShield(int amount) {
        currentShield = amount;
        if (amount < 0 || currentShield < 0) currentShield = 0;
        if (amount > MaxHp / 2) currentShield = MaxHp / 2;

        //SyncUI();
        UpdateUI();

        return currentShield;
    }

    public void PlayEffect(ENUM_BATTLE_VFX effectEnum) {
        BattleEffectManager.PlayEffect(effectEnum, ObjectTransform);
    }

    public void UpdateUI() {
        enemyObject.UpdateUI();
    }
}
