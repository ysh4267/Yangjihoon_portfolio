using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 전투중 플레이어 정보를 초기 세팅하기 위한 클래스
public class PlayerBattleStatus {
	public int[] status = new int[System.Enum.GetValues(typeof(ENUM_STATUS)).Length];
	public int max_hp;          // 최대 체력
	public int current_hp;      // 현재 체력
	public int max_ap;          // 최대 기력
	public int current_ap;      // 현재 기력
	public int extra_draw;      // 추가 드로우 수치

	public PlayerBattleStatus() {
		status[(int)ENUM_STATUS.HP] = 0;
		status[(int)ENUM_STATUS.STR] = 0;
		status[(int)ENUM_STATUS.DEX] = 0;
		status[(int)ENUM_STATUS.INT] = 0;

		current_hp = 50;
		max_hp = 50;
		current_ap = 3;
		max_ap = 3;
		extra_draw = 0;
	}
}
