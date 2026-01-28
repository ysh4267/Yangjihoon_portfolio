using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적 애니메이션 이벤트 핸들러 클래스
/// 애니메이션 키프레임에서 호출되는 공격, 피격, 사망 등의 이벤트 처리
/// </summary>
public class EnemyAnimationEvent : MonoBehaviour {
	BattleEnemyObject enemyObject = null;
	bool isMyTurn => BattleManager.GetInstance().CurrentTurn == enemyObject.enemyPhaseTargetEnum;


	private void Start() {
		if (!gameObject.transform.parent.parent.TryGetComponent(out enemyObject))
			enemyObject = gameObject.transform.parent.parent.parent.GetComponent<BattleEnemyObject>();
	}

	void CameraFocus() {
		if (isMyTurn)
			Camera.main.ZoomCamera(transform.parent.parent);
	}

	void EnemyBehavior() {
		enemyObject.currentBehavior?.Invoke();
		BattleManager.GetInstance().battlePlayerObject.UpdateUI();
	}

	void SoundPlay() {
		enemyObject.soundPlay?.Invoke();
	}

	void SetCameraBack() {
		if (isMyTurn && !BattleManager.GetInstance().battleDialogueManager.isDialoguePlaying) {
			StopAllCoroutines();
			Camera.main.SetCameraBack();
		}
	}

	void OnDamaged() {
		SoundManager.GetInstance().PlayEffectSound(enemyObject.enemyData.enemyPattern.HitSound);
		if (isMyTurn) {
			StartCoroutine(SetCameraBackAfterAnimationEnd());
		}
	}

	void KillEnemy() {
		SoundManager.GetInstance().PlayEffectSound(enemyObject.enemyData.enemyPattern.DeathSound);
		if (isMyTurn) {
			SetCameraBack();
		}
	}

	void EndAnimation() {
		// this method is obsolete
	}

	IEnumerator SetCameraBackAfterAnimationEnd() {
		yield return new WaitUntil(() => GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);
		SetCameraBack();
	}
}
