public class Enemy : IIndexableDTO
{
    public int Index { get; set; }
    public PrefabData prefab;
    public int hp;
    public IBattleEnemy enemyPattern;
    public int? characterIndex;
}
