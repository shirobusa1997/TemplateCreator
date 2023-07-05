# TemplateCreator
[STABLE Repos] Editor extension to create project template packages (.tgz) for Unity Software.

TemplateCreatorは、現在作業中のプロジェクトデータから、UnityHub上で利用できるプロジェクトテンプレートパッケージを生成するためのエディタ拡張です。

このエディタ拡張を使用することで、ユーザは簡単な操作を行うだけで、任意のUnityプロジェクトをプロジェクトテンプレート化し、UnityHubからプロジェクトを新規作成する際のテンプレートとして簡単に再利用できるようになります。

## 0. ご利用になる前に

### 検証済み環境について

本エディタ拡張は、以下の環境にて開発・検証を行っています。

- OS
    - Windows 11
- Unity Software
    - Unity 2019.4.31f1 LTS (.NET 4.x)
    - Unity 2021.3.16f1 LTS (.NET Standard 2.1)
- IDE
    - Microsoft Visual Studio 2022 + Microsoft Visual Studio Code (with C# Extension)

### ライセンス関連について

このアプリケーションは、以下のライブラリを使用しています。

- SharpZipLib
    - 公式 Github Repository
    
    https://github.com/icsharpcode/SharpZipLib
    
    - ライセンス表記 (MIT License)
    
    ```jsx
    Copyright © 2000-2018 SharpZipLib Contributors
    
    Permission is hereby granted, free of charge, to any person obtaining a copy of this
    software and associated documentation files (the "Software"), to deal in the Software
    without restriction, including without limitation the rights to use, copy, modify, merge,
    publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
    to whom the Software is furnished to do so, subject to the following conditions:
    
    The above copyright notice and this permission notice shall be included in all copies or
    substantial portions of the Software.
    
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
    INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
    PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
    FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
    OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
    DEALINGS IN THE SOFTWARE.
    ```

### 本エディタ拡張の信頼性について

このエディタ拡張は、一通りの作業が行えることを当方の複数の環境にて検証を行っていますが、あくまで**一個人が現在進行系で開発中のプログラム**となります。

**本エディタ拡張を使用することによって生じる損害等については、一切の責任を負いかねます**。

### 本エディタ拡張のアップデート・保守について

このエディタ拡張は、主にユーザビリティの面で改善の必要があるため、今後一定期間にて継続的にアップデート・保守作業を行います。アップデート・保守を行うに当たり、ユーザの皆様からのご意見やご指摘・ご要望があれば、よりよいアップデート・保守に繋がります。

エディタ拡張の利用に当たり、気になった点や不具合等がありましたら、Twitter DM (@CONSTANTAN_moja) や、Github Repository の Issue ページにて、共有いただけますと幸いです。

### UnityPackage から導入する場合

BOOTH あるいは Github Repository 内 Release ページ (後日公開予定) より、最新の UnityPackage ファイルをダウンロードし、使用したいプロジェクトにインポートしてください。

通常では、`Assets/Plugins` にDLLファイルが配置されます。

正常に導入できていれば、Unityエディタウィンドウのメニューバーに、以下のような項目が追加されます。

```jsx
Tools > MJTStudio > TemplateCreator 
```

## 2. ユーザーガイド

詳細な使用方法については、以下のドキュメントページをご参照ください。

https://mjtstudio.notion.site/TemplateCreator-55e1727a348640958d38e3876ca64e7d?pvs=4

## 3. よくある質問

- このプログラムは安全ですか？
    - 重ねてになりますが、一通りの作業が行えることを当方の複数の環境にて検証を行っていますが、あくまで**「一個人が現在進行系で開発中のプログラム」**となります。本エディタ拡張を使用することによって生じる損害等については、**一切の責任を負いかねますことを、ご了承ください。**
    また、コード本体については、開発中にも悪意のあるコードが含まれてしまわないよう、細心の注意を払っておりますが、もしコード本体を確認したい場合は、後日 Github リポジトリを公開予定ですので、公開後にリポジトリページよりご確認いただけますと幸いです。
    (現状、コメント挿入などコード整理が十分に行えていない部分があるため、BOOTH にて配布しているデータは、DLLライブラリ化し難読化しています。ご了承ください。)

## 4. お問い合わせ

本エディタ拡張について、不明な点や指摘点、ご提案等ありましたら、後日オープン予定の Github リポジトリの Issue ページに投稿いただくか、以下の場所からメッセージを送信いただけますと幸いです。

- BOOTH ストアページのメッセージ
    
    [Unityエディタ拡張「TemplateCreator」(試験版)](https://shop-mojatto.booth.pm/items/4870221)
    
- 作成者の Twitter ダイレクトメッセージ
    
    [CONSTANTAN_moja](https://twitter.com/CONSTANTAN_moja)