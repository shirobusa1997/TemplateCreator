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

// SharpZipLib Namespaces
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;

// UnityEngine Namespaces
using UnityEngine;

// Package Namespaces
namespace MJTStudio.TemplateCreator.Editor
{
    /// <summary>
    /// パッケージのアーカイブ関係の処理を提供するライブラリクラスです。
    /// </summary>
    public static class ArchiverLibrary
    {
        /// <summary>
        /// 指定されたパスのファイルをTarアーカイブ化し、GZip形式で圧縮します。
        /// </summary>
        /// <param name="destPath">圧縮ファイル作成先のパス</param>
        /// <param name="targetPaths">Tarアーカイブ・圧縮を行う対象のパス</param>
        /// <param name="isRecursive">配下ディレクトリのファイルも再帰的にTarアーカイブ化するかどうか</param>
        /// <param name="compressionLevel">圧縮レベル (初期値:1)</param>
        /// <returns></returns>
        public static bool CompressDirectoriesToTgz(string destPath, string[] targetPaths, bool isRecursive, int compressionLevel = 1)
        {
            // 圧縮ファイル生成のためのストリームを作成する
            using (FileStream fs = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
            // GZip圧縮のためのストリームを作成する
            using (GZipOutputStream gZipOutputStream = new GZipOutputStream(fs))
            {
                // 圧縮レベルを設定する
                gZipOutputStream.SetLevel(compressionLevel);

                // Tarアーカイブを作成する
                using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gZipOutputStream, TarBuffer.DefaultBlockFactor))
                {
                    // ファイルのTarアーカイブ化・圧縮を試行する
                    try
                    {
                        // アーカイブ元のファイルを維持するように設定する
                        tarArchive.SetKeepOldFiles(true);

                        // アーカイブ対象ファイル文字コード変換を行わないように設定する
                        tarArchive.AsciiTranslate = false;

                        // 指定されたパスの数繰り返す
                        foreach (string targetPath in targetPaths)
                        {
                            // 指定されたパスのファイルをTarアーカイブのエントリとして生成する
                            var tarEntry = TarEntry.CreateEntryFromFile(targetPath);

                            // Tarアーカイブにエントリを追加する
                            tarArchive.WriteEntry(tarEntry, isRecursive);
                        }
                    }
                    // ファイルのアーカイブ中に何らかの例外がスローされた場合の処理
                    catch (Exception e)
                    {
                        Debug.LogError(
                            typeof(ArchiverLibrary).Name
                            + ": ファイルのアーカイブ中に例外がスローされました。\n"
                            + "RawMessage: "
                            + e
                        );

                        return false;
                    }

                    // Tarアーカイブのストリームを閉じる
                    tarArchive.Close();
                }
            }

            //
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destPath"></param>
        /// <param name="targetPaths"></param>
        /// <param name="isRecursive"></param>
        /// <param name="compressionLevel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        // public static async Task CompressDirectoriesToTgzAsync(
        //     string destPath,
        //     string[] targetPaths,
        //     bool isRecursive,
        //     int compressionLevel = 1,
        //     CancellationToken cancellationToken = default
        // )
        // {
            
        // }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFolderPath"></param>
        /// <param name="tgzFilePath"></param>
        /// <returns></returns>
        // public static async Task ArchiveAndCompressFolderAsync(string sourceFolderPath, string tgzFilePath)
        // {
        //     // フォルダ内のすべてのファイルを取得する
        //     string[] files = Directory.GetFiles(sourceFolderPath, "*", SearchOption.AllDirectories);

        //     // Tarアーカイブファイルを作成する
        //     using var archiveStream = new MemoryStream();
        //     using (var archive = new TarArchive(archiveStream))
        //     {
        //         foreach (string file in files)
        //         {
        //             // アーカイブにファイルを追加する
        //             string entryName = file.Substring(sourceFolderPath.Length).Replace('\\', '/');
        //             var entry = new TarArchiveEntry(entryName);
        //             entry.Size = new FileInfo(file).Length;
        //             entry.LastModificationTime = File.GetLastWriteTime(file);
        //             await using var entryStream = await File.OpenRead(file).ConfigureAwait(false);
        //             await archive.WriteEntryHeaderAsync(entry).ConfigureAwait(false);
        //             await entryStream.CopyToAsync(archive).ConfigureAwait(false);
        //             await archive.FlushAsync().ConfigureAwait(false);
        //         }
        //     }

        //     // Tgzファイルを作成する
        //     using var tgzStream = new FileStream(tgzFilePath, FileMode.Create);
        //     using (var gzip = new GZipStream(tgzStream, CompressionLevel.Optimal, leaveOpen: true))
        //     {
        //         await archiveStream.CopyToAsync(gzip).ConfigureAwait(false);
        //         await gzip.FlushAsync().ConfigureAwait(false);
        //     }
        // }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    // public static async Task Main(string[] args)
    // {
    //     string sourceFolderPath = @"C:\path\to\folder";
    //     string tgzFilePath = @"C:\path\to\output.tgz";

    //     await ArchiveAndCompressFolderAsync(sourceFolderPath, tgzFilePath);
    // }
}