using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
*** 필독 ***
BattleStatus는 IBattleStatus 인터페이스의 멤버 함수를 그대로 사용하면서 여러 IBattleStatus를 타겟으로 사용하기 위한 복합 객체 역할을 함.
예를 들어, 플레이어, 적1, 적3의 체력을 회복하려면 기존에는 일일이 타겟 정보를 가져와서 최소 3줄의 코드를 작성해야 했지만
이 객체를 사용하면 플레이어, 적1, 적3의 정보가 담긴 ENUM을 인자로 넣고 단 한 줄의 코드로 명령을 실행할 수 있음.

그리고 이렇게 ENUM을 인자로 넣는 것의 가장 큰 장점은 바로 ENUM의 연산을 사용할 수 있다는 것인데,
예를 들어 적 1을 제외한 나머지 모든 대상에게 피해를 준다고 하면
~ENUM_BATTLE_PHASE_TARGET.ENEMY1 & ENUM_BATTLE_PHASE_TARGET.ALL 을 인자로 집어넣고 나온 BattleStatus에 Damage 함수만 호출시키면 됨.
또한 하나의 루프로 모든 대상을 탐색하여 반환하기 때문에 최적화의 측면도 있음.
*/
public class BattleStatus : IBattleStatusAction {
    private List<IBattleStatus> Targets;
    public bool IsNoneTarget => Targets == null || Targets.Count == 0;
    public ENUM_BATTLE_PHASE_TARGET TargetEnum { get; private set; }
    public List<IBattleStatus> TargestList => Targets;

    private BattleStatus() {
        Targets = new List<IBattleStatus>();
        TargetEnum = 0;
    }

    public BattleStatus(IBattleStatus targetStatus) : this() {
        if (targetStatus == null) {
            return;
        }

        Targets.Add(targetStatus);
        TargetEnum = targetStatus.TargetEnum;
    }

    public BattleStatus(ENUM_BATTLE_PHASE_TARGET target) : this() {
        if (target == ENUM_BATTLE_PHASE_TARGET.NONE || target == 0) {
            return;
        }

        TargetEnum = target;
        if (target.HasFlag(BattleManager.GetInstance().battlePlayerObject.battlePlayerStatus.TargetEnum)) {
            Targets.Add(BattleManager.GetInstance().battlePlayerObject.battlePlayerStatus);
        }

        foreach (var enemy in BattleManager.GetInstance().battleEnemyManager.BattleEnemyObjectList) {
            if (target.HasFlag(enemy.enemyPhaseTargetEnum)) {
                Targets.Add(enemy.enemyStatus);
            }
        }
    }

    public void AddTarget(IBattleStatus status) { // TargetEnum에 맞추어 대상을 추가함.
        if (status == null) return;
        if (TargetEnum.HasFlag(status.TargetEnum) && !Targets.Contains(status)) {
            Targets.Add(status);
        }
    }

    public void ChangeTarget(IBattleStatus status) {
        if (IsNoneTarget || status == null) return;
        var target = Targets.Find((battleStatus) => battleStatus.TargetEnum == status.TargetEnum);
        var i = Targets.IndexOf(target);
        Targets.RemoveAt(i);
        Targets.Insert(i, status);
    }

    public void RemoveTarget(IBattleStatus status) {
        if (IsNoneTarget || status == null) return;
        Targets.Remove(status);
    }

    public void ProceedTargetAction(Action<IBattleStatus> action) {
        if (IsNoneTarget) return;
        for (int i = 0; i < Targets.Count; i++) {
            if (Targets[i].CurrentHp > 0)
                action(Targets[i]);
        }
    }

    public void ProceedRandomTargetAction(Action<IBattleStatus> action) {
        if (IsNoneTarget) return;
        int ran = UnityEngine.Random.Range(0, Targets.Count);
        if (Targets[ran].CurrentHp > 0)
            action(Targets[ran]);
    }

    public int Damage(IBattleFactor factor, BattleDamage amount) {
        int result = 0;

        ProceedTargetAction((target) => {
            result += target.Damage(factor, amount);
        });

        return result;
    }

    public int GainHP(IBattleFactor factor, int amount) {
        int result = 0;

        ProceedTargetAction((target) => {
            result += target.GainHP(factor, amount);
        });

        return result;
    }

    public void SetHP(int value) {
        ProceedTargetAction((target) => target.SetHP(value));
    }

    public int LoseAP(int amount) {
        int result = 0;

        ProceedTargetAction((target) => {
            result += target.LoseAP(amount);
        });

        return result;
    }

    public int GainAP(IBattleFactor factor, int amount) {
        int result = 0;

        ProceedTargetAction((target) => {
            result += target.GainAP(factor, amount);
        });

        return result;
    }

    public void GainMaxAP(int amount) {
        ProceedTargetAction((target) => target.GainMaxAP(amount));
    }


    public void GainShield(IBattleFactor factor, int amount) {
        ProceedTargetAction((target) => target.GainShield(factor, amount));
    }

    public int LoseShield(int amount) {
        int result = 0;

        ProceedTargetAction((target) => {
            result += target.LoseShield(amount);
        });

        return result;
    }

    public int SetShield(int amount) {
        int result = 0;

        ProceedTargetAction((target) => {
            result = target.SetShield(amount);
        });

        return result;
    }

    public void GainBuff(IBattleFactor factor, ENUM_BUFF_INDEX buffIndex, int count, params int[] _params) {
        ProceedTargetAction((target) => target.GainBuff(factor, buffIndex, count, _params));
    }

    public int LoseBuffCount(ENUM_BUFF_INDEX buffEnum, int count) {
        int result = 0;

        ProceedTargetAction((target) => {
            result += target.LoseBuffCount(buffEnum, count);
        });

        return result;
    }

    public void PlayEffect(ENUM_BATTLE_VFX effectEnum) {
        // 보통 모든 대상에게 효과를 적용할 때 사용됨.
        ProceedTargetAction((target) => target.PlayEffect(effectEnum));
    }

    public void UpdateUI() {
    }
}
