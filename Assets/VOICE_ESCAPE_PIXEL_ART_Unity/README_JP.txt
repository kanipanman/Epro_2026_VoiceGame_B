VOICE ESCAPE Phase 10 ドット絵版（全差し替え）

Characters/Player と Characters/Killer：上下左右それぞれ待機・歩行_01・歩行_02（各12枚）。
CharacterSheets：4列（下・上・左・右）× 3行（待機・歩行01・歩行02）のスプライトシート。
Floor：2700×1900ピクセルの全面床。PPU=100、Scale=(1,1,1)で27×19ユニット。
Walls：奥・左右・下・壊れた壁のモジュール。
Obstacles：中央の大型コンクリートブロック、瓦礫など。
Generators/Lockers/Doors：既存処理へ差し替え可能な装置の画像。

Unity設定：Texture Type=Sprite (2D and UI)、Filter Mode=Point (no filter)、Compression=None。
既存のCollider2DとRigidbody2Dは削除せず、SpriteRenderer側だけ差し替えること。
黒背景から透過を抽出しているため、細い輪郭に微調整が必要な場合があります。
