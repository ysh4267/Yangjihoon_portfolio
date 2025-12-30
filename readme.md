# 🎮 Unity & C# Developer

### 🛠 기술 역량

.NET C#, Unity C#, SQLite, SQL, Python

---

## 🏆 Main Project: [샴블즈 (Shambles)](https://play.google.com/store/apps/details?id=com.gravity.shambles.aos&hl=ko)

**Role**: 리드 프로그래머 (Lead Programmer)

#### 📋 핵심 개발 및 설계

* **Sqlite (DAO, DTO)**

  * **Data Transfer Object (DTO)**: 데이터 구조 정의 및 객체 맵핑
    * **데이터 통신 비용 최적화 전략**: 물리적인 DB I/O 및 네트워크 환경에 따른 성능 저하를 방지하기 위해 데이터 객체 구조를 이원화하여 설계
    * **메인 데이터 DTO**: 단일 쿼리로 모델의 모든 속성을 Fetch. 초기 로딩 시 데이터 통신 횟수(Round-trip)를 최소화하여 대량의 데이터를 한 번의 비용으로 수집
    * **최적화된 DTO**: 식별자와 실시간 동기화가 필요한 필수 상태값만 포함. 빈번한 업데이트나 대규모 인덱싱 작업 시 발생하는 I/O 부하를 최적화
    * 📄 [Card.cs](https://github.com/[사용자ID]/[레포지토리명]/blob/main/Assets/Scripts/Data/Card.cs) | [CardLiteDBData.cs](https://github.com/[사용자ID]/[레포지토리명]/blob/main/Assets/Scripts/Data/CardLiteDBData.cs)

    ```csharp
    // [Heavy DTO] Card.cs - 통신 횟수 최소화 모델
    [System.Serializable]
    public class Card : ICollectionDTO {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int cost;
        public Illustration Illust { get; set; } // 리소스 정보 포함
        public List<ENUM_CARD_PROPERTY> cardPropertyList; // 관계 데이터 포함
    }

    // [Lite DTO] CardLiteDBData.cs - I/O 비용 최적화 모델
    public class CardLiteDBData : IIndexableDTO {
        public int Index { get; set; }       // 식별자
        public int temporaryID { get; set; } // 세션 식별용
        public bool isInDeck { get; set; }   // 실시간 상태값
        public bool isFixedInDeck { get; set; }
    }
    ```

  * **Data Access Object (DAO)**: SQL 쿼리 실행 및 DB 통신 로직 캡슐화
    * 📄 [CardDao.cs](https://github.com/[사용자ID]/[레포지토리명]/blob/main/Assets/Scripts/Database/CardDao.cs)

    ```csharp
    // [DAO] CardDao.cs - 통신 레이어
    public class CardDao : CollectionDao {
        public static Card GetCard(int cardIndex) {
            // 복잡한 JOIN 쿼리를 통해 통신 비용을 1회로 제한
            string query = $"SELECT c.card_index, c.cost, n.text, d.text FROM CardTable c ...";

            using (ExlixDataReader it = SQLiteManager.SelectQuery(query)) {
                if (!it.Read()) return null;
                return new Card {
                    Index = it.GetSafeValue<int>(0),
                    cost = it.GetSafeValue<int>(1),
                    Name = it.GetSafeValue<string>(2),
                    Description = it.GetSafeValue<string>(3)
                };
            }
        }
    }
    ```

  * **Manager & Reader**: DB 연결 관리 및 데이터 파싱 유틸리티
    * 📄 [SQLiteManager.cs](https://github.com/[사용자ID]/[레포지토리명]/blob/main/Assets/Scripts/Database/SQLiteManager.cs) | [ExlixDataReader.cs](https://github.com/[사용자ID]/[레포지토리명]/blob/main/Assets/Scripts/Database/ExlixDataReader.cs)

    ```csharp
    // [Manager] SQLiteManager.cs - DB 커넥션 종단점
    public static class SQLiteManager {
        public static ExlixDataReader SelectQuery(string query, ENUM_DATABASE_PATH path) {
            OpenConnection(path);
            SqliteCommand cmd = new SqliteCommand(query, connection[(int)path]);
            return new ExlixDataReader(cmd.ExecuteReader());
        }
    }

    // [Reader] ExlixDataReader.cs - 데이터 가공 및 예외 처리
    public class ExlixDataReader : IDisposable {
        public T GetSafeValue<T>(int colIndex) {
            object theValue = dataReader.GetValue(colIndex);
            if (DBNull.Value != theValue) {
                return (T)Convert.ChangeType(theValue, typeof(T));
            }
            return default; // DBNull 세이프 처리
        }
    }
    ```

* **Game Architecture**: 싱글톤 및 상태 패턴 기반의 확장성 있는 게임 루프 설계
* **Scripting**: 카드 배틀 시스템 로직 및 덱 빌딩 알고리즘 구현
* **UI/UX Framework**: 효율적인 UI 관리를 위한 MVC 패턴 기반 UI 매니저 구축
* **Optimization**: 가비지 컬렉션(GC) 최소화 및 런타임 메모리 사용량 최적화

---

### 🚀 Contact

* **Email**: your-email@example.com
