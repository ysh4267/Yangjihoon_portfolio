using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PlayerBattleStatusDao : MonoBehaviour {
    // 플레이어 전투 스탯 조회
    public static PlayerBattleStatus GetPlayerBattleStatus(int playerIndex = 1) {
        var query =
            $"SELECT current_hp " +
            $"FROM {DataBaseTableDefine.PlayerStatusTable} " +
            $"WHERE player_index = {playerIndex} ";

        var it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

        int currentHp = it.GetSafeValue<int>(0);
        string equipScriptQuery = string.Empty;
        query =
            $"SELECT SUM(hp), SUM(str), SUM(dex), SUM(int), SUM(ap), SUM(extra_draw) " +
            $"FROM (" +
            GetPortraitQuery() +
            GetEquippedQuery(out equipScriptQuery) +
            GetPassiveSkillsQuery();

        query = query.Substring(0, query.LastIndexOf("UNION ALL "));
        query += ")";

        it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.GAME_DATA);

        if (false == it.Read()) {
            return null;
        }

        PlayerBattleStatus status = new PlayerBattleStatus();
        status.status[(int)ENUM_STATUS.HP] = it.GetSafeValue<int>(0).GetPositive();
        status.status[(int)ENUM_STATUS.STR] = it.GetSafeValue<int>(1).GetPositive();
        status.status[(int)ENUM_STATUS.DEX] = it.GetSafeValue<int>(2).GetPositive();
        status.status[(int)ENUM_STATUS.INT] = it.GetSafeValue<int>(3).GetPositive();
        status.current_hp = currentHp.GetPositive();
        status.max_hp = (status.status[(int)ENUM_STATUS.HP] * (SettingManager.GetSettingData().difficulty == ENUM_DIFFICULTY.Traveler ? 10 : 5)).GetPositive(moreThanZero: true);
        status.current_ap = it.GetSafeValue<int>(4).GetPositive();
        status.max_ap = status.current_ap;
        status.extra_draw = it.GetSafeValue<int>(5).GetPositive();

        if (!equipScriptQuery.IsNullQueryString()) {
            it = SQLiteManager.SelectQuery(equipScriptQuery, ENUM_DATABASE_PATH.GAME_DATA);

            while (it.Read()) {
                if (it.GetSafeValue<string>(0).IsNullQueryString()) continue;

                var subject = Activator.CreateInstance(Type.GetType(it.GetSafeValue<string>(0)));
                if (subject is IStatusEquipment) status = (subject as IStatusEquipment).GetEffectStatus(status);
            }
        }

        status.max_hp = (status.status[(int)ENUM_STATUS.HP] * (SettingManager.GetSettingData().difficulty == ENUM_DIFFICULTY.Traveler ? 10 : 5)).GetPositive(moreThanZero: true);
        status.current_hp = currentHp.GetPositive();
        return status;

        // 초상화 쿼리 조회
        string GetPortraitQuery() {
            var query =
                $"SELECT portrait_index " +
                $"FROM {DataBaseTableDefine.PlayerInfoTable} " +
                $"WHERE player_index = {playerIndex} ";

            var it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

            if (false == it.Read()) {
                return string.Empty;
            }

            int portraitIndex = it.GetSafeValue<int>(0);

            if (portraitIndex == 0) {
                portraitIndex = 1;
            }

            return $"SELECT hp, str, dex, int, ap, extra_draw " +
                    $"FROM {DataBaseTableDefine.PortraitStatusTable} " +
                    $"WHERE portrait_index = {portraitIndex} " +
                    $"UNION ALL ";
        }

        // 패시브 스킬 쿼리 조회
        string GetPassiveSkillsQuery() {
            var query =
                $"SELECT unlocked_passive_skill_list " +
                $"FROM {DataBaseTableDefine.PlayerUnlockListTable} " +
                $"WHERE player_index = {playerIndex} ";

            var it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

            if (false == it.Read()) {
                return string.Empty;
            }

            List<int> passiveSkillList = it.GetTextValueToIntList(0);
            passiveSkillList.RemoveAll(x => x == 0);
            if (passiveSkillList.Count == 0)
                return string.Empty;

            return $"SELECT hp, str, dex, int, ap, extra_draw " +
                    $"FROM {DataBaseTableDefine.SkillPassiveTable} " +
                    $"WHERE {passiveSkillList.IntArrayToORString(columnName: "skill_index")} " +
                    $"UNION ALL ";
        }

        // 장착 아이템 쿼리 조회
        string GetEquippedQuery(out string equipScriptQuery) {
            equipScriptQuery = string.Empty;

            var query =
                $"SELECT * " +
                $"FROM {DataBaseTableDefine.PlayerEquipTable} " +
                $"WHERE player_index = {playerIndex} ";

            var it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

            if (false == it.Read()) {
                return string.Empty;
            }

            int[] equippedArray = { it.GetSafeValue<int>(1), it.GetSafeValue<int>(2), it.GetSafeValue<int>(3), it.GetSafeValue<int>(4), it.GetSafeValue<int>(5), it.GetSafeValue<int>(6) };
            equippedArray = equippedArray.Where(x => x != 0).ToArray();
            if (equippedArray.Count() == 0)
                return string.Empty;

            equipScriptQuery =
                $"SELECT equip_script " +
                $"FROM {DataBaseTableDefine.EquipmentTable} " +
                $"WHERE {equippedArray.IntArrayToORString("equip_index")}";

            return $"SELECT stat_hp, stat_str, stat_dex, stat_int, stat_ap, extra_draw " +
                    $"FROM {DataBaseTableDefine.EquipmentStatTable} " +
                    $"WHERE {equippedArray.IntArrayToORString(columnName: "equip_index")} " +
                    $"UNION ALL ";
        }
    }
}
