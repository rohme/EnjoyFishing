![icon](http://i.imgur.com/pmstLip.png)EnjoyFishing
---
EnjoyFishingはFF11の釣りを支援するツールです。

## 使用方法
1. FF11を起動し、キャラクター選択後にEnjoyFishingを管理者モードで起動してください。  
	最初に、使用中のアドオン／プラグインを取得するのにチャットログが流れますのでびびらないように。
2. 起動しない場合、[動かない場合](#user-content-動かない場合)を参照してください。
3. 竿を餌をカバンに入れて装備してください。**ワードローブに入っている釣具は認識しません。**
4. デフォルトの設定では、釣る対象を未知の魚で外道（アイテム）・モンスター以外としています。  
とりあえず水辺に立って、そのまま開始ボタンで実行してください。
5. 掛かった魚がリストに追加されていくので、釣りたい魚にチェックを入れてください。
6. 目的に合わせて設定を変更してください。
7. Enjoy Fishing!!

## 主な機能
### 魚の管理方法
EnjoyFishingでは魚を扱うモードをIDモードと名前モードの二つから選択することができます。  
これは、釣った魚がクリティカルか否か、または匹数によってIDが分かれている為です。  
例えば、さびき針でキュスを釣った場合にには、`キュス1匹`・`キュス2匹`・`キュス3匹`・`キュス1匹クリティカル`・`キュス2匹クリティカル`・`キュス3匹クリティカル` と６種類のIDの魚が存在し、それぞれを別々に扱うのがIDモードで、ひと括りに扱うのが名前モードです。

### 魚リスト
![魚リスト](http://i.imgur.com/96OHRJE.png)

* モード  
    釣る対象の魚をID別・魚名別から選択することができます。  
    また切り替えた場合、魚リストのチェックがすべてクリアされます。
* 絞込
    - エリアで絞込・・・現在いるエリアで釣ったことのある魚で表示を絞り込むことができます。
    - 餌で絞込・・・現在装備しているエサで釣ったことのある魚で表示を絞り込むことができます。
* 魚リスト  
	ここでチェックされた魚が釣り対象の魚となります。（後述の学習モードがチェックされている場合は、未知の魚も対象となります）
* 未知の魚がかかってリリースされた場合、IDが含まれた魚名が一時的に割り当てられますが、魚名が判明した段階で変更されます。
* 魚名の末尾には以下の情報が付随されて表示されます。
    - (S)・・・小型の魚
    - (L)・・・大型の魚
    - (I)・・・アイテム・外道など
    - (M)・・・モンスター
    - x3・・・さびき針などで一度に複数匹釣れた場合に表示されます。（ID別の場合のみ）
    - !・・・クリティカルで釣れた魚の場合に表示されます。（ID別の場合のみ）

### 釣り設定
![釣り設定](http://i.imgur.com/96OHRJE.png)

#### 動作
* 学習モード  
    学習モードとは、未知のIDの魚がかかった場合には釣り上げて学習する機能です。  
    但し、除外条件にかかる魚は釣りません。  
    竿折れ・糸切れなどは考慮されていませんので、大型の魚がいる所で釣りをする場合は注意してください。
* スニーク  
	釣り中はスニークをかけておく機能です。  
	ログに`スニークが切れそうだ。`と表示された場合には、かけ直しを行います。  
	かけ直しを行う場合、詠唱中に既存のBUFFを切るのですが、詠唱終了何秒前に切るかを設定できます。  
	デフォルトは詠唱完了１秒前にしていますが、うまく動作しない場合はこの数値を調整してみてください。  
	仕組み上、かけ直しの時に一瞬だけスニークが切れるので、過信は禁物です。  
	この機能は、アドオンのCancelを使用していないと使えません。
* 強制HP0  
    魚のHPを強制的にゼロにします。  
    指定した範囲でランダムで待ち時間が入ります。  
    設定によっては魚がかかった瞬間に釣れてしまうので注意してください。
* 除外条件  
    - 小型(!)除外・・・かかった魚が小型魚の場合、リリースを行います。
    - 大型(!!!)除外・・・かかった魚が大型魚の場合、リリースを行います。
    - 外道除外・・・かかった魚がアイテムの場合、リリースを行います。
    - モンスター除外・・・かかった魚がモンスターの場合、リリースを行います。
* 反応時間  
    魚がかかってから格闘を開始するまでの待ち時間を設定できます。  
    指定した範囲でランダムにウェイトが入ります。
* リキャスト時間  
    魚を釣った後、次に釣るまでの待ち時間を設定できます。  
    指定した範囲でランダムにウェイトが入ります。
* ヴァナ時間  
    指定した範囲のヴァナ時間だけ釣りを実行することを指定できます。  
    夜間（１８：００～６：００）だけ実行したい場合には、１８～５を指定します。
* 地球時間  
    指定した範囲の地球時間だけ釣りを実行することを指定できます。  
    指定方法はヴァナ時間と同じです。

#### 停止条件
* 釣果数  
    本日の釣果数が指定された釣果数になった場合、釣りを停止します。  
    但し、０時で本日釣り上げた数のカウンターが０にクリアされます。
* 釣果無し  
    連続して指定された回数だけ何もかからなかった場合、釣りを停止します。
* スキル  
    指定された釣りスキル以上になった場合、釣りを停止します。  
    スキル上げで、スキル上限がわかっている魚を釣る場合に使用します。
* Tell/Say/Party/LS/Shout  
    それぞれの会話を受信した場合、釣りを停止します。
* 再始動  
    Tell/Say/Party/LS/Shoutを受信して停止した場合でも、指定した時間待った後に釣りを再開します。

#### 鞄いっぱい
* サッチェル/サック/ケース  
    鞄がいっぱいになった場合、鞄に入っている魚を移動しても良い場所を指定します。  
    この機能は、プラグインのItemizerを使用していないと使えません。
* コマンド  
    鞄がいっぱいになり、魚を移動できる場所もなくなった場合に実行されるコマンドを指定します。
    このコマンドは、FF11のチャットラインで実行できるコマンドなら何でも指定することができます。  
    デジョンしたい場合`/ma デジョン <me>`を指定   
    ピアスでセルビナにいきたい場合`/item セルビナピアス <me>`を指定  
    スクリプトを実行した場合`//exec script.txt`を指定

#### 竿・エサなし
* サッチェル/サック/ケース  
    竿折れが発生して予備の竿が鞄に入ってない場合や、エサが鞄から無くなった場合に、装備中の竿およびエサを指定した場所から取り出します。  
    この機能は、プラグインのItemizerを使用していないと使えません。
* コマンド  
    竿切れ、エサ切れが発生した場合に実行されるコマンドを指定します。  
    指定できるコマンドは「鞄いっぱい」の時と同様です。

### 情報
![情報](http://i.imgur.com/SceAkjr.png)

* 本日の釣果情報がサマリーで表示されます。
* 結果  
    以下の分類別に集計されます
    - Catch・・・釣り上げた
    - NoBait・・・何も釣れなかった
    - NoCatch・・・バラした、時間切れ
    - Releace・・・リリースした
    - LineBreak・・・糸切れ
    - RodBreak・・・竿折れ
* 結果％  
    分類別ごとの比率を表示します。
* 全体％  
    全体からの比率を表示します。

### 履歴
#### 情報
![履歴](http://i.imgur.com/XAVKpFZ.png)

* 日付  
    表示する日付を指定します。
* 更新  
    最新情報に更新します。
* ステータス  
    表示するデータを指定した値で絞り込みます。
* 魚  
    表示する魚を指定した値で絞り込みます。
* 表示できる項目は初期設定以外にもあり、[設定タブ](#user-content-設定)にて表示非表示を設定する事ができます。  
また、表示列順の入れ替え・列幅の変更・ソートを行うことができます。

#### 合計
![合計](http://i.imgur.com/K0W086X.png)

* 表示内容は釣り情報と同じですが、日付・ステータス・魚名での絞込みが可能です。

#### 設定
![設定](http://i.imgur.com/VsqOKnh.png)

##### 一般
* 設定の保存  
    設定の保存をキャラクター別にするか、すべてのキャラクターで統一するかを選択できます。
* Addon/Plugin  
    現在利用可能なアドオンおよびプラグインを表示します。  
    読み込まれていないものは名前が灰色で表示されます。  
    再取得したい場合は再取得ボタンを押してください。

##### ステータスバー表示
* 画面下部のステータスバーに表示する項目を指定できます。

##### 履歴 列表示
* 履歴の詳細タブに表示する項目を指定できます。

## 動かない場合
* 管理者権限で実行してください。
* [Windower4](http://windower.net/)をインストールする。
* 最新の[FFACE](http://www.ffevo.net/)をインストールする。
* [VisualStudio2013のランタイム](http://www.microsoft.com/ja-JP/download/details.aspx?id=40784)をインストールする。**（必ずx86版を使用してください）**
* [.Net4.0](http://www.microsoft.com/ja-jp/download/details.aspx?id=30653)をインストールする。

##今後の予定
* 自動ハラキリ
* 敵から攻撃受けたらデジョン等
* 開始前に指定した装備への着替
* MMMの釣りに対応
* 竿が折れた場合の自動修復
* 魚の情報をサーバーにて一元管理（よさげなフリーサーバーはないかのぅ・・・）

## 開発環境
* Windows7 Ultimate 64bit
* [Microsoft Visual Studio Express 2013 C#](http://www.visualstudio.com/ja-jp/products/visual-studio-express-vs.aspx)
* [.NET Framework 4.0](http://www.microsoft.com/ja-jp/net/)
* [Windower4](http://windower.net/)
* [FFACE](http://www.ffevo.net/)
* [FFACETools](https://github.com/h1pp0/FFACETools_ffevo.net/)

## ソース
EnjoyFishingは以下のサイトで、GPLv2ライセンスにて公開しています。  
[https://github.com/rohme/EnjoyFishing](https://github.com/rohme/EnjoyFishing)

## 免責事項
本ソフトはフリーソフトです。自由にご使用ください。  
このソフトウェアを使用したことによって生じたすべての障害・損害・不具合等に関しては、作者は一切の責任を負いません。各自の責任においてご使用ください。  

## 修正履歴
* 2015-01-08 Ver1.0.2
	- HPが0になっていないのに釣り上げたように見ていた
	- 釣り中に戦利品からアイテムが流れてきた場合、それを魚と誤認して登録していた
	- ディレクトリFishDBの名前がFIshDBになっていた
* 2015-01-06 Ver1.0.1
	- 竿残数とエサ残数の表示機能を追加
	- 釣果数が正常に計算されておらず、本日釣果数がうまく動作しなかった
	- IDモード時、開始ボタンで魚リストのチェックが消えていた
* 2015-01-05 Ver1.0.0
	- スニーク釣り機能を追加
	- FFを複数起動しているとき、POL選択画面が正常に動作していなかった
	- 起動時アドオンチェックにて、無限ループする
	- キャラ変更で再ログイン後フリーズする（何故か再現できなかったので、ログイン後キャラクターが表示されるまで待ったり、ウェイトを入れたりして調整してみた。）
	- キャラクター選択中には画面コントロールをロックする
	- XMLファイルに排他処理追加
* 2014-12-31 ver0.0.3 プレリリース3
	- プラットフォームを.NET4.5から.NET4.0に変更(XP対応)
	- キャラクター切り替え時にCPUリソースを大量に使ってしまう
	- 管理者権限で実行するようにmanifestの埋め込み
* 2014-12-30 ver0.0.2 プレリリース2
	- 大きな魚を小さな魚として登録していた
	- 竿折れが発生したとき、代わりの竿を装備しなかった
* 2014-12-28 ver0.0.1 プレリリース1