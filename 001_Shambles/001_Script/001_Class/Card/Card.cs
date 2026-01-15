using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Card : ICollectionDTO, ICard, ICardDBData, IDownloadableContent {
    public int Index { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public IRenderableData.ItemType datatype { get => IRenderableData.ItemType.card; set { } }
    public bool isCharacterCard;
    public int cost;
    public Illustration Illust { get; set; }
    public string cardFactionString;
    public string cardTypeString;
    public string cardTypeValueString;
    public IBattlePlayerCard battleCardScript;
    public ENUM_CARD_TYPE CardType { get; set; }
    public ENUM_FACTION cardFactionEnum;
    public ENUM_RARITY Rarity { get; set; }
    public List<ENUM_CARD_PROPERTY> cardPropertyList;
    public List<int> buffIndexList;
    public BattleCardValues battleCardValues;

    public bool isFixedInDeck { get; set; }

    public int temporaryID { get; set; }
    public bool isInDeck { get; set; }
	public ENUM_PRODUCT_LIST productID { get; set; }

	public Card() {
        Illust = new Illustration();
        cardPropertyList = new List<ENUM_CARD_PROPERTY>();
        battleCardValues = new BattleCardValues();
    }
}
