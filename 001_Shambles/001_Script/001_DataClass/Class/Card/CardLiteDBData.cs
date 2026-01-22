using System;

// 대량의 카드목록을 불러올때를 위한 Index 정보만을 포함하는 데이터타입
public class CardLiteDBData : ICloneable, ICardDBData, IIndexableDTO {
	public int Index { get; set; }          //DB상 분류
	public int temporaryID { get; set; }    //플레이중 부여되는 임시 ID
	public bool isFixedInDeck { get; set; } //덱에 고정된 카드여부
	public bool isInDeck { get; set; }      //덱에 포함되는지 여부

	public CardLiteDBData(int _temporaryId, int _cardIndex, bool _isFixedInDeck, bool _isInDeck) {
		temporaryID = _temporaryId;
		Index = _cardIndex;
		isInDeck = _isInDeck;
		isFixedInDeck = _isFixedInDeck;
	}

	public object Clone() {
		return new CardLiteDBData(temporaryID, Index, isInDeck, isFixedInDeck);
	}
}
