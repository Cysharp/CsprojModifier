# CsprojModifier

CsprojModifier は Unity Editor が .csproj を生成する際に追加の処理を行うことで、Visual Studio や Rider のような IDE での開発体験を向上させます。

CsprojModifier は次の特徴を備えています:

- 生成された .csproj に追加のプロジェクトを `Import` 要素で追加する
- 生成された .csproj に Analzyer の参照を追加する
  - 2020.2 以降で Rider または Visual Studio Code を利用している場合には無効

![](docs/images/Screen-01.png)

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [Works with](#works-with)
- [Install](#install)
- [Features](#features)
  - [生成された .csproj に追加のプロジェクトを `Import` 要素で追加する](#%E7%94%9F%E6%88%90%E3%81%95%E3%82%8C%E3%81%9F-csproj-%E3%81%AB%E8%BF%BD%E5%8A%A0%E3%81%AE%E3%83%97%E3%83%AD%E3%82%B8%E3%82%A7%E3%82%AF%E3%83%88%E3%82%92-import-%E8%A6%81%E7%B4%A0%E3%81%A7%E8%BF%BD%E5%8A%A0%E3%81%99%E3%82%8B)
    - [例](#%E4%BE%8B)
  - [生成された .csproj に Analzyer の参照を追加する](#%E7%94%9F%E6%88%90%E3%81%95%E3%82%8C%E3%81%9F-csproj-%E3%81%AB-analzyer-%E3%81%AE%E5%8F%82%E7%85%A7%E3%82%92%E8%BF%BD%E5%8A%A0%E3%81%99%E3%82%8B)
    - [使用方法](#%E4%BD%BF%E7%94%A8%E6%96%B9%E6%B3%95)
- [License](#license)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Works with
- Unity Editor 2019.4 (LTS) or later
- Visual Studio 2019 or Rider

## Install
Package Manager から git 経由でインストールします。

```
https://github.com/Cysharp/CsprojModifier.git?path=src/CsprojModifier/Assets/CsprojModifier
```

## Features

### 生成された .csproj に追加のプロジェクトを `Import` 要素で追加する
任意の追加のプロジェクトファイル (.props や .target) を生成された .csproj に `Import` 要素を使用して追加します。これによりプロジェクトにファイルを追加したり、参照を追加したりが可能になります。

**注意:** .csproj は Visual Studio や Rider のような IDE でのみ使用され、 Unity Editor での実際のビルドには使用されません

#### 例
例えば、次のような `YourAwesomeApp.DesignTime.props` というファイルを作成し、インポートすることで [BannedApiAnalyzer](https://github.com/dotnet/roslyn-analyzers/tree/main/src/Microsoft.CodeAnalysis.BannedApiAnalyzers) を使用できます。

```xml
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <Analyzer Include="Assets/Plugins/Editor/Analyzers/Microsoft.CodeAnalysis.BannedApiAnalyzers.dll" />
    <Analyzer Include="Assets/Plugins/Editor/Analyzers/Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll" />
  </ItemGroup>
  <ItemGroup>
    <AdditionalFiles Include="$(ProjectDir)\BannedSymbols.txt" />
  </ItemGroup>
</Project>
```

[BannedApiAnalyzer](https://github.com/dotnet/roslyn-analyzers/tree/main/src/Microsoft.CodeAnalysis.BannedApiAnalyzers) は BannedSymbols.txt が `AdditionalFiles` としてプロジェクトに追加されていることを期待しているため、これによってうまく機能するようになります。

### 生成された .csproj に Analzyer の参照を追加する
[Roslyn Analyzer は Unity 2020.2 以降でサポートされました](https://docs.unity3d.com/Manual/roslyn-analyzers.html)。しかしながら、現在 Roslyn Analyzer は .csproj に追加されず、コンパイル時にのみ使用されます。

この拡張は `Analyzer` 要素を .csproj プロジェクトファイルが生成される際に追加します。その結果、Visual Studio でコード編集時にも Roslyn Analyzer の恩恵を受けることができます。(もちろん、2020.2 以前にも対応します)

#### 使用方法
[Unity 2020.2 と同様の方法で Roslyn Analyzer をプロジェクトに追加します](https://docs.unity3d.com/Manual/roslyn-analyzers.html)

- Roslyn Analyzer ライブラリを追加する
- Plugin インスペクターですべてのターゲットプラットフォームからチェックを外す
- `RoslynAnalyzer` アセットラベルをライブラリに付与する

## License
MIT License