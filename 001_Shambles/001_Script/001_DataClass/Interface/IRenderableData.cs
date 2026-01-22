using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 해당 항목이 인 게임 UI 상에서 표기될때 필요한 데이터
public interface IRenderableData : IIndexableDTO {
	enum ItemType { card, skill, equipment, starterPack, portrait, buff }
	public ItemType datatype { get; set; }
	public Illustration Illust { get; set; }
	public string Description { get; set; }
	public string Name { get; set; }

}
