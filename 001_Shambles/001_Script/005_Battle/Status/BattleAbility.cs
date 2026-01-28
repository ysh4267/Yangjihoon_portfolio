public struct BattleAbility {
    public int amount { get; set; }
    public ENUM_PLAYER_ABILITY_TYPE abilityType { get; set; }
    
    public BattleAbility(int _amount, ENUM_PLAYER_ABILITY_TYPE _abilityType) {
        amount = _amount;
        abilityType = _abilityType;
    }
}
