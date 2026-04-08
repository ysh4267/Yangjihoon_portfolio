# 양지훈

## 기술 역량

### 핵심 기술
* .NET C#
* Unity C#
* Unity Engine
* SQLite

### 기타 기술
* Python
* SQLite에 기반한 데이터베이스 ERD 구축
* Android, iOS, PC, Steam 빌드 및 배포 환경 구축
* Steamworks API 및 글로벌 소셜 시스템 연동
* Firebase 및 Unity Analytics 기반 로그/백엔드 적용
* Amazon S3를 활용한 데이터 동기화 및 클라우드 관리

---

# 프로젝트

## 1. [[샴블즈(Shambles)]](https://github.com/ysh4267/ysh4267/tree/main/001_Shambles)
[![Unity](https://img.shields.io/badge/Unity-000000?logo=unity&logoColor=white&labelColor=555555)](https://unity.com/) [![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](https://dotnet.microsoft.com/ko-kr/languages/csharp) [![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=white&labelColor=555555)](https://dotnet.microsoft.com/) [![SQLite](https://img.shields.io/badge/SQLite-003B57?logo=sqlite&logoColor=white&labelColor=555555)](https://www.sqlite.org/) [![Amazon S3](https://img.shields.io/badge/Amazon_S3-FF9900?logo=amazons3&logoColor=white)](https://aws.amazon.com/s3/)

* [Steam 상점 페이지](https://store.steampowered.com/app/2289630/_/?l=koreana)
* [Google Play 상점 페이지](https://play.google.com/store/apps/details?id=com.gravity.shambles.aos)
* [App Store 상점 페이지](https://apps.apple.com/kr/app/%EC%83%B4%EB%B8%94%EC%A6%88-%EC%A2%85%EB%A7%90%EC%9D%98-%ED%9B%84%EC%86%90%EB%93%A4/id6740197039)

### 트레일러 영상
<a href="https://youtu.be/nS2DFa193OY"><img src="https://img.youtube.com/vi/nS2DFa193OY/maxresdefault.jpg" width="800"></a>

### 프로젝트 정보
* 작업기간: 2023 ~ 2025
* 인원: 기획 2명, 아트 4명, 프로그래밍 3명
* 역할: 리드 프로그래머
* 프로젝트 종류: Unity
    * 장르: 텍스트 RPG, 덱빌딩, 로그라이크
    * 플랫폼: Android, iOS, PC (Steam)
    * 시점: 2D

### 게임 개요
포스트 아포칼립스 세계관을 배경으로 하는 텍스트 RPG, 덱빌딩, 로그라이크 결합 장르입니다. 벙커에서 나온 탐험가가 되어 500년 뒤 변해버린 세상을 탐험하고 수많은 선택을 통해 세계의 운명을 결정하는 시나리오를 포함합니다. 선택에 따라 결과가 달라지는 수많은 분기점과 멀티 엔딩 시스템을 제공하며, 300종 이상의 카드와 200종 이상의 스킬 및 장비를 통한 전략적 덱빌딩 전투가 가능합니다. 100개 이상의 구역과 숨겨진 던전으로 구성된 광대한 세계 탐험과 도감 시스템을 지원합니다.

#### 특징
* SQLite 기반 DataReader, DTO, DAO 아키텍처로 데이터 모델과 접근 로직을 분리하여 대규모 게임 데이터 관리
* 다형성 인터페이스와 매니저 중심 설계로 전투 시스템을 구현하고 시드 기반 결정적 랜덤으로 동일 결과 보장
* 언어 및 플랫폼별 폰트 프리셋, 동적 수치 태그 파싱, 팝업 애니메이션 등 UI 시스템 구축
* JSON 암호화 저장, 클라우드 동기화, 업적, 데이터 분석 등 게임 서비스 매니저 구현
* Excel 데이터 워크플로우 및 외부 파싱 도구를 활용한 개발 환경 구축

### 관련 미디어
<img src="https://img.youtube.com/vi/nS2DFa193OY/1.jpg" width="200"> <img src="https://img.youtube.com/vi/nS2DFa193OY/2.jpg" width="200"> <img src="https://img.youtube.com/vi/nS2DFa193OY/3.jpg" width="200">

---

## 2. [[CrossingHighway]](https://github.com/ysh4267/CrossingHighway)
[![C++](https://img.shields.io/badge/C++-00599C?logo=cplusplus&logoColor=white)](https://isocpp.org/) [![DirectX 10](https://img.shields.io/badge/DirectX_10-006600?logo=microsoft&logoColor=white)](https://learn.microsoft.com/en-us/windows/win32/direct3d10/d3d10-graphics) [![HLSL](https://img.shields.io/badge/HLSL-5C2D91?logoColor=white)](https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl)

### 플레이 영상
[![CrossingHighway 플레이 영상](https://img.youtube.com/vi/JVxjvq_K-Mc/0.jpg)](https://youtu.be/JVxjvq_K-Mc)

### 프로젝트 정보
* 작업기간: 2021.05 ~ 2021.06
* 인원: 프로그래밍 2명
* 역할: 프로그래머
* 프로젝트 종류: Direct 3D
  * 장르: 3D 아케이드
  * 플랫폼: Windows Desktop
  * 시점: 3D

### 게임 개요
3D로 구현한 크로싱 하이웨이 스타일의 아케이드 게임입니다. 플레이어는 차량이 달리는 도로를 피해 앞으로 전진하며, 점수에 따라 차량 속도와 BGM이 변화하는 난이도 시스템을 포함합니다.

#### 특징
* Direct3D 11 기반 렌더링 파이프라인을 구축하여 HLSL 셰이더, OBJ 모델 로딩, WIC/DDS 텍스처 시스템 구현
* AABB 충돌 감지로 96대 차량 및 벽 오브젝트와의 충돌 처리
* 2개의 맵 세그먼트 교대 배치로 무한 스크롤 맵 구현
* 점수 기반 난이도 시스템으로 차량 가속 및 BGM 단계별 전환

---

## 3. [[Herogue]](https://github.com/ysh4267/Herogue)
[![Unity](https://img.shields.io/badge/Unity-000000?logo=unity&logoColor=white&labelColor=555555)](https://unity.com/) [![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](https://dotnet.microsoft.com/ko-kr/languages/csharp)

### 프로젝트 정보
* 작업기간: 2021.10 ~ 2021.12
* 인원: 개인 프로젝트
* 프로젝트 종류: Unity
    * 장르: 3D 로그라이크 액션
    * 플랫폼: Windows Desktop
    * 시점: 3D

### 게임 개요
3D 로그라이크 액션 게임입니다. 절차적으로 생성되는 맵을 탐험하며, 근접/원거리 유형의 적과 다중 패턴 보스를 처치하고 스테이지를 클리어하는 구조입니다.

#### 특징
* 10x10 그리드 기반 Flood Fill 방식의 절차적 맵 생성 및 방 타입 자동 배치 시스템 구현
* 코루틴 기반 FSM 패턴으로 근접, 원거리, 보스 유형의 적 AI 상태 머신 구현
* 레이캐스트와 Vector3.Reflect를 활용한 투사체 반사 경로 시각화 및 탄막 패턴 구현
* 방 기반 카메라 전환 및 몬스터 리스트 추적을 통한 클리어 판정 시스템 구현

---

## 외부도구

### 1. [[DataParser]](https://github.com/ysh4267/ysh4267/tree/main/900_ExternalTool/001_DataParser)
> C#, .NET Framework, SQLite, Excel Interop

#### 개요
엑셀 형식의 기획 데이터를 SQLite 데이터베이스 및 JSON 포맷으로 변환하는 전용 파싱 엔진입니다. 기획 데이터의 수동 반영 과정을 자동화하여 개발 워크플로우를 개선하기 위해 제작했습니다.

#### 특징
* 멀티쓰레딩 기법과 COM 객체 호출 최적화를 통한 대용량 데이터 파싱 속도 개선
* 프로젝트 환경에 최적화된 SQLite 데이터베이스 및 JSON 포맷 결과물 생성
* 데이터 정합성 유지를 위한 파싱 엔진 최적화

### 2. [[DataIntegrityChecker]](https://github.com/ysh4267/ysh4267/tree/main/900_ExternalTool/002_DataIntegrityChecker)
> Python, SQLite, Regex, Pandas

#### 개요
다국어 텍스트 데이터의 결함을 탐지하고 정합성을 검증하기 위한 자동화 검사 도구입니다. 대규모 다국어 데이터의 품질 관리 프로세스를 구축하기 위해 제작했습니다.

#### 특징
* 정규 표현식을 활용하여 언어별 특수 태그 및 제어 문자의 일치 여부 전수 조사
* SQLite 및 Pandas를 활용한 대규모 원문 데이터의 구조적 분석 및 결함 탐지
* 검사 결과를 리포트로 생성하여 데이터 정합성 관리 효율성 증대

### 3. [[BulkEmailSender]](https://github.com/ysh4267/ysh4267/tree/main/900_ExternalTool/003_BulkEmailSender)
> C#, .NET Framework, SMTP, Excel Interop

#### 개요
엑셀 수신자 목록을 기반으로 개별 맞춤형 메일을 대량 발송하는 유틸리티입니다. 대규모 메일 발송 작업 중에도 안정적인 실행 환경을 유지하기 위해 제작했습니다.

#### 특징
* 비동기 처리 방식을 도입하여 대량 발송 시에도 UI 프리징 방지 및 안정성 확보
* SMTP 프로토콜을 직접 구현하여 전송 과정의 정확도와 보안성 강화
* 엑셀 Interop을 활용한 수신자 리스트 및 메일 내용 매칭 최적화

---

## 연락처
* Email: ysh4267@gmail.com
