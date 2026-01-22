using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어 도감 정보만을 취합한 데이터
public class PlayerUnlockList {
	public List<int> clearedAreaList;
	public List<int> unlockedEquipmentList;
	public List<int> unlockedActiveSkillList;
	public List<int> unlockedPassiveSkillList;
	public List<int> playedStoryEventList;
	public List<int> newUnlockedJournalList;
	public List<int> newUnlockedCharacterList;

	public PlayerUnlockList() {
		clearedAreaList = new List<int>();
		unlockedEquipmentList = new List<int>();
		unlockedActiveSkillList = new List<int>();
		unlockedPassiveSkillList = new List<int>();
		playedStoryEventList = new List<int>();
		newUnlockedJournalList = new List<int>();
		newUnlockedCharacterList = new List<int>();
	}
}
