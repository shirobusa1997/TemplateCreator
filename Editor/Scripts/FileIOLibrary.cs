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
namespace MJTStudio.TemplateCreator.Editor
{
    /// <summary>
    /// ファイル処理に関わる処理を提供するライブラリクラスです。
    /// </summary>
    public static class FileIOLibrary
    {
        /// <summary>
        /// 指定されたパスにアクセス権限があるかをチェックする際に使用するダミーのフォルダ名
        /// </summary>
        private const string FOLDER_NAME_FOR_WRITE_PERMISSION_CHECK = "_DUMMY";

        /// <summary>
        /// 指定されたパスに書き込み権限があるかをチェックします。
        /// </summary>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        public static bool CheckFileWritePermission(string targetPath)
        {
            // ダミーのフォルダを作成して削除することで、指定されたパスに書き込み権限があるかをチェックする
            // ダミーのフォルダ名を加えた権限チェック先へのパスを生成する
            string tempPath = Path.Combine(targetPath, FOLDER_NAME_FOR_WRITE_PERMISSION_CHECK);

            // ディレクトリ作成を試みる
            try
            {
                //
                Directory.CreateDirectory(tempPath);
                Directory.Delete(tempPath);
            }
            // 例外処理
            // 例外内容に関わらず、権限なしとみなしfalseを返す
            catch (Exception e)
            {
                Debug.LogError(
                    typeof(FileIOLibrary).Name
                    + ": 指定されたパスへの書き込み権限がありません。\n"
                    + "RawMessage: "
                    + e
                );
                return false;
            }

            // ここまで到達した場合は、指定されたパスに書き込み権限があるとみなしtrueを返す
            Debug.Log(
                typeof(FileIOLibrary).Name
                + ": 書き込みチェック OK\n"
                + "-> "
                + tempPath
            );
            return true;
        }

        /// <summary>
        /// 指定されたパスのディレクトリを、目的のパスに非同期にコピーする
        /// </summary>
        /// <param name="targetDirPath">コピーするディレクトリのパス</param>
        /// <param name="destDirPath">コピー先のディレクトリのパス</param>
        /// <param name="isRecursive">サブディレクトリも再帰的にコピーするか</param>
        /// <returns>タスクインスタンス</returns>
        public static async Task CopyDirectoryAsync(
            string targetDirPath, 
            string destDirPath, 
            bool   isRecursive, 
            int    parentTaskID = -1, 
            string taskName = "None", 
            CancellationToken cts = default
        )
        {
            // キャンセルされた場合は例外をスローする
            cts.ThrowIfCancellationRequested();

            // ディレクトリ情報を取得
            DirectoryInfo targetDirInfo = new DirectoryInfo(targetDirPath);

#       if UNITY_2020_1_OR_NEWER
            //
            int id = -1;

            if (parentTaskID != -1)
            {
                //
                id = Progress.Start(
                    (taskName != "None") ? taskName : ("Copy Directory: " + targetDirInfo.Name),
                    parentId: parentTaskID
                );

                Progress.Report(
                    id,
                    0.0f,
                    "Copy Directory: "
                    + targetDirInfo.Name
                );
            }
#       else
            EditorUtility.DisplayProgressBar(
                "Copy Directory: " + targetDirInfo.Name,
                "Copy Directory: " + targetDirInfo.Name,
                0.0f
            );   
#       endif

            // ディレクトリが存在しない場合は例外をスローする
            if (!targetDirInfo.Exists)
            {
                //
                string message =
                    ": 指定されたディレクトリは存在しません。\n"
                    + "-> "
                    + targetDirInfo.FullName
                ;

                // コール元に例外をスロー
                throw new InvalidOperationException(
                    typeof(FileIOLibrary).Name
                    + message
                );
            }

            // ディレクトリ内のファイル情報を取得
            string[] filesPaths       = Directory.GetFiles(targetDirPath, "*", SearchOption.AllDirectories);
            int      currentFileCount = 0;
            int      totalFileCount   = filesPaths.Length;
            int      fileCountDigits  = totalFileCount.ToString().Length;

            // コピー先にディレクトリが存在しない場合は新たに作成する
            Directory.CreateDirectory(destDirPath);

            // 指定されたパスの数分繰り返す
            foreach(string filePath in filesPaths)
            {
                // 指定ディレクトリの複製を試みる
                try
                {
                    // 個々のファイルのコピー先のパスを生成する
                    string relativeTargetPath = GetRelativePath(targetDirPath, filePath);
                    string destPath           = Path.Combine(destDirPath, relativeTargetPath);

                    // コピー先のディレクトリが存在しない場合は新たに作成する
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                    // ファイルをコピーする
                    using (FileStream targetStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (FileStream destStream = File.Create(destPath))
                        {
                            await targetStream.CopyToAsync(destStream);
                        }
                    }

                    //
                    currentFileCount++;

#               if UNITY_2020_1_OR_NEWER
                    //
                    Progress.Report(
                        id,
                        (float)currentFileCount / (float)totalFileCount,
                        currentFileCount.ToString($"D{fileCountDigits}") + "/" + totalFileCount.ToString($"D{fileCountDigits}") + " files copied."
                    );
#               else
                    //
                    EditorUtility.DisplayProgressBar(
                        "Copy Directory: " + targetDirInfo.Name,
                        currentFileCount.ToString($"D{fileCountDigits}") + "/" + totalFileCount.ToString($"D{fileCountDigits}") + " files copied.",
                        (float)currentFileCount / (float)totalFileCount
                    );
#               endif
                }
                //
                catch (IOException e)
                {
                    Debug.LogWarning(
                        typeof(FileIOLibrary).Name
                        + ": このファイルはアクセスが拒否されたため、コピーされませんでした。\n"
                        + "-> "
                        + filePath
                        + "\nRawMessage: "
                        + e
                    );
                    continue;
                }
                //
                catch (Exception e)
                {
                    Debug.LogError(
                        typeof(FileIOLibrary).Name
                        + ": ハンドリングされていない例外がスローされました。処理を中断します。\n"
                        + "RawMessage: "
                        + e
                    );

#               if UNITY_2020_1_OR_NEWER
                    //
                    Progress.Cancel(id);
#               else
                    //
                    EditorUtility.ClearProgressBar();
#               endif

                    return;
                }
            }

#       if UNITY_2020_1_OR_NEWER
            //
            Progress.Finish(id);
#       else
            //
            EditorUtility.ClearProgressBar();
#       endif
        }

        /// <summary>
        /// 指定された2つのパス間の相対パスを取得します。
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        public static string GetRelativePath(string basePath, string targetPath)
        {
            // 基準パス、到達先パス、相対パスのUriを生成する
            Uri baseUri     = new Uri(basePath + Path.DirectorySeparatorChar);
            Uri targetUri   = new Uri(targetPath);
            Uri relativeUri = baseUri.MakeRelativeUri(targetUri);

            // 相対パスをディレクトリ区切り文字に置換して返す
            return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
        }
    }    
}