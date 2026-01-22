using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System;
using System.Runtime.CompilerServices;

public class CardDao : CollectionDao {
	/// <summary>
	/// BattleCardScript 객체를 포함하고 있지 않은 순수 UI 출력 용도의 카드 객체를 반환합니다.
	/// </summary>
	public static Card GetCard(int cardIndex, bool isFixed = false) {
		string query =
			$"SELECT " +
			$"{DataBaseTableDefine.CardTable}.card_index AS 'card_index', " +
			$"{DataBaseTableDefine.CardTable}.cost AS 'cost', " +
			$"{DataBaseTableDefine.CardTable}.illust_index AS 'illust_index', " +
			$"{DataBaseTableDefine.CardFactionTable}.{SettingManager.CurrentLanguageString} AS 'faction', " +
			$"{DataBaseTableDefine.CardTypeTable}.{SettingManager.CurrentLanguageString} AS 'type', " +
			$"{DataBaseTableDefine.CardNameTable}.{SettingManager.CurrentLanguageString} AS 'name', " +
			$"{DataBaseTableDefine.CardDescribeTable}.{SettingManager.CurrentLanguageString} AS 'describe', " +
			$"{DataBaseTableDefine.CardTable}.card_faction_index AS 'faction_index', " +
			$"{DataBaseTableDefine.CardTable}.card_type_value AS 'card_value', " +
			$"{DataBaseTableDefine.CardTable}.card_type_index AS 'card_type_index', " +
			$"{DataBaseTableDefine.CardTable}.card_buff_index_list AS 'card_buff_index_list', " +
			$"{DataBaseTableDefine.CardTable}.card_property_list AS 'card_property_list', " +
			$"{DataBaseTableDefine.CardTable}.card_rarity AS 'card_rarity', " +
			$"{DataBaseTableDefine.CardTable}.is_character_card AS 'is_character_card', " +
			$"{DataBaseTableDefine.CardTable}.product_id " +
			$"FROM {DataBaseTableDefine.CardTable} " +
			$"LEFT JOIN {DataBaseTableDefine.CardFactionTable} " +
			$"ON {DataBaseTableDefine.CardTable}.card_faction_index = {DataBaseTableDefine.CardFactionTable}.card_faction_index " +
			$"LEFT JOIN {DataBaseTableDefine.CardTypeTable} " +
			$"ON {DataBaseTableDefine.CardTable}.card_type_index = {DataBaseTableDefine.CardTypeTable}.card_type_index " +
			$"LEFT JOIN {DataBaseTableDefine.CardNameTable} " +
			$"ON {DataBaseTableDefine.CardTable}.card_index = {DataBaseTableDefine.CardNameTable}.card_index " +
			$"LEFT JOIN {DataBaseTableDefine.CardDescribeTable} " +
			$"ON {DataBaseTableDefine.CardTable}.card_index = {DataBaseTableDefine.CardDescribeTable}.card_index " +
			$"WHERE {DataBaseTableDefine.CardTable}.card_index = {cardIndex}";

		CustomDataReader it = SQLiteManager.SelectQuery(query);

		if (false == it.Read()) {
			return GetCard(620);
		}

		Card card = new Card();
		card.Index = it.GetSafeValue<int>(0);
		card.cost = it.GetSafeValue<int>(1);
		card.Illust = IllustrationDao.GetIllust(it.GetSafeValue<int>(2));
		card.cardFactionString = it.GetSafeValue<string>(3);
		card.cardTypeString = it.GetSafeValue<string>(4);
		card.Name = it.GetSafeValue<string>(5);
		card.Description = it.GetSafeValue<string>(6);
		card.cardFactionEnum = (ENUM_FACTION)it.GetSafeValue<int>(7);
		card.cardTypeValueString = it.GetSafeValue<string>(8) ?? "0";
		card.CardType = (ENUM_CARD_TYPE)it.GetSafeValue<int>(9);
		card.buffIndexList = it.GetTextValueToIntList(10);
		card.cardPropertyList = JsonParser.JsonToObject<List<ENUM_CARD_PROPERTY>>(it.GetSafeValue<string>(11)) ?? new List<ENUM_CARD_PROPERTY>();
		card.Rarity = it.GetEnumFromString<ENUM_RARITY>(12, ENUM_RARITY.COMMON);
		card.isCharacterCard = !(0 == it.GetSafeValue<int>(13));
		card.productID = JsonParser.JsonToObject<ENUM_PRODUCT_LIST>(it.GetSafeValue<string>(14));
		card.isFixedInDeck = isFixed;
		return card;
	}

	// 모든 카드 데이터를 가져와 리스트로 반환
	public static List<Card> GetAllCards() {
		string query =
			$"SELECT " +
			$"{DataBaseTableDefine.CardTable}.card_index AS 'card_index', " +
			$"{DataBaseTableDefine.CardTable}.cost AS 'cost', " +
			$"{DataBaseTableDefine.CardTable}.illust_index AS 'illust_index', " +
			$"{DataBaseTableDefine.CardFactionTable}.{SettingManager.CurrentLanguageString} AS 'faction', " +
			$"{DataBaseTableDefine.CardTypeTable}.{SettingManager.CurrentLanguageString} AS 'type', " +
			$"{DataBaseTableDefine.CardNameTable}.{SettingManager.CurrentLanguageString} AS 'name', " +
			$"{DataBaseTableDefine.CardDescribeTable}.{SettingManager.CurrentLanguageString} AS 'describe', " +
			$"{DataBaseTableDefine.CardTable}.card_faction_index AS 'faction_index', " +
			$"{DataBaseTableDefine.CardTable}.card_type_value AS 'card_value', " +
			$"{DataBaseTableDefine.CardTable}.card_type_index AS 'card_type_index', " +
			$"{DataBaseTableDefine.CardTable}.card_buff_index_list AS 'card_buff_index_list', " +
			$"{DataBaseTableDefine.CardTable}.card_property_list AS 'card_property_list', " +
			$"{DataBaseTableDefine.CardTable}.card_rarity AS 'card_rarity', " +
			$"{DataBaseTableDefine.CardTable}.is_character_card AS 'is_character_card', " +
			$"{DataBaseTableDefine.CardTable}.product_id " +
			$"FROM {DataBaseTableDefine.CardTable} " +
			$"LEFT JOIN {DataBaseTableDefine.CardFactionTable} " +
			$"ON {DataBaseTableDefine.CardTable}.card_faction_index = {DataBaseTableDefine.CardFactionTable}.card_faction_index " +
			$"LEFT JOIN {DataBaseTableDefine.CardTypeTable} " +
			$"ON {DataBaseTableDefine.CardTable}.card_type_index = {DataBaseTableDefine.CardTypeTable}.card_type_index " +
			$"LEFT JOIN {DataBaseTableDefine.CardNameTable} " +
			$"ON {DataBaseTableDefine.CardTable}.card_index = {DataBaseTableDefine.CardNameTable}.card_index " +
			$"LEFT JOIN {DataBaseTableDefine.CardDescribeTable} " +
			$"ON {DataBaseTableDefine.CardTable}.card_index = {DataBaseTableDefine.CardDescribeTable}.card_index";

		CustomDataReader it = SQLiteManager.SelectQuery(query);

		var cards = new List<Card>();

		while (it.Read()) {
			Card card = new Card();
			card.Index = it.GetSafeValue<int>(0);
			card.cost = it.GetSafeValue<int>(1);
			card.Illust = IllustrationDao.GetIllust(it.GetSafeValue<int>(2));
			card.cardFactionString = it.GetSafeValue<string>(3);
			card.cardTypeString = it.GetSafeValue<string>(4);
			card.Name = it.GetSafeValue<string>(5);
			card.Description = it.GetSafeValue<string>(6);
			card.cardFactionEnum = (ENUM_FACTION)it.GetSafeValue<int>(7);
			card.cardTypeValueString = it.GetSafeValue<string>(8) ?? "0";
			card.CardType = (ENUM_CARD_TYPE)it.GetSafeValue<int>(9);
			card.buffIndexList = it.GetTextValueToIntList(10);
			card.cardPropertyList = JsonParser.JsonToObject<List<ENUM_CARD_PROPERTY>>(it.GetSafeValue<string>(11)) ?? new List<ENUM_CARD_PROPERTY>();
			card.Rarity = it.GetEnumFromString<ENUM_RARITY>(12, ENUM_RARITY.COMMON);
			card.isCharacterCard = !(0 == it.GetSafeValue<int>(13));
			card.productID = JsonParser.JsonToObject<ENUM_PRODUCT_LIST>(it.GetSafeValue<string>(14));
			cards.Add(card);
		}

		return cards;
	}


	/// <summary>
	/// BattleCardScript 객체를 포함하는 전투에서 사용할 카드 객체를 반환합니다.
	/// </summary>
	public static Card GetBattleCard(int cardIndex) {
		string query =
			$"SELECT " +
			$"{DataBaseTableDefine.CardTable}.card_script AS 'card_script' " +
			$"FROM {DataBaseTableDefine.CardTable} " +
			$"WHERE {DataBaseTableDefine.CardTable}.card_index = {cardIndex}";

		CustomDataReader it = SQLiteManager.SelectQuery(query);

		if (false == it.Read()) {
			Debug.Log("Failed");
			Debug.Log(cardIndex);
			return GetBattleCard(620);
		}

		Card card = GetCard(cardIndex);
		try {
			card.battleCardScript = (IBattlePlayerCard)Activator.CreateInstance(Type.GetType(it.GetSafeValue<string>(0)));//... 카드 효과 스크립트 삽입
		}
		catch {
			Debug.LogWarning($"Warning {it.GetSafeValue<string>(0)} is invaild script");
			card.battleCardScript = (IBattlePlayerCard)new Card_001_Jab();//... 카드 효과 스크립트 삽입
		}
		card.battleCardScript.FactionEnum = card.cardFactionEnum;
		card.battleCardScript.cardDestination = card.buffIndexList.Contains((int)ENUM_BUFF_INDEX.CONSUMABLE) || card.buffIndexList.Contains((int)ENUM_BUFF_INDEX.ONE_OFF)
												? ENUM_CARD_DESTINATION.DESTROY : ENUM_CARD_DESTINATION.TO_GRAVEYARD;
		card.battleCardScript.Cost = new BattleCost(card.cost);
		card.battleCardScript.battleCardValues = card.Description.ParseBattleCardDescriptionValues();
		card.battleCardScript.Equipments = new List<Equipment>();
		return card;
	}

	// 특정 속성을 가진 배틀 카드 리스트 반환
	public static List<Card> GetBattleCardsByProperty(ENUM_CARD_PROPERTY property) {
		string query =
			$"SELECT " +
			$"{DataBaseTableDefine.CardTable}.card_index AS 'card_index' " +
			$"FROM {DataBaseTableDefine.CardTable} " +
			$"WHERE {DataBaseTableDefine.CardTable}.card_property_list LIKE '%{property}%'";

		CustomDataReader it = SQLiteManager.SelectQuery(query);

		var cardList = new List<Card>();
		if (false == it.Read()) {
			return cardList;
		}

		do {
			cardList.Add(GetBattleCard(it.GetSafeValue<int>(0)));
		} while (it.Read());

		return cardList;
	}

	// 카드 인덱스 리스트를 카드 객체 리스트로 변환
	public static List<Card> PharseCardIndexListToObjectList(List<int> cardIndexList) {
		if (cardIndexList.HasValue() == false) return null;
		List<Card> cardList = new List<Card>();
		if (cardIndexList.Count != 0) {
			foreach (var item in cardIndexList) {
				cardList.Add(GetCard(item));
			}
		}
		return cardList;
	}

	// 해금된 카드 리스트 반환
	public static List<Card> GetUnlockedCardList() {
		string query =
			$"SELECT " +
			$"{DataBaseTableDefine.UnlockedCardTable}.card_index AS 'card_index' " +
			$"FROM {DataBaseTableDefine.UnlockedCardTable} " +
			$"WHERE {DataBaseTableDefine.UnlockedCardTable}.is_unlocked LIKE 'true'";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);

		if (false == it.Read()) {
			return default;
		}

		List<Card> cardList = new List<Card>();
		do {
			var index = it.GetSafeValue<int>(0);
			Card card = GetCard(index);
			if (card != null && card.Index != 0)
				cardList.Add(card);
		} while (true == it.Read());

		return cardList;
	}

	// 카드 타입 텍스트 반환
	public static string GetCardTypeText(ENUM_CARD_TYPE cardType) {
		string query =
			$"SELECT " +
			$"{DataBaseTableDefine.CardTypeTable}.{SettingManager.CurrentLanguageString} AS 'card_type' " +
			$"FROM {DataBaseTableDefine.CardTypeTable} " +
			$"WHERE {DataBaseTableDefine.CardTypeTable}.card_type_index = {(int)cardType} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.GAME_DATA);

		if (false == it.Read()) {
			return default;
		}

		return it.GetSafeValue<string>(0);
	}

	// 선택된 카드 프리셋의 카드 인덱스 리스트 반환
	public static List<int> GetSelectedCardListPresetIndexData(int? cardListPresetIndex) {
		if (cardListPresetIndex == null) return null;
		string query = $"SELECT " +
			$"{DataBaseTableDefine.CardListPresetTable}.card_list_preset_index, " +
			$"{DataBaseTableDefine.CardListPresetTable}.card_list " +
			$"FROM {DataBaseTableDefine.CardListPresetTable} " +
			$"WHERE {DataBaseTableDefine.CardListPresetTable}.card_list_preset_index = {cardListPresetIndex} ";

		CustomDataReader it = SQLiteManager.SelectQuery(query);

		if (false == it.Read()) {
			return null;
		}

		int presetIndex = it.GetSafeValue<int>(0);
		List<int> cardIndexList = it.GetTextValueToIntList(1);

		return cardIndexList;
	}

	// CollectionDao 부모클래스 메소드들 랩핑
	// 해금된 카드 인덱스 리스트 반환
	public static List<int> GetUnlockedIndexList() => GetUnlockedIndexList<Card>();
	// 전체 카드 개수 반환
	public static int GetTotalCount() => GetTotalCount(DataBaseTableDefine.CardTable);
	// 해금된 카드 개수 반환
	public static int GetUnlockedCount() => GetUnlockedCount<Card>();
	// 확인하지 않은 카드 개수 반환
	public static int GetUncheckedCount() => GetUncheckedCount<Card>();
	// 카드 해금 처리
	public static bool Unlock(int _index, bool isUnlock = true) => Unlock<Card>(_index, isUnlock);
	// 카드 확인 처리
	public static void Check(int _index, bool isChecked = false) => Check<Card>(_index, isChecked);
	// 카드 해금 여부 확인
	public static bool IsUnlocked(int _index) => IsUnlocked<Card>(_index);
	// 카드 확인 여부 확인
	public static bool IsChecked(int _index) => IsChecked<Card>(_index);
	// 카드 존재 여부 확인
	public static bool IsExist(int _index) => IsExist<Card>(_index);
}
