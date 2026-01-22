using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 카드 자체정보가 아닌 매 플레이시마다 변동되는 데이터
public interface ICardDBData {
	public int temporaryID { get; set; }
	public bool isFixedInDeck { get; set; }
	public bool isInDeck { get; set; }
}
