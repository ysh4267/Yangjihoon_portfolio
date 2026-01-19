# BulkEmailSender (대량 메일 발송 시스템)

엑셀 파일에 정리된 수신자 목록을 읽어들여, 개별 맞춤형 이메일을 대량으로 전송하는 Windows Forms 기반 유틸리티입니다.

Microsoft Office Interop을 통해 엑셀 데이터를 직접 추출하며, 비동기(Async) 처리 방식을 도입하여 대규모 발송 작업 중에도 UI가 멈추지 않고 실시간으로 진행 상황을 모니터링할 수 있도록 설계되었습니다.

## 주요 기능 및 프로세스

프로그램의 주요 기능과 내부 처리 흐름은 다음과 같습니다.

### 1. 엑셀 데이터 로드 (Excel Data Loading)
사용자가 지정한 엑셀 파일에서 이메일 주소와 본문 내용을 추출합니다.

- **작동 원리**
    1. 사용자가 파일 찾기 버튼을 통해 엑셀 파일을 선택합니다.
    2. `Form.SendMailButton_Click` 이벤트 발생 시 `ExcelManager.ReadData`가 호출됩니다.
    3. `Microsoft.Office.Interop.Excel`을 사용하여 엑셀 인스턴스를 생성하고 워크시트에 접근합니다.
    4. 데이터가 존재하는 행(Row)을 순차적으로 읽어 (이메일, 내용) 형태의 튜플 리스트로 변환하여 반환합니다.

### 2. 메일 발송 (SMTP Sending)
추출된 데이터를 바탕으로 SMTP 프로토콜을 이용해 메일을 전송합니다.

- **작동 원리**
    1. `MailManager` 클래스가 초기화되며 Gmail SMTP 서버(`smtp.gmail.com`, 포트 587, SSL 사용)와 계정 인증 정보를 설정합니다.
    2. `MailManager.SendMail` 메소드는 수신자 이메일, 제목, 본문을 인자로 받아 `MailMessage` 객체를 생성합니다.
    3. `SmtpClient.Send`를 호출하여 실제 전송을 수행하고, 성공 여부를 문자열로 반환합니다.

### 3. 비동기 작업 및 UI 갱신 (Async Processing & UI Update)
대량 전송 시 프로그램 응답 대기를 방지하기 위해 별도의 작업 흐름에서 발송을 처리합니다.

- **작동 원리**
    1. `Form.cs`에서 `Task.Run`을 사용하여 메일 발송 루프를 백그라운드 쓰레드로 분리합니다.
    2. 루프 내에서 차례대로 메일을 발송하며, 그 결과를 `Invoke` 메소드를 통해 UI 쓰레드(`DebugTextBox`, `progressBar1`)에 안전하게 반영합니다.

---

## 핵심 로직: 비동기 발송 시퀀스

사용자 경험(UX)을 저해하지 않으면서 대량 처리를 수행하는 핵심 로직입니다.

1. **초기화**: 사용자가 계정 정보와 내용을 입력하고 전송 버튼을 누르면 `ExcelManager`와 `MailManager` 인스턴스를 생성합니다.
2. **데이터 준비**: `ExcelManager.ReadData`가 엑셀 파일을 열고 전체 수신자 목록을 메모리 리스트로 가져옵니다.
3. **병렬 작업 시작**:
    - `progressBar`의 최대값을 데이터 개수에 맞춰 설정합니다.
    - `Task.Run`으로 비동기 작업을 시작하여 메인 UI가 멈추는 것을 방지합니다.
4. **발송 루프**:
    - 리스트를 순회하며 `_mailManager.SendMail`을 호출해 메일을 보냅니다.
    - 각 발송 직후, `DebugTextBox.Invoke`와 `progressBar1.Invoke`를 호출하여 실시간으로 성공 메시지와 진행률을 화면에 표시합니다.
5. **종료**: 모든 데이터의 순회가 끝나면 작업이 종료됩니다.