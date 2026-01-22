using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System;
using System.Linq;
using System.ComponentModel;

// SQLite 데이터 리더를 감싸서 안전하게 데이터를 로드하고 파싱하는 래퍼 클래스
public class CustomDataReader : IDisposable {
	private SqliteDataReader dataReader;

	public CustomDataReader(SqliteDataReader reader) {
		dataReader = reader;
	}

	#region Read
	// 다음 행으로 이동
	public bool Read() {
		return dataReader.Read();
	}

	// 컬럼 인덱스에 해당하는 값을 안전하게 가져오는 제네릭 메서드
	public T GetSafeValue<T>(int colIndex) {
		object theValue = dataReader.GetValue(colIndex);
		Type theValueType = typeof(T);

		if (DBNull.Value != theValue) {
			if (false == IsNullableType(theValueType)) {
				return (T)Convert.ChangeType(theValue, theValueType);
			}
			else {
				NullableConverter theNullableConverter = new NullableConverter(theValueType);
				return (T)Convert.ChangeType(theValue, theNullableConverter.UnderlyingType);
			}
		}
		return default;
	}

	// Nullable 타입인지 확인하는 헬퍼 메서드
	private static bool IsNullableType(Type theValueType) {
		bool result = (theValueType.IsGenericType && theValueType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
		return result;
	}

	// 리더 닫기
	public void Close() {
		dataReader.Close();
	}

	// 리소스 해제
	public void Dispose() {
		Close();
	}
	#endregion

	#region Parsing
	// 문자열 컬럼 값을 boolean으로 변환
	public bool GetBoolFromString(int colIndex) {
		var boolString = GetSafeValue<string>(colIndex);
		return !string.IsNullOrEmpty(boolString) && boolString.IndexOf("true", StringComparison.OrdinalIgnoreCase) >= 0;
	}

	// 문자열 컬럼 값을 Enum으로 변환 (Levenshtein 거리 사용)
	public T GetEnumFromString<T>(int colIndex, T? defaultEnumValue = null) where T : struct, Enum {
		string input = GetSafeValue<string>(colIndex);

		if (string.IsNullOrEmpty(input)) {
			var enumValues = Enum.GetValues(typeof(T));
			return defaultEnumValue ?? (T)enumValues.GetValue(enumValues.Length - 1);
		}

		var enumList = Enum.GetValues(typeof(T)).Cast<T>();

		// 유사도가 가장 높은 Enum 값 찾기
		T mostSimilarEnum = enumList.First();
		int minDistance = int.MaxValue;

		foreach (var enumValue in enumList) {
			int distance = ComputeLevenshteinDistance(input.ToLower(), enumValue.ToString().ToLower());
			if (distance < minDistance) {
				minDistance = distance;
				mostSimilarEnum = enumValue;
			}
		}

		return mostSimilarEnum;

		int ComputeLevenshteinDistance(string s, string t) {
			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			if (n == 0) return m;
			if (m == 0) return n;

			for (int i = 0; i <= n; d[i, 0] = i++) { }
			for (int j = 0; j <= m; d[0, j] = j++) { }

			for (int i = 1; i <= n; i++) {
				for (int j = 1; j <= m; j++) {
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

					d[i, j] = Math.Min(
						Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
						d[i - 1, j - 1] + cost);
				}
			}

			return d[n, m];
		}
	}

	// 문자열 컬럼 값을 쉼표로 분리하여 문자열 리스트로 반환
	public List<string> GetTextValueToStringList(int colIndex) {
		string theValue = this.GetSafeValue<string>(colIndex);
		if (null == theValue) {
			return new List<string>();
		}
		if ("" == theValue) {
			return new List<string>();
		}

		List<string> ValueList = new List<string>();

		string[] ValueStrList = theValue.Split(',');
		foreach (var value in ValueStrList) {
			ValueList.Add(value);
		}
		return ValueList;
	}

	// 문자열 컬럼 값을 쉼표로 분리하여 정수 리스트로 반환
	public List<int> GetTextValueToIntList(int colIndex) {
		string theValue = this.GetSafeValue<string>(colIndex);
		if (null == theValue) {
			return new List<int>();
		}

		if ("" == theValue) {
			return new List<int>();
		}

		List<int> ValueList = new List<int>();
		string[] ValueStrList = theValue.Split(',');
		foreach (var value in ValueStrList) {
			ValueList.Add(int.Parse(value));
		}

		return ValueList;
	}

	// 문자열 컬럼 값을 쉼표로 분리하여 정수 HashSet으로 반환
	public HashSet<int> GetTextValueToHashSet(int colIndex) {
		string theValue = this.GetSafeValue<string>(colIndex);
		if (null == theValue) {
			return new HashSet<int>();
		}
		if ("" == theValue) {
			return new HashSet<int>();
		}

		HashSet<int> ValueHashSet = new HashSet<int>();
		string[] ValueStrList = theValue.Split(',');
		foreach (var value in ValueStrList) {

			ValueHashSet.Add(int.Parse(value));
		}
		return ValueHashSet;
	}
	#endregion


}
