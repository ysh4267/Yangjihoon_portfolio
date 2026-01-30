[System.Flags] // 비트 필드로 구현시 모든 열거형의 각자 또는 합의 경우의 수는 겹치지 않고 존재 ex) 14의 열거형 값은 반드시 적1, 2, 3의 합이며 역도 성립
public enum ENUM_BATTLE_PHASE_TARGET
{
    PLAYER  = 1, // 0b_0000_0001
    ENEMY1  = 2, // 0b_0000_0010
    ENEMY2  = 4, // 0b_0000_0100
    ENEMY3  = 8, // 0b_0000_1000
    ALL_ENEMIES   = ENEMY1 | ENEMY2 | ENEMY3, // 14, all enemy, 0b_0000_1110
    ALL     = PLAYER | ENEMY1 | ENEMY2 | ENEMY3, // 15, all characters, 0b_0000_1111
    NONE         // 16, 0b_0001_0000
}
