using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 모든 하위 데이터를 불러온 플레이어 데이터 타입
public class PlayerInfo {
	public int playerInfoIndex;         // 데이터 저장슬롯 (사용되지 않음)
	public int? seedValue;              // 모든 랜덤 함수에 사용될 시드값

	// 단순 데이터 (이름, 스텟 등)
	public string name;     // 이름
	public int statHp;      // 체력 (해당 스테이터스 * 5의 값만큼이 기본 최대체력임)
	public int statStr;     // 힘
	public int statInt;     // 지능
	public int statDex;     // 민첩
	public int statAp;      // 추가 마나
	public int statExtraDraw;   //추가 드로우
	public int gold;        // 골드
	public int level;       // 레벨
	public int exp;         // 경험치
	public int maxHp;       // 최대체력
	public int currentHp;   // 현재 체력
	public int maxMp;       // 최대 마나
	public int currentMp;   // 현재 마나
	public int skillPoint;  // 스킬 포인트

	// 플레이용 복합 데이터
	public Skill playerSkill;           // 스킬
	public StarterPack starterPack;     // 시작 덱
	public Dictionary<ENUM_EQUIPMENT_PART, Equipment> equipmentStatusDict;  // 장착중인 장비
	public Portrait portrait;           // 플레이어 캐릭터
	public List<Card> cardDeckList;     // 현재 덱 정보
	public List<Card> cardBagList;      // 보유중인 모든 카드 목록
	public List<Area> clearedAreaList;  // 지나온 구역 정보
	public List<Equipment> unlockedEquipmentList;   // 구매한 장비 목록
	public List<Skill> unlockedSkillList;           // 구매한 스킬 목록
	public List<int> newUnlockedJournalIndexList;   // 이번 회차에 새로 해금한 저널 목록
	public List<int> newUnlockedCharacterIndexList; // 이번 회차에 새로 해금한 캐릭터 목록
	public List<StoryEvent> playedEventList;        // 진행했던 이벤트 목록
	public List<StoryEvent> queuedEventList;    // 다음 이벤트 리스트에 등장할 목록
	public List<int> selectedStorySelectionIndexList;   // 선택한 선택지 목록
	public List<int> currentEndingIndexList;    // 현재 사망시 출력될 엔딩 목록


	public int? currentSceneIndex;      // 현재 씬
	public int? currentAreaIndex;       // 현재 구역
	public int? currentEventIndex;      // 현재 이벤트
	public int? currentEventCount;      // 현재 이벤트 횟수
	public int? currentBattleIndex;     // 현재 전투
	public int? currentMapIndex;        // 현재 맵
	public int? currentDeadEndingIndex; // 현재 사망시 출력될 사망 원인
	public int? currentEncounterRerollCount;    //현재 남은 이벤트 리롤
	public List<int> currentRevealedAreaIndexList;  //이번 회차 밝혀진 구역
	public Dictionary<ENUM_PLAYER_STATISTICS_TYPE, string> statistics;  // 서버에 로그를 전송하기 위한 통계 데이터 

	// 내부 데이터 조회시 null 오류를 방지 하기위한 생성자
	public PlayerInfo() {
		playerSkill = new Skill();
		starterPack = new StarterPack();
		portrait = new Portrait();
		equipmentStatusDict = new Dictionary<ENUM_EQUIPMENT_PART, Equipment>() {
			{ENUM_EQUIPMENT_PART.HEAD, null },
			{ENUM_EQUIPMENT_PART.SHIRT, null },
			{ENUM_EQUIPMENT_PART.PANTS, null },
			{ENUM_EQUIPMENT_PART.WEAPON, null },
			{ENUM_EQUIPMENT_PART.TRINKET, null },
			{ENUM_EQUIPMENT_PART.ETC, null }
		};
		cardDeckList = new List<Card>();
		cardBagList = new List<Card>();
		clearedAreaList = new List<Area>();
		unlockedEquipmentList = new List<Equipment>();
		unlockedSkillList = new List<Skill>();
		playedEventList = new List<StoryEvent>();
		queuedEventList = new List<StoryEvent>();
		selectedStorySelectionIndexList = new List<int>();
		currentEndingIndexList = new List<int>();
	}
}
