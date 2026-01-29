using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 씬에 배치되는 개별 적 개체 클래스
/// 적 데이터, 상태, UI, 애니메이션, 행동 패턴 스크립트를 통합 관리
/// </summary>
public class BattleEnemyObject : MonoBehaviour, IComparable<BattleEnemyObject> {
	public ENUM_BATTLE_PHASE_TARGET enemyPhaseTargetEnum;  // 전투 진행 중 해당 적을 식별하는 Enum 값
	//Data
	public Enemy enemyData;                                // DB에서 로드한 적 데이터 (HP, 이름, 프리팹 경로 등)
	public BattleEnemyStatus enemyStatus = null;           // 적의 상태를 제어하는 클래스 (상태 데이터 포함)
	//Script
	public IBattleEnemy enemyScript;                       // 적의 행동 패턴을 정의하는 스크립트
	public IBattleEnemyPhase enemyPhaseScript;             // 적의 페이즈 전환 스크립트 (enemyScript와 동일 객체이나 연산 최적화를 위해 별도 캐싱)
	public Action currentBehavior;                         // 애니메이션 이벤트로 호출될 행동 스크립트 델리게이트
	public Action soundPlay;                               // 애니메이션 이벤트로 호출될 사운드 델리게이트
	//UI
	public BattleEnemyStatusUI enemyStatusUI = null;       // HP바 및 상태 표시 UI
	public BattleBuffIconList enemyBuffUI = null;          // 버프/디버프 아이콘 리스트 UI
	public GameObject enemySprite;                         // 적 스프라이트 게임오브젝트
	public Transform enemySpriteTransform;                 // 적 스프라이트의 부모 Transform

	public bool isUndestroyable;                           // 적 파괴 불가 플래그 (특수 연출용)

	// IComparable 구현 - 적 정렬을 위한 비교 메서드
	public int CompareTo(BattleEnemyObject other) {
		if (other == null) return 1;
		return enemyPhaseTargetEnum.CompareTo(other.enemyPhaseTargetEnum);
	}

	// 초기화 메서드
	public void Initialize(in Enemy enemyRawData) {
		isUndestroyable = false;
		// 유효성 검사: 적 데이터가 없으면 기본 적 로드
		if (enemyRawData == null || enemyRawData.prefab == null || enemyRawData.prefab.path.IsNullQueryString())
			enemyData = EnemyDao.GetEnemy(1); // 대신귀 여운거 미를드 리겠습 니다
		else
			enemyData = enemyRawData;

		// 내부 데이터 초기화
		enemyStatus = new BattleEnemyStatus();
		enemyStatus.Initialize(this);

		// 스프라이트 로드
		enemySprite = Instantiate(Resources.Load<GameObject>(enemyData.prefab.path), enemySpriteTransform);
		// 행동 패턴 스크립트 초기화 및 다음 행동 UI 표시
		enemyScript = enemyData.enemyPattern;
		enemyScript.InitializeEnemyAction(this);
		enemyScript.UpdateEnemyActionUI();

		// 카드/스킬 사용을 위한 콜라이더 크기를 스프라이트 기준으로 설정
		if (enemyScript is IBattleEnemyColliderSize) {
			var collider = GetComponent<BoxCollider>();
			collider.size = new Vector2(collider.size.x * (enemyScript as IBattleEnemyColliderSize).xColliderRatio, collider.size.y * (enemyScript as IBattleEnemyColliderSize).yColliderRatio);
		}

		// UI 초기화 (게임 로직 초기화 완료 후 실행)
		enemyStatusUI.gameObject.SetActive(true);
		enemyStatusUI.Initialize();
		enemyStatusUI.UpdateUI();

		// 조우한 적을 도감에 등록
		if (enemyData.characterIndex != null) {
			BattleManager.GetInstance().battleStatisticsManager.AddCharacter(enemyData.characterIndex.Value);
		}
	}

	// UI 갱신
	public void UpdateUI() {
		enemyStatusUI.UpdateUI();
		if (BattleManager.GetInstance().CurrentTurn == ENUM_BATTLE_PHASE_TARGET.PLAYER)
			enemyScript.UpdateEnemyActionUI();
	}

	// 행동 패턴 스크립트의 현재 적 행동을 실행
	public void ProceedEnemyAction() {
		try {
			enemyScript.ProceedEnemyAction();
		}
		catch (Exception e) {
			Debug.LogError(e.Message);
		}
		BattleManager.GetInstance().battlePhaseManager.ProceedPhase(enemyPhaseTargetEnum, ENUM_BATTLE_PHASE_ACTION.PLAY_CARD);
	}

	// 카드 또는 스킬을 적에게 드래그했을때 나오는 화살표 효과 온오프
	public void SetTargeted(IBattleFactor factor, bool isTargeting, BattleDamage battleDamage = null) {
		enemyStatusUI.SetTargeted(isTargeting);
		if (isTargeting && battleDamage != null && battleDamage.damage > 0) {
			BattleDamage damage = enemyStatus.DamageCalc(factor, battleDamage);
			int damageValue = damage.damage;

			if (damage.damageType.Equals(ENUM_DAMAGE_TYPE.NORMAL))
				damageValue -= enemyStatus.CurrentShield;

			if (damageValue >= 0)
				enemyStatusUI.UpdateExpectedHPBarUI(Mathf.FloorToInt(damageValue));
		}
		else {
			enemyStatusUI.ReturnExpectedHPBarUI();
		}
	}

	// 타겟 불가 적에게 카드를 드래그했을때 나오는 적 밝기 감소 효과 온오프
	public void SetTranslucent(bool active) {
		if (active) {
			foreach (var sprite in enemySprite.GetComponentsInChildren<SpriteRenderer>()) {
				if (sprite.gameObject.CompareTag("AmbientEnemy")) continue; //배경과 합쳐진 적들의 프리팹에서 진짜 배경들에 AmbientEnemy 태그를 달아야 함
				sprite.SetBrightness(0.5f);
			}
		}
		else {
			foreach (var sprite in enemySprite.GetComponentsInChildren<SpriteRenderer>()) {
				sprite.SetBrightness(1f);
			}
		}
	}

	// 적 대사 출력 (DB에서 대사 정보를 가져오며, 대사가 있는 적만 실행됨)
	public void PlayDialogue() {
		BattleManager.GetInstance().battleDialogueManager.Show(this);
	}

	// 적 오브젝트 파괴
	public void Destroy() {
		if (!isUndestroyable) {
			Destroy(enemySprite);
			gameObject.SetActive(false);
		}
	}
}
