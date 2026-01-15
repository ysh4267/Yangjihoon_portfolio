import sqlite3
import re
import pandas as pd
import os
import configparser
import sys

# 실행 파일의 현재 디렉토리 경로
base_dir = os.path.dirname(os.path.abspath(sys.argv[0]))
config_path = os.path.join(base_dir, 'config.ini')


# 설정 파일 생성 함수
def create_default_config(path):
    with open(path, 'w', encoding='utf-8') as configfile:
        configfile.write("[DEFAULT]\n")
        configfile.write("# 데이터베이스 경로 입력\n")
        configfile.write("db_path = C:/Users/gameData.exlix\n\n")

        configfile.write("[target]\n")
        configfile.write("# 찾고자 하는 단어 리스트 (쉼표로 구분, 큰 따옴표 필요 없음)\n")
        configfile.write("target_words = 단어1, 단어2\n\n")

        configfile.write("[languages]\n")
        configfile.write("# 언어별 컬럼 리스트 갯수 제한 없음. (쉼표로 구분)\n")
        configfile.write("language_columns = ko_kr, en_us, ja_jp\n\n")

        configfile.write("[database]\n")
        configfile.write("# 테이블 이름과 PK 컬럼명 설정\n")
        configfile.write("table_name = card_describe\n")
        configfile.write("primary_key = card_index\n")

    print(f"기본 설정 파일이 {path}에 생성되었습니다. 설정을 수정한 후 다시 실행하세요.")


# config.ini 파일이 존재하는지 확인
if not os.path.exists(config_path):
    print("config.ini 파일을 찾을 수 없습니다. 기본 설정 파일을 생성합니다...")
    create_default_config(config_path)
    input("Press Enter to exit...")
    sys.exit(1)

# 설정 파일 읽기 (Raw 모드)
config = configparser.RawConfigParser()
try:
    config.read(config_path, encoding='utf-8')

    # 필수 섹션 확인
    if not config.has_section('target') or not config.has_section('languages') or not config.has_section('database'):
        raise configparser.MissingSectionHeaderError(config_path, 0, "필수 섹션이 누락되었습니다.")

    # 설정 값 가져오기
    db_path = config['DEFAULT'].get('db_path', 'C:/Users/gameData.exlix')
    target_words = config['target']['target_words'].split(', ')
    language_columns = config['languages']['language_columns'].split(', ')
    table_name = config['database']['table_name']
    primary_key = config['database']['primary_key']

except (configparser.MissingSectionHeaderError, configparser.NoOptionError, configparser.ParsingError) as e:
    print("config.ini 파일에 오류가 있습니다. 기본 설정 파일을 생성합니다...")
    create_default_config(config_path)
    input("Press Enter to exit...")
    sys.exit(1)

# 데이터베이스 파일 존재 여부 확인
if not os.path.exists(db_path):
    print(f"데이터베이스 파일을 찾을 수 없습니다: {db_path}")
    input("Press Enter to exit...")
    sys.exit(1)

print(f"Config file path: {config_path}")
print("데이터베이스 경로:", db_path)
print("찾을 단어 목록:", target_words)
print("언어 컬럼 목록:", language_columns)
print(f"테이블 이름: {table_name}, 기본 키 컬럼: {primary_key}")
print("\nSQLite 데이터베이스에 연결 중...")

# SQLite 데이터베이스에 연결
conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# 테이블에서 데이터 읽기
query = f"SELECT {primary_key}, {', '.join(language_columns)} FROM {table_name}"
cursor.execute(query)
rows = cursor.fetchall()

print("데이터베이스에서 데이터를 가져왔습니다. 단어 빈도 분석을 시작합니다...")

# 각 행에서 단어 리스트의 빈도를 세고 결과를 저장합니다.
results = []
for row in rows:
    record_id = row[0]
    language_texts = row[1:]

    for word in target_words:
        counts = {}
        for lang_col, text in zip(language_columns, language_texts):
            escaped_word = re.escape(word)
            counts[lang_col] = len(re.findall(escaped_word, text)) if text is not None else 0

        if len(set(counts.values())) > 1:
            result_entry = {
                primary_key: record_id,
                'word': word,
            }
            for lang_col in language_columns:
                result_entry[f'{lang_col}_count'] = counts[lang_col]
            results.append(result_entry)
            break

print("단어 빈도 분석이 완료되었습니다. 결과를 저장합니다...")

# 결과를 데이터프레임으로 변환하여 출력
filtered_df = pd.DataFrame(results)

# 결과 파일 경로를 설정
output_path = os.path.join(os.path.dirname(db_path), "result.txt")

# 결과를 텍스트 파일로 저장
with open(output_path, "w", encoding="utf-8") as file:
    file.write(filtered_df.to_string(index=False))

print(f"결과가 {output_path}에 저장되었습니다.")
input("작업이 완료되었습니다. 프로그램을 종료하려면 Enter 키를 누르세요.")

# SQLite 연결을 닫습니다.
conn.close()
