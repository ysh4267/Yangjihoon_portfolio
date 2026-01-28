using UnityEngine;
using static ENUM_CARD_TYPE;

/// <summary>
/// 예제 적 클래스
/// 적의 행동 패턴과 UI 갱신을 구현한 템플릿
/// </summary>
public class ExampleEnemy : IBattleEnemy {
	#region 기본 속성
	// 현재 패턴 인덱스
	public int PatternIndex { get; set; }
	// 해당 적 오브젝트
	public BattleEnemyObject thisEnemyObject { get; set; }
	// 타겟 상태
	public BattleStatus target { get; set; }
	// 공격 사운드
	public AudioClip AttackSound { get; set; }
	// 보조 공격 사운드
	public AudioClip Attack2Sound { get; set; }
	// 피격 사운드
	public AudioClip HitSound { get; set; }
	// 사망 사운드
	public AudioClip DeathSound { get; set; }
	// 다음 패턴 타입
	public ENUM_CARD_TYPE NextPatternTypeEnum { get; set; }
	#endregion

	#region 카드 객체
	// 공격 카드 객체
	Card_444401_EnemyNormalAttack attack = new Card_444401_EnemyNormalAttack();
	// 방어 카드 객체
	Card_444403_EnemyNormalShield shield = new Card_444403_EnemyNormalShield();
	// 버프 카드 객체
	Card_444404_EnemyNormalBuff buff = new Card_444404_EnemyNormalBuff();
	// 애니메이터 컴포넌트
	Animator animator;
	#endregion

	#region 적 동작
	// 적 행동 초기화 및 사운드 설정
	public void InitializeEnemyAction(BattleEnemyObject enemyObject) {
		thisEnemyObject = enemyObject;
		attack.InitializeCardAction(thisEnemyObject.enemyStatus);
		shield.InitializeCardAction(thisEnemyObject.enemyStatus);
		buff.InitializeCardAction(thisEnemyObject.enemyStatus);
		buff.BuffInitialize(ENUM_BUFF_INDEX.SHARPNESS);

		PatternIndex = Random.Range(0, 3);

		AttackSound = SoundManager.GetInstance().GetAudioClip(ENUM_BATTLE_EFFECT_SOUND.SFX_attack_hit);
		Attack2Sound = null;
		HitSound = SoundManager.GetInstance().GetAudioClip(ENUM_BATTLE_EFFECT_SOUND.SFX_Enemy_hit_normal);
		DeathSound = null;

		animator = thisEnemyObject.enemySprite.GetComponent<Animator>();
		target = BattleManager.GetInstance().GetBattleStatus(BattleManager.GetInstance().battlePlayerObject.battlePlayerStatus);
	}

	// 패턴에 따른 적 행동 실행
	public void ProceedEnemyAction() {
		if (PatternIndex % 3 == 0) {
			// 공격 패턴 예시
			thisEnemyObject.currentBehavior = (() => {
				attack.ProceedCardAction(target, 10);
				if (AttackSound != null) SoundManager.GetInstance().PlayEffectSound(AttackSound);
			});
			animator.PlayAttack();
		}
		else if (PatternIndex % 3 == 1) {
			// 방어 패턴 예시
			thisEnemyObject.currentBehavior = (() => {
				shield.ProceedCardAction(thisEnemyObject.enemyStatus, 5);
			});
			animator.PlayShield();
		}
		else {
			// 버프 패턴 예시
			PatternIndex = -1;
			thisEnemyObject.currentBehavior = (() => {
				buff.ProceedCardAction(thisEnemyObject.enemyStatus, 2);
			});
			animator.PlaySpecial();
		}
		PatternIndex++;
	}

	// 다음 행동 UI 갱신
	public void UpdateEnemyActionUI() {
		if (PatternIndex % 3 == 0) {
			NextPatternTypeEnum = ATTACK;
			thisEnemyObject.enemyStatusUI.UpdateNextActionUI(NextPatternTypeEnum, attack.DamageCalc(10).damage);
		}
		else if (PatternIndex % 3 == 1) {
			NextPatternTypeEnum = SHIELD;
			thisEnemyObject.enemyStatusUI.UpdateNextActionUI(NextPatternTypeEnum, shield.ShieldCalc(5));
		}
		else {
			NextPatternTypeEnum = BUFF;
			thisEnemyObject.enemyStatusUI.UpdateNextActionUI(NextPatternTypeEnum);
		}
	}
	#endregion
}
