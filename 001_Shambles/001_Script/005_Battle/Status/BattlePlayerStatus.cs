using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using static ENUM_BATTLE_PHASE_TARGET;
using static ENUM_BATTLE_PHASE_ACTION;

public class BattlePlayerStatus : IBattleStatus {
    //player 게임 내 오브젝트 객체
    BattlePlayerObject playerObject;
    //실제 값을 담기 위한 데이터
    CharacterBattleStatus playerStatus;
    int currentShield;
    //버프 지속시간과 추가 삭제를 관리하기 위한 카운터
    CharacterBuffCounter playerBuffCounter;
    //스킬
    Skill playerSkill;
    int skillCoolDown;
    //전투 진행중 적용될 값
    BattleDynamicValues playerBattleDynamicValues;
    //착용중인 장버 정보
    Dictionary<ENUM_EQUIPMENT_PART, Equipment> playerEquipments;

    //외부 참조를 위한 프롬프트 모음
    public ENUM_BATTLE_PHASE_TARGET TargetEnum => PLAYER;
    // 전투 중 변경시킬 수 없음.(이유: 장비 등 전처리 과정에서 이전 장비의 영향을 받아서는 안됨.) 대신 DynamicValues의 status 관련 변수 수정 필요.
    public int[] Status => playerStatus.status; 

    public int MaxHp => playerStatus.max_hp;

    public int CurrentHp => playerStatus.current_hp;

    public int MaxAp => playerStatus.max_ap;

    public int CurrentAp => playerStatus.current_ap;

    public int ExtraDraw => playerStatus.extra_draw;

    public int CurrentShield => currentShield;

	// 최대 방어도 = 최대 체력 / 2, 저주 적용 시 공식 변경
	public int MaxShield {
		get {
			// 저주 001: 최대 방어도 20% 감소
			if (playerBattleDynamicValues.isCurseFlag_001) {
				return (int)((playerStatus.max_hp / 2f) * 0.8f);
			}
			// 저주 001_2: 최대 방어도 40 고정
			else if (playerBattleDynamicValues.isCurseFlag_001_2) {
				return (40);
			}
			return (playerStatus.max_hp / 2);
		}
	}

    public Transform ObjectTransform => playerObject.transform;
    public CharacterBuffCounter BuffCounter => playerBuffCounter;
    public BattleDynamicValues DynamicValues => playerBattleDynamicValues;
    public Skill PlayerSkill => playerSkill;
    public int SkillCoolDown => skillCoolDown;
    public Dictionary<ENUM_EQUIPMENT_PART, Equipment> PlayerEquipments => playerEquipments;

    // 플레이어 전투 데이터 초기화, 스테이터스/버프/스킬/장비 로드
    public void Initialize(BattlePlayerObject _playerObject) {
        playerObject = _playerObject;
        BattleManager.GetInstance().AddBattleStatus(this);

        // DB에서 플레이어 전투 스테이터스 로드
        playerStatus = PlayerBattleStatusDao.GetPlayerBattleStatus(PlayerInfoDao.GetDefaultPlayerInfoIndex());

        // 현재 체력이 최대 체력을 초과하지 않도록 보정
        if (playerStatus.current_hp > playerStatus.max_hp) {
            playerStatus.current_hp = playerStatus.max_hp;
        }

        playerBuffCounter = new CharacterBuffCounter(_playerObject.battlePlayerBuffUI, this, TURN_END);
        playerBattleDynamicValues = new BattleDynamicValues(this);
        // 장착된 스킬 로드 및 초기화
        int skillIndex = PlayerInfoDao.GetPlayerSkillInfo();
        if (skillIndex != 0) {
            playerSkill = SkillDao.GetActiveSkillInfo(skillIndex);
            if (playerSkill != null) {
                playerSkill.skillScript.InitializeCardAction(playerSkill, this);
            }
        }

        // 장착된 장비 로드 및 파트별 등록
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

    // 플레이어에게 대미지를 적용하여 버프/장비/저주에 따른 가감 및 승제 연산을 수행하고 방어도와 체력을 차감
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
        // 대미지가 0 이하이거나 무적/회피 버프가 활성화된 경우 대미지 적용을 건너뜀
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

    // 사망 처리를 지연시켜 동시 실행되는 Phase 처리가 완료된 후 전투 종료 여부를 확인
    private async void KillPlayerDelay() {
        BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, DEAD_STANDBY);
        await Task.Delay(100);
        if (CurrentHp <= 0) BattleManager.GetInstance().CheckBattleEnd();
    }

    // 스킬 쿨다운을 지정된 값으로 설정
    public void SetSkillCoolDown(int amount) {
        skillCoolDown = amount;
        if (skillCoolDown < 0) skillCoolDown = 0;
        BattleManager.GetInstance().battleSkill.UpdateUI();
    }

    // 스킬 쿨다운을 지정된 수치만큼 감소
    public void ReduceSkillCoolDown(int amount = 1) {
        skillCoolDown -= amount;
        if (skillCoolDown < 0) skillCoolDown = 0;
        BattleManager.GetInstance().battleSkill.UpdateUI();
    }

    // 플레이어의 스킬을 교체하고 스킬 UI를 갱신
    public void ChangePlayerSkill(Skill skill) {
        playerSkill = skill;
        if (playerSkill != null) {
            playerSkill.skillScript.InitializeCardAction(playerSkill, this);
        }
        BattleManager.GetInstance().battleSkill.Initialize(playerObject.battlePlayerStatus);
        BattleManager.GetInstance().battleSkill.UpdateUI();

    }

    // AP를 획득하여 최대치를 초과하지 않도록 보정 후 변동량을 반환
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

    // 최대 AP를 증감하고 현재 AP가 최대치를 초과하지 않도록 보정
    public void GainMaxAP(int amount) {
        playerStatus.max_ap += amount;
        if (playerStatus.max_ap < 0) playerStatus.max_ap = 0;
        if (playerStatus.current_ap > playerStatus.max_ap) playerStatus.current_ap = playerStatus.max_ap;
        playerObject.battlePlayerStatusUI.UpdateMpUI();
    }

    // 최대 AP를 지정된 값으로 설정
    public void SetMaxAP(int value) {
        if (value < 0) return;
        playerStatus.max_ap = value;
        playerObject.battlePlayerStatusUI.UpdateMpUI();
    }

    // 현재 AP를 지정된 값으로 설정
    public void SetAP(int value) {
        if (value < 0) return;
        playerStatus.current_ap = value;
        playerObject.battlePlayerStatusUI.UpdateMpUI();
    }

    // AP를 소모하여 변동량을 반환, 0 미만이 되지 않도록 보정
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

    // AP를 회복, 값을 지정하지 않으면 최대치까지 전부 회복
    public void RestoreMP(int? value = null) {
        if (value != null) playerStatus.current_ap += value.Value;
        else playerStatus.current_ap = MaxAp;
        if (playerStatus.current_ap > MaxAp) playerStatus.current_ap = MaxAp;
        BattleManager.GetInstance().battlePhaseManager.ProceedPhase(ENUM_BATTLE_PHASE_TARGET.PLAYER, ENUM_BATTLE_PHASE_ACTION.RESTORE_AP);
        playerObject.battlePlayerStatusUI.UpdateMpUI();
    }

    // 방어도를 획득하여 버프/장비/저주에 따른 보정을 적용하고 최대치를 초과하지 않도록 제한
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

    // 방어도를 감소시키고 변동량을 반환
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

    // 방어도를 지정된 값으로 설정하고 0~MaxShield 범위로 보정
    public int SetShield(int amount) {
        currentShield = amount;
        if (amount < 0 || currentShield < 0) currentShield = 0;
        if (amount > MaxShield) currentShield = MaxShield;

        //SyncUI();
        UpdateUI();

        return currentShield;
    }

    // 체력을 회복하여 버프/장비/저주에 따른 회복 제한을 적용하고 변동량을 반환
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

    // 현재 체력을 지정된 값으로 설정하고 0~MaxHp 범위로 보정
    public void SetHP(int value) {
        playerStatus.current_hp = value;
        if (playerStatus.current_hp < 0) playerStatus.current_hp = 0;
        else if (playerStatus.current_hp > MaxHp) playerStatus.current_hp = MaxHp;

        UpdateUI();
    }

    // 최대 체력을 지정된 값으로 설정하고 현재 체력이 초과하지 않도록 보정
    public void SetMaxHP(int value) {
        if (value < 0) return;
        playerStatus.max_hp = value;
        if (playerStatus.current_hp > playerStatus.max_hp)
            playerStatus.current_hp = playerStatus.max_hp;
        UpdateUI();
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

    // 플레이어 및 카드 UI를 갱신
    public void UpdateUI() {
        playerObject.UpdateUI();
        BattleManager.GetInstance().battleCardManager.UpdateUI();
    }

    // 필요 AP 이상 보유 여부를 확인
    public bool IsApEnough(int requiredAp) {
        if (playerStatus.current_ap >= requiredAp) {
            return true;
        }
        return false;
    }

    // 특정 장비의 장착 여부를 확인, 파트를 지정하면 해당 파트만 확인
    public bool IsEquiped(int equipmentIndex, ENUM_EQUIPMENT_PART? part = null) {
        if (part != null) return playerEquipments[part.Value].Index == equipmentIndex;
        else {
            foreach (var equipment in playerEquipments.Values) {
                if (equipment.Index == equipmentIndex) return true;
            }
            return false;
        }
    }

    // 플레이어 오브젝트 위치에 이펙트를 재생
    public void PlayEffect(ENUM_BATTLE_VFX effectEnum) {
        // BattleEffectManager.PlayEffect(effectEnum, ObjectTransform);
    }
}
