using System.Collections.Generic;

/// <summary>
/// JSON 파일로부터 로드된 문자열 데이터를 담는 데이터 컨테이너 클래스
/// </summary>
public class TextDefineStringData {
	public Dictionary<ENUM_DEFINE_STRING, string> stringData;
	public Dictionary<ENUM_DEFINE_LIST_STRING, string[]> listStringData;
}