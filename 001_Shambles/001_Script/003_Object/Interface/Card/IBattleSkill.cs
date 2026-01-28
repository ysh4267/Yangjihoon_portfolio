using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 전투 스킬 인터페이스
public interface IBattleSkill : IBattleCard
{
	// 현재 사용 가능 횟수
    int CurrentCount { set; get; }
	// 최대 사용 가능 횟수
    int UsableCount { get; }
	// 사용 가능 여부 (false면 다른 조건과 무관하게 사용 불가)
    bool IsUsable { get; set; }
}
