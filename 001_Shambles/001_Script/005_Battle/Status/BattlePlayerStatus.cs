using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using static ENUM_BATTLE_PHASE_TARGET;
using static ENUM_BATTLE_PHASE_ACTION;

public class BattlePlayerStatus : IBattleStatus {
    //original values
    BattlePlayerObject playerObject;

    int skillCoolDown;
    int currentShield;
    PlayerBattleStatus playerStatus;
    CharacterBuffCounter playerBuffCounter;
    BattleDynamicValues playerBattleDynamicValues;
    Skill playerSkill;
    Dictionary<ENUM_EQUIPMENT_PART, Equipment> playerEquipments;

    public ENUM_BATTLE_PHASE_TARGET TargetEnum => PLAYER;

    //str, dex, int... status for initialize
    public int[] Status => playerStatus.status; // 전투 중 변경시킬 수 없음.(이유: 장비 등 전처리 과정에서 이전 장비의 영향을 받아서는 안됨.) 대신 DynamicValues의 status 관련 변수 수정 필요.

    //flexable status
    public int MaxHp => playerStatus.max_hp;

    public int CurrentHp => playerStatus.current_hp;

    public int MaxAp => playerStatus.max_ap;

    public int CurrentAp => playerStatus.current_ap;

    public int ExtraDraw => playerStatus.extra_draw;

    public int CurrentShield => currentShield;

	public int MaxShield {
		get {
			if (playerBattleDynamicValues.isCurseFlag_001) {
				return (int)((playerStatus.max_hp / 2f) * 0.8f);
			}
			else if (playerBattleDynamicValues.isCurseFlag_001_2) {
				return (40);
			}
			return (playerStatus.max_hp / 2);
		}
	}

    //Buff and Dynamic values
    public CharacterBuffCounter BuffCounter => playerBuffCounter;
    public BattleDynamicValues DynamicValues => playerBattleDynamicValues;

    public Transform ObjectTransform => playerObject.transform;
    public int SkillCoolDown => skillCoolDown;
    public Skill PlayerSkill => playerSkill;
    public Dictionary<ENUM_EQUIPMENT_PART, Equipment> PlayerEquipments => playerEquipments;

    public void Initialize(BattlePlayerObject _playerObject) {
        playerObject = _playerObject;
        BattleManager.GetInstance().AddBattleStatus(this);

        playerStatus = PlayerBattleStatusDao.GetPlayerBattleStatus(PlayerInfoDao.GetDefaultPlayerInfoIndex());

        if (playerStatus.current_hp > playerStatus.max_hp) {
            playerStatus.current_hp = playerStatus.max_hp;
        }

        playerBuffCounter = new CharacterBuffCounter(_playerObject.battlePlayerBuffUI, this, TURN_END);
        playerBattleDynamicValues = new BattleDynamicValues(this);
        int skillIndex = PlayerInfoDao.GetPlayerSkillInfo();
        if (skillIndex != 0) {
            playerSkill = SkillDao.GetActiveSkillInfo(skillIndex);
            if (playerSkill != null) {
                playerSkill.skillScript.InitializeCardAction(playerSkill, this);
            }
        }

        PlayerEquip equip = PlayerEquipDao.GetPlayerEquipInfo(PlayerInfoDao.GetDefaultPlayerInfoIndex());
        playerEquipments = new Dictionary<ENUM_EQUIPMENT_PART, Equipment>();
        for (int i = 0; i < Enum.GetValues(typeof(ENUM_EQUIPMENT_PART)).Length; i++) {
            if (equip.currentEquipments[i] != null) {
                var equipment = EquipmentDao.GetEquipmentInfo(equip.currentEquipments[i]);
                playerEquipments.Add((ENUM_EQUIPMENT_PART)i, equipment);
                var equipmentScript = equipment.battleEquipmentScript;
                if (equipmentScript != null) {
                    equipmentScript.ThisEquipment = equipment;
                }
            }
        }
    }

    public int Damage(IBattleFactor factor, BattleDamage damage) {
        BattleDamage battleDamage = new BattleDamage(damage);

        float m_damage = battleDamage.damage;

        //대미지 타입 변경
        if (DynamicValues.isBuffFlag_012) battleDamage.damageType = ENUM_DAMAGE_TYPE.PENETRATE;

        if (battleDamage.damageSourceType == ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK) {
            DynamicValues.lastDamagedByTarget = battleDamage.attacker;
            if (battleDamage.attacker?.DynamicValues.isBuffFlag_001 ?? false) battleDamage.damageType = ENUM_DAMAGE_TYPE.PENETRATE;
            BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, DIRECT_DAMAGED_STANDBY);

            //가감 연산
            if (DynamicValues.isBuffFlag_002) m_damage -= 2f;
            if (DynamicValues.isBuffFlag_002_2) m_damage -= 5f;
            if (DynamicValues.hasEquipment_Example_001 && BuffCounter.BuffExist(ENUM_BUFF_INDEX.EXAMPLE_BUFF_001)) m_damage -= 4f;

            if (DynamicValues.hasEquipment_Example_002 && BuffCounter.BuffExist(ENUM_BUFF_INDEX.EXAMPLE_BUFF_002)) m_damage -= 1f;
            if (DynamicValues.hasEquipment_Example_002 && BuffCounter.BuffExist(ENUM_BUFF_INDEX.EXAMPLE_BUFF_002_2)) m_damage -= 1f;

            if (DynamicValues.hasEquipment_Example_003 && BuffCounter.BuffExist(ENUM_BUFF_INDEX.EXAMPLE_BUFF_002)) m_damage -= 3f;
            if (DynamicValues.hasEquipment_Example_003 && BuffCounter.BuffExist(ENUM_BUFF_INDEX.EXAMPLE_BUFF_002_2)) m_damage -= 3f;

            if (DynamicValues.hasEquipment_Example_004 && BuffCounter.BuffExist(ENUM_BUFF_INDEX.EXAMPLE_BUFF_002_2)) m_damage -= BuffCounter.GetBuffCount(ENUM_BUFF_INDEX.EXAMPLE_BUFF_002_2);

            //승제 연산
            if (DynamicValues.isBuffFlag_011) {
                if (damage.attacker?.DynamicValues.isCardEffect_004 ?? false) m_damage *= 1.5f;
                else m_damage *= 1.2f;
            }
            if (DynamicValues.isBuffFlag_008 && CurrentShield > 0) m_damage *= 1.5f;
            if (DynamicValues.isCardEffect_001) m_damage *= 0.5f;
            if (battleDamage.attacker != null) {
                if (battleDamage.attacker.DynamicValues.isBuffFlag_005 && CurrentShield > 0) m_damage *= 1.5f;
			}
			if (DynamicValues.isCurseFlag_002) {
				m_damage *= 1.15f;
			}
			else if (DynamicValues.isCurseFlag_002_2) {
				m_damage *= 1.3f;
			}
		}

		BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, DAMAGE_STANDBY);

        battleDamage.SetDamageValue(Mathf.FloorToInt(m_damage));

        int prevHp = CurrentHp;
        if (!(battleDamage.damage <= 0 || DynamicValues.isBuffFlag_014 || DynamicValues.isBuffFlag_006)) {
			if (factor is IBattlePlayerCard) {
				BattleManager.GetInstance().battleArchive.PlayerAct.Add((ENUM_BATTLE_PLAYER_ACT_TYPE.Damaged, battleDamage.damage));
			}
			if (battleDamage.damageType == ENUM_DAMAGE_TYPE.PENETRATE) {
                playerStatus.current_hp -= battleDamage.damage;
            }
            else if (battleDamage.damageType == ENUM_DAMAGE_TYPE.NORMAL) {
                //damage to shield first
                int remainDamage = battleDamage.damage;

                //no shield
                if (currentShield <= 0) {
                    currentShield = 0;
                }
                //has enough shield
                else if (currentShield > battleDamage.damage) {
                    currentShield -= battleDamage.damage;
                    BattleManager.GetInstance().battleEventManager.OnTargetShieldDamaged?.Invoke(this, factor, remainDamage);
                    remainDamage = 0;
                }
                //shield <= damage
                else {
                    remainDamage = battleDamage.damage - currentShield;
					int prevShield = currentShield;
                    currentShield = 0;
					BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, BROKE_SHIELD);
                    BattleManager.GetInstance().battleEventManager.OnTargetShieldDamaged?.Invoke(this, factor, prevShield);
                    SoundManager.GetInstance().PlayEffectSound(ENUM_EFFECT_SOUND.Player_shield_break);
				}

                playerStatus.current_hp -= remainDamage;

			}

			if (CurrentHp < prevHp) {
                BattleManager.GetInstance().battlePhaseManager.ProceedPhase(TargetEnum, HP_DAMAGED);
                if (battleDamage.damageSourceType == ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK && factor != null)
                    BattleManager.GetInstance().battleEventManager.OnTargetHpDamaged?.Invoke(this, factor, prevHp - CurrentHp);
                BattleManager.GetInstance().battleArchive.HP_DAMAGED += prevHp - CurrentHp; // 데미지를 받은 플레이어의 체력 감소량을 기록
            }

            // Ready to Death
            if (playerStatus.current_hp <= 0) {
                playerStatus.current_hp = 0;
                KillPlayerDelay();
            }

            GameObject.Destroy(BattleEffectManager.PlayUIEffect(ENUM_BATTLE_VFX_UI.BLOOD), 2f);
            BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, DAMAGED);

            if (battleDamage.damageSourceType == ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK) {
                if (battleDamage.attacker != null) {
                    if (DynamicValues.hasEquipment_Example_005 && battleDamage.damage >= 15) battleDamage.attacker.GainBuff(null, ENUM_BUFF_INDEX.EXAMPLE_BUFF_003, 3);
                    if (battleDamage.attacker.DynamicValues.isBuffFlag_004) battleDamage.attacker.GainHP(battleDamage.attacker.BuffCounter.GetBuff(ENUM_BUFF_INDEX.EXAMPLE_BUFF_004).battleBuffScript, (int)(battleDamage.damage * 0.3f));
                }
                Camera.main.BlurCamera(BattleCameraEffectDefine.blur_duration, battleDamage.damage);
                Camera.main.ShakeCamera(BattleCameraEffectDefine.shake_duration, battleDamage.damage);
                BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, DIRECT_DAMAGED);
            }
        }

        if (factor != null && battleDamage.damageSourceType == ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK)
            BattleManager.GetInstance().battleEventManager.OnTargetDamaged?.Invoke(this, factor, battleDamage.damage);

        UpdateUI();

        return battleDamage.damage;
    }

    private async void KillPlayerDelay() {
        BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, DEAD_STANDBY);
        await Task.Delay(100);
        if (CurrentHp <= 0) BattleManager.GetInstance().CheckBattleEnd();
    }

    public void SetSkillCoolDown(int amount) {
        skillCoolDown = amount;
        if (skillCoolDown < 0) skillCoolDown = 0;
        BattleManager.GetInstance().battleSkill.UpdateUI();
    }

    public void ReduceSkillCoolDown(int amount = 1) {
        skillCoolDown -= amount;
        if (skillCoolDown < 0) skillCoolDown = 0;
        BattleManager.GetInstance().battleSkill.UpdateUI();
    }

    public void ChangePlayerSkill(Skill skill) {
        playerSkill = skill;
        if (playerSkill != null) {
            playerSkill.skillScript.InitializeCardAction(playerSkill, this);
        }
        BattleManager.GetInstance().battleSkill.Initialize(playerObject.battlePlayerStatus);
        BattleManager.GetInstance().battleSkill.UpdateUI();

    }

    public int GainAP(IBattleFactor factor, int amount) {
        int change = 0;

        if (amount > 0) {
            int previousMp = playerStatus.current_ap;
            playerStatus.current_ap += amount;
            if (playerStatus.current_ap > playerStatus.max_ap) playerStatus.current_ap = playerStatus.max_ap;
            change = playerStatus.current_ap - previousMp;

            if (change != 0) {
                SoundManager.GetInstance().PlayEffectSound(ENUM_EFFECT_SOUND.Player_AP_up);
                BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, GAIN_AP);
            }
        }

        if (factor != null)
            BattleManager.GetInstance().battleEventManager.OnTargetGainAp?.Invoke(this, factor);

        playerObject.battlePlayerStatusUI.UpdateMpUI();

        return change;
    }

    public void GainMaxAP(int amount) {
        playerStatus.max_ap += amount;
        if (playerStatus.max_ap < 0) playerStatus.max_ap = 0;
        if (playerStatus.current_ap > playerStatus.max_ap) playerStatus.current_ap = playerStatus.max_ap;
        playerObject.battlePlayerStatusUI.UpdateMpUI();
    }

    public void SetMaxAP(int value) {
        if (value < 0) return;
        playerStatus.max_ap = value;
        playerObject.battlePlayerStatusUI.UpdateMpUI();
    }

    public void SetAP(int value) {
        if (value < 0) return;
        playerStatus.current_ap = value;
        playerObject.battlePlayerStatusUI.UpdateMpUI();
    }

    public int LoseAP(int amount) {
        int previousAp = playerStatus.current_ap;
        playerStatus.current_ap -= amount;
        if (playerStatus.current_ap < 0) {
            playerStatus.current_ap = 0;
        }
        if (playerStatus.current_ap.Equals(previousAp)) return 0;

        BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, LOSE_AP);
        playerObject.battlePlayerStatusUI.UpdateMpUI();

        return previousAp - playerStatus.current_ap;
    }

    public void RestoreMP(int? value = null) {
        if (value != null) playerStatus.current_ap += value.Value;
        else playerStatus.current_ap = MaxAp;
        if (playerStatus.current_ap > MaxAp) playerStatus.current_ap = MaxAp;
        BattleManager.GetInstance().battlePhaseManager.ProceedPhase(ENUM_BATTLE_PHASE_TARGET.PLAYER, ENUM_BATTLE_PHASE_ACTION.RESTORE_AP);
        playerObject.battlePlayerStatusUI.UpdateMpUI();
    }

    public void GainShield(IBattleFactor factor, int amount) {
        int change = 0;
		
		if (DynamicValues.isBuffFlag_018) return;
		if (DynamicValues.hasEquipment_Example_008) return;
        if (DynamicValues.isBuffFlag_010) amount -= (int)(BuffCounter.GetBuffCount(ENUM_BUFF_INDEX.EXAMPLE_BUFF_005) / 3f * amount);
		if (DynamicValues.isCurseFlag_003) amount = (int)(amount * 0.7f);
		if (DynamicValues.isCurseFlag_003_2) amount = (int)(amount * 0.7f);

		if (!(amount <= 0 || DynamicValues.hasEquipment_Example_008)) {
            int previousShield = currentShield;
            if (DynamicValues.isSkillEffect_001) amount *= 2;

            currentShield += amount;
            if (currentShield > MaxShield) currentShield = MaxShield;
            change = currentShield - previousShield;

            if (change != 0) {
                SoundManager.GetInstance().PlayEffectSound(ENUM_EFFECT_SOUND.Player_shield_up);
                BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, GAIN_SHIELD);
                BattleManager.GetInstance().battleStatisticsManager.AddTotalGainedShieldValue(currentShield - previousShield);
            }

            #region 방어도 업적
            //전투에서 방어도를 100 이상 쌓으십시오.
            if (currentShield >= 100) {
                AchievementEventManager.UnlockAchievements(ENUM_ACHIEVEMENT.I_Hate_Getting_Hurt);
            }
            #endregion
        }

        if (factor != null){
			BattleManager.GetInstance().battleEventManager.OnTargetGainShield?.Invoke(this, factor, change);
		}
		if (factor is IBattlePlayerCard) {
			BattleManager.GetInstance().battleArchive.PlayerAct.Add((ENUM_BATTLE_PLAYER_ACT_TYPE.Shield, amount));
		}

		UpdateUI();
    }

    public int LoseShield(int amount) {
        int change = 0;

        if (amount <= 0) return 0;
        int previousShield = currentShield;
        currentShield -= amount;
        if (currentShield < 0) currentShield = 0;
        change = previousShield - currentShield;

        //SyncUI();
        BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, LOSE_SHIELD);
        UpdateUI();

        return change;
    }

    public int SetShield(int amount) {
        currentShield = amount;
        if (amount < 0 || currentShield < 0) currentShield = 0;
        if (amount > MaxShield) currentShield = MaxShield;

        //SyncUI();
        UpdateUI();

        return currentShield;
    }

    public int GainHP(IBattleFactor factor, int amount) {
        int change = 0;

		if (DynamicValues.isBuffFlag_009) return change;
		if (DynamicValues.isBuffFlag_018) return change;
		if (DynamicValues.isCurseFlag_003_2) return change;

		if (amount > 0) {
			if (DynamicValues.hasEquipment_Example_009 && factor is IBattlePlayerCard) amount *= 2;

			int previousHp = playerStatus.current_hp;
            playerStatus.current_hp += amount;
            if (playerStatus.current_hp > playerStatus.max_hp) playerStatus.current_hp = playerStatus.max_hp;
            change = playerStatus.current_hp - previousHp;

            if (change != 0) {
            /*
				//SoundManager.GetInstance().PlayEffectSound(ENUM_EFFECT_SOUND.Player_hp_heal);
                //BattleEffectManager.PlayUIEffect(ENUM_BATTLE_VFX_UI.HEAL);
			*/
                BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, HEALED);
                BattleManager.GetInstance().battleStatisticsManager.AddTotalRecoveredHpValue(playerStatus.current_hp - previousHp);
            }
        }

        if (factor != null) {
            BattleManager.GetInstance().battleEventManager.OnTargetGainHp?.Invoke(this, factor);
        }
		if (factor is IBattlePlayerCard) { 
			BattleManager.GetInstance().battleArchive.PlayerAct.Add((ENUM_BATTLE_PLAYER_ACT_TYPE.Heal, amount));
		}

		UpdateUI();

        return change;
    }

    public void SetHP(int value) {
        playerStatus.current_hp = value;
        if (playerStatus.current_hp < 0) playerStatus.current_hp = 0;
        else if (playerStatus.current_hp > MaxHp) playerStatus.current_hp = MaxHp;

        UpdateUI();
    }

    public void SetMaxHP(int value) {
        if (value < 0) return;
        playerStatus.max_hp = value;
        if (playerStatus.current_hp > playerStatus.max_hp)
            playerStatus.current_hp = playerStatus.max_hp;
        UpdateUI();
    }

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

    public int LoseBuffCount(ENUM_BUFF_INDEX buffEnum, int count) {
        return BuffCounter.SubtractBuffCount(buffEnum, count);
    }

    public void UpdateUI() {
        playerObject.UpdateUI();
        BattleManager.GetInstance().battleCardManager.UpdateUI();
    }

    public bool IsApEnough(int requiredAp) {
        if (playerStatus.current_ap >= requiredAp) {
            return true;
        }
        return false;
    }

    public bool IsEquiped(int equipmentIndex, ENUM_EQUIPMENT_PART? part = null) {
        if (part != null) return playerEquipments[part.Value].Index == equipmentIndex;
        else {
            foreach (var equipment in playerEquipments.Values) {
                if (equipment.Index == equipmentIndex) return true;
            }
            return false;
        }
    }

    public void PlayEffect(ENUM_BATTLE_VFX effectEnum) {
        // BattleEffectManager.PlayEffect(effectEnum, ObjectTransform);
    }
}
