// =========================================================================
//  __  __      _ _______ _____ _______ _    _ _____ _____ ____  
// |  \/  |    | |__   __/ ____|__   __| |  | |  __ \_   _/ __ \ 
// | \  / |    | |  | | | (___    | |  | |  | | |  | || || |  | |
// | |\/| |_   | |  | |  \___ \   | |  | |  | | |  | || || |  | |
// | |  | | |__| |  | |  ____) |  | |  | |__| | |__| || || |__| |
// |_|  |_|\____/   |_| |_____/   |_|   \____/|_____/_____\____/ 
//
// Project Name : TemplateCreator
// Created by   : Mojatto (Mojatto Studio)
//
// Copyright (c) 2023 Mojatto Studio All Rights Reserved.
// =========================================================================

// Standard Namespaces
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

// UnityEngine Namespaces
using UnityEngine;
using UnityEditor;

// Package Namespaces
using MJTStudio.Editor;

namespace MJTStudio.TemplateCreator.Editor
{
    // TODO:エディタ拡張GUI実装部とパッケージ生成処理部を分離する (2023年7月1日)
    /// <summary>
    /// エディタ拡張「TemplateCreator」本体実装クラスです。
    /// </summary>
    public sealed class TemplateCreator : EditorWindow, IDisposable
    {
        /// <summary>
        /// 本エディタ拡張モジュールのバージョン番号です。
        /// </summary>
        private const string APPLICATION_VERSION = "v0.2.0";

        /// <summary>
        /// 
        /// </summary>
        // private const string APPLICATION_DOC_URL = "https://mjtstudio.net/portfolio/app/template-creator/";
        private const string APPLICATION_DOC_URL = "https://mjtstudio.notion.site/TemplateCreator-55e1727a348640958d38e3876ca64e7d?pvs=4";

        /// <summary>
        /// 
        /// </summary>
        private const string APPLICATION_EDITOR_PROJECT_TEMPLATE_DIRECTORY_PATH = "Data/Resources/PackageManager/ProjectTemplates";

        /// <summary>
        /// パッケージのルートディレクトリの名前です。
        /// </summary>
        private const string PACKAGE_DIRECTORY_ROOT_FOLDER_NAME = "package";

        /// <summary>
        /// テンプレート化するプロジェクトデータを格納するフォルダの名前です。
        /// </summary>
        private const string PACKAGE_DIRECTORY_PROJECTDATA_FOLDER_NAME = "ProjectData~";

        /// <summary>
        /// テンプレートパッケージ情報ファイルの名前です。
        /// </summary>
        private const string PACKAGE_MANIFEST_JSON_FILE_NAME = "package.json";

        /// <summary>
        /// 生成されるテンプレートパッケージの拡張子です。
        /// </summary>
        private const string PACKAGE_EXTENSION = ".tgz";

        /// <summary>
        /// テンプレート化するプロジェクトのAssetsフォルダの名前です。
        /// </summary>
        private const string TARGET_DIRECTORY_ASSET_FOLDER_NAME = "Assets";

        /// <summary>
        /// テンプレート化するプロジェクトのPackagesフォルダの名前です。
        /// </summary>
        private const string TARGET_DIRECTORY_PACKAGES_FOLDER_NAME = "Packages";

        /// <summary>
        /// テンプレート化するプロジェクトのProjectSettingsフォルダの名前です。
        /// </summary>
        private const string TARGET_DIRECTORY_PROJECTSETTINGS_FOLDER_NAME = "ProjectSettings";

        /// <summary>
        /// 前回設定されたパッケージ情報を保持しておくためのEditorUserSettingsのキーです。
        /// </summary>
        private const string EDITOR_PREVIOUS_PACKAGE_INFO_USERSETTINGS_KEY = "PreviousPackageInfo";

        /// <summary>
        /// 本クラス内で、毎ループ処理を非同期に待機させる際の待機時間[ms]です。
        /// </summary>
        private const int UNTIASK_LOOP_DURATION_MS = 5;

        /// <summary>
        /// 進捗表示目的で、パッケージ生成処理を一時停止させる際の待機時間[ms]です。
        /// </summary>
        private const int UNTIASK_DELAY_MILLISECONDS_FOR_PACEMAKING = 1000;

        /// <summary>
        /// テンプレートパッケージ生成前にプロジェクトデータから除外するファイルパスの配列です。
        /// </summary>
        /// <value></value>
        private static readonly string[] PACKAGE_EXCLUDE_FILE_PATHS = {
            "ProjectSettings/ProjectVersion.txt"
        };

        /// <summary>
        /// 本コンポーネントの処理状態を保持するフィールドです。
        /// </summary>
        private WorkState workState = WorkState.Initializing;

        /// <summary>
        /// パッケージ情報を保持するフィールドです。
        /// エディタ拡張ウィンドウの各フォームを編集すると、プロパティ経由でこのフィールドが変更されます。
        /// </summary>
        private PackageInfo packageInfo;

        /// <summary>
        /// 非同期処理のキャンセル指示を行うためのトークンソースインスタンスです。
        /// </summary>
        private CancellationTokenSource cts;

        /// <summary>
        /// 作業用ディレクトリまでのパスを保持するフィールドです。
        /// </summary>
        private string workDirectoryPath;

        /// <summary>
        /// 成果物の生成先のパスを保持するフィールドです。
        /// </summary>
        private string packageDestinationPath;

        /// <summary>
        /// 現在使用しているUnityエディタのプロジェクトテンプレートディレクトリのパスを保持するフィールドです。
        /// 生成されるパッケージを、直接Unity Hubに登録する場合に使用します。
        /// </summary>
        private string editorProjectTemplateDirectoryPath;

        /// <summary>
        /// クラスのリセット(再コンパイル後・EditorWindow再表示後など)時のコールバックメソッドです。
        /// </summary>
        public void Reset()
        {
            // コンポーネントを初期化
            Initialize();
        }

        /// <summary>
        /// アンマネージドリソースを明示的に開放します。
        /// </summary>
        public void Dispose()
        {
            // パッケージ情報をクリア
            packageInfo = null;

            // 作業フォルダをクリーンアップ
            CleanUpWorkspace();

            // 作業用ディレクトリのパス情報をクリア
            workDirectoryPath = null;

            // パッケージ生成先のパス情報をクリア
            packageDestinationPath = null;
        }

        /// <summary>
        /// 現在開いているプロジェクトを用いて、プロジェクトテンプレートパッケージを生成します。
        /// </summary>
        /// <param name="destinationPath"></param>
        public async Task GenerateTemplatePackage(Action onSuccess = null, Action onFailed = null)
        {
            // タスク数を指定
            const int maxTaskCount = 5;

            // 現在のタスク達成状態を保持するローカルメンバ
            int count = 0;
            
            // C#コンソールのカレントディレクトリを保持 (プロジェクトのルートディレクトリが標準)
            string projectDir = Directory.GetCurrentDirectory();

#       if UNITY_2020_1_OR_NEWER
            // 進捗状況の表示開始
            int id = UnityEditor.Progress.Start(
                name: "GenerateTemplatePackage",
                description: "PROGRESS_DESCRIPTION"
            );
#       else
            // 進捗状況の表示開始
            EditorUtility.DisplayProgressBar(
                title: "GenerateTemplatePackage",
                info: "PROGRESS_DESCRIPTION",
                progress: 0f
            );
#       endif

            try
            {
                // 処理状態を「処理中」に設定
                workState = WorkState.InProgress;

                // 作業ディレクトリのルートまでのパスを取得
                workDirectoryPath = Path.Combine(FileUtil.GetUniqueTempPathInProject(), PACKAGE_DIRECTORY_ROOT_FOLDER_NAME);

#           if UNITY_2020_1_OR_NEWER
                // 進捗状況を更新
                UnityEditor.Progress.Report(
                    id: id,
                    currentStep: count,
                    totalSteps: maxTaskCount,
                    description: $"パッケージ生成用作業ディレクトリを生成中"
                );
#           else
                // 進捗状況を更新
                EditorUtility.DisplayProgressBar(
                    title: "GenerateTemplatePackage",
                    info: $"パッケージ生成用作業ディレクトリを生成中",
                    progress: (float)count / (float)maxTaskCount
                );
#           endif

                // 作業ディレクトリ構造を構築
                CreateWorkDirectory();

                await Task.Delay(UNTIASK_DELAY_MILLISECONDS_FOR_PACEMAKING);

#           if UNITY_2020_1_OR_NEWER
                // 進捗状況を更新
                count++;
                UnityEditor.Progress.Report(
                    id: id,
                    currentStep: count,
                    totalSteps: maxTaskCount,
                    description: $"パッケージ情報ファイルを生成中"
                );
#           else
                // 進捗状況を更新
                EditorUtility.DisplayProgressBar(
                    title: "GenerateTemplatePackage",
                    info: $"パッケージ情報ファイルを生成中",
                    progress: (float)count / (float)maxTaskCount
                );
#           endif

                // パッケージ情報ファイルを生成
                await GeneratePackageInfoFile();

                await Task.Delay(UNTIASK_DELAY_MILLISECONDS_FOR_PACEMAKING);

                // 進捗状況を更新
                count++;

#           if UNITY_2020_1_OR_NEWER
                UnityEditor.Progress.Report(
                    id: id,
                    currentStep: count,
                    totalSteps: maxTaskCount,
                    description: $"プロジェクトデータを複製中"
                );
#           else
                EditorUtility.DisplayProgressBar(
                    title: "GenerateTemplatePackage",
                    info: $"プロジェクトデータを複製中",
                    progress: (float)count / (float)maxTaskCount
                );
#           endif

                // プロジェクトデータを複製
#           if UNITY_2020_1_OR_NEWER
                await DuplicateProjectDatas(id);
#           else
                await DuplicateProjectDatas();
#           endif

                await Task.Delay(UNTIASK_DELAY_MILLISECONDS_FOR_PACEMAKING);

                // 進捗状況を更新
                count++;

#           if UNITY_2020_1_OR_NEWER
                UnityEditor.Progress.Report(
                    id: id,
                    currentStep: count,
                    totalSteps: maxTaskCount,
                    description: $"除外対象のファイルを削除中"
                );
#           else
                EditorUtility.DisplayProgressBar(
                    title: "GenerateTemplatePackage",
                    info: $"除外対象のファイルを削除中",
                    progress: (float)count / (float)maxTaskCount
                );
#           endif

                // 除外リストに登録されたファイルを削除
                await RemoveExcludedFiles();

                await Task.Delay(UNTIASK_DELAY_MILLISECONDS_FOR_PACEMAKING);

                // 進捗状況を更新
                count++;

#           if UNITY_2020_1_OR_NEWER
                UnityEditor.Progress.Report(
                    id: id,
                    currentStep: count,
                    totalSteps: maxTaskCount,
                    description: $"プロジェクトデータをアーカイブ・パッケージング中"
                );
#           else
                EditorUtility.DisplayProgressBar(
                    title: "GenerateTemplatePackage",
                    info: $"プロジェクトデータをアーカイブ・パッケージング中",
                    progress: (float)count / (float)maxTaskCount
                );
#           endif

                await Task.Delay(UNTIASK_DELAY_MILLISECONDS_FOR_PACEMAKING);

                // C#コンソールのカレントディレクトリを作業フォルダに指定
                Directory.SetCurrentDirectory(Path.GetDirectoryName(workDirectoryPath));

                // プロジェクトデータをアーカイブ化・圧縮
                GeneratePackage();
            }
            // 例外ハンドリング
            catch(Exception e)
            {
                Debug.LogError(
                    this.name
                    + ": プロジェクトテンプレートの作成中に例外がスローされました。作成に失敗しました。\n"
                    + "RawMessage:"
                    + e
                );

                // C#コンソールのカレントディレクトリをプロジェクトのルートディレクトリに戻す
                Directory.SetCurrentDirectory(projectDir);

                // 処理失敗とみなし対応するメソッドを実行
                onFailed?.Invoke();

                // 処理状態を「待機中」に設定
                workState = WorkState.Ready;

                // 進捗状況を更新
#           if UNITY_2020_1_OR_NEWER
                UnityEditor.Progress.Report(
                    id: id,
                    currentStep: count,
                    totalSteps: maxTaskCount,
                    description: $"例外発生 (クリーンアップ中)"
                );
#           else
                EditorUtility.DisplayProgressBar(
                    title: "GenerateTemplatePackage",
                    info: $"例外発生 (クリーンアップ中)",
                    progress: (float)count / (float)maxTaskCount
                );
#           endif

                // 作業フォルダをクリーンアップ
                CleanUpWorkspace();

                // 進捗状況を更新
#           if UNITY_2020_1_OR_NEWER
                UnityEditor.Progress.Report(
                    id: id,
                    currentStep: count,
                    totalSteps: maxTaskCount,
                    description: $"例外発生により中断"
                );
#           else
                EditorUtility.DisplayProgressBar(
                    title: "GenerateTemplatePackage",
                    info: $"例外発生により中断",
                    progress: (float)count / (float)maxTaskCount
                );
#           endif

                await Task.Delay(5000);

                // 進捗状況をリセット
#           if UNITY_2020_1_OR_NEWER
                UnityEditor.Progress.Remove(id);
#           else
                EditorUtility.ClearProgressBar();
#           endif

                return;
            }

            // C#コンソールのカレントディレクトリをプロジェクトのルートディレクトリに戻す
            Directory.SetCurrentDirectory(projectDir);

            // 処理成功とみなしtrueを返す
            onSuccess?.Invoke();

            Debug.Log(
                this.name
                + ": プロジェクトテンプレートパッケージの生成が完了しました。\n"
                + "-> "
                + Path.Combine(packageDestinationPath, packageInfo.name + PACKAGE_EXTENSION)
            );

            workState = WorkState.Ready;

            // 進捗状況を更新
#       if UNITY_2020_1_OR_NEWER
            UnityEditor.Progress.Report(
                id: id,
                currentStep: maxTaskCount,
                totalSteps: maxTaskCount,
                description: $"パッケージ生成完了"
            );
#       else
            EditorUtility.DisplayProgressBar(
                title: "GenerateTemplatePackage",
                info: $"パッケージ生成完了",
                progress: (float)count / (float)maxTaskCount
            );
#       endif

            await Task.Delay(UNTIASK_DELAY_MILLISECONDS_FOR_PACEMAKING);

            //
            EditorUtility.DisplayDialog(
                "TemplateCreator - Dialog",
                "プロジェクトテンプレートパッケージの作成に成功しました。\n"
                + "UnityHubから使用する場合は、一度UnityHubを完全に再起動したうえで、「NewProject」ボタンから確認してください。",
                "OK"
            );

            // 進捗状況をリセット
#       if UNITY_2020_1_OR_NEWER
            UnityEditor.Progress.Finish(
                id,
                UnityEditor.Progress.Status.Succeeded
            );
#       else
            EditorUtility.ClearProgressBar();
#       endif

            // プロジェクトテンプレートパッケージの生成先をファイルブラウザーで表示
            OpenDestinationDirectoryInFileBrowser();
        }

        /// <summary>
        /// このクラスを明示的に初期化します。
        /// </summary>
        private void Initialize()
        {
            // プロジェクトディレクトリのパスを取得
            packageInfo = new PackageInfo();
            packageDestinationPath = "";

            // 起動中のエディタバージョンに対応したプロジェクトテンプレートの格納先パスを取得し保持する
            editorProjectTemplateDirectoryPath = 
                Path.Combine(
                    Path.GetDirectoryName(EditorApplication.applicationPath),
                    APPLICATION_EDITOR_PROJECT_TEMPLATE_DIRECTORY_PATH
                ).Replace("\\", "/");
            Debug.Log("EditorProjectTemplateDirectoryPath: \n" + editorProjectTemplateDirectoryPath);

            // エディタ拡張ウィンドウの最大・最小サイズを固定
            minSize = new Vector2(320f, 800f);
            maxSize = new Vector2(320f, 800f);
            
            // エディタ拡張ウィンドウの初期サイズを設定し固定
            var windowPosition = position;
            windowPosition.size = new Vector2(320f, 800f);
            position = windowPosition;

            // 指定キーに前回値が保持されていれば、内容をパースしてクラスインスタンスとして保持する
            var previousPackageInfoJSON = EditorUserSettings.GetConfigValue(EDITOR_PREVIOUS_PACKAGE_INFO_USERSETTINGS_KEY);
            if (previousPackageInfoJSON != null)
            {
                var previousPackageInfo = JsonUtility.FromJson<PackageInfo>(previousPackageInfoJSON);
                packageInfo = previousPackageInfo;
            }

            workState = WorkState.Ready;
        }
        
        /// <summary>
        /// 一時フォルダに作業用ディレクトリ構造を構築します。
        /// </summary>
        private void CreateWorkDirectory()
        {
            // 作業用ディレクトリ構造を生成
            Directory.CreateDirectory(workDirectoryPath);

            // プロジェクトデータ格納用フォルダを生成
            Directory.CreateDirectory(Path.Combine(workDirectoryPath, PACKAGE_DIRECTORY_PROJECTDATA_FOLDER_NAME));
        }

        /// <summary>
        /// 指定されたパッケージ情報を用いて、パッケージ情報ファイル(.json)を生成します。
        /// </summary>
        private async Task GeneratePackageInfoFile()
        {
            // 書き込みモードでJSONファイル本体を新たに生成・ストリームを取得
            StreamWriter sw = File.CreateText(Path.Combine(workDirectoryPath, PACKAGE_MANIFEST_JSON_FILE_NAME));

            // シリアライズ用クラスからJSON形式のテキストデータを生成
            string outputContent = JsonUtility.ToJson(packageInfo);

            // テキストデータをJSONファイルに非同期に書き込む
            await sw.WriteAsync(outputContent);

            // バッファにデータが残っている場合は残りのデータも非同期に書き込む
            await sw.FlushAsync();

            // JSONファイルを閉じる
            sw.Dispose();
        }

        /// <summary>
        /// プロジェクトデータを作業用ディレクトリ内のプロジェクトデータ格納用フォルダに複製します。
        /// </summary>
        private async Task DuplicateProjectDatas(int parentTaskID = -1)
        {
            // コピー元フォルダのパスを取得
            string targetDirectoryPath = Directory.GetCurrentDirectory();

            // コピー先フォルダのパスを取得
            string duplicateDestPath = Path.Combine(workDirectoryPath, PACKAGE_DIRECTORY_PROJECTDATA_FOLDER_NAME);
            
            // 格納先ファイルが存在していない場合は例外をスローして中断させる
            if (!Directory.Exists(duplicateDestPath))
            {
                throw new DirectoryNotFoundException("プロジェクトデータ格納先のフォルダが存在していません。");
            }

            // Assetsフォルダを複製
            // Packagesフォルダを複製
            // ProjectSettingsフォルダを複製
            await Task.WhenAll(
                FileIOLibrary.CopyDirectoryAsync(
                    Path.Combine(targetDirectoryPath, TARGET_DIRECTORY_ASSET_FOLDER_NAME),
                    Path.Combine(duplicateDestPath, TARGET_DIRECTORY_ASSET_FOLDER_NAME),
                    true,
                    parentTaskID,
                    "Assetsフォルダを複製中..."
                ),
                FileIOLibrary.CopyDirectoryAsync(
                    Path.Combine(targetDirectoryPath, TARGET_DIRECTORY_PACKAGES_FOLDER_NAME),
                    Path.Combine(duplicateDestPath, TARGET_DIRECTORY_PACKAGES_FOLDER_NAME),
                    true,
                    parentTaskID,
                    "Packagesフォルダを複製中..."
                ),
                    FileIOLibrary.CopyDirectoryAsync(
                    Path.Combine(targetDirectoryPath, TARGET_DIRECTORY_PROJECTSETTINGS_FOLDER_NAME),
                    Path.Combine(duplicateDestPath, TARGET_DIRECTORY_PROJECTSETTINGS_FOLDER_NAME),
                    true,
                    parentTaskID,
                    "ProjectSettingsフォルダを複製中..."
                )
            );
        }

        /// <summary>
        /// 除外リストに登録されたファイルを、プロジェクトデータ格納フォルダから削除します。
        /// </summary>
        /// <returns></returns>
        private async Task RemoveExcludedFiles()
        {
            // 検索時の起点となるパスを生成 (プロジェクトデータ格納フォルダを起点に検索)
            string searchRootPath = Path.Combine(
                workDirectoryPath,
                PACKAGE_DIRECTORY_PROJECTDATA_FOLDER_NAME
            );

            // 除外リストの項目数分繰り返す
            foreach(string targetPath in PACKAGE_EXCLUDE_FILE_PATHS)
            {
                // 除外リストに登録されたファイルがプロジェクトデータ格納フォルダに存在していれば、削除する
                string targetExtendedPath = Path.Combine(searchRootPath, targetPath);
                targetExtendedPath = targetExtendedPath.Replace("/", "\\");
                if (File.Exists(targetExtendedPath))
                {
                    File.Delete(targetExtendedPath);

                    Debug.Log(
                        this.name
                        + ": "
                        + "以下のパスのファイルは、除外リストにより削除されました。\n-> "
                        + targetPath
                    );
                }
                else
                {
                    Debug.LogWarning(
                        this.name
                        + ": "
                        + "除外リストに登録されている以下のパスは、プロジェクトデータから見つかりませんでした。\n-> "
                        + targetExtendedPath
                    );
                }

                // 5[ms] 待機
                await Task.Delay(UNTIASK_LOOP_DURATION_MS);
            }
        }

        /// <summary>
        /// プロジェクトデータをアーカイブ化・圧縮しテンプレートパッケージ(.tgz)を生成します。
        /// </summary>
        /// <returns></returns>
        private void GeneratePackage()
        {
            // アーカイブ化するフォルダを指定
            string[] targetPaths = new []{PACKAGE_DIRECTORY_ROOT_FOLDER_NAME};

            // テンプレートパッケージ生成先(ファイル名含むフルパス)を指定
            string destinationPackagePath = Path.Combine(packageDestinationPath, packageInfo.name + PACKAGE_EXTENSION);

            Debug.Log(
                this.name
                + ": "
                + targetPaths[0]
                + " -> "
                + destinationPackagePath
            );

            // アーカイブ化・圧縮を実行
            bool result = ArchiverLibrary.CompressDirectoriesToTgz(
                destinationPackagePath,
                targetPaths,
                true
            );

            // 何らかの要因で処理に失敗した場合、例外をスローする
            if (!result)
            {
                throw new InvalidOperationException(
                    name
                    + ": テンプレートパッケージの生成に失敗しました。"
                );
            }
        }

        /// <summary>
        /// プラットフォームごとに対応したファイルブラウザを用いて、成果物の生成先ディレクトリを表示します。
        /// </summary>
        private void OpenDestinationDirectoryInFileBrowser()
        {
            var path = Path.Combine(packageDestinationPath, packageInfo.name + PACKAGE_EXTENSION).Replace("/", "\\");
            System.Diagnostics.Process.Start("explorer.exe", "/select," + path);
        }

        /// <summary>
        /// 作業ディレクトリをクリーンアップします。
        /// </summary>
        private void CleanUpWorkspace()
        {
            // 作業用ディレクトリが存在していれば、配下のファイルも含めて削除する
            if (Directory.Exists(workDirectoryPath))
            {
                Directory.Delete(Path.GetDirectoryName(workDirectoryPath), true);
            }
        }

        /// <summary>
        /// メニューバーから対応する項目が選択されたときのコールバックメソッドです。
        /// </summary>
        [MenuItem("Tools/MJTStudio/TemplateCreator %t")]
        public static void Open()
        {
            // ウィンドウ表示
            var window = CreateInstance<TemplateCreator>();
            window.titleContent.text = "[Experimental] TemplateCreator";

            // ユーティリティウィンドウとして表示
            window.ShowUtility();
        }

        /// <summary>
        /// エディタ拡張ウィンドウを描画します。
        /// </summary>
        private void RenderConfigulationMenu()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.Space(10f);

                GUILayout.Label("テンプレートパッケージ名", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "プロジェクトテンプレートのパッケージ名を指定してください。\n"
                    + "パッケージ名は、以下のような命名規則で指定する必要があります。\n"
                    + "com.[会社名].template.[パッケージ名]",
                    MessageType.None
                );
                packageInfo.name = EditorGUILayout.TextField(packageInfo.name);

                EditorGUILayout.Space(10f);

                GUILayout.Label("テンプレートパッケージ表示名", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "プロジェクトテンプレートの表示名を指定してください。\n"
                    + "ここに入力された内容が、UnityHub上でのプロジェクト新規作成時に表示されます。",
                    MessageType.None
                );
                packageInfo.displayName = EditorGUILayout.TextField(packageInfo.displayName);

                EditorGUILayout.Space(10f);

                GUILayout.Label("バージョン番号", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "プロジェクトテンプレートのバージョン番号を指定してください。\n"
                    + "特にバージョン番号による管理を行っていない場合は、この項目の変更は不要です。",
                    MessageType.None
                );
                packageInfo.version = EditorGUILayout.TextField(packageInfo.version);

                EditorGUILayout.Space(10f);

                GUILayout.Label("ターゲットUnityバージョン", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "作成するテンプレートが対象とするUnityエディタバージョンを指定してください。\n"
                    + "ex: Unity 2021.3.16f1 の場合 => 2021.3",
                    MessageType.None
                );
                packageInfo.unity = EditorGUILayout.TextField(packageInfo.unity);

                EditorGUILayout.Space(10f);

                GUILayout.Label("説明文", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "作成するテンプレートの説明文を指定してください。\n"
                    + "ここに入力された内容が、UnityHub上でのプロジェクト新規作成時に表示されます。",
                    MessageType.None
                );
                // packageInfo.description = EditorGUILayout.TextArea(packageInfo.description);
                packageInfo.description = GUILayout.TextArea(packageInfo.description, GUILayout.Height(50f));

                EditorGUILayout.Space(15f);

                EditorGUILayout.Separator();

                EditorGUILayout.Space(15f);

                GUILayout.Label("テンプレートパッケージ作成先", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "作成するテンプレートの説明文を指定してください。\n"
                    + "ここに入力された内容が、UnityHub上でのプロジェクト新規作成時に表示されます。",
                    MessageType.None
                );
                using(new EditorGUILayout.HorizontalScope())
                {
                    packageDestinationPath = EditorGUILayout.TextArea(packageDestinationPath);

                    if (GUILayout.Button("...", GUILayout.MaxWidth(30f)))
                    {
                        packageDestinationPath = EditorUtility.OpenFolderPanel(
                            "プロジェクトテンプレートパッケージの生成先を指定...",
                            "Assets",
                            "_"
                        );
                    }
                }
                if (GUILayout.Button("UnityHubのテンプレート保存先を指定"))
                {
                    //
                    if (!FileIOLibrary.CheckFileWritePermission(editorProjectTemplateDirectoryPath))
                    {
                        EditorUtility.DisplayDialog(
                            "TemplateCreator - Dialog",
                            "UnityHubのテンプレート保存先に書き込み権限がありません。\n"
                            + "UnityHubのテンプレート保存先に書き込み権限を付与して再試行するか、"
                            + "別の場所にパッケージを生成した後、手動でUnityHubのテンプレート保存先に移動してください。",
                            "OK"
                        );
                        return;
                    }

                    //
                    if (EditorUtility.DisplayDialog(
                        "TemplateCreator - Dialog",
                        "UnityHubのテンプレート保存先を指定しますか？\n"
                        + "※UnityHubのテンプレート保存先を指定すると、UnityHub上でのプロジェクト新規作成時に、\n"
                        + "  このテンプレートが表示されるようになります。",
                        "はい",
                        "いいえ"
                    ))
                    {
                        //
                        packageDestinationPath = editorProjectTemplateDirectoryPath;
                    }
                }

                EditorGUILayout.Space(15f);

                EditorGUILayout.Separator();

                EditorGUILayout.Space(15f);

                EditorGUILayout.HelpBox(
                    "処理に時間がかかりすぎる場合は、プロジェクトの軽量化を検討してください。",
                    MessageType.Info
                );

                EditorGUI.BeginDisabledGroup(packageDestinationPath == "");
                {
                    if (GUILayout.Button("パッケージ生成"))
                    {
                        // 処理状態を「処理中」に設定
                        workState = WorkState.InProgress;

                        // パッケージ情報を前回設定値として保存
                        SavePackageInfo();


                        // パッケージ生成処理を開始
#                       pragma warning disable 
                        GenerateTemplatePackage();
#                       pragma warning restore
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        /// <summary>
        /// 現在設定されているパッケージ情報を、前回設定値としてEditorUserSettingsに保存します。
        /// </summary>
        private void SavePackageInfo()
        {
            // パッケージ情報の保存を試行
            try
            {
                // パッケージ情報をJSON形式にシリアライズ
                var previousPackageInfoJSON = JsonUtility.ToJson(packageInfo);

                // EditorUserSettingsにパッケージ情報を書き込み
                EditorUserSettings.SetConfigValue(
                    EDITOR_PREVIOUS_PACKAGE_INFO_USERSETTINGS_KEY,
                    previousPackageInfoJSON
                );
            }
            catch(Exception e)
            {
                Debug.LogError(
                    nameof(TemplateCreator)
                    + ": パッケージ情報の前回値保存に失敗しました。¥n"
                    + "RawMessage: "
                    + e
                );

                return;
            }

            Debug.Log(
                nameof(TemplateCreator)
                + ": パッケージ情報の前回値保存に成功しました。"
            );
        }

        /// <summary>
        /// エディタGUIの描画が更新されたときのコールバックメソッドです。
        /// </summary>
        private void OnGUI()
        {
            // CreateSpace(5f);
            // EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            // CreateSpace(5f);

            // モジュールの処理状態で分岐
            switch(workState)
            {
                // 初期化状態の場合 (何も表示しない)
                case WorkState.Initializing:
                {
                    break;
                }

                // 準備完了状態の場合 (パッケージ設定画面を表示)
                case WorkState.Ready:
                {
                    RenderConfigulationMenu();
                    break;
                }

                // パッケージ生成処理中の場合 (処理中である旨のメッセージを表示)
                case WorkState.InProgress:
                {
                    // TODO: 処理の進行状況の概要をここで表示させれば良いかなと思っている
                    // Mojatto, 2023/05/22 1:18

                    EditorGUILayout.HelpBox(
                        "プロジェクトテンプレートのパッケージを生成しています。\nこの処理には時間がかかります。",
                        MessageType.Info
                    );

                    EditorGUILayout.HelpBox(
                        "処理中は、プロジェクトに変更を加えないでください !!",
                        MessageType.Warning
                    );
                    break;
                }
            }

            GUILayout.FlexibleSpace();
            CreateSpace(5f);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(APPLICATION_VERSION);

                InspectorLibrary.RenderDocumentationButton(APPLICATION_DOC_URL);
            }
            EditorGUILayout.EndHorizontal();

        }

        /// <summary>
        /// 入力された値で高さ方向にスペースを挿入します。
        /// </summary>
        /// <param name="width"></param>
        private static void CreateSpace(float width)
        {
            // 
            EditorGUILayout.Space(width);
        }
    }
}