# 양지훈

## 기술 역량

### 핵심 기술
* .NET C#
* Unity C#
* Unity Engine
* SQLite를 활용한 데이터베이스 ERD 구축

### 기타 기술
* Python
* Android, iOS, PC, Steam 빌드 및 배포 환경 구축
* Steamworks API 및 글로벌 소셜 시스템 연동
* Firebase 및 Unity Analytics 기반 로그/백엔드 적용
* Amazon S3를 활용한 데이터 동기화 및 클라우드 관리

---

# 프로젝트

## 1. [[샴블즈(Shambles)]](https://github.com/ysh4267/Yangjihoon_portfolio/tree/main/001_Shambles)
> Unity, C#, SQLite

* [Steam 상점 페이지](https://store.steampowered.com/app/2289630/_/?l=koreana)
* [Google Play 상점 페이지](https://play.google.com/store/apps/details?id=com.gravity.shambles.aos)
* [App Store 상점 페이지](https://apps.apple.com/kr/app/%EC%83%B4%EB%B8%94%EC%A6%88-%EC%A2%85%EB%A7%90%EC%9D%98-%ED%9B%84%EC%86%90%EB%93%A4/id6740197039)

### 게임 개요
포스트 아포칼립스 세계관을 배경으로 하는 텍스트 RPG, 덱빌딩, 로그라이크 결합 장르입니다. 벙커에서 나온 탐험가가 되어 500년 뒤 변해버린 세상을 탐험하고 수많은 선택을 통해 세계의 운명을 결정하는 시나리오를 포함합니다. 선택에 따라 결과가 달라지는 수많은 분기점과 멀티 엔딩 시스템을 제공하며, 300종 이상의 카드와 200종 이상의 스킬 및 장비를 통한 전략적 덱빌딩 전투가 가능합니다. 100개 이상의 구역과 숨겨진 던전으로 구성된 광대한 세계 탐험과 도감 시스템을 지원합니다.

### 특징
* SQLite 데이터베이스를 이용한 대규모 카드 시너지 및 선택지 분기 데이터 관리
* 데이터 모델과 접근 로직을 분리한 DTO 및 DAO 아키텍처 설계
* 인터페이스 다형성을 활용한 확장성 있는 전투 엔진 및 복합 타겟팅 전투 로직 구현
* 언어 및 플랫폼별 최적화된 표시를 지원하는 폰트 프리셋 및 텍스트 파싱 시스템 구축

### 관련 미디어
[![프로젝트 소개 영상](https://img.youtube.com/vi/nS2DFa193OY/maxresdefault.jpg)](https://www.youtube.com/watch?v=nS2DFa193OY)

---

## 2. 
* 

---

## 3. 
* 

---

## 외부도구

### 1. [[DataParser]](https://github.com/ysh4267/Yangjihoon_portfolio/tree/main/900_ExternalTool/001_DataParser)
> C#, .NET Framework, SQLite, Excel Interop

#### 개요
엑셀 형식의 기획 데이터를 SQLite 데이터베이스 및 JSON 포맷으로 변환하는 전용 파싱 엔진입니다. 기획 데이터의 수동 반영 과정을 자동화하여 개발 워크플로우를 개선하기 위해 제작했습니다.

#### 특징
* 멀티쓰레딩 기법과 COM 객체 호출 최적화를 통한 대용량 데이터 파싱 속도 개선
* 프로젝트 환경에 최적화된 SQLite 데이터베이스 및 JSON 포맷 결과물 생성
* 데이터 정합성 유지를 위한 파싱 엔진 최적화

### 2. [[DataIntegrityChecker]](https://github.com/ysh4267/Yangjihoon_portfolio/tree/main/900_ExternalTool/002_DataIntegrityChecker)
> Python, SQLite, Regex, Pandas

#### 개요
다국어 텍스트 데이터의 결함을 탐지하고 정합성을 검증하기 위한 자동화 검사 도구입니다. 대규모 다국어 데이터의 품질 관리 프로세스를 구축하기 위해 제작했습니다.

#### 특징
* 정규 표현식을 활용하여 언어별 특수 태그 및 제어 문자의 일치 여부 전수 조사
* SQLite 및 Pandas를 활용한 대규모 원문 데이터의 구조적 분석 및 결함 탐지
* 검사 결과를 리포트로 생성하여 데이터 정합성 관리 효율성 증대

### 3. [[BulkEmailSender]](https://github.com/ysh4267/Yangjihoon_portfolio/tree/main/900_ExternalTool/003_BulkEmailSender)
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
