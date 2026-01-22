using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipDao {
    // 플레이어 장비 정보 조회
    public static PlayerEquip GetPlayerEquipInfo(int characterIndex = 1) {
        string query = $"SELECT * FROM {DataBaseTableDefine.PlayerEquipTable} WHERE {DataBaseTableDefine.PlayerEquipTable}.player_index = {characterIndex}";

        CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

        if (false == it.Read()) {
            return default;
        }

        PlayerEquip charEquip = new PlayerEquip();

        int equipPartCount = System.Enum.GetValues(typeof(ENUM_EQUIPMENT_PART)).Length;

        for (int i = 0; i < equipPartCount; i++) {
            charEquip.currentEquipments[i] = it.GetSafeValue<int?>(i+1);
        }

        return charEquip;
    }
}
