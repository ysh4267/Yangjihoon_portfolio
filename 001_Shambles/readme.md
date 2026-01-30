# 샴블즈 (Shambles)

<a href="https://store.steampowered.com/app/2289630/_/?l=koreana"><img src="https://cdn.simpleicons.org/steam/171A21" height="60" alt="Steam"></a><sub>(Steam)</sub> &nbsp; &nbsp; &nbsp;
<a href="https://play.google.com/store/apps/details?id=com.gravity.shambles.aos"><img src="https://cdn.simpleicons.org/googleplay/41E0FD" height="60" alt="Google Play"></a><sub>(Google Play)</sub> &nbsp; &nbsp; &nbsp;
<a href="https://apps.apple.com/kr/app/%EC%83%B4%EB%B8%94%EC%A6%88-%EC%A2%85%EB%A7%90%EC%9D%98-%ED%9B%84%EC%86%90%EB%93%A4/id6740197039"><img src="https://cdn.simpleicons.org/appstore/0066CC" height="60" alt="App Store"></a><sub>(App Store)</sub>


## 게임 개요

포스트 아포칼립스 세계관을 배경으로 한 2D 텍스트 RPG, 덱빌딩, 로그라이크 게임입니다.

*   **장르**: 텍스트 RPG, 덱빌딩, 로그라이크
*   **개발 기간**: 2년
*   **출시일**: 2025.03.27 (Mobile) / 2025.06.26 (PC)
*   **참여 인원**: 기획 2명, 아트 4명, 프로그래밍 3명
*   **역할**: 리드 프로그래머

## 사용된 기술 스택

[![Unity](https://img.shields.io/badge/Unity-000000?logo=unity&logoColor=white&labelColor=555555)](https://unity.com/) [![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](https://dotnet.microsoft.com/ko-kr/languages/csharp) [![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=white&labelColor=555555)](https://dotnet.microsoft.com/) [![SQLite](https://img.shields.io/badge/SQLite-003B57?logo=sqlite&logoColor=white&labelColor=555555)](https://www.sqlite.org/) [![Amazon S3](https://img.shields.io/badge/Amazon_S3-FF9900?logo=amazons3&logoColor=white)](https://aws.amazon.com/s3/)

[![Visual Studio](https://img.shields.io/badge/Visual_Studio-5C2D91?logo=visualstudio&logoColor=white)](https://visualstudio.microsoft.com/) [![Visual Studio Code](https://img.shields.io/badge/Visual_Studio_Code-007ACC?logo=visualstudiocode&logoColor=white)](https://code.visualstudio.com/)


## 기술적 특징

* [데이터베이스](#데이터베이스)
    * [DataReader](#datareader)
    * [Data Transfer Object (DTO)](#data-transfer-object-dto)
    * [Data Access Object (DAO)](#data-access-object-dao)
    * [데이터베이스 작동 방식](#데이터베이스-작동-방식)
* [전투 시스템](#전투-시스템)
    * [BattleManager](#battlemanager)
    * [BattlePhaseManager](#battlephasemanager)
    * [BattleEnemyManager](#battleenemymanager)
    * [BattleEventManager](#battleeventmanager)
    * [스테이터스 시스템](#스테이터스-시스템)
    * [전투 로직](#전투-로직)
* [UI](#ui)
    * [폰트](#폰트)
    * [텍스트 데이터](#텍스트-데이터)
    * [팝업 관리](#팝업-관리)
* [매니저 클래스](#매니저-클래스)
    * [Json 파일관리](#json-파일관리)
    * [클라우드 저장](#클라우드-저장)
    * [소셜 플랫폼 업적 진행도관리](#소셜-플랫폼-업적-진행도관리)
    * [유니티 분석](#유니티-분석)
    * [게임 초기화 로딩](#게임-초기화-로딩)

### [데이터베이스](#기술적-특징)

메인 게임 데이터 관리에는 **SQLite**를 사용합니다. SQLite는 서버 없이 단일 파일로 동작하는 경량 데이터베이스로, 모바일 환경에서도 빠른 읽기 성능과 적은 메모리 사용량을 제공합니다. 덕분에 게임 데이터를 하나의 DB 파일에서 효율적으로 관리할 수 있습니다.

* #### DataReader

    Mono.Data.Sqlite 라이브러리에서 제공하는 `SqliteDataReader`는 DB 조회 결과를 순회하며 읽는 기본 클래스입니다. 이 클래스를 직접 사용할 경우 `DBNull` 처리, 타입 변환, 리소스 해제 등을 매번 수동으로 처리해야 하므로, 이를 감싸는 `DataReader` 래퍼 클래스를 구현하여 안전하고 편리한 데이터 접근을 제공합니다.

    `SQLiteManager`는 DB 연결과 쿼리 실행을 담당하는 정적 클래스입니다. 게임 데이터, 플레이어 데이터, 유저 데이터 총 3개의 DB 파일을 관리하며, 각 DB에 대한 연결을 배열로 유지합니다. 연결이 닫혀있으면 자동으로 열고, DB 파일이 존재하지 않으면 StreamingAssets에서 복사하여 생성합니다. 씬이 언로드될 때 열려있는 모든 데이터 리더를 자동으로 종료하여 리소스 누수를 방지합니다.

    `TextParser`는 문자열 데이터를 특정 규칙에 따라 파싱하는 유틸리티 클래스입니다. 카드 설명 텍스트에 포함된 태그(`#damage:`, `#shield:` 등)를 실제 수치로 변환하고, 스탯 기반 계산을 수행합니다.

    * **타입 안전 변환**: 제네릭 메서드를 통해 `DBNull` 처리와 `Nullable` 타입 변환을 자동으로 수행합니다.
    * **Enum 자동 매칭**: 레벤슈타인 거리 알고리즘을 활용하여 문자열 오타나 표기 차이가 있어도 가장 유사한 Enum 값을 찾아 반환합니다.
    * **리소스 자동 관리**: `IDisposable` 인터페이스를 구현하여 `using` 문과 함께 사용 시 자동으로 리소스를 해제합니다.
```csharp
public static CustomDataReader SelectQuery(string query, ENUM_DATABASE_PATH enumDataBasePath = ENUM_DATABASE_PATH.GAME_DATA) {
    if (query == null) return null;

    try {
        OpenConnection(enumDataBasePath);
        using (SqliteCommand cmd = new SqliteCommand(query, connection[(int)enumDataBasePath])) {
            CustomDataReader customReader = new CustomDataReader(cmd.ExecuteReader());
            readerList.Add(customReader);
            return customReader;
        }
    }
    catch (Exception e) {
        // ... (에러 핸들링 및 DB 복구 로직 생략)
        return null;
    }
}
```

> **ParseStatusBasedCardDescriptionText**
>
> 카드 설명 텍스트에 포함된 동적 태그(예: `#damage`)를 파싱하여 실제 수치로 변환합니다. 카드의 현재 스탯 정보와 연동된 `StatusCalc` 메소드를 통해 최종 적용 수치를 계산하고, 이를 색상 태그가 포함된 문자열로 포맷팅하여 직관적인 툴팁을 생성합니다.

```csharp
public static string ParseStatusBasedCardDescriptionText(this string rawText, int[] status, Card card) {
    foreach (var item in textList) {
        if (item.Contains(damageTag) || item.Contains(shieldTag) || item.Contains(healTag)) {
            int baseValue = int.Parse(temp[temp.Length - 1]);
            int calcValue = Mathf.FloorToInt(StatusCalc(item, card.cardFactionEnum, baseValue));
            result += calcValue.ColoredStringValueWithValue(baseValue);
        }
    }
    return result;
}
```

> - [CustomDataReader.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/002_DataReader/CustomDataReader.cs)
> - [SQLiteManager.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/002_DataReader/SQLiteManager.cs)
> - [TextParser.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/002_DataReader/TextParser.cs)


* #### Data Transfer Object (DTO)

    물리적인 DB I/O 발생과 네트워크 환경에 따른 성능 저하를 방지하고자 데이터 객체 구조를 이원화하여 설계했습니다.

    * **메인 데이터 DTO**
        * 단일 쿼리로 모델의 모든 속성을 Fetch합니다.
        * 초기 로딩 시 데이터 통신 횟수(Round-trip)를 최소화하여 대량의 데이터를 한 번의 비용으로 수집합니다.
    * **최적화된 DTO**
        * 식별자와 실시간 동기화가 필요한 필수 상태값만 포함합니다.
        * 빈번한 업데이트나 대규모 인덱싱 작업 시 발생하는 I/O 부하를 최적화합니다.
    * **다형성 기반 인터페이스 설계**
        * 공통 인터페이스를 통해 서로 다른 타입의 DTO들을 일관된 방식으로 처리합니다.
        * 카드, 장비, 버프 등 다양한 게임 오브젝트가 동일한 인터페이스를 상속받아 통합 관리됩니다.
        * 새로운 DTO 타입 추가 시 기존 로직 수정 없이 확장이 가능합니다.

```csharp
// 메인 데이터 - 여러 DAO를 통해 조합되는 완전한 데이터 객체
public class PlayerInfo {
    public string name;
    public int statHp, statStr, statInt, statDex, gold, level, exp;  // 단순 데이터
    
    // 여러 DAO를 거쳐 클래스 형태로 로드되는 복합 데이터
    public Skill playerSkill;                                    // SkillDao.GetSkill(index)
    public StarterPack starterPack;                              // StarterPackDao.GetStarterPack(index)
    public Portrait portrait;                                    // PortraitDao.GetPortrait(index)
    public Dictionary<ENUM_EQUIPMENT_PART, Equipment> equipmentStatusDict;  // EquipmentDao.GetEquipment(index)
    public List<Card> cardDeckList;                              // CardDao.GetCard(index)
    public List<Area> clearedAreaList;                           // AreaDao.GetArea(index)
    // ...
}

// 최적화된 DTO - 인덱스만 포함하여 경량화
public class PlayerRawInfo {
    public string name;
    public int statHp, statStr, statInt, statDex, gold, level, exp;  // 동일한 단순 데이터
    
    // 클래스 대신 인덱스만 저장하여 I/O 최소화
    public int playerSkillIndex;
    public int? starterPackIndex;
    public int? portraitIndex;
    public int?[] equipedEquipment;                              // 인덱스 배열
    public List<CardLiteDBData> playerDeckIndexList;             // 경량화된 카드 데이터
    public List<int> clearedAreaIndexList;                       // 인덱스만 저장
    // ...
}
```

```csharp
// 인덱스 기반 DTO
public interface IIndexableDTO {
    int Index { get; set; }
}

// UI 렌더링용 DTO
public interface IRenderableData : IIndexableDTO {
    enum ItemType { card, skill, equipment, starterPack, portrait, buff }
    public ItemType datatype { get; set; }
    public Illustration Illust { get; set; }
}

// 카드 인터페이스
public interface ICard : IRenderableData, IRarity {
    public ENUM_CARD_TYPE CardType { get; set; }
}
```

> - [Data Classes](https://github.com/ysh4267/Yangjihoon_portfolio/tree/main/001_Shambles/001_Script/001_DataClass/Class)
> - [Interfaces](https://github.com/ysh4267/Yangjihoon_portfolio/tree/main/001_Shambles/001_Script/001_DataClass/Interface)


* #### Data Access Object (DAO)

    DAO는 데이터베이스 접근 로직을 캡슐화하여 비즈니스 로직과 데이터 접근을 분리합니다. 각 데이터 모델별로 전용 DAO 클래스를 구성하여 관련 쿼리와 파싱 로직을 집중 관리합니다. 특히 C#의 Boxing/Unboxing 과정에서 발생하는 성능 부하를 최소화하기 위해, 데이터 변환 로직을 쿼리 단계에서 처리하도록 DAO를 전문화하여 런타임 오버헤드를 줄였습니다.

    * **테이블명 상수화**: 테이블 이름은 `TableDefine` Enum으로 정의하여 오타를 방지하고 일관성을 유지합니다.
    * **다양한 쿼리 메소드**: 단일 조회, 전체 조회, 조건별 필터링 등 상황에 맞는 최적의 쿼리 메소드를 제공합니다.
    * **리소스 관리**: `DataReader`는 `using` 문 내에서 호출하여 사용 후 자동으로 리소스를 해제합니다.
    * **쿼리 최적화**: LEFT JOIN을 활용하여 관련 데이터를 단일 쿼리로 조회하고, 불필요한 Round-trip을 최소화합니다.

```csharp
public class CardDao : CollectionDao {
    // 단일 카드 조회 - CardTable + CardNameTable + CardTypeTable + ... LEFT JOIN
    public static Card GetCard(int cardIndex) {
        string query =
            $"SELECT ... FROM {DataBaseTableDefine.CardTable} " +
            $"LEFT JOIN {DataBaseTableDefine.CardNameTable} " +
            $"ON {DataBaseTableDefine.CardTable}.card_index = {DataBaseTableDefine.CardNameTable}.card_index " +
            $"LEFT JOIN {DataBaseTableDefine.CardTypeTable} ... " +
            $"WHERE {DataBaseTableDefine.CardTable}.card_index = {cardIndex}";
        
        DataReader it = SQLiteManager.SelectQuery(query);
        Card card = new Card();
        card.Illust = IllustrationDao.GetIllust(it.GetSafeValue<int>(2));  // 다른 DAO 호출
        // ...
        return card;
    }
    
    // 전투용 카드 조회 - GetCard() 호출 후 스크립트 테이블에서 추가 데이터 로드
    public static Card GetBattleCard(int cardIndex) {
        Card card = GetCard(cardIndex);  // 기본 카드 데이터 재사용
        card.battleCardScript = (IBattlePlayerCard)Activator.CreateInstance(...);
        return card;
    }
    
    // 해금된 카드 조회 - UnlockedCardTable(유저 DB)에서 인덱스 조회 후 GetCard() 호출
    public static List<Card> GetUnlockedCardList() {
        string query = $"SELECT card_index FROM {DataBaseTableDefine.UnlockedCardTable} " +
                       $"WHERE is_unlocked = 'true'";
        DataReader it = SQLiteManager.SelectQuery(query, ENUM_DATABASE_PATH.USER_DATA);
        // 각 인덱스로 GetCard() 호출하여 카드 객체 생성
    }
    
    // 카드 타입 텍스트 조회 - CardTypeTable 단독 조회
    public static string GetCardTypeText(ENUM_CARD_TYPE cardType) { ... }
}
```

> - [DAO Classes](https://github.com/ysh4267/Yangjihoon_portfolio/tree/main/001_Shambles/001_Script/001_DataClass/DAO)


### [데이터베이스 작동 방식](#기술적-특징)

---

**1. 데이터 호출**

게임 내에서 특정 데이터가 필요해지면 해당 DAO 클래스의 Get 메소드를 호출합니다. DAO 메서드는 필요한 데이터를 조회하기 위한 SQL 쿼리를 작성하며, 테이블 이름은 Enum 상수로 관리하여 오타를 방지합니다. 필요한 경우 여러 테이블을 LEFT JOIN하여 관련 데이터를 한 번에 가져옵니다.

> **GetData**
> 
> DAO 클래스의 데이터 조회 메서드입니다. 인덱스를 받아 해당 데이터를 DB에서 조회하고 DTO 객체로 반환합니다.

```csharp
public static ExampleDataClass GetData(int index) {
    string query =
        $"SELECT " +
        $"{TableDefine.DataTable}.data_index AS 'data_index', " +
        $"{TableDefine.DataTable}.data_name AS 'data_name', " +
        $"{TableDefine.DataTable}.data_value AS 'data_value', " +
        $"{TableDefine.TypeTable}.{SettingManager.CurrentLanguageString} AS 'type', " +
        $"{TableDefine.DescTable}.{SettingManager.CurrentLanguageString} AS 'description', " +
        // ... 중략 ...
        $"FROM {TableDefine.DataTable} " +
        $"LEFT JOIN {TableDefine.TypeTable} " +
        $"ON {TableDefine.DataTable}.type_index = {TableDefine.TypeTable}.type_index " +
        $"LEFT JOIN {TableDefine.DescTable} " +
        $"ON {TableDefine.DataTable}.data_index = {TableDefine.DescTable}.data_index " +
        // ... 중략 ...
        $"WHERE {TableDefine.DataTable}.data_index = {index}";
```

---

**2. SQLiteManager로 쿼리 실행**

작성된 쿼리를 `SQLiteManager.SelectQuery()`에 전달하면 DB 연결 상태를 확인하고 쿼리를 실행한 뒤, 결과를 순회할 수 있는 데이터 리더 객체를 반환합니다. 조회된 데이터를 직접 반환하지 않고 `DataReader` 객체를 반환하는 이유는 DAO마다 필요한 데이터 구조와 파싱 방식이 다르기 때문에, 데이터 변환 책임을 각 DAO에 위임하여 유연성을 확보하기 위함입니다.

```csharp
    DataReader it = SQLiteManager.SelectQuery(query);

    if (false == it.Read()) {
        return null;
    }
```

> **SelectQuery**
> 
> SQL 쿼리를 실행하고 결과를 순회할 수 있는 `DataReader` 객체를 반환합니다. DB 연결이 닫혀있으면 자동으로 열고, 오류 발생 시 에러 처리 후 복구를 시도합니다.

```csharp
public static DataReader SelectQuery(string query, ENUM_DATABASE_PATH enumDataBasePath = ENUM_DATABASE_PATH.GAME_DATA) {
    if (query == null) return null;

    try {
        OpenConnection(enumDataBasePath);
        using (SqliteCommand cmd = new SqliteCommand(query, connection[(int)enumDataBasePath])) {
            DataReader dataReader = new DataReader(cmd.ExecuteReader());
            readerList.Add(dataReader);
            return dataReader;
        }
    }
    catch (Exception e) {
        Debug.LogError(e.Message);
        HandleError();
        return null;
    }
}
```

---

**3. 데이터 리더로 데이터 파싱**

반환된 데이터 리더의 메서드들을 사용하여 DB에서 읽어온 원시 데이터를 DTO 객체의 적절한 타입으로 변환합니다.

```csharp
    ExampleDataClass data = new ExampleDataClass();
    data.Index = it.GetSafeValue<int>(0);
    data.Name = it.GetSafeValue<string>(1);
    data.Value = it.GetSafeValue<int>(2);
    data.Type = it.GetEnumFromString<ENUM_DATA_TYPE>(3);
    return data;
}
```

> **GetSafeValue\<T\>**
> 
> DB 컬럼의 값을 지정한 타입 `T`로 변환합니다. `DBNull`이 들어오면 해당 타입의 기본값을 반환합니다.

```csharp
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
```

---


### [전투 시스템](#기술적-특징)

전투 시스템은 싱글톤 패턴의 `BattleManager`를 중심으로 설계되었습니다. 카드, 버프, 장비, 적 등 전투에 참여하는 모든 오브젝트는 다형성 기반의 인터페이스를 상속받아 캡슐화되어 있지만, 실제 효과 발동과 상호작용은 대부분 매니저를 통해 수행됩니다. 이는 오브젝트 간 직접 참조를 방지하고, 새로운 오브젝트 추가 시 기존 로직 수정 없이 확장할 수 있도록 설계한 것입니다.

전투 시작 시 플레이어 고유 시드값으로 랜덤 상태를 초기화하여, 동일한 행동 시퀀스가 항상 동일한 결과를 보장합니다. 게임을 종료 후 다시 접속해도 동일한 선택을 한다면 같은 결과로 이어집니다. Enum 비트 연산을 활용한 복합 타겟팅 시스템으로 단일 메서드 호출로 여러 대상에게 효과를 적용하며, 옵저버 패턴을 통해 버프, 장비, 업적 등이 전투 이벤트를 구독하여 느슨한 결합을 유지합니다.

아래는 전투 시스템의 핵심 로직을 담당하는 4개의 특징적인 매니저 클래스에 대한 설명입니다.


* #### BattleManager

    전투 시스템의 핵심 싱글톤 매니저 클래스입니다. 전투 진행에 필요한 모든 서브 매니저와 UI 컴포넌트를 통합 관리합니다.

    * **서브 매니저 통합**: `BattlePhaseManager`, `BattleEnemyManager`, `BattleEventManager`, `BattleCardManager` 등 모든 전투 관련 매니저를 소유
    * **전투 상태 관리**: 전투 시작/종료 조건 확인, 승리/패배 처리
    * **UI 매니저**: 플레이어 상태 UI, 적 배치, 오브젝트 풀, 스킬 입력 시스템, 설명 팝업, 대화창, 전투 연출 등
    * **업적 시스템 연동**: 전투 시작/종료 시 다양한 조건의 업적 달성 여부를 자동으로 체크

* #### BattlePhaseManager

    전투 페이즈 흐름을 제어하는 매니저 클래스입니다. 턴 시작/종료, 전투 시작/종료 등 페이즈별 등록된 델리게이트를 실행하고 콜렉터를 관리합니다.

    * **페이즈 델리게이트 시스템**: `IBattlePhaseEffect` 인터페이스를 통해 버프, 장비, 패시브 등이 특정 페이즈에 동작을 등록
    * **타겟-액션 조합**: `(ENUM_BATTLE_PHASE_TARGET, ENUM_BATTLE_PHASE_ACTION)` 튜플 키로 등록된 동작 관리
    * **일회용 페이즈 동작**: n턴 후 자동 실행 및 제거되는 `DisposablePhaseEffect` 지원
    * **페이즈 콜렉터**: 특정 시점 사이에 발생한 이벤트를 수집하여 카운트 기반 로직 구현

* #### BattleEnemyManager

    전투 중 적 개체들의 생명주기와 행동을 총괄하는 매니저 클래스입니다. 적 생성/사망, 턴 진행, 타겟팅, UI 갱신 등을 담당합니다.

    * **적 리스트 관리**: 최대 4마리까지의 적 오브젝트를 딕셔너리로 관리
    * **패턴 기반 행동**: 적의 턴마다 `ProceedEnemyPattern`을 통해 순차적으로 행동 실행
    * **동적 적 추가/제거**: 전투 중 적 소환, 사망, 교체 처리
    * **타겟팅 시스템**: 플레이어 카드 사용 시 타겟 지정 UI 연동

* #### BattleEventManager

    전투 중 발생하는 이벤트 델리게이트를 관리하는 클래스입니다. 옵저버 패턴을 통해 전투 내 다양한 이벤트를 구독할 수 있습니다.

    * **턴 이벤트**: `OnTurnStart`, `OnUseCard`, `OnDrawCard` 등
    * **피해/회복 이벤트**: `OnTargetDamaged`, `OnTargetGainHp`, `OnTargetGainShield` 등
    * **버프 이벤트**: `OnTargetGainBuff`, `OnTargetLoseBuff`
    * **수치 수정 함수**: `DamageAddition`, `CostSet` 등 Func 델리게이트로 동적 수치 계산

```csharp
public class BattleManager : MonoBehaviour, IDisposable {
    // 전투 진행 핵심 매니저 클래스
    [SerializeField] public BattlePhaseManager battlePhaseManager = null;
    [SerializeField] public BattlePlayerObject battlePlayerObject = null;
    [SerializeField] public BattleEnemyManager battleEnemyManager = null;
    [SerializeField] public BattleEventManager battleEventManager = null;
    [SerializeField] public BattleCardManager battleCardManager = null;
    // ...
}
```

> **ProceedEnemyPattern**
>
> 재귀 호출을 사용하여 적들의 턴을 순차적으로 진행합니다. `BattleManager.IsBattleEnd`를 체크하여 전투 종료 시 중단하고, 현재 타겟이 `ENEMY3`를 초과하면 플레이어 턴(`TURN_START_STAND_BY`)으로 넘깁니다. 타겟 적이 존재하지 않을 경우 비트 시프트 연산(`(int)current << 1`)을 통해 다음 순번의 적을 즉시 탐색합니다.

```csharp
public void ProceedEnemyPattern(ENUM_BATTLE_PHASE_TARGET current) {
    if (BattleManager.GetInstance().IsBattleEnd == true) return;
    if (current > ENEMY3) {
        BattleManager.GetInstance().battlePhaseManager.ProceedPhase(PLAYER, TURN_START_STAND_BY);
        return;
    }
    var targetEnemy = GetEnemyObject(current);
    if (targetEnemy == null) {
        ProceedEnemyPattern((ENUM_BATTLE_PHASE_TARGET)((int)current << 1));
        return;
    }
    StartCoroutine(ProceedEnemy(targetEnemy));
}
```

> **ProceedEnemyAction**
>
> 비트 플래그(`HasFlag`)를 활용하여 다수의 적에게 일괄적으로 특정 `Action`을 수행합니다. `BattleEnemyObjectList`를 순회하며 입력받은 `enemyTargets` 플래그에 포함된 적(`targetEnemy`)에게만 델리게이트로 전달받은 로직을 실행합니다.

```csharp
public void ProceedEnemyAction(ENUM_BATTLE_PHASE_TARGET enemyTargets, Action<BattleEnemyObject> action) {
    for (int i = 0; i < BattleEnemyObjectList.Count; i++) {
        var targetEnemy = BattleEnemyObjectList[i];
        if (enemyTargets.HasFlag(targetEnemy.enemyPhaseTargetEnum)) {
            action(targetEnemy);
        }
    }
}
```

```csharp
// BattleEventManager - 전투 이벤트 델리게이트
public class BattleEventManager : MonoBehaviour {
    // 턴/카드 이벤트
    public Action OnTurnStart;
    public Action<Card> OnUseCard = null;
    public Action<Card> OnDrawCard = null;
    
    // 수치 수정 Func 델리게이트
    public Func<IBattlePlayerCard, int> DamageAddition = null;
    public Func<IBattlePlayerCard, int> CostSet = null;
    
    // 타겟 기반 이벤트
    public Action<IBattleStatus, IBattleFactor, int> OnTargetDamaged = null;
    public Action<IBattleStatus, IBattleFactor, int> OnTargetGainShield = null;
    public Action<IBattleStatus, IBattleFactor, Buff, int> OnTargetGainBuff = null;
    public Action<BattleEnemyObject> OnEnemyDead = null;
}
```

> - [BattleManager.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/005_Battle/Manager/BattleManager.cs)
> - [BattlePhaseManager.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/005_Battle/Manager/BattlePhaseManager.cs)
> - [BattleEnemyManager.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/005_Battle/Manager/BattleEnemyManager.cs)
> - [BattleEventManager.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/005_Battle/Manager/BattleEventManager.cs)


* #### 스테이터스 시스템

    전투 중 동적으로 변하는 수치들을 관리하는 시스템입니다. 핵심 인터페이스와 클래스를 통해 플레이어와 적의 상태를 일관되게 처리합니다.

    * **IBattleFactor**: 카드, 스킬, 적 행동, 버프 등 `BattleStatus`에 영향을 미치는 모든 요인을 통칭하는 마커 인터페이스입니다. 피해/회복의 원인을 추적하고 효과 발동 시 출처를 명확히 구분합니다.
    * **IBattleStatus**: 플레이어와 적의 전투 상태를 정의하는 인터페이스입니다. 속성(Attributes)과 행동(Action)을 분리하여 단일 책임 원칙을 준수합니다.
        * `IBattleStatusAttributes`: HP, AP, 실드, 스탯 배열, 동적 수치, 버프 카운터 등의 속성
        * `IBattleStatusAction`: 피해, 회복, 실드 획득, 버프 부여 등의 행동 메서드
    * **BattleStatus (복합 타겟 시스템)**: 여러 `IBattleStatus` 대상에게 동시에 효과를 적용하기 위한 복합 객체입니다. Enum 플래그 연산을 활용하여 유연한 타겟팅이 가능합니다.
        * Enum 비트 연산으로 다중 타겟 지정 (예: `~ENEMY1 & ALL` 구문은 적1을 제외한 모든 대상을 의미)
        * 단일 메서드 호출로 여러 대상에게 동시에 효과 적용
        * 타겟 추가/제거/교체 동적 관리.
    * **BattleDynamicValues**: 전투 중 동적으로 변화하는 상태값들을 관리하는 클래스입니다. 버프/디버프 효과, 키워드 상태, 장비 플래그, 스탯 보정치 등을 포함합니다.
        * 스탯 보정 시스템: 합연산(`statusAddition`), 곱연산(`statusMultiply`), 최종값(`statusFinal`) 분리
        * 이벤트 기반 UI 갱신: 스탯 변경 시 자동으로 `STAT_CHANGED` 이벤트 발생
        * 상태 플래그 관리: 관통, 경화, 은신, 무적, 기절 등 다양한 상태 효과
    * **BattleDamage**: 피해 정보를 캡슐화하는 데이터 클래스입니다. 연산자 오버로딩을 통해 피해량 계산을 직관적으로 처리합니다.

```csharp
[System.Flags]
public enum ENUM_BATTLE_PHASE_TARGET
{
    PLAYER  = 1, // 0b_0000_0001
    ENEMY1  = 2, // 0b_0000_0010
    ENEMY2  = 4, // 0b_0000_0100
    ENEMY3  = 8, // 0b_0000_1000
    ALL_ENEMIES   = ENEMY1 | ENEMY2 | ENEMY3, // 14
    ALL     = PLAYER | ENEMY1 | ENEMY2 | ENEMY3, // 15
    NONE         // 16
}
```

```csharp
// 여러 IBattleStatus를 타겟으로 사용하기 위한 복합 객체
public class BattleStatus : IBattleStatusAction {
    private List<IBattleStatus> Targets;
    
    // Enum 플래그 연산으로 다중 타겟 지정
    public BattleStatus(ENUM_BATTLE_PHASE_TARGET target) {
        if (target.HasFlag(playerStatus.TargetEnum)) { Targets.Add(playerStatus); }
        foreach (var enemy in enemyList) {
            if (target.HasFlag(enemy.TargetEnum)) { Targets.Add(enemy); }
        }
    }
    
    // 모든 타겟에 피해 적용
    public int Damage(IBattleFactor factor, BattleDamage amount) {
        ProceedTargetAction((target) => result += target.Damage(factor, amount));
        return result;
    }
}
```

> - [IBattleStatus.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/005_Battle/Interface/IBattleStatus.cs)
> - [IBattleStatusAttributes.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/005_Battle/Interface/IBattleStatusAttributes.cs)
> - [IBattleStatusAction.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/005_Battle/Interface/IBattleStatusAction.cs)
> - [BattleStatus.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/005_Battle/Status/BattleStatus.cs)
> - [BattlePlayerStatus.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/005_Battle/Status/BattlePlayerStatus.cs)
> - [BattleEnemyStatus.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/005_Battle/Status/BattleEnemyStatus.cs)
> - [BattlePlayerObject.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/005_Battle/Entity/BattlePlayerObject.cs)
> - [BattleEnemyObject.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/005_Battle/Entity/BattleEnemyObject.cs)
> - [ENUM_BATTLE_PHASE_TARGET.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/006_Enum/ENUM_BATTLE_PHASE_TARGET.cs)


* #### 전투 로직

    전투에서 사용되는 카드, 버프, 장비, 적 등의 오브젝트는 UI에 표시되는 데이터를 가진 상위 객체이며, 각 오브젝트는 실제 기능을 수행하는 **전투 로직 클래스**를 변수로 소유합니다. 이 클래스들은 공통 인터페이스를 상속받아 다형성 기반으로 동작하며, 효과 처리는 `BattleManager`를 통해 일관되게 수행됩니다.

    전투 로직 클래스는 DB에 저장된 클래스명(문자열)을 키로 사용하여, DAO에서 `Activator.CreateInstance`를 통해 런타임에 동적으로 인스턴스화됩니다. 이를 통해 새로운 카드나 버프를 추가할 때 코드 수정 없이 DB 데이터만 추가하면 됩니다.

    * **카드 (Card)**: 플레이어 카드와 적 카드 모두 `IBattleFactor`를 상속받아 동일한 효과 처리 파이프라인을 사용합니다.
        * `IBattlePlayerCard`: 세력, 코스트, 수치 데이터, 사용 후 목적지, 연계 장비 등
        * `IBattleEnemyCard`: 소유자/타겟 상태, 카드 초기화 및 발동 메서드
        * 적의 공격과 스킬도 동일한 카드 시스템을 기반으로 동작하여 로직 재사용성을 극대화했습니다.
    * **버프 (Buff)**: `IBattleBuff` 인터페이스를 통해 모든 버프/디버프를 일관되게 관리합니다.
        * 버프/디버프 유형 구분 (`ENUM_BUFF_TYPE`)
        * 카운터 감소 방식 지정 (`ENUM_BUFF_COUNTER_TYPE`: 턴 종료, 피격 시, 행동 시 등)
        * 활성화/종료 시점 콜백 메서드 제공
        * `IBattleBuffActive`, `IBattleBuffPassive`로 능동/수동 버프 분리
    * **장비 (Equipment)**: `IBattleEquipment` 인터페이스를 통해 장비 효과를 관리합니다. 장비 효과 또한 내부적으로는 `Buff` 형태로 생성되어 처리되므로, 적/플레이어의 행동과 유사한 메커니즘을 공유합니다.
    * **적 (Enemy)**: `IBattleEnemy`를 통해 행동 패턴을 결정하며, 공격/방어/버프 등의 모든 행위는 `IBattleEnemyCard`를 사용하여 플레이어가 카드를 사용하는 것과 완벽히 동일한 메커니즘으로 처리됩니다. 이를 통해 플레이어와 적의 로직이 일관성 있게 관리되며 자주 반복되는 효과의 구현이 간단해집니다.
        * 패턴 기반 행동 시스템 (공격, 방어, 버프 등) -> `EnemyCard` 사용
        * 플레이어 카드 로직과 동일한 파이프라인 공유
        * 적별 고유 사운드 에셋 관리

```csharp
// DAO에서 클래스명으로 동적 인스턴스 생성
card.battleCardScript = (IBattlePlayerCard)Activator.CreateInstance(Type.GetType(it.GetSafeValue<string>(0)));
enemy.enemyPattern = (IBattleEnemy)Activator.CreateInstance(Type.GetType(it.GetSafeValue<string>(1)));
```

```csharp
// 모든 전투 요인의 최상위 마커 인터페이스. 데미지 계산 및 효과 적용의 주체(Causer)로 추적됨.
public interface IBattleFactor { }

// 플레이어 카드와 적 스킬의 공통 인터페이스. 동일한 파이프라인에서 처리됨.
public interface IBattleCard : IBattleFactor {
    IBattleStatus OwnerStatus { get; set; } // 카드의 소유자 상태
    void ProceedCardAction(IBattleStatus cardTargetStatus = null); // 카드 효과 실행
}

// 플레이어 카드 인터페이스
public interface IBattlePlayerCard : IBattleCard { }

// 적 카드 인터페이스 (불필요한 오버헤드 제거 및 최적화 목적)
public interface IBattleEnemyCard : IBattleCard { }

// 버프 인터페이스. 턴/행동 기반으로 카운팅되며 효과를 발동함.
public interface IBattleBuff : IBattleFactor {
    void ActivateBuffEffect(); // 버프 효과 발동
}
```

> - [Battle Interfaces](https://github.com/ysh4267/Yangjihoon_portfolio/tree/main/001_Shambles/001_Script/005_Battle/Interface)
> - [Card.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/003_Object/Class/Card/PlayerCard/ExamplePlayerCard.cs)
> - [Skill.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/003_Object/Class/Card/SkillCard/ExampleSkillCard.cs)
> - [Buff.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/003_Object/Class/Buff/ExampleBuff.cs)
> - [Equipment.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/003_Object/Class/Equipment/ExampleEquipment.cs)
> - [Enemy.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/003_Object/Class/Enemy/ExampleEnemy.cs)
> - [EnemyCards](https://github.com/ysh4267/Yangjihoon_portfolio/tree/main/001_Shambles/001_Script/003_Object/Class/Card/EnemyCard)


---


### [UI](#기술적-특징)

다양한 플랫폼과 해상도 환경에 대응하기 위해, 기기별 UI 프리셋을 런타임에 적용하도록 구현했습니다. 반복적인 UI 작업을 자동화하여 개발 효율과 유지보수성을 확보했습니다.


* #### 폰트

    * **프리셋 기반 자동 조정**: `TextMeshPro`를 기반으로 언어와 플랫폼별 텍스트 크기와 폰트 스타일을 관리하는 프리셋 에디터를 사용합니다.
    * **환경 대응**: 각 텍스트 오브젝트가 자신의 프리셋 타입을 명시하여, 실행 환경(모바일/PC, 한국어/영어 등)에 맞춰 자동으로 최적의 표시 상태로 조정됩니다.

> **GetFontPresetData**
>
> 현재 설정된 언어에 맞는 폰트 프리셋 데이터를 반환합니다. 인덱스 범위를 초과하는 요청이 들어올 경우 기본값(마지막 프리셋)을 반환하여 런타임 에러를 방지합니다.

```csharp
public static FontPresetData GetFontPresetData {
    get {
        if ((int)SettingManager.CurrentLanguage >= FontPreset.Length) {
            // 인덱스 초과 시 기본값 반환
            FontPreset = new FontPresetData[4] { new FontPresetData(0), new FontPresetData(1), new FontPresetData(2), new FontPresetData(3) };
            return FontPreset[FontPreset.Length - 1];
        }
        return FontPreset[(int)SettingManager.CurrentLanguage];
    }
}
```

> **CurrentFontSize**
>
> 사용자의 텍스트 크기 설정(Small, Medium, Large)을 확인하여, 그에 대응하는 미리 정의된 폰트 사이즈 객체를 반환합니다.

```csharp
public static FontSize CurrentFontSize {
    get {
        switch (SettingManager.GetSettingData().textSize) {
            case ENUM_TEXT_SIZE.SMALL: return smallFont;
            case ENUM_TEXT_SIZE.MEDIUM: return mediumFont;
            case ENUM_TEXT_SIZE.LARGE: return largeFont;
            default: return mediumFont;
        }
    }
}
```

> - [FontPresetDefine.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/004_UI/Font/FontPresetDefine.cs)
> - [FontSizeDefine.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/004_UI/Font/FontSizeDefine.cs)


* #### 텍스트 데이터

    * **JSON-Enum 1:1 매칭**: 모든 게임 내 텍스트 데이터를 JSON 파일로 관리하며, 코드 내 Enum과 1:1로 매칭되어 유지보수성을 확보합니다.
    * **자동화된 다국어 처리**: 언어별 JSON 데이터만 변경하면 즉시 게임에 반영되며, 특정 언어 항목 누락 시 기본 언어 값으로 대체되는 페일세이프 기능을 지원합니다.
    * **오류 방지**: 키가 존재하지 않는 경우에도 자동으로 빈 문자열을 할당하여 런타임 에러를 방지합니다.

> **TextDefine**
>
> 현재 언어 설정에 맞는 텍스트 데이터를 반환합니다. 데이터가 메모리에 없는 경우 실시간으로 로드하여 반환하는 지연 로딩 방식을 사용합니다.

```csharp
public static TextDefineString Current {
    get {
        if (!DefineString.TryGetValue(SettingManager.CurrentLanguage, out var defineString)) {
            defineString = new TextDefineString(SettingManager.CurrentLanguage);
            DefineString.Add(SettingManager.CurrentLanguage, defineString);
        }
        return DefineString[SettingManager.CurrentLanguage];
    }
}
```

> **TextDefineString**
>
> 기본 언어 데이터를 먼저 로드한 뒤 선택된 언어 데이터를 덮어씌웁니다. 특정 키가 번역본에 누락되었더라도 기본 언어 값이 유지되어 텍스트가 비어 보이는 현상을 방지합니다.

```csharp
public TextDefineString(ENUM_LANGUAGE languageEnum) {
    // 기본 언어(한국어)를 먼저 로드한 뒤, 선택된 언어 데이터로 덮어쓰는 방식을 사용합니다.
    var baseLanguage = ENUM_LANGUAGE.ko_KR;
    JsonDataManager.ReadLanguageData<TextDefineStringData>(baseLanguage, out var baseTextData);
    textData = baseTextData.stringData;
    listTextData = baseTextData.listStringData;

    JsonDataManager.ReadLanguageData<TextDefineStringData>(languageEnum, out var overrideTextData);

    // 데이터 병합 (Override)
    foreach (var item in overrideTextData.stringData) {
        textData[item.Key] = item.Value;
    }
}
```

> - [TextDefine.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/004_UI/Font/TextDefine.cs)
> - [TextDefineString.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/004_UI/Font/TextDefineString.cs)
> - [TextDefineStringData.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/004_UI/Font/TextDefineStringData.cs)


* #### 팝업 관리

    * **팝업 객체 표준화**: `IPopup` 인터페이스를 상속받아 모든 팝업 객체를 통합 관리하고, `CloseAnimationPopup` 클래스를 통해 종료 시 공통된 DOTween 애니메이션 시퀀스가 실행되도록 구현하였습니다.

> **CloseAnimationPopup**
>
> 팝업 UI의 기반이 되는 클래스로, 팝업이 활성화되는 즉시 `GameManager`의 팝업 스택에 등록되어 '뒤로가기' 키를 통한 순차석 닫기를 지원합니다. 닫기 요청(Close Request) 발생 시 즉시 객체가 사라지지 않고, 지정된 축소 및 페이드 애니메이션을 모두 수행한 뒤에 비활성화 되도록 생명주기를 관리합니다.

```csharp
public class CloseAnimationPopup : MonoBehaviour, IPopup {
    // 닫기 요청 시 애니메이션 실행 후 비활성화 처리
    public virtual void ClosePopupRequest() {
        if (isAnimated == false) {
            ClosePopup();
            return;
        }
        
        // DOTween을 사용하여 스케일 축소 및 배경 페이드아웃 연출
        // 연출 종료 시 콜백(OnComplete)을 통해 오브젝트 비활성화 및 상태 초기화 수행
    }

    // 활성화/비활성화 시 매니저 스택 자동 관리
    protected virtual void OnEnable() {
        GameManager.GetInstance()?.PushPopup(this);
    }
    protected virtual void OnDisable() {
        GameManager.GetInstance()?.PopPopup();
    }
}
```

> - [IPopup.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/004_UI/Popup/IPopup.cs)
> - [CloseAnimationPopup.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/004_UI/Popup/CloseAnimationPopup.cs)
---
### [매니저 클래스](#기술적-특징)

게임의 핵심 기능을 담당하는 매니저 클래스들은 싱글톤 패턴이나 정적 클래스로 구현되어 전역적인 접근성을 제공하며, 각 기능별로 책임을 명확히 분리했습니다.

#### Json 파일관리

게임 데이터의 로컬 저장 및 관리를 위한 유틸리티 클래스입니다. `Newtonsoft.Json`을 래핑하여 직렬화/역직렬화를 수행하며, 보안이 필요한 데이터에 대해 암호화를 지원합니다.

*   **AES 암호화**: 배포 빌드(`Release`)에서는 데이터 무결성 보호를 위해 AES 알고리즘으로 파일을 암호화하여 저장합니다. (디버그 모드에서는 비활성화)
*   **데이터 무결성 검사**: 파일 로드 시 `CheckIntagrity` 메서드를 통해 클래스의 필드 및 프로퍼티 누락 여부를 검증하여 데이터 오염을 방지합니다.
*   **플랫폼별 파일 처리**: Android(UnityWebRequest)와 PC/iOS(FileStream) 환경에 맞춰 파일 읽기/쓰기 방식을 분기하여 처리합니다.

> **ReadData<T>**
>
> 제네릭 타입 `T`를 받아 파일 입출력을 수행합니다. 파일 시스템에서 JSON 텍스트를 읽어온 후, 암호화 플래그를 확인하여 필요한 경우 AES 복호화를 선행합니다. 이후 `Newtonsoft.Json`을 통해 객체로 역직렬화하고, `CheckIntagrity` 메소드로 데이터 무결성을 검증합니다. 검증 실패 시 `false`를 반환하여 데이터 오염을 알립니다.

```csharp
public static bool ReadData<T>(ENUM_JSON_FILE fileType, out T data, bool isIntact = false) where T : new() {
    string jsonData = File.ReadAllText(filePath);
    
    if (fileEncryptionStatus[fileType] == true) {
        jsonData = Decrypt(jsonData, fileNameList[fileType]);
    }

    data = JsonConvert.DeserializeObject<T>(jsonData, settings);

    if (!isIntact && !CheckIntagrity<T>(jsonData)) {
        data = new T();
        return false; 
    }
    return true;
}
```

> **CheckIntagrity<T>**
>
> `JObject`로 파싱된 JSON 데이터와 리플렉션으로 추출한 타입 `T`의 필드 정보를 대조합니다. 클래스에 정의된 모든 필드가 JSON 데이터 내에 키값으로 존재하는지 순회하며 검사합니다. 필수 데이터가 하나라도 누락되었을 경우 즉시 실패 처리하여 불완전한 데이터 로드를 차단합니다.

```csharp
private static bool CheckIntagrity<T>(string jsonData) where T : new() {
    JObject jsonObj = JObject.Parse(jsonData);
    Type type = typeof(T);

    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
        if (!jsonObj.TryGetValue(field.Name, StringComparison.OrdinalIgnoreCase, out JToken _)) {
            return false;
        }
    }
    return true;
}
```

> - [JsonDataManager.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/007_Manager/JsonDataManager.cs)

#### 클라우드 저장

Unity Services의 Cloud Save를 활용하여 플랫폼 간 데이터 동기화를 지원합니다. Google Play Games 및 Apple Game Center 로그인과 연동되어 데이터를 저장합니다.

*   **멀티 플랫폼 인증**: Android(Google Play), iOS(Game Center) 등 실행 플랫폼에 따라 적절한 인증 서비스를 자동으로 초기화하고 로그인합니다.
*   **비동기 처리**: `async/await` 패턴을 사용하여 대용량 데이터 저장/로드 시 메인 스레드 멈춤 현상(Freezing)을 방지했습니다.
*   **데이터 충돌 방지**: 클라우드 데이터와 로컬 데이터를 비교/검증하는 로직을 포함하여 데이터 손실을 최소화합니다.

> **SaveInternal**
>
> 데이터 저장 전 `Unity Services` 초기화 및 로그인 상태를 이중으로 점검하여 안전성을 확보합니다. 로그인이 되어있지 않다면 즉시 재로그인을 시도하며, 최종 실패 시 에러 콜백을 반환합니다. 모든 검증이 통과되면 `CloudSaveService` API를 통해 비동기적으로 데이터를 클라우드에 업로드합니다.

```csharp
async Task SaveInternal(string json, Action<CLOUD_SAVE_RESULT> onErrorCallback) {
    await UnityServices.InitializeAsync();
    
    if (!AuthenticationService.Instance.IsSignedIn) {
        await InitializeAndLoginAsync();
        
        if (!AuthenticationService.Instance.IsSignedIn) {
            onErrorCallback?.Invoke(CLOUD_SAVE_RESULT.ERROR_LOGIN_FAIL);
            return;
        }
    }

    await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
        { PlayerDataKey, json }
    });
}
```

> - [CloudSaveManager.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/007_Manager/CloudSaveManager.cs)

#### 소셜 플랫폼 업적 진행도관리

다양한 스토어(Steam, Google Play, App Store, Stove)의 업적 시스템을 단일 인터페이스로 통합 관리합니다. 플랫폼별 SDK 차이를 캡슐화하여 비즈니스 로직에서 플랫폼 의존성을 제거했습니다.

*   **통합 인터페이스**: `IncrementAchievementProgress`, `UnlockAchievements` 등 통일된 메서드로 모든 플랫폼의 업적을 제어합니다.
*   **진행도 누적 관리**: 단순 해금뿐만 아니라 진행형 업적(예: 100회 달성)의 진행도를 로컬 및 서버에 동기화합니다.
*   **ID 매핑 시스템**: 플랫폼별로 상이한 업적 ID를 내부 Enum 키와 매핑하여 코드 일관성을 유지합니다.

> **OnAchievementUnlocked**
>
> 특정 업적 해금 요청 시, 현재 빌드된 타겟 플랫폼(`STEAMWORKS_NET`, `PLAY_STORE`, `APP_STORE`)에 맞춰 적절한 API를 호출합니다. Steam의 `UnlockAchievement`나 모바일의 `Social.ReportProgress` 등 상이한 플랫폼별 구현을 캡슐화하여, 비즈니스 로직에서는 단일 메소드 호출만으로 모든 플랫폼에 대응할 수 있습니다.

```csharp
public static void OnAchievementUnlocked(int achievementIndex) {
#if STEAMWORKS_NET
    if (SteamManager.Initialized) {
        SteamManager.Instance.UnlockAchievement($"achievement_{achievementIndex}");
    }
#elif PLAY_STORE
    Social.ReportProgress(GetAchievementGoogleIDFromIndex(achievementIndex), 100.0f, (bool success) => { });
#elif APP_STORE
    Social.ReportProgress(GetAchievementIDFromIndex(achievementIndex), 100.0f, (bool success) => { });
#endif
}
```

> - [AchievementEventManager.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/007_Manager/AchievementEventManager.cs)

#### 유니티 분석

유저의 행동 데이터를 수집하여 게임 밸런스 조정 및 개선에 활용하기 위한 분석 모듈입니다. 수집된 로그는 Unity Dashboard의 **SQL Data Explorer**를 활용하여 복합적인 상관관계를 분석하는 데 사용됩니다.

*   **Type-Safe 이벤트 파라미터**: `UnityAnalyticsEvent` 내부 클래스를 통해 파라미터 타입을 강제하여, 잘못된 데이터 타입 전송으로 인한 로그 누락을 방지합니다.
*   **이벤트 키 관리**: Enum 키를 사용하여 이벤트 명칭의 오타를 방지하고 관리를 용이하게 했습니다.
*   **SQL 기반 고차원 분석**: 단순한 지표 모니터링을 넘어, 여러 이벤트 간의 인과관계를 쿼리로 분석합니다. (※ 개인 식별 정보는 수집하지 않습니다.)
    *   *활용 예시*: "특정 이벤트를 진행한 유저 집단이 이후 전투에서 특정 카드를 덱에 포함시킨 비율" 등을 쿼리하여 스토리 몰입도와 전략 선택의 상관관계를 도출할 수 있습니다.

> **UnityAnalyticsEvent**
>
> `Event` 클래스를 상속받아 파라미터 타입을 강력하게 규제합니다. 프로퍼티 Setter 내부에서 `HasValue`나 `IsNullOrEmpty` 체크를 수행하여, 유효하지 않은 데이터(Null)가 전송되는 것을 원천적으로 차단하고 필수 데이터만 선별적으로 로그에 포함시킵니다.

```csharp
public class UnityAnalyticsEvent : Event {
    public UnityAnalyticsEvent(string EventName) : base(EventName) { }
    
    public int? example_param_int { set { if (value.HasValue) SetParameter("example_param_int", value.Value); } }
    public string example_param_str { set { if (!string.IsNullOrEmpty(value)) SetParameter("example_param_str", value); } }
}
```

> - [UnityAnalyticsManager.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/007_Manager/UnityAnalyticsManager.cs)

#### 게임 초기화 로딩

게임 시작 시 필요한 모든 리소스와 데이터를 순차적으로 로드하고 무결성을 점검하는 초기화 파이프라인입니다.

*   **순차적 로딩 프로세스**: 리소스 다운로드 -> 분석 툴 초기화 -> 데이터 경로 설정 -> 로그인 -> 버전 체크 -> DB 로드 순으로 안전하게 게임을 초기화합니다.
*   **버전 제어 및 업데이트**: 앱 버전과 데이터 버전을 분리 관리하여, 앱 업데이트 없이 데이터 패치만으로 밸런스를 수정할 수 있는 구조를 구축했습니다.
*   **공지사항 시스템**: 이미지 캐싱을 지원하는 공지사항 팝업 데이터를 로딩 중에 미리 받아와 메인 화면 진입 시 즉시 표시합니다.

> **Start (Loading Sequence)**
>
> 게임 초기화의 전체 흐름을 제어하는 코루틴입니다. `SocialLogin`(소셜 로그인), `CheckVersion`(무결성 검사), `LoadProductData`(구매 복원), `LoadData`(로컬 데이터 로드) 등 각 단계를 순차적으로 실행하며, `yield return`을 통해 앞선 프로세스가 완벽히 종료된 후에만 다음 단계로 진입하여 초기화 순서를 보장합니다.

```csharp
IEnumerator Start() {
    yield return SocialLogin();

    yield return CheckVersion();

    ProductPurchasedStatusLoader productLoader = new ProductPurchasedStatusLoader();
    yield return productLoader.LoadProductData();

    yield return LoadData(ENUM_JSON_FILE.GameData);
}
```

> - [LoadingSceneManager.cs](https://github.com/ysh4267/Yangjihoon_portfolio/blob/main/001_Shambles/001_Script/007_Manager/LoadingSceneManager.cs)