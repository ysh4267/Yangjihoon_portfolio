using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardListDao {
	/*
     ======================IMPORTANT=======================
     In case of adding new cards, generating temporaryID is necessary.
     Although, DO NOT allocate new temporaryID to exsisting cards.
     =======================================================
     */

	#region Deck Functions
	// 덱에 새 카드들 추가
	public static void AddNewCardsInDeck(List<int> cards) {
		if (cards.HasValue() == false) return;
		string query =
			$"INSERT INTO {DataBaseTableDefine.PlayerDeckTable} " +
			$"(card_index, is_in_deck) " +
			$"VALUES ";


		foreach (int index in cards) {
			query += $"('{index}', '{true.BooleanToString()}'), ";
		}

		query = query.Substring(0, query.Length - 2);

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	public static void AddNewCardsInDeck(List<int> cards, bool isFixedInDeck) {
		if (cards.HasValue() == false) return;
		string query =
			$"INSERT INTO {DataBaseTableDefine.PlayerDeckTable} " +
			$"(card_index, is_fixed_in_deck, is_in_deck) " +
			$"VALUES ";


		foreach (int index in cards) {
			query += $"('{index}', '{isFixedInDeck.BooleanToString()}', '{true.BooleanToString()}'), ";
		}

		query = query.Substring(0, query.Length - 2);

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	public static void AddNewCardsInDeck(List<Card> cards) {
		if (cards.HasValue() == false) return;
		string query =
			$"INSERT INTO {DataBaseTableDefine.PlayerDeckTable} " +
			$"(card_index, is_fixed_in_deck, is_in_deck) " +
			$"VALUES ";

		foreach (Card card in cards) {
			query += $"('{card.Index}', '{card.isFixedInDeck.BooleanToString()}', '{true.BooleanToString()}'), ";
		}

		query = query.Substring(0, query.Length - 2);

		var it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	public static void AddNewCardsInDeck(List<CardLiteDBData> cards) {
		if (cards.HasValue() == false) return;
		string query =
				$"INSERT INTO {DataBaseTableDefine.PlayerDeckTable} " +
				$"(temporary_id, card_index, is_fixed_in_deck, is_in_deck) " +
				$"VALUES ";

		foreach (CardLiteDBData card in cards) {
			query += $"({card.temporaryID}, '{card.Index}', '{card.isFixedInDeck.BooleanToString()}', '{true.BooleanToString()}'), ";
		}

		query = query.Substring(0, query.Length - 2);

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 덱에 새 카드 추가
	public static void AddNewCardInDeck(Card card) {
		if (card == null) return;
		string query =
			$"INSERT INTO {DataBaseTableDefine.PlayerDeckTable} " +
			$"(card_index, is_fixed_in_deck, is_in_deck) " +
			$"VALUES " +
			$"('{card.Index}', '{card.isFixedInDeck.BooleanToString()}', '{true.BooleanToString()}')";
		var it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 플레이어 덱 리스트 반환
	public static List<Card> GetPlayerDeckList(int playerIndex = 1, bool isBattleCard = false) {
		string query =
			$"SELECT " +
			$"temporary_id, " +
			$"card_index, " +
			$"is_fixed_in_deck, " +
			$"is_in_deck " +
			$"FROM {DataBaseTableDefine.PlayerDeckTable} " +
			$"WHERE is_in_deck = 'true'"; ;
		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		if (false == it.Read()) {
			return default;
		}
		List<Card> cardList = new List<Card>();

		do {
			Card card = new Card();
			card = isBattleCard ? CardDao.GetBattleCard(it.GetSafeValue<int>(1)) : CardDao.GetCard(it.GetSafeValue<int>(1));
			card.temporaryID = it.GetSafeValue<int>(0);
			card.isFixedInDeck = it.GetSafeValue<bool>(2);
			card.isInDeck = it.GetSafeValue<bool>(3);
			cardList.Add(card);
		} while (it.Read());

		return cardList;
	}
	// 플레이어 덱 리스트의 갯 수를 반환
	public static int GetPlayerDeckCount() {
		string query =
			$"SELECT " +
			$"COUNT(*) " +
			$"FROM {DataBaseTableDefine.PlayerDeckTable} " +
			$"WHERE is_in_deck = 'true'";
		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		if (false == it.Read()) {
			return 0;
		}

		return it.GetSafeValue<int>(0);
	}

	// 플레이어의 새로운 덱 리스트 추가
	public static void AddPlayerNewDeckList(List<int> cardIndexList) {
		if (cardIndexList.HasValue() == false) return;
		// 모든 is_in_deck 항목을 False로 설정
		string clearQuery = $"UPDATE {DataBaseTableDefine.PlayerDeckTable} SET is_in_deck = 'false' WHERE is_in_deck = 'true'";
		CustomDataReader it = SQLiteManager.SelectQuery(clearQuery, ENUM_DATABASE_PATH.PLAYER_DATA);

		// 새로운 카드를 덱에 추가
		string insertQuery =
			$"INSERT INTO {DataBaseTableDefine.PlayerDeckTable} " +
			$"(card_index, is_in_deck) " +
			$"VALUES ";
		foreach (int index in cardIndexList) {
			insertQuery += $"('{index}', '{true.BooleanToString()}'), ";
		}
		insertQuery = insertQuery.Substring(0, insertQuery.Length - 2); // 마지막 콤마와 공백 제거
		it = SQLiteManager.SelectQuery(insertQuery, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	//temporaryIDList의 유효하지 않은 항목에 대해선 아무 행위도 취하지 않고 넘어감.
	public static void UpdatePlayerDeckList(List<int> temporaryIDList) {
		if (temporaryIDList.HasValue() == false) return;
		// 모든 is_in_deck 항목을 False로 설정
		string clearQuery = $"UPDATE {DataBaseTableDefine.PlayerDeckTable} SET is_in_deck = 'false' WHERE is_in_deck = 'true'";
		CustomDataReader it = SQLiteManager.SelectQuery(clearQuery, ENUM_DATABASE_PATH.PLAYER_DATA);

		// temporaryIDList에 해당하는 항목의 is_in_deck 값을 True로 설정
		string setTrueQuery = $"UPDATE {DataBaseTableDefine.PlayerDeckTable} SET is_in_deck = 'true' WHERE temporary_id IN ({string.Join(", ", temporaryIDList)})";
		it = SQLiteManager.SelectQuery(setTrueQuery, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 덱의 모든 카드 제거
	public static void RemoveAllCardsInDeck() {
		string query =
			$"DELETE " +
			$"FROM {DataBaseTableDefine.PlayerDeckTable}";

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 덱에서 특정 카드들 제거 (ID 리스트)
	public static void RemoveCards(List<int> cardIDs) {
		if (cardIDs == null || cardIDs.Count == 0) return;

		string query =
			$"DELETE " +
			$"FROM {DataBaseTableDefine.PlayerDeckTable} " +
			$"WHERE temporary_id IN (";

		foreach (int card in cardIDs) {
			query += $"{card}, ";
		}
		query = query.Substring(0, query.Length - 2);
		query += $")";

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 덱에서 특정 카드들 제거 (LiteDBData 리스트)
	public static void RemoveCards(List<CardLiteDBData> cards) {
		if (cards.HasValue() == false) return;
		string query =
			$"DELETE " +
			$"FROM {DataBaseTableDefine.PlayerDeckTable} " +
			$"WHERE temporary_id IN (";

		foreach (CardLiteDBData card in cards) {
			query += $"{card.temporaryID}, ";
		}
		query = query.Substring(0, query.Length - 2);
		query += $")";

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}


	// 덱에서 특정 카드들 제거 (Card 객체 리스트)
	public static void RemoveCards(List<Card> cards) {
		if (cards.HasValue() == false) return;
		string query =
			$"DELETE " +
			$"FROM {DataBaseTableDefine.PlayerDeckTable} " +
			$"WHERE temporary_id IN (";

		foreach (Card card in cards) {
			query += $"{card.temporaryID}, ";
		}
		query = query.Substring(0, query.Length - 2);
		query += $")";

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}


	#endregion

	#region Bag Functions
	// 가방에 새 카드들 추가
	public static void AddNewCardsInBag(List<int> card) {
		if (card.HasValue() == false) return;
		string query =
		$"INSERT INTO {DataBaseTableDefine.PlayerDeckTable} " +
		$"(card_index, is_in_deck) " +
		$"VALUES ";

		foreach (int index in card) {
			query += $"('{index}', '{false.BooleanToString()}'), ";
		}
		query = query.Substring(0, query.Length - 2);

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}
	public static void AddNewCardsInBag(List<Card> cards) {
		if (cards.HasValue() == false) return;
		string query =
			$"INSERT INTO {DataBaseTableDefine.PlayerDeckTable} " +
			$"(card_index, is_fixed_in_deck, is_in_deck) " +
			$"VALUES ";

		foreach (Card card in cards) {
			query += $"('{card.Index}', '{card.isFixedInDeck.BooleanToString()}', '{false.BooleanToString()}'), ";
		}

		query = query.Substring(0, query.Length - 2);

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}
	public static void AddNewCardsInBag(List<CardLiteDBData> cards) {
		if (cards.HasValue() == false) return;
		string query =
				$"INSERT INTO {DataBaseTableDefine.PlayerDeckTable} " +
				$"(temporary_id, card_index, is_fixed_in_deck, is_in_deck) " +
				$"VALUES ";

		foreach (CardLiteDBData card in cards) {
			query += $"({card.temporaryID}, '{card.Index}', '{card.isFixedInDeck.BooleanToString()}', '{false.BooleanToString()}'), ";
		}

		query = query.Substring(0, query.Length - 2);

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 가방에 새 카드 추가
	public static void AddNewCardInBag(Card card) {
		if (card == null) return;
		string query =
			$"INSERT INTO {DataBaseTableDefine.PlayerDeckTable} " +
			$"(card_index, is_fixed_in_deck, is_in_deck) " +
			$"VALUES " +
			$"('{card.Index}', '{card.isFixedInDeck.BooleanToString()}', '{false.BooleanToString()}')";
		var it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 플레이어 가방 리스트 반환
	public static List<Card> GetPlayerBagList() {
		string query =
			$"SELECT " +
			$"temporary_id, " +
			$"card_index, " +
			$"is_fixed_in_deck, " +
			$"is_in_deck " +
			$"FROM {DataBaseTableDefine.PlayerDeckTable} " +
			$"WHERE is_in_deck = 'false'";
		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		if (false == it.Read()) {
			return new List<Card>();
		}
		List<Card> cardList = new List<Card>();
		do {
			Card card = new Card();
			card = CardDao.GetCard(it.GetSafeValue<int>(1));
			card.temporaryID = it.GetSafeValue<int>(0);
			card.isFixedInDeck = it.GetSafeValue<bool>(2);
			card.isInDeck = it.GetSafeValue<bool>(3);
			cardList.Add(card);
		} while (it.Read());

		return cardList;
	}
	#endregion

	// 플레이어의 모든 카드 리스트 반환 (덱 + 가방)
	public static List<Card> GetAllPlayerCardList(int playerIndex = 1, bool isBattleCard = false) {
		string query =
			$"SELECT " +
			$"temporary_id, " +
			$"card_index, " +
			$"is_fixed_in_deck, " +
			$"is_in_deck " +
			$"FROM {DataBaseTableDefine.PlayerDeckTable}";

		CustomDataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		if (false == it.Read()) {
			return default;
		}
		List<Card> cardList = new List<Card>();

		do {
			Card card = new Card();
			card = isBattleCard ? CardDao.GetBattleCard(it.GetSafeValue<int>(1)) : CardDao.GetCard(it.GetSafeValue<int>(1));
			card.temporaryID = it.GetSafeValue<int>(0);
			card.isFixedInDeck = it.GetSafeValue<bool>(2);
			card.isInDeck = it.GetSafeValue<bool>(3);
			cardList.Add(card);
		} while (it.Read());

		return cardList;
	}
	// 카드 데이터 업데이트
	public static void UpdateCardData(Card card) {
		if (card == null) return;
		string query =
		$"UPDATE {DataBaseTableDefine.PlayerDeckTable} SET " +
		$"is_in_deck = '{card.isInDeck.BooleanToString()}' " +
		$"WHERE temporary_id = {card.temporaryID}";

		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	// 카드의 덱 포함 여부 토글
	public static void SwitchIsInDeckBoolean(Card card) {
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerDeckTable} SET " +
			$"is_in_deck = '{(card.isInDeck ? false : true).BooleanToString()}' " +
			$"WHERE temporary_id = {card.temporaryID}";
		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	public enum ENUM_DECK_COLUMN_TYPE {
		REMOVE_UNTIL_COUNT
	}

	private static Dictionary<ENUM_DECK_COLUMN_TYPE, string> deckColumns = new Dictionary<ENUM_DECK_COLUMN_TYPE, string>() {
		{ ENUM_DECK_COLUMN_TYPE.REMOVE_UNTIL_COUNT, "remove_until_count" }
	};

	// 특정 컬럼 값 조회
	public static T GetValue<T>(ENUM_DECK_COLUMN_TYPE columnType, int cardID) where T : IComparable<T>, IConvertible {
		if (!SQLiteManager.IsColumnExist(DataBaseTableDefine.PlayerDeckTable, deckColumns[columnType], ENUM_DATABASE_PATH.PLAYER_DATA))
			return default;

		string query =
			$"SELECT {deckColumns[columnType]} " +
			$"FROM {DataBaseTableDefine.PlayerDeckTable} " +
			$"WHERE temporary_id = {cardID}";

		var it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);

		if (!it.Read()) return default;
		return it.GetSafeValue<T>(0);
	}

	// 컬럼 생성 또는 업데이트
	public static void MakeOrUpdateColumn<T>(ENUM_DECK_COLUMN_TYPE columnType, T value, ENUM_DB_DATA_TYPE dataType, int cardID) where T : IComparable<T>, IConvertible {
		SQLiteManager.ExistOrMakeColumn(DataBaseTableDefine.PlayerDeckTable, deckColumns[columnType], dataType, ENUM_DATABASE_PATH.PLAYER_DATA);
		string query =
			$"UPDATE {DataBaseTableDefine.PlayerDeckTable} SET " +
			$"{deckColumns[columnType]} = {value} " +
			$"WHERE temporary_id = {cardID}";
		SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.PLAYER_DATA);
	}

	//public static List<int> GetCardIndexList()
	//{
	//
	//}
}
