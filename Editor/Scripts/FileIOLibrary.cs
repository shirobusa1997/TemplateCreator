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
            bool isRecursive,
            int parentTaskID = -1,
            string taskName = "None",
            CancellationToken cts = default,
            System.Collections.Generic.IEnumerable<string> relativeExclusionPaths = null
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

            // Create a list of tasks to copy files in parallel
            System.Collections.Generic.List<Task> copyTasks = new System.Collections.Generic.List<Task>();

            foreach (string filePath in filesPaths)
            {
                copyTasks.Add(Task.Run(async () =>
                {
                    // 指定ディレクトリの複製を試みる
                    try
                    {
                        // 個々のファイルのコピー先のパスを生成する
                        string relativeTargetPath = GetRelativePath(targetDirPath, filePath);

                        // Check if the file should be excluded
                        if (relativeExclusionPaths != null)
                        {
                            bool excluded = false;
                            foreach (string exclusion in relativeExclusionPaths)
                            {
                                if (relativeTargetPath.StartsWith(exclusion + Path.DirectorySeparatorChar) || relativeTargetPath == exclusion)
                                {
                                    excluded = true;
                                    break;
                                }
                            }
                            if (excluded)
                            {
                                // Decrement total file count as this file will be skipped
                                Interlocked.Decrement(ref totalFileCount);
                                // Update progress immediately for skipped files to maintain accuracy if possible, or just skip.
                                // For simplicity, we just skip here. The progress might be slightly off if many files are excluded.
                                // A more accurate way would be to pre-filter filesPaths or adjust totalFileCount more dynamically.
                                return;
                            }
                        }

                        string destPath = Path.Combine(destDirPath, relativeTargetPath);

                        // コピー先のディレクトリが存在しない場合は新たに作成する
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                        // ファイルをコピーする
                        using (FileStream targetStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (FileStream destStream = File.Create(destPath))
                            {
                                await targetStream.CopyToAsync(destStream, cts);
                            }
                        }

                        // Increment currentFileCount atomically and update progress
                        int currentCount = Interlocked.Increment(ref currentFileCount);
                        int currentTotalFiles = Volatile.Read(ref totalFileCount); // Read the potentially modified totalFileCount

#                   if UNITY_2020_1_OR_NEWER
                        //
                        if (id != -1) // Only report progress if a valid progress ID exists
                        {
                            Progress.Report(
                                id,
                                (currentTotalFiles > 0) ? (float)currentCount / (float)currentTotalFiles : 0.0f,
                                currentCount.ToString($"D{fileCountDigits}") + "/" + currentTotalFiles.ToString($"D{fileCountDigits}") + " files copied."
                            );
                        }
#                   else
                        //
                        EditorUtility.DisplayProgressBar(
                            "Copy Directory: " + targetDirInfo.Name,
                            currentCount.ToString($"D{fileCountDigits}") + "/" + currentTotalFiles.ToString($"D{fileCountDigits}") + " files copied.",
                            (currentTotalFiles > 0) ? (float)currentCount / (float)currentTotalFiles : 0.0f
                        );
#                   endif
                    }
                    // Handle IOException specifically for a file, allowing other files to continue copying
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
                        // If a file is skipped due to IOException, decrement totalFileCount.
                        Interlocked.Decrement(ref totalFileCount);
                    }
                    // Catch other exceptions that might occur during a single file copy
                    catch (Exception e)
                    {
                        Debug.LogError(
                            typeof(FileIOLibrary).Name
                            + ": ファイルコピー中にエラーが発生しました: " + filePath + "\n"
                            + "RawMessage: "
                            + e
                        );
                        // If a file is skipped due to other exceptions, decrement totalFileCount.
                        Interlocked.Decrement(ref totalFileCount);
                        // Optionally, rethrow if you want Task.WhenAll to catch it, or handle specific cleanup
                    }
                }, cts));
            }

            try
            {
                await Task.WhenAll(copyTasks);
            }
            catch (Exception e) // Catch exceptions from Task.WhenAll (e.g., if a task was faulted)
            {
                Debug.LogError(
                    typeof(FileIOLibrary).Name
                    + ": ファイルの並列コピー中にエラーが発生しました。\n"
                    + "RawMessage: "
                    + e
                );
#               if UNITY_2020_1_OR_NEWER
                //
                if (id != -1) Progress.Cancel(id);
#               else
                //
                EditorUtility.ClearProgressBar();
#               endif
                // Depending on the desired behavior, you might want to rethrow or handle more gracefully
                throw; // Rethrowing the exception to indicate failure of the copy operation
            }

            // Final progress update to ensure it reaches 100% if all files were processed or skipped.
            // This is important if totalFileCount was decremented.
            int finalCurrentCount = Volatile.Read(ref currentFileCount);
            int finalTotalCount = Volatile.Read(ref totalFileCount);

#       if UNITY_2020_1_OR_NEWER
            if (id != -1)
            {
                Progress.Report(
                    id,
                    (finalTotalCount > 0 && finalCurrentCount >= finalTotalCount) ? 1.0f : ((finalTotalCount > 0) ? (float)finalCurrentCount / finalTotalCount : 0.0f),
                    (finalTotalCount > 0 && finalCurrentCount >= finalTotalCount) ? "Copy complete." : (finalCurrentCount.ToString($"D{fileCountDigits}") + "/" + finalTotalCount.ToString($"D{fileCountDigits}") + " files copied.")
                );
                Progress.Finish(id);
            }
#       else
            if (finalTotalCount > 0 && finalCurrentCount >= finalTotalCount)
            {
                 EditorUtility.ClearProgressBar(); // Or set to 100% and then clear
            }
            else
            {
                EditorUtility.DisplayProgressBar(
                    "Copy Directory: " + targetDirInfo.Name,
                     finalCurrentCount.ToString($"D{fileCountDigits}") + "/" + finalTotalCount.ToString($"D{fileCountDigits}") + " files copied.",
                    (finalTotalCount > 0) ? (float)finalCurrentCount / finalTotalCount : 0.0f
                );
            }
            // Ensure progress bar is cleared if it wasn't by reaching 100%
            // This might be redundant if Progress.Finish or ClearProgressBar is always called on success/failure paths
            if (!(finalTotalCount > 0 && finalCurrentCount >= finalTotalCount)) {
                 EditorUtility.ClearProgressBar();
            }
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