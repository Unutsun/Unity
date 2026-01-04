# ブロック崩し Unity版 セットアップガイド

このガイドでは、HTML5/Canvas版のブロック崩しをUnityで動かすための手順を説明します。

## 前提条件

- Unity 2021.3 LTS 以上
- TextMeshPro パッケージ（UIテキスト用）

## 1. プロジェクト作成

1. Unity Hub を開く
2. 「新規プロジェクト」をクリック
3. 設定:
   - テンプレート: **2D (Built-in Render Pipeline)**
   - プロジェクト名: `BreakoutGame`
   - 保存場所: 任意

## 2. スクリプトのインポート

1. Unity エディタで `Assets` フォルダを右クリック
2. Create > Folder で `Scripts` フォルダを作成
3. `Unity/Scripts/` 内の以下のファイルをコピー:
   - `BallController.cs`
   - `PaddleController.cs`
   - `BrickController.cs`
   - `BrickManager.cs`
   - `GameManager.cs`
   - `UIManager.cs`

## 3. タグの設定

1. Edit > Project Settings > Tags and Layers
2. Tags セクションで以下を追加:
   - `Ball`
   - `Paddle`
   - `Brick`
   - `Wall`
   - `DeadZone`

## 4. Physics Material 作成

1. Assets フォルダで右クリック
2. Create > 2D > Physics Material 2D
3. 名前: `BouncyBall`
4. インスペクタで設定:
   - Friction: `0`
   - Bounciness: `1`

## 5. ゲームオブジェクト作成

### 5.1 カメラ設定
1. Main Camera を選択
2. インスペクタ:
   - Size: `5`
   - Background: 任意の色（例: 暗い青）

### 5.2 Ball（ボール）
1. Hierarchy で右クリック > 2D Object > Sprites > Circle
2. 名前を `Ball` に変更
3. Transform:
   - Position: `(0, -3, 0)`
   - Scale: `(0.4, 0.4, 1)`
4. インスペクタで:
   - タグ: `Ball`
5. コンポーネント追加:
   - Rigidbody2D:
     - Gravity Scale: `0`
     - Collision Detection: `Continuous`
   - CircleCollider2D:
     - Material: `BouncyBall`
   - BallController スクリプト

### 5.3 Paddle（パドル）
1. Hierarchy で右クリック > 2D Object > Sprites > Square
2. 名前を `Paddle` に変更
3. Transform:
   - Position: `(0, -4, 0)`
   - Scale: `(2, 0.3, 1)`
4. インスペクタで:
   - タグ: `Paddle`
5. コンポーネント追加:
   - BoxCollider2D
   - PaddleController スクリプト

### 5.4 Brick（ブロック）プレハブ
1. Hierarchy で右クリック > 2D Object > Sprites > Square
2. 名前を `Brick` に変更
3. Transform:
   - Scale: `(1.5, 0.5, 1)`
4. インスペクタで:
   - タグ: `Brick`
5. コンポーネント追加:
   - BoxCollider2D
   - BrickController スクリプト
6. Prefabs フォルダを作成し、Brick をドラッグしてプレハブ化
7. Hierarchy から元の Brick を削除

### 5.5 BrickManager
1. Hierarchy で右クリック > Create Empty
2. 名前を `BrickManager` に変更
3. BrickManager スクリプトをアタッチ
4. インスペクタで:
   - Brick Prefab: 作成した Brick プレハブを設定

### 5.6 Walls（壁）
1. Hierarchy で右クリック > Create Empty
2. 名前を `Walls` に変更
3. 子オブジェクトとして以下を作成:

**Left Wall:**
- Position: `(-8.5, 0, 0)`
- Scale: `(1, 12, 1)`
- BoxCollider2D 追加
- タグ: `Wall`

**Right Wall:**
- Position: `(8.5, 0, 0)`
- Scale: `(1, 12, 1)`
- BoxCollider2D 追加
- タグ: `Wall`

**Top Wall:**
- Position: `(0, 5.5, 0)`
- Scale: `(18, 1, 1)`
- BoxCollider2D 追加
- タグ: `Wall`

**DeadZone（下境界）:**
- Position: `(0, -5.5, 0)`
- Scale: `(18, 1, 1)`
- BoxCollider2D 追加 (Is Trigger: ✓)
- タグ: `DeadZone`

### 5.7 GameManager
1. Hierarchy で右クリック > Create Empty
2. 名前を `GameManager` に変更
3. GameManager スクリプトをアタッチ
4. インスペクタで参照を設定:
   - Ball: Ball オブジェクト
   - Paddle: Paddle オブジェクト
   - Brick Manager: BrickManager オブジェクト

## 6. UI 設定

### 6.1 Canvas 作成
1. Hierarchy で右クリック > UI > Canvas
2. Canvas Scaler:
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`

### 6.2 スコア・残機表示
1. Canvas 下に UI > Text - TextMeshPro を作成
2. 名前: `ScoreText`
3. 位置: 左上
4. テキスト: `スコア: 0`

5. 同様に `LivesText` を作成
6. 位置: 右上
7. テキスト: `残機: 3`

### 6.3 パネル作成（例: StartPanel）
1. Canvas 下に UI > Panel を作成
2. 名前: `StartPanel`
3. 子に Button と Text を追加
4. 同様に:
   - `PausePanel`
   - `GameOverPanel`
   - `VictoryPanel`

### 6.4 UIManager 設定
1. Canvas に UIManager スクリプトをアタッチ
2. インスペクタで各 UI 要素を設定

## 7. Ball の参照設定

1. Ball オブジェクトを選択
2. BallController コンポーネントの Paddle フィールドに Paddle オブジェクトをドラッグ

## 8. シーン保存

1. File > Save As
2. Scenes フォルダを作成
3. `MainScene.unity` として保存

## 9. テストプレイ

1. Play ボタンをクリック
2. 動作確認:
   - パドルが ←→ キーで移動するか
   - マウスでパドルが追従するか
   - Space キーでボールが発射されるか
   - ブロックが壊れるか
   - スコアが更新されるか
   - ボールを落としたら残機が減るか

## トラブルシューティング

### ボールが反射しない
- Physics Material 2D の Bounciness が 1 になっているか確認
- Rigidbody2D の Collision Detection が Continuous になっているか確認

### パドルが動かない
- PaddleController の Min X / Max X が適切か確認
- カメラの Size に合わせて調整

### ブロックが生成されない
- BrickManager の Brick Prefab が設定されているか確認
- Brick プレハブに BoxCollider2D があるか確認

### UIが表示されない
- Canvas の Render Mode が適切か確認
- TextMeshPro がインポートされているか確認

## mcp-unity を使った自動セットアップ

Claude Code + mcp-unity を使うと、自然言語でゲームオブジェクトを作成・設定できます:

```
例: 「ボールを作成して、Rigidbody2Dを追加してGravity Scaleを0に設定して」
```

詳細は `README.yaml` の `mcp_unity_usage` セクションを参照してください。
