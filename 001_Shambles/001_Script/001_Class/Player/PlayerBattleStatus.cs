using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleStatus
{
    public int[] status = new int[System.Enum.GetValues(typeof(ENUM_STATUS)).Length];
    public int max_hp;
    public int current_hp;
    public int max_ap;
    public int current_ap;
    public int extra_draw;

    public PlayerBattleStatus()
    {
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
