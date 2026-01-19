# DataParser

게임 개발 환경에서 엑셀 형식의 기획 데이터를 SQLite DB 및 JSON 포맷으로 변환하는 도구입니다.

기획 데이터의 수정 사항을 게임 엔진(Unity 등)에 즉시 반영할 수 있도록 지원하며, 단순 데이터 변환 외에도 데이터가 참조하는 리소스(이미지 등)의 실제 존재 여부를 검증하는 무결성 체크 기능을 포함하고 있습니다.

## 주요 기능 및 프로세스

본 프로그램이 수행하는 주요 기능의 작동 흐름은 다음과 같습니다.

### 1. 스토리 이벤트 및 텍스트 변환 (Story Event & Text Conversion)
엑셀에 정의된 이벤트 로직과 대사 데이터를 파싱하여 로컬 DB 파일로 변환합니다.

- **작동 원리**
    1. 변환 모드(StoryEvent 또는 StoryEventText) 선택 및 파일 경로를 설정합니다.
    2. `MainForm`에서 변환 요청 시 `DataConverter.ConvertEventDataExcelToSQLite`가 호출됩니다.
    3. `DataReader.ReadAllDataFromExcel`을 통해 엑셀 데이터를 메모리에 일괄 적재합니다.
    4. 각 시트별로 비동기 작업(`ProcessEventDataSheet`)을 생성하여 병렬로 데이터 파싱을 수행합니다.
    5. 조건, 보상 등 게임 로직을 분석하여 딕셔너리 형태로 가공하고, `DataReader.UpdateOrInsertData`를 통해 DB에 저장합니다. (기존 데이터가 존재할 경우 갱신, 없을 경우 삽입)

### 2. 리소스 무결성 검사 (DataCheck)
DB 데이터가 참조하는 리소스 파일이 실제 프로젝트 경로에 존재하는지 검증합니다.

- **작동 원리**
    1. `DataCheck` 모드 실행 시 `DataChecker.CheckEventIllustData`가 호출됩니다.
    2. DB 내 정의된 모든 일러스트 파일 경로를 조회합니다.
    3. 실제 에셋 폴더 내 파일 목록과 대조하여, 누락된 리소스가 있을 경우 로그를 통해 출력합니다.

### 3. UI 텍스트 추출 (UIText)
UI에 사용되는 정적 문자열을 다국어 지원이 가능한 JSON 형식으로 변환합니다.

- **작동 원리**
    1. `DataConverter.CreateLanguageData`가 실행됩니다.
    2. 엑셀의 언어별(한국어, 영어 등) 컬럼 데이터를 읽어 딕셔너리를 구성합니다.
    3. 언어별 JSON 파일(`ko_KR.json`, `en_US.json` 등)로 개별 저장합니다.

### 4. 데이터 초기화 (DataSwipe)
데이터 테스트 및 재구축을 위해 특정 테이블의 데이터를 일괄 삭제합니다.
- `DataConverter.SwipeEventData`가 `QueryTableNameStrings`에 정의된 테이블 목록을 순회하며 `DELETE` 쿼리를 실행합니다.

---

## 핵심 로직: 엑셀 데이터 DB 변환 시퀀스

가장 핵심적인 기능인 '엑셀 -> SQLite' 변환 프로세스의 내부 처리 순서입니다.

1. **초기화**: 변환 작업 시작 시 `DataConverter` 및 관련 설정을 초기화합니다.
2. **데이터 로딩**: `DataReader`가 엑셀 시트의 사용 영역(UsedRange) 전체를 2차원 배열로 한 번에 로드합니다. (COM Interop 호출 최소화를 통한 성능 최적화)
3. **병렬 처리**: 다수의 시트를 효율적으로 처리하기 위해 `Task.Run`을 사용하여 시트별 처리 작업을 쓰레드로 분산합니다.
4. **데이터 파싱**:
    - 각 쓰레드에서 `ProcessEventDataSheet`가 실행됩니다.
    - `QuickLoadData` 등의 내부 메소드를 통해 셀 데이터를 추출하고, `QueryColumnNameStrings`에 정의된 스키마에 맞춰 매핑합니다.
5. **DB 저장**:
    - 파싱된 데이터는 `DataReader.UpdateOrInsertData`로 전달됩니다.
    - 멀티쓰레드 환경에서의 데이터 충돌 방지를 위해, DB 쓰기 작업 시 내부적으로 락(lock)을 사용하여 순차적인 처리를 보장합니다.
6. **완료**: 모든 시트의 변환 작업이 종료되면 완료 알림이 발생합니다.
