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

namespace MJTStudio.TemplateCreator.Editor
{
    /// <summary>
    /// エディタ拡張の動作状態を表す列挙型です。
    /// </summary>
    public enum WorkState
    {
        Initializing,   // 初期化中   ※デフォルト値
        Ready,          // 準備完了
        InProgress      // 処理中
    }

    /// <summary>
    /// エディタ拡張のワークフローの種類を表す列挙型です。
    /// </summary>
    public enum WorkflowType
    {
        None,                   // 未定義   ※デフォルト値
        RegisterToUnityHub,     // パッケージ生成 & Unity Hub への登録
        PackageGenerationOnly   // パッケージ生成のみ
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class PackageInfo
    {
        /// <summary>
        /// 生成するパッケージ名称を保持するフィールドです。
        /// 例: com.<組織名>.template.<プロジェクト名など>
        /// </summary>
        public string name = "com.COMPANYNAME.template.PROJECTNAME";

        /// <summary>
        /// 
        /// </summary>
        public string displayName = "よく使うものをまとめたテンプレートプロジェクト";

        /// <summary>
        /// 
        /// </summary>
        public string version = "1.0.0";

        /// <summary>
        /// 
        /// </summary>
        public string type = "template";

        /// <summary>
        /// 
        /// </summary>
        public string unity = "2021.3";

        /// <summary>
        /// 
        /// </summary>
        public string description = "よく使うものをまとめたテンプレートプロジェクトです。";
    }
}