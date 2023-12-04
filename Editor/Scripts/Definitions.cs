using System.Security.Cryptography;
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
    /// パッケージ情報のデータ構造を定義するクラスです。
    /// </summary>
    [Serializable]
    public sealed class PackageInfo
    {
        /// <summary>
        /// 生成するパッケージ名称を保持するフィールドです。
        /// PackageManagerでの識別情報として使用されます。
        /// 例: com.<組織名>.template.<プロジェクト名など>
        /// </summary>
        public string name = "com.COMPANYNAME.template.PROJECTNAME";

        /// <summary>
        /// 生成するパッケージの表示名を保持するフィールドです。
        /// UnityHub上で表示されます。
        /// </summary>
        public string displayName = "よく使うものをまとめたテンプレートプロジェクト";

        /// <summary>
        /// 生成するパッケージのバージョン番号を保持するフィールドです。
        /// UnityHub上で表示されます。
        /// </summary>
        public string version = "1.0.0";

        /// <summary>
        /// 生成するパッケージの種別を指定するフィールドです。
        /// Unity プロジェクトのテンプレートでは、"template" と指定する必要があります。
        /// </summary>
        public string type = "template";

        /// <summary>
        /// 生成するパッケージがサポートする Unity エディタバージョンを指定するフィールドです。
        /// ここで指定した Unity エディタバージョンでプロジェクトを作成しようとする場合にのみ、
        /// 当該パッケージがテンプレートリストに表示されるようになります。
        /// </summary>
        public string unity = "2021.3";

        /// <summary>
        /// 生成するパッケージの説明文を保持するフィールドです。
        /// UnityHub上で表示されます。
        /// </summary>
        public string description = "よく使うものをまとめたテンプレートプロジェクトです。";

        /// <summary>
        /// このクラスのコンストラクタです。
        /// 引数を指定しない場合、すべてのフィールドにおいてデフォルト値が適用されます。
        /// </summary>
        public PackageInfo() {}

        /// <summary>
        /// このクラスのコンストラクタです。
        /// 各種パラメータの初期値を指定したい場合に使用します。
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="_displayName"></param>
        /// <param name="_version"></param>
        /// <param name="_type"></param>
        /// <param name="_unity"></param>
        /// <param name="_description"></param>
        public PackageInfo(
            string _name,
            string _displayName,
            string _version,
            string _type,
            string _unity,
            string _description
        )
        {
            name        = _name;
            displayName = _displayName;
            version     = _version;
            type        = _type;
            unity       = _unity;
            description = _description;
        }
    }
}