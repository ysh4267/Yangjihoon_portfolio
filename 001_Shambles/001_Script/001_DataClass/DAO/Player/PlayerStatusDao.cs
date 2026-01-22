using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusDao 
{
    // 플레이어 상태 정보 조회
    public static PlayerStatus GetPlayerStatus(int characterIndex=1) {
        string query =
        $"SELECT " +
        $"{DataBaseTableDefine.PlayerStatusTable}.name AS 'name', " +
        $"{DataBaseTableDefine.PlayerStatusTable}.gold AS 'gold', " +
        $"{DataBaseTableDefine.PlayerStatusTable}.level AS 'level', " +
        $"{DataBaseTableDefine.PlayerStatusTable}.exp AS 'exp', " +
        $"{DataBaseTableDefine.PlayerStatusTable}.skill_point AS 'skill_point', " +
        $"{DataBaseTableDefine.PlayerStatusTable}.current_hp AS 'current_hp' " +
        $"FROM {DataBaseTableDefine.PlayerStatusTable} WHERE {DataBaseTableDefine.PlayerStatusTable}.player_index = {characterIndex}";

        CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

        if (false == it.Read()) {
            return default;
        }
        PlayerStatus status = new PlayerStatus();
        status.name = it.GetSafeValue<string>(0);
        status.gold = it.GetSafeValue<int>(1);
        status.level = it.GetSafeValue<int>(2);
        status.exp = it.GetSafeValue<int>(3);
        status.skillPoint = it.GetSafeValue<int>(4);
        status.currentHp = it.GetSafeValue<int>(5);

        return status;
    }


}
