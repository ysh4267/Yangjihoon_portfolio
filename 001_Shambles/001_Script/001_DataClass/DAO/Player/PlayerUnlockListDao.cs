using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUnlockListDao
{
    // 플레이어 해금 리스트 조회
    public static PlayerUnlockList GetPlayerUnlockList(int characterIndex = 1)
    {
        string query =
           $"SELECT " +
           $"{DataBaseTableDefine.PlayerUnlockListTable}.player_index, " +
           $"{DataBaseTableDefine.PlayerUnlockListTable}.cleared_area_list, " +
           $"{DataBaseTableDefine.PlayerUnlockListTable}.unlocked_equipment_list, " +
           $"{DataBaseTableDefine.PlayerUnlockListTable}.unlocked_active_skill_list, " +
           $"{DataBaseTableDefine.PlayerUnlockListTable}.unlocked_passive_skill_list, " +
           $"{DataBaseTableDefine.PlayerUnlockListTable}.unlocked_event_list, " +
           $"{DataBaseTableDefine.PlayerUnlockListTable}.new_unlocked_journal_list, " +
           $"{DataBaseTableDefine.PlayerUnlockListTable}.new_unlocked_character_list " +
           $"FROM {DataBaseTableDefine.PlayerUnlockListTable}";

        CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

        if (false == it.Read())
        {
            return default;
        }

        PlayerUnlockList characterUnlockList = new PlayerUnlockList();

        characterUnlockList.clearedAreaList = it.GetTextValueToIntList(1);
        characterUnlockList.unlockedEquipmentList = it.GetTextValueToIntList(2);
        characterUnlockList.unlockedActiveSkillList = it.GetTextValueToIntList(3);
        characterUnlockList.unlockedPassiveSkillList = it.GetTextValueToIntList(4);
        characterUnlockList.playedStoryEventList = it.GetTextValueToIntList(5);
        characterUnlockList.newUnlockedJournalList = it.GetTextValueToIntList(6);
        characterUnlockList.newUnlockedCharacterList = it.GetTextValueToIntList(7);

        return characterUnlockList;
    }
    // 해금된 모든 스킬 HashSet 반환 (액티브 + 패시브)
    public static HashSet<int> GetEveryUnlockedSkillHashSet(int characterIndex = 1)
    {// Includes Active & Passive
        string query =
            $"SELECT " +
            $"{DataBaseTableDefine.PlayerUnlockListTable}.unlocked_active_skill_list AS 'unlocked_active_skill_list', " +
            $"{DataBaseTableDefine.PlayerUnlockListTable}.unlocked_passive_skill_list AS 'unlocked_passive_skill_list' " +
            $"FROM {DataBaseTableDefine.PlayerUnlockListTable}";

        CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

        if (false == it.Read())
        {
            return default;
        }
        HashSet<int> hashSet = new HashSet<int>();

        foreach (int item in it.GetTextValueToIntList(0))
        {
            hashSet.Add(item);
        }
        foreach (int item in it.GetTextValueToIntList(1))
        {
            hashSet.Add(item);
        }

        return hashSet;
    }
    // 해금된 액티브 스킬 리스트 반환
    public static List<int> GetUnlockedActiveSkillList()
    {
        string query =
           $"SELECT " +
           $"{DataBaseTableDefine.PlayerUnlockListTable}.unlocked_active_skill_list AS 'unlocked_active_skill_list' " +
           $"FROM {DataBaseTableDefine.PlayerUnlockListTable}";

        CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

        if (false == it.Read())
        {
            return default;
        }
        List<int> skillList = new List<int>();
        foreach (int item in it.GetTextValueToIntList(0))
        {
            skillList.Add(item);
        }
        return skillList;
    }
    // 해금된 패시브 스킬 리스트 반환
    public static List<int> GetUnlockedPassiveSkillList()
    {
        string query =
           $"SELECT " +
           $"{DataBaseTableDefine.PlayerUnlockListTable}.unlocked_passive_skill_list AS 'unlocked_passive_skill_list' " +
           $"FROM {DataBaseTableDefine.PlayerUnlockListTable}";

        CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

        if (false == it.Read())
        {
            return default;
        }
        List<int> skillList = new List<int>();
        foreach (int item in it.GetTextValueToIntList(0))
        {
            skillList.Add(item);
        }
        return skillList;
    }
}
