using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDao {
    // 적 정보 조회
    public static Enemy GetEnemy(int enemyIndex) {
        string query =
            $"SELECT " +
           $"{DataBaseTableDefine.EnemyTable}.hp AS 'hp', " +
           $"{DataBaseTableDefine.EnemyTable}.enemy_script AS 'enemy_script', " +
           $"{DataBaseTableDefine.PrefabDataTable}.prefab_path AS 'prefab_path', " +
           $"{DataBaseTableDefine.EnemyTable}.character_index AS 'character_index' " +
           $"FROM {DataBaseTableDefine.EnemyTable} " +
           $"LEFT JOIN {DataBaseTableDefine.PrefabDataTable} " +
           $"ON {DataBaseTableDefine.EnemyTable}.prefab_data_index = {DataBaseTableDefine.PrefabDataTable}.prefab_data_index " +
           $"WHERE {DataBaseTableDefine.EnemyTable}.enemy_index = {enemyIndex} ";

        CustomDataReader it = SQLiteManager.SelectQuery(query);

        if (false == it.Read()) {
            return default;
        }

        Enemy enemy = new Enemy();

        //hp&name
        enemy.Index = enemyIndex;
        enemy.hp = it.GetSafeValue<int>(0);
        enemy.enemyPattern = (IBattleEnemy)Activator.CreateInstance(Type.GetType(it.GetSafeValue<string>(1)));
        
        //prefab
        PrefabData prefab = new PrefabData();
        prefab.path = it.GetSafeValue<string>(2);
        enemy.prefab = prefab;
        enemy.characterIndex = it.GetSafeValue<int?>(3);


        return enemy;
    }
}
