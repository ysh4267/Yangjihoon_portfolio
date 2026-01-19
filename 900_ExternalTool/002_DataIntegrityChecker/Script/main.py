import sqlite3
import re
import pandas as pd
import os
import configparser
import sys

# 기본 설정 파일(config.ini) 생성
def create_default_config(path):
    with open(path, 'w', encoding='utf-8') as configfile:
        configfile.write("[DEFAULT]\n")
        configfile.write("# 데이터베이스 경로 입력\n")
        configfile.write("db_path = C:/path/to/database.exlix\n\n")

        configfile.write("[target]\n")
        configfile.write("# 찾고자 하는 단어 리스트 (쉼표로 구분)\n")
        configfile.write("target_words = dummy_word1, dummy_word2\n\n")

        configfile.write("[languages]\n")
        configfile.write("# 언어별 컬럼 리스트\n")
        configfile.write("language_columns = lang_1, lang_2, lang_3\n\n")

        configfile.write("[database]\n")
        configfile.write("# 테이블 이름과 PK 컬럼명 설정\n")
        configfile.write("table_name = target_table\n")
        configfile.write("primary_key = id_column\n")

    print(f"기본 설정 파일이 {path}에 생성되었습니다. 설정을 수정한 후 다시 실행하세요.")

# 설정 파일 및 데이터베이스 경로 확인 후 설정 반환
def load_environment():
    base_dir = os.path.dirname(os.path.abspath(sys.argv[0]))
    config_path = os.path.join(base_dir, 'config.ini')

    if not os.path.exists(config_path):
        print("config.ini 파일을 찾을 수 없습니다. 기본 설정 파일을 생성합니다...")
        create_default_config(config_path)
        input("Press Enter to exit...")
        sys.exit(1)

    config = configparser.RawConfigParser()
    try:
        config.read(config_path, encoding='utf-8')

        db_path = config['DEFAULT'].get('db_path', 'C:/path/to/database.exlix')
        target_words = config['target']['target_words'].split(', ')
        language_columns = config['languages']['language_columns'].split(', ')
        table_name = config['database']['table_name']
        primary_key = config['database']['primary_key']

        return db_path, target_words, language_columns, table_name, primary_key
    except Exception as e:
        print("config.ini 파일에 오류가 있습니다.")
        input("Press Enter to exit...")
        sys.exit(1)

# 데이터베이스 연결 및 단어 빈도 분석 수행
def analyze_data(db_path, target_words, language_columns, table_name, primary_key):
    if not os.path.exists(db_path):
        print(f"데이터베이스 파일을 찾을 수 없습니다: {db_path}")
        input("Press Enter to exit...")
        sys.exit(1)

    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    query = f"SELECT {primary_key}, {', '.join(language_columns)} FROM {table_name}"
    cursor.execute(query)
    rows = cursor.fetchall()

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
                result_entry = {primary_key: record_id, 'word': word}
                for lang_col in language_columns:
                    result_entry[f'{lang_col}_count'] = counts[lang_col]
                results.append(result_entry)
                break

    conn.close()
    return results

# 분석 결과를 파일로 저장
def save_results(results, db_path, primary_key):
    if not results:
        print("불일치 항목이 발견되지 않았습니다.")
        return

    filtered_df = pd.DataFrame(results)
    output_path = os.path.join(os.path.dirname(db_path), "result.txt")

    with open(output_path, "w", encoding="utf-8") as file:
        file.write(filtered_df.to_string(index=False))

    print(f"결과가 {output_path}에 저장되었습니다.")

# 메인 실행 제어 로직
def main():
    db_path, target_words, language_columns, table_name, primary_key = load_environment()

    print("데이터베이스 경로:", db_path)
    print("찾을 단어 목록:", target_words)
    print("\nSQLite 데이터베이스에 연결 및 분석 시작...")

    results = analyze_data(db_path, target_words, language_columns, table_name, primary_key)

    save_results(results, db_path, primary_key)

    input("작업이 완료되었습니다. 프로그램을 종료하려면 Enter 키를 누르세요.")

if __name__ == "__main__":
    main()