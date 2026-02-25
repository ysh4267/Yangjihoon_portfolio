using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;

// 에디터 윈도우를 통해 씬 로드 및 폴더/파일 접근 기능을 제공하는 유틸리티 클래스
public class LoadSceneWindow : EditorWindow {
	[SerializeField]
	private Vector2 scrollPos = Vector2.zero;

	// 씬 경로 상수
	private const string SCENE_ROOT_PATH = "Assets/Scenes/";
	private const string SCENE_EXTENSION = ".unity";

	// DB 경로 상수
	private const string DB_FOLDER_NAME = "/DB/";
	private const string DB_EXTENSION = ".db";

	// 씬 이름 상수
	private const string SCENE_001 = "ExampleScene_001";
	private const string SCENE_002 = "ExampleScene_002";
	private const string SCENE_003 = "ExampleScene_003";
	private const string SCENE_004 = "ExampleScene_004";
	private const string SCENE_005 = "ExampleScene_005";
	private const string SCENE_006 = "ExampleScene_006";
	private const string SCENE_007 = "ExampleScene_007";
	private const string SCENE_008 = "ExampleScene_008";
	private const string SCENE_009 = "ExampleScene_009";
	private const string SCENE_010 = "ExampleScene_010";
	private const string SCENE_011 = "ExampleScene_011";
	private const string SCENE_012 = "ExampleScene_012";

	// DB 파일명 상수
	private const string DB_FILE_001 = "example_data_001";
	private const string DB_FILE_002 = "example_data_002";
	private const string DB_FILE_003 = "example_data_003";

	// 체크 데이터 파일명 상수
	private const string CHECK_DATA_FILE = "example_check_data.json";

	// URL 프로토콜 접두어
	private const string FILE_PROTOCOL = "file:///";

	// 메뉴 항목 등록 및 윈도우 표시
	[MenuItem("Window/LoadScene Window")]
	public static void ShowWindow() {
		GetWindow(typeof(LoadSceneWindow));
	}

	void OnEnable() {

	}

	// 지정된 경로의 씬을 열기 전 현재 씬 저장 여부를 확인
	void OpenScene(string scenePath) {
		if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
			EditorSceneManager.OpenScene(scenePath);
		}
	}

	// 씬 경로 조합 헬퍼
	private static string GetScenePath(string sceneName) {
		return SCENE_ROOT_PATH + sceneName + SCENE_EXTENSION;
	}

	// DB 파일 경로 조합 헬퍼
	private static string GetDbFilePath(string basePath, string fileName) {
		return FILE_PROTOCOL + basePath + DB_FOLDER_NAME + fileName + DB_EXTENSION;
	}

	// GUI 레이아웃 구성
	private void OnGUI() {
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		// 씬 로드 버튼 영역
		if (GUILayout.Button(SCENE_001, GUILayout.Height(30))) {
			OpenScene(GetScenePath(SCENE_001));
		}
		if (GUILayout.Button(SCENE_002, GUILayout.Height(30))) {
			OpenScene(GetScenePath(SCENE_002));
		}
		if (GUILayout.Button(SCENE_003, GUILayout.Height(30))) {
			OpenScene(GetScenePath(SCENE_003));
		}
		if (GUILayout.Button(SCENE_004, GUILayout.Height(30))) {
			OpenScene(GetScenePath(SCENE_004));
		}
		if (GUILayout.Button(SCENE_005, GUILayout.Height(30))) {
			OpenScene(GetScenePath(SCENE_005));
		}
		if (GUILayout.Button(SCENE_006, GUILayout.Height(30))) {
			OpenScene(GetScenePath(SCENE_006));
		}
		if (GUILayout.Button(SCENE_007, GUILayout.Height(30))) {
			OpenScene(GetScenePath(SCENE_007));
		}
		if (GUILayout.Button(SCENE_008, GUILayout.Height(30))) {
			OpenScene(GetScenePath(SCENE_008));
		}
		if (GUILayout.Button(SCENE_009, GUILayout.Height(30))) {
			OpenScene(GetScenePath(SCENE_009));
		}
		if (GUILayout.Button(SCENE_010, GUILayout.Height(30))) {
			OpenScene(GetScenePath(SCENE_010));
		}
		if (GUILayout.Button(SCENE_011, GUILayout.Height(30))) {
			OpenScene(GetScenePath(SCENE_011));
		}
		if (GUILayout.Button(SCENE_012, GUILayout.Height(30))) {
			OpenScene(GetScenePath(SCENE_012));
		}

		// 폴더 접근 버튼 영역
		GUILayout.BeginHorizontal(GUILayout.Height(40));
		if (GUILayout.Button("DB Folder", GUILayout.Height(40))) {
			Application.OpenURL(FILE_PROTOCOL + Application.streamingAssetsPath);
		}
		if (GUILayout.Button("Save Folder", GUILayout.Height(40))) {
			Application.OpenURL(FILE_PROTOCOL + Application.persistentDataPath);
		}
		GUILayout.EndHorizontal();

		// StreamingAssets DB 파일 접근 버튼 영역
		GUILayout.BeginHorizontal(GUILayout.Height(40));
		if (GUILayout.Button("DB_001", GUILayout.Height(40))) {
			Application.OpenURL(GetDbFilePath(Application.streamingAssetsPath, DB_FILE_001));
		}
		if (GUILayout.Button("DB_002", GUILayout.Height(40))) {
			Application.OpenURL(GetDbFilePath(Application.streamingAssetsPath, DB_FILE_002));
		}
		if (GUILayout.Button("DB_003", GUILayout.Height(40))) {
			Application.OpenURL(GetDbFilePath(Application.streamingAssetsPath, DB_FILE_003));
		}
		GUILayout.EndHorizontal();

		// PersistentData DB 파일 접근 버튼 영역
		GUILayout.BeginHorizontal(GUILayout.Height(40));
		if (GUILayout.Button("InGame DB_001", GUILayout.Height(40))) {
			Application.OpenURL(GetDbFilePath(Application.persistentDataPath, DB_FILE_001));
		}
		if (GUILayout.Button("InGame DB_002", GUILayout.Height(40))) {
			Application.OpenURL(GetDbFilePath(Application.persistentDataPath, DB_FILE_002));
		}
		if (GUILayout.Button("InGame DB_003", GUILayout.Height(40))) {
			Application.OpenURL(GetDbFilePath(Application.persistentDataPath, DB_FILE_003));
		}
		GUILayout.EndHorizontal();

		// 체크 데이터 초기화 및 열기 버튼 영역
		if (GUILayout.Button("Reset Check Data", GUILayout.Height(40))) {
			JsonDataManager.WriteData<CheckData>(ENUM_JSON_FILE.CheckData, new CheckData());
		}
		if (GUILayout.Button("CheckData", GUILayout.Height(30))) {
			Application.OpenURL(FILE_PROTOCOL + Application.persistentDataPath + "/" + CHECK_DATA_FILE);
		}
		EditorGUILayout.EndScrollView();
	}
}
