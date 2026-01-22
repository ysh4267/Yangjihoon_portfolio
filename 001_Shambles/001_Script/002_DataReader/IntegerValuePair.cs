// 최소값과 최대값 범위를 가지고 값의 포함 여부를 판단하는 데이터 클래스
public class IntegerValuePair {
	public int? maxValue;
	public int? minValue;

	public IntegerValuePair() { }

	public IntegerValuePair(int? _min, int? _max) {
		minValue = _min;
		maxValue = _max;
	}

	// 두 값 내부에 있는지 확인 후 BOOL값 추가
	public bool IntegerCompare(int value) {
		if (maxValue.HasValue)
			if (maxValue < value)
				return false;
		if (minValue.HasValue)
			if (minValue > value)
				return false;
		return true;
	}
}
