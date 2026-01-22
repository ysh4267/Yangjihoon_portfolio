using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// 객체와 JSON 문자열 간의 변환을 담당하는 유틸리티 클래스
public class JsonParser {
	// 객체 -> 문자열로 된 json (unity -> db)
	public static string ObjectToJson(object obj) {
		return JsonConvert.SerializeObject(obj);
	}

	// 문자열로 된 json -> 원하는 타입의 객체 (db -> unity)
	public static T JsonToObject<T>(string jsonData) {

		return jsonData == null ? default(T) : JsonConvert.DeserializeObject<T>(jsonData);
	}
}
