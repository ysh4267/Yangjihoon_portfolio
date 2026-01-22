public class Enemy : IIndexableDTO {
	public int Index { get; set; }      // DB 인덱스
	public PrefabData prefab;           // 프리펩 에셋 정보
	public int hp;                      // 최대 체력
	public IBattleEnemy enemyPattern;   // 적 패턴
	public int? characterIndex;         // 연결된 도감 인덱스
}
