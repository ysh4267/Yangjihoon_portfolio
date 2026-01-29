using UnityEngine;

/// <summary>
/// 전투 개체(플레이어, 적)의 상태를 정의하는 통합 인터페이스
/// IBattleStatusAttributes(속성 접근)와 IBattleStatusAction(행동 수행)을 상속
/// 다형성을 통해 플레이어와 적을 동일한 방식으로 처리 가능
/// </summary>
public interface IBattleStatus : IBattleStatusAttributes, IBattleStatusAction {
}
