using System;

public class CardLiteDBData : ICloneable, ICardDBData, IIndexableDTO {
    public int temporaryID { get; set; }
    public bool isFixedInDeck { get; set; }
    public bool isInDeck { get; set; }
    public int Index { get; set; }

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
