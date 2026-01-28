/// <summary>
/// 전투 중 발생하는 피해 정보를 캡슐화하는 데이터 클래스
/// 피해량, 피해 유형, 공격자, 세력 정보를 포함
/// </summary>
public class BattleDamage {
	public int damage { get; private set; }
	public ENUM_DAMAGE_TYPE damageType { get; set; }
	public ENUM_DAMAGE_SOURCE_TYPE damageSourceType { get; set; }
	public IBattleStatus attacker { get; set; }
	public ENUM_FACTION faction { get; set; }

	public BattleDamage(int _damage, ENUM_DAMAGE_TYPE _type = ENUM_DAMAGE_TYPE.NORMAL, ENUM_DAMAGE_SOURCE_TYPE _damageSourceType = ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK, IBattleStatus _attacker = null, ENUM_FACTION _faction = ENUM_FACTION.NORMAL) {
		if (_damageSourceType == ENUM_DAMAGE_SOURCE_TYPE.DIRECT_ATTACK && _attacker == null)
			UnityEngine.Debug.LogWarning("Attacker must be set when damage source type is DIRECT");

		damage = _damage;
		damageType = _type;
		damageSourceType = _damageSourceType;
		attacker = _attacker;
		faction = _faction;
	}

	public BattleDamage(BattleDamage obj) {
		damage = obj.damage;
		damageType = obj.damageType;
		damageSourceType = obj.damageSourceType;
		attacker = obj.attacker;
		faction = obj.faction;
	}

	public void SetDamageValue(int damage) {
		this.damage = damage;
	}

	public static BattleDamage operator +(int a, BattleDamage b) {
		b.damage += a;
		return b;
	}

	public static BattleDamage operator *(float a, BattleDamage b) {
		b.damage = (int)(b.damage * a);
		return b;
	}
}
