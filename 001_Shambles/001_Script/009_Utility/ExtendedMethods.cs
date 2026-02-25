using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 프로젝트 전역에서 사용하는 확장 메서드 모음 클래스
public static class ExtendedMethods {

#region Collection

	// 컬렉션이 null이 아니고 요소가 1개 이상인지 확인
	public static bool HasValue(this ICollection collection) {
		return collection != null && collection.Count > 0;
	}

	// Queue의 마지막 요소를 반환
	public static T Last<T>(this Queue<T> queue) {
		return queue.ToArray()[queue.Count - 1];
	}

	// IIndexableDTO 리스트에서 특정 인덱스를 가진 항목이 존재하는지 확인
	public static bool ContainsIndex<T>(this List<T> indexableDataList, int? index) where T : IIndexableDTO {
		if (!index.HasValue) return false;

		foreach (var item in indexableDataList) {
			if (item.Index == index.Value)
				return true;
		}

		return false;
	}

	// IIndexableDTO 리스트에서 특정 인덱스를 가진 항목의 개수를 반환
	public static int ContainsIndexCount<T>(this List<T> indexableDataList, int index) where T : IIndexableDTO {
		int count = 0;

		foreach (var item in indexableDataList) {
			if (item.Index == index)
				count++;
		}

		return count;
	}

	// 값 타입 리스트를 Nullable 리스트로 변환
	public static List<T?> ToNullableList<T>(this List<T> list) where T : struct {
		return list.ConvertAll(x => (T?)x);
	}

	// 리스트에 특정 범위(inclusiveMin ~ exclusiveMax) 내 값이 포함되어 있는지 확인
	public static bool ContainsIndexFromTo(this List<int> list, int inclusiveMin, int exclusiveMax) {
		foreach (var idx in list) {
			if (idx >= inclusiveMin && idx < exclusiveMax) return true;
		}

		return false;
	}

	// 리스트 내 요소 중 HashSet에 포함된 항목이 있는지 확인
	public static bool ContainsListToHash<T>(this List<T> objList, HashSet<T> objHashSet) {
		foreach (T obj in objList) {
			if (objHashSet.Contains(obj)) return true;
		}

		return false;
	}

	// 리스트의 두 요소를 교환
	public static void Swap<T>(List<T> list, int origin, int target) {
		T temp;
		temp = list[origin];
		list[origin] = list[target];
		list[target] = temp;
	}

	// 배열의 두 요소를 교환
	public static void Swap<T>(T[] list, int origin, int target) {
		T temp;
		temp = list[origin];
		list[origin] = list[target];
		list[target] = temp;
	}

	// Dictionary의 value가 일치하는 모든 key를 리스트로 반환
	public static List<TKey> FindKeysByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value) {
		if (dictionary.HasValue() == false) return new List<TKey>();
		return dictionary.Where(pair => EqualityComparer<TValue>.Default.Equals(pair.Value, value))
						 .Select(pair => pair.Key)
						 .ToList();
	}

#endregion

#region Enum

	// 열거자에서 다음 열거 상수를 반환 (circulation이 true이면 순환)
	public static T GetNextEnum<T>(this T enumVal, bool circulation = false) where T : System.Enum {
		if (!typeof(T).IsEnum) throw new ArgumentException($"{typeof(T).FullName}는 Enum 타입이 아닙니다.");

		T[] arr = (T[])Enum.GetValues(enumVal.GetType());
		int i = Array.IndexOf(arr, enumVal);
		return (i >= arr.Length - 1) ? (circulation ? arr[arr.Length - 1] : arr[0]) : arr[i + 1];
	}

	// 열거자에서 이전 열거 상수를 반환 (circulation이 true이면 순환)
	public static T GetPreviousEnum<T>(this T enumVal, bool circulation = false) where T : struct {
		if (!typeof(T).IsEnum) throw new ArgumentException($"{typeof(T).FullName}는 Enum 타입이 아닙니다.");

		T[] arr = (T[])Enum.GetValues(enumVal.GetType());
		int i = Array.IndexOf(arr, enumVal);
		return (i <= 0) ? (circulation ? arr[0] : arr[arr.Length - 1]) : arr[i - 1];
	}

	// 열거형의 전체 상수 개수를 반환
	public static int GetEnumCount<T>(this T enumMember) where T : Enum {
		if (!typeof(T).IsEnum) throw new ArgumentException($"{typeof(T).FullName}는 Enum 타입이 아닙니다.");

		return Enum.GetValues(typeof(T)).Length;
	}

#endregion

#region EditorUtility

#region Color

	// MaskableGraphic의 색상을 float 값으로 변경 (null인 채널은 기존 값 유지)
	public static void ChangeColor<T>(this T component, float? r = null, float? g = null, float? b = null, float? alpha = null) where T : MaskableGraphic {
		Color color = component.color;
		component.color = new Color(r ?? color.r, g ?? color.g, b ?? color.b, alpha ?? color.a);
	}

	// MaskableGraphic의 색상을 0~255 int 값으로 변경 (null인 채널은 기존 값 유지)
	public static void ChangeColor<T>(this T component, int? r = null, int? g = null, int? b = null, int? alpha = null) where T : MaskableGraphic {
		Color color = component.color;
		component.color = new Color(r / 255f ?? color.r, g / 255f ?? color.g, b / 255f ?? color.b, alpha / 255f ?? color.a);
	}

	// MaskableGraphic의 색상을 Hex 문자열로 변경
	public static void ChangeColor<T>(this T component, string hex) where T : MaskableGraphic {
		Color color = component.color;
		component.color = hex.HexToRGB();
	}

	// SpriteRenderer의 RGB 값을 동일한 밝기 값으로 설정
	public static void SetBrightness(this SpriteRenderer renderer, float value) {
		Color color = renderer.color;
		color.r = value;
		color.g = value;
		color.b = value;
		renderer.color = color;
	}

	// SpriteRenderer의 알파 값을 설정
	public static void SetAlpha(this SpriteRenderer component, float a) {
		Color color = component.color;
		color.a = a;
		component.color = color;
	}

	// MaskableGraphic의 알파 값을 설정
	public static void SetAlpha<T>(this T component, float a) where T : MaskableGraphic {
		Color color = component.color;
		color.a = a;
		component.color = color;
	}

	// Color를 Hex 코드 문자열로 변환
	public static string ColorToHexCode(this Color color) {
		string m_HexCode = "#";
		m_HexCode += Convert.ToString((int)(color.r * 255), 16);
		m_HexCode += Convert.ToString((int)(color.g * 255), 16);
		m_HexCode += Convert.ToString((int)(color.b * 255), 16);
		return m_HexCode;
	}

	// 6자리 Hex 문자열을 Color로 변환
	public static Color HexToRGB(this string hex) {
		if (hex.Length != 6) {
			Debug.LogError("Hex string should have 6 characters.");
			return Color.white;
		}

		byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

		return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
	}

#endregion

#region Position

	// Transform의 localPosition의 X 값을 이동
	public static void MoveLocalX(this Transform transform, float deltaX) {
		Vector3 newPosition = transform.localPosition + new Vector3(deltaX, 0, 0);
		transform.localPosition = newPosition;
	}

	// Transform의 localPosition의 Y 값을 이동
	public static void MoveLocalY(this Transform transform, float deltaY) {
		Vector3 newPosition = transform.localPosition + new Vector3(0, deltaY, 0);
		transform.localPosition = newPosition;
	}

	// Transform의 localPosition의 Z 값을 이동
	public static void MoveLocalZ(this Transform transform, float deltaZ) {
		Vector3 newPosition = transform.localPosition + new Vector3(0, 0, deltaZ);
		transform.localPosition = newPosition;
	}

	// Transform의 worldPosition의 X 값을 이동
	public static void MoveX(this Transform transform, float deltaX) {
		Vector3 newPosition = transform.position + new Vector3(deltaX, 0, 0);
		transform.position = newPosition;
	}

	// Transform의 worldPosition의 Y 값을 이동
	public static void MoveY(this Transform transform, float deltaY) {
		Vector3 newPosition = transform.position + new Vector3(0, deltaY, 0);
		transform.position = newPosition;
	}

	// Transform의 worldPosition의 Z 값을 이동
	public static void MoveZ(this Transform transform, float deltaZ) {
		Vector3 newPosition = transform.position + new Vector3(0, 0, deltaZ);
		transform.position = newPosition;
	}

	// Transform의 localPosition의 X 값을 설정
	public static void SetLocalX(this Transform transform, float x) {
		Vector3 newPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
		transform.localPosition = newPosition;
	}

	// Transform의 localPosition의 Y 값을 설정
	public static void SetLocalY(this Transform transform, float y) {
		Vector3 newPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
		transform.localPosition = newPosition;
	}

	// Transform의 localPosition의 Z 값을 설정
	public static void SetLocalZ(this Transform transform, float z) {
		Vector3 newPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
		transform.localPosition = newPosition;
	}

	// Transform의 worldPosition의 X 값을 설정
	public static void SetX(this Transform transform, float x) {
		Vector3 newPosition = new Vector3(x, transform.position.y, transform.position.z);
		transform.position = newPosition;
	}

	// Transform의 worldPosition의 Y 값을 설정
	public static void SetY(this Transform transform, float y) {
		Vector3 newPosition = new Vector3(transform.position.x, y, transform.position.z);
		transform.position = newPosition;
	}

	// Transform의 worldPosition의 Z 값을 설정
	public static void SetZ(this Transform transform, float z) {
		Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, z);
		transform.position = newPosition;
	}

#endregion

	// Stretched 앵커 기준 RectTransform의 상하좌우 오프셋을 Vector4로 반환 (x=top, y=bottom, z=left, w=right)
	public static Vector4 GetStratchedAnchoredEdges(this RectTransform rectTransform) {
		float top = rectTransform.anchoredPosition.y + rectTransform.sizeDelta.y / 2;
		float bottom = -(rectTransform.anchoredPosition.y - rectTransform.sizeDelta.y / 2);
		float left = rectTransform.anchoredPosition.x - rectTransform.sizeDelta.x / 2;
		float right = rectTransform.anchoredPosition.x + rectTransform.sizeDelta.x / 2;

		return new Vector4(top, bottom, left, right);
	}

	// Vector4의 각 요소를 절대값으로 변환
	public static Vector4 Abs(this Vector4 vector) {
		return new Vector4(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z), Mathf.Abs(vector.w));
	}

	// TMP_Text의 높이를 텍스트 내용에 맞게 조정
	public static void FitTextHeight(this TMP_Text tmp, float extraHeight = 0) {
		tmp.rectTransform.sizeDelta = new Vector2(tmp.rectTransform.sizeDelta.x, tmp.preferredHeight + extraHeight);
	}

	// 1프레임 이후 UI를 강제 갱신 (coroutineClass에는 보통 this를 입력)
	public static void ForcedRefreshUIEndOfFrame(this RectTransform targetRect, MonoBehaviour coroutineClass) {
		coroutineClass.StartCoroutine(WaitEumerator());

		IEnumerator WaitEumerator() {
			WaitForEndOfFrame wait = new WaitForEndOfFrame();
			yield return wait;
			LayoutRebuilder.ForceRebuildLayoutImmediate(targetRect);
		}
	}

	// 부모 오브젝트에서 컴포넌트를 탐색하여 out으로 반환
	public static bool TryGetComponentInParent<T>(this GameObject obj, out T component) {
		component = obj.GetComponentInParent<T>();
		return component != null;
	}

#endregion

#region Debug

	// IIndexableDTO 리스트의 전체 항목과 개수를 디버그 로그로 출력
	public static void Debug_List<T>(this IEnumerable<T> source) where T : IIndexableDTO {
		string str = "";
		int count = 0;
		foreach (var item in source) {
			str += item.ToString();
			count++;
		}
		Debug.Log(str);
		Debug.Log(count);
	}

#endregion

#region Random

	// IIndexableDTO 리스트에서 특정 인덱스를 가진 첫 번째 항목을 제거
	public static void RemoveIndex<T>(this List<T> indexableDataList, int? index) where T : IIndexableDTO {
		if (!index.HasValue) return;
		for (int i = 0; i < indexableDataList.Count; i++) {
			if (indexableDataList[i].Index == index.Value) {
				indexableDataList.Remove(indexableDataList[i]);
				return;
			}
		}
	}

	// IIndexableDTO 리스트에서 인덱스 목록에 해당하는 항목들을 제거
	public static void RemoveIndexes<T>(this List<T> indexableDataList, List<int> indexList) where T : IIndexableDTO {
		if (indexList == null || indexList.Count == 0) return;
		for (int i = 0; i < indexList.Count; i++) {
			indexableDataList.RemoveIndex(indexList[i]);
		}
	}

	// 리스트에서 랜덤으로 하나의 요소를 반환
	public static T RandomValue<T>(this List<T> dataList) {
		if (dataList.HasValue() == false) return default;
		else if (dataList.Count == 1) return dataList[0];
		return dataList[UnityEngine.Random.Range(0, dataList.Count)];
	}

	// 리스트에서 랜덤으로 count개의 요소를 반환 (중복 허용 여부 선택)
	public static List<T> RandomValue<T>(this List<T> dataList, int count, bool isDuplicapable = false) {
		if (dataList.HasValue() == false) return default;

		if (dataList.Count < count && isDuplicapable == false) {
			Debug.LogError("Count Value is bigger than List.Count");
			isDuplicapable = true;
		}

		if (!isDuplicapable) {
			List<T> _dataList = new List<T>(dataList);
			for (int i = 0; i < _dataList.Count - 1; i++) {
				int j = UnityEngine.Random.Range(i, _dataList.Count);
				T _temp = _dataList[i];
				_dataList[i] = _dataList[j];
				_dataList[j] = _temp;
			}

			return _dataList.GetRange(0, count);
		}
		else {
			List<T> result = new List<T>();
			for (int i = 0; i < count; i++) {
				int index = UnityEngine.Random.Range(0, dataList.Count);
				result.Add(dataList[index]);
			}
			return result;
		}
	}

	// 리스트를 Fisher-Yates 알고리즘으로 셔플하여 새 리스트를 반환
	public static List<T> ShuffleList<T>(this List<T> list) {
		List<T> resultList = list.ToList();
		int randNumber;
		for (int i = 0; i < list.Count; i++) {
			randNumber = UnityEngine.Random.Range(i, list.Count);
			Swap(resultList, i, randNumber);
		}
		return resultList;
	}
	
	// 배열을 Fisher-Yates 알고리즘으로 셔플하여 새 배열을 반환
	public static T[] ShuffleArray<T>(T[] list) {
		T[] resultList = new T[list.Length];
		list.CopyTo(resultList, 0);
		int randNumber;
		for (int i = 0; i < list.Length; i++) {
			randNumber = UnityEngine.Random.Range(i, list.Length);
			Swap(resultList, i, randNumber);
		}
		return resultList;
	}

	// 가중치 기반 랜덤 선택으로 리스트를 재구성하여 반환
	public static List<T> RandomizeByWeight<T>(this IEnumerable<T> source, Func<T, int> weightSelector, int? seedValue = null, int? count = null) {
		System.Random rand = seedValue.HasValue ? new System.Random(seedValue.Value) : new System.Random();
		List<T> randomizedList = new List<T>();
		List<T> remainingList = new List<T>(source);

		int totalWeight = remainingList.Sum(weightSelector);
		int targetCount = count ?? source.Count();

		while (randomizedList.Count < targetCount && remainingList.Count > 0) {
			if (totalWeight <= 0) {
				randomizedList.AddRange(remainingList.Take(targetCount - randomizedList.Count));
				break;
			}

			int randomValue = rand.Next(totalWeight);
			for (int i = 0; i < remainingList.Count; i++) {
				T item = remainingList[i];
				if (randomValue < weightSelector(item)) {
					randomizedList.Add(item);
					totalWeight -= weightSelector(item);
					remainingList[i] = remainingList[remainingList.Count - 1];
					remainingList.RemoveAt(remainingList.Count - 1);
					break;
				}
				else {
					randomValue -= weightSelector(item);
				}
			}
		}
		return randomizedList;
	}


#endregion
}
