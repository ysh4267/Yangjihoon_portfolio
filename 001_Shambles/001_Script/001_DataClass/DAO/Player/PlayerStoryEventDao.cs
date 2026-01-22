using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStoryEventDao {
    // 플레이어 스토리 이벤트 정보 조회
    public static PlayerStoryEvent GetPlayerStoryEvent(int index) {
        string query =
        $"SELECT " +
        $"{DataBaseTableDefine.PlayerStoryEventTable}.player_index AS 'player_index', " +
        $"{DataBaseTableDefine.PlayerStoryEventTable}.story_event_index_queue_list AS 'story_event_index_queue_list', " +
        $"{DataBaseTableDefine.PlayerStoryEventTable}.current_area_index AS 'current_area_index', " +
        $"{DataBaseTableDefine.PlayerStoryEventTable}.current_event_index AS 'current_event_index', " +
        $"{DataBaseTableDefine.PlayerStoryEventTable}.current_battle_index AS 'current_battle_index', " +
        $"{DataBaseTableDefine.PlayerStoryEventTable}.story_selection_index_list AS 'story_selection_index_list', " +
        $"{DataBaseTableDefine.PlayerStoryEventTable}.current_event_count AS 'current_event_count', " +
        $"{DataBaseTableDefine.PlayerStoryEventTable}.current_ending_index_list AS 'current_ending_index_list', " +
        $"{DataBaseTableDefine.PlayerStoryEventTable}.current_dead_ending_index AS 'current_dead_ending_index', " +
        $"{DataBaseTableDefine.PlayerStoryEventTable}.current_dead_ending_sentence_index AS 'current_dead_ending_sentence_index', " +
        $"{DataBaseTableDefine.PlayerStoryEventTable}.current_revealed_area_list AS 'current_revealed_area_list', " +
        $"{DataBaseTableDefine.PlayerStoryEventTable}.current_encounter_reroll_count AS 'current_encounter_reroll_count' " +
        $"FROM {DataBaseTableDefine.PlayerStoryEventTable} WHERE {DataBaseTableDefine.PlayerStoryEventTable}.player_index = {index}";

        CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
        if (false == it.Read()) {
            return default;
        }

        PlayerStoryEvent eventData = new PlayerStoryEvent();

        List<int> storyEventIndexList = it.GetTextValueToIntList(1);
        if (storyEventIndexList.Count > 0) {
            foreach (var item in storyEventIndexList) {
                eventData.storyEventIndexQueueList.Add(StoryEventDao.GetStoryEvent(item));
            }
        }
        eventData.currentAreaIndex = it.GetSafeValue<int?>(2);
        eventData.currentEventIndex = it.GetSafeValue<int?>(3);
        eventData.currentBattleIndex = it.GetSafeValue<int?>(4);
        eventData.selectedStorySelectionIndexList = it.GetTextValueToIntList(5);
        eventData.currentEventCount = it.GetSafeValue<int?>(6);
        eventData.currentEndingIndexList = it.GetTextValueToIntList(7);
        eventData.currentDeadEndingIndex = it.GetSafeValue<int?>(8);
        eventData.currentDeadEndingSentenceIndex = it.GetSafeValue<int?>(9);
        eventData.currentRevealedAreaIndexList = it.GetTextValueToIntList(10);
        eventData.currentEncounterRerollCount = it.GetSafeValue<int?>(11);

        return eventData;
    }
}
