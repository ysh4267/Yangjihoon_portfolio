using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Card : ICollectionDTO, ICard, ICardDBData, IDownloadableContent {
	public int Index { get; set; }              //DB 인덱스
	public string Name { get; set; }            //카드 이름
	public string Description { get; set; }     //카드 설명
	public bool isCharacterCard;                //플레이어가 사용하는 카드인지 여부
	public int cost;                            //카드 기본 코스트
	public Illustration Illust { get; set; }    //카드 일러스트 정보
	public string cardFactionString;            //카드 진영 (표기용
	public string cardTypeString;               //카드 속성
	public string cardTypeValueString;          //카드 속성 (표기용
	public IBattlePlayerCard battleCardScript;          //카드 작동 로직
	public ENUM_CARD_TYPE CardType { get; set; }        //카드 타입
	public ENUM_FACTION cardFactionEnum;                //카드 진영
	public ENUM_RARITY Rarity { get; set; }             //레어도
	public List<ENUM_CARD_PROPERTY> cardPropertyList;   //카드 분류 태그
	public List<int> buffIndexList;                     //해당 카드가 보유한 버프혹은 디버프목록
	public BattleCardValues battleCardValues;           //카드 설명 가변값 정리
	public bool isFixedInDeck { get; set; }             //고정 카드 여부
	public int temporaryID { get; set; }                //플레이중 부여된 카드 ID
	public bool isInDeck { get; set; }                  //카드 덱 포함 여부
	public ENUM_PRODUCT_LIST productID { get; set; }    //DLC 적용 여부
	public IRenderableData.ItemType datatype { get => IRenderableData.ItemType.card; set { } }  //게임 내 UI상 타입 분류

	public Card() {
		Illust = new Illustration();
		cardPropertyList = new List<ENUM_CARD_PROPERTY>();
		battleCardValues = new BattleCardValues();
	}
}
