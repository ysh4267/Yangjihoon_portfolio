using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo {
    public int playerInfoIndex;
    public Skill playerSkill;
    public StarterPack starterPack;
    public Dictionary<ENUM_EQUIPMENT_PART, Equipment> equipmentStatusDict;
    public Portrait portrait;
    public string name;
    public int statHp;
    public int statStr;
    public int statInt;
    public int statDex;
    public int statAp;
    public int statExtraDraw;
    public int gold;
    public int level;
    public int exp;
    public int maxHp;
    public int currentHp;
    public int maxMp;
    public int currentMp;
    public int skillPoint;
    public List<Card> cardDeckList;
    public List<Card> cardBagList;
    public List<Area> clearedAreaList;
    public List<Equipment> unlockedEquipmentList;
    public List<Skill> unlockedSkillList;
    public List<StoryEvent> playedEventList;
    public List<int> newUnlockedJournalIndexList;
    public List<int> newUnlockedCharacterIndexList;
    public List<StoryEvent> queuedEventList;
    public int? currentAreaIndex;
    public int? currentEventIndex;
    public int? currentBattleIndex;
    public int? currentEventCount;
    public List<int> selectedStorySelectionIndexList;
    public List<int> currentEndingIndexList;
    public int? currentDeadEndingIndex;
    public int? currentSceneIndex;
    public int? seedValue;
    public int? currentMapIndex;
    public List<int> currentRevealedAreaIndexList;
    public int? currentEncounterRerollCount;
    public Dictionary<ENUM_PLAYER_STATISTICS_TYPE, string> statistics;

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

    public static PlayerInfo Clone() {
        PlayerInfo clone = new PlayerInfo();

        clone.playerSkill = new Skill();
        clone.starterPack = StarterPackDao.GetStarterPack(1);
        clone.portrait = PortraitDao.GetPortraitInfo(1);
        clone.name = "이름은최대여덟자";
        clone.statHp = 10;
        clone.statStr = 10;
        clone.statInt = 10;
        clone.statDex = 10;
        clone.gold = 100;
        clone.level = 1;
        clone.exp = 0;
        clone.maxHp = 100;
        clone.currentHp = 100;
        clone.maxMp = 100;
        clone.currentMp = 100;
        clone.skillPoint = 0;
        clone.currentHp = 5;
        clone.currentAreaIndex = 0;
        clone.equipmentStatusDict = new Dictionary<ENUM_EQUIPMENT_PART, Equipment>() {
            {ENUM_EQUIPMENT_PART.HEAD, null },
            {ENUM_EQUIPMENT_PART.SHIRT, null },
            {ENUM_EQUIPMENT_PART.PANTS, null },
            {ENUM_EQUIPMENT_PART.WEAPON, null },
            {ENUM_EQUIPMENT_PART.TRINKET, null },
            {ENUM_EQUIPMENT_PART.ETC, null }
        };
        clone.cardDeckList = new List<Card>();
        clone.cardBagList = new List<Card>();
        clone.clearedAreaList = new List<Area>();
        clone.unlockedEquipmentList = new List<Equipment>();
        clone.unlockedSkillList = new List<Skill>();
        clone.playedEventList = new List<StoryEvent>();
        clone.queuedEventList = new List<StoryEvent>();
        clone.selectedStorySelectionIndexList = new List<int>();
        clone.currentEndingIndexList = new List<int>();
		clone.currentEncounterRerollCount = 0;
        return clone;
    }
}
