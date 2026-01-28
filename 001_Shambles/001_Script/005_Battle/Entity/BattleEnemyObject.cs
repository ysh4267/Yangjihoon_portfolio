using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 씬에 배치되는 개별 적 개체 클래스
/// 적 데이터, 상태, UI, 애니메이션, 행동 패턴 스크립트를 통합 관리
/// </summary>
public class BattleEnemyObject : MonoBehaviour, IComparable<BattleEnemyObject> {
	public ENUM_BATTLE_PHASE_TARGET enemyPhaseTargetEnum;
	public BattleEnemyStatus enemyStatus = null;
	public BattleEnemyStatusUI enemyStatusUI = null;
	public BattleBuffIconList enemyBuffUI = null;

	public IBattleEnemy enemyScript;
	public IBattleEnemyPhase enemyPhaseScript;
	public Action currentBehavior;
	public Action soundPlay;
	public Transform enemySpriteTransform;
	public GameObject enemySprite;
	public Enemy enemyData;
	public bool isUndestroyable;

	public int CompareTo(BattleEnemyObject other) {
		if (other == null) return 1;
		return enemyPhaseTargetEnum.CompareTo(other.enemyPhaseTargetEnum);
	}

	public void Initialize(in Enemy enemyRawData) {
		isUndestroyable = false;

		if (enemyRawData == null || enemyRawData.prefab == null || enemyRawData.prefab.path.IsNullQueryString())
			enemyData = EnemyDao.GetEnemy(1); // 대신귀 여운거 미를드 리겠습 니다
		else
			enemyData = enemyRawData;

		enemyStatus = new BattleEnemyStatus();
		enemyStatus.Initialize(this);

		enemySprite = Instantiate(Resources.Load<GameObject>(enemyData.prefab.path), enemySpriteTransform);
		enemyScript = enemyData.enemyPattern;
		enemyScript.InitializeEnemyAction(this);
		enemyScript.UpdateEnemyActionUI();
		if (enemyScript is IBattleEnemyColliderSize) {
			var collider = GetComponent<BoxCollider>();
			collider.size = new Vector2(collider.size.x * (enemyScript as IBattleEnemyColliderSize).xColliderRatio, collider.size.y * (enemyScript as IBattleEnemyColliderSize).yColliderRatio);
		}

		enemyStatusUI.gameObject.SetActive(true);
		enemyStatusUI.Initialize();
		enemyStatusUI.UpdateUI();

		if (enemyData.characterIndex != null) {
			BattleManager.GetInstance().battleStatisticsManager.AddCharacter(enemyData.characterIndex.Value);

			//if (BattleManager.GetInstance().battlePlayerObject.battlePlayerStatus.PlayerEquipments.ContainsKey(ENUM_EQUIPMENT_PART.ETC)) {
			//    var etcEquipment = BattleManager.GetInstance().battlePlayerObject.battlePlayerStatus.PlayerEquipments[ENUM_EQUIPMENT_PART.ETC];
			//    if (etcEquipment.Index == 524 && !CharacterDao.IsUnlocked(enemyData.characterIndex.Value)) { // 도감에 새로 등록되는 적에 한하여
			//        BattleManager.GetInstance().battleArchive.AddGold(etcEquipment, 100);
			//    }
			//}
		}
	}

	public void UpdateUI() {
		enemyStatusUI.UpdateUI();
		if (BattleManager.GetInstance().CurrentTurn == ENUM_BATTLE_PHASE_TARGET.PLAYER)
			enemyScript.UpdateEnemyActionUI();
	}

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

	// 적 대사 출력. DB에 있는 대사 정보를 가져오며 대사가 있는 적에 한해서만 올바르게 실행됨. 각각의 대사는 연결리스트 형태로 줄줄이 나옴.
	public void PlayDialogue() {
		BattleManager.GetInstance().battleDialogueManager.Show(this);
	}

	public void Destroy() {
		if (!isUndestroyable) {
			Destroy(enemySprite);
			gameObject.SetActive(false);
		}
	}
}
