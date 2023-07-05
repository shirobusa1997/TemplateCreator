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
    /// 
    /// </summary>
    public static class FileIOLibrary
    {
        /// <summary>
        /// 
        /// </summary>
        private const string FOLDER_NAME_FOR_WRITE_PERMISSION_CHECK = "_DUMMY";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        public static bool CheckFileWritePermission(string targetPath)
        {
            //
            string tempPath = Path.Combine(targetPath, FOLDER_NAME_FOR_WRITE_PERMISSION_CHECK);

            try
            {
                //
                Directory.CreateDirectory(tempPath);
                Directory.Delete(tempPath);
            }
            //
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

            Debug.Log(
                typeof(FileIOLibrary).Name
                + ": 書き込みチェック OK\n"
                + "-> "
                + tempPath
            );

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetDirPath"></param>
        /// <param name="destDirPath"></param>
        /// <param name="isRecursive"></param>
        /// <returns></returns>
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

                // throw new exception
                throw new InvalidOperationException(
                    typeof(FileIOLibrary).Name
                    + message
                );
            }

            // ディレクトリ内のファイル情報を取得
            // DirectoryInfo[] targetDirs = targetDirInfo.GetDirectories();
            string[] filesPaths       = Directory.GetFiles(targetDirPath, "*", SearchOption.AllDirectories);
            int      currentFileCount = 0;
            int      totalFileCount   = filesPaths.Length;
            int      fileCountDigits  = totalFileCount.ToString().Length;

            // コピー先にディレクトリが存在しない場合は新たに作成する
            Directory.CreateDirectory(destDirPath);

            //
            // foreach(FileInfo fileInfo in targetDirInfo.GetFiles())
            foreach(string filePath in filesPaths)
            {
                //
                try
                {
                    //
                    string relativeTargetPath = GetRelativePath(targetDirPath, filePath);
                    string destPath           = Path.Combine(destDirPath, relativeTargetPath);

                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                    //
                    using (FileStream targetStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        //
                        using (FileStream destStream = File.Create(destPath))
                        {
                            //
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

            //
            // if (isRecursive)
            // {
            //     //
            //     foreach (DirectoryInfo subDir in targetDirs)
            //     {
            //         //
            //         string subDestDir = Path.Combine(destDirPath, subDir.Name);

            //         //
            //         await CopyDirectoryAsync(subDir.FullName, subDestDir, true);
            //     }
            // }
        }

        /// <summary>
        /// 指定された2つのパス間の相対パスを取得します。
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        public static string GetRelativePath(string basePath, string targetPath)
        {
            //
            Uri baseUri     = new Uri(basePath + Path.DirectorySeparatorChar);
            Uri targetUri   = new Uri(targetPath);
            Uri relativeUri = baseUri.MakeRelativeUri(targetUri);

            //
            return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
        }
    }    
}