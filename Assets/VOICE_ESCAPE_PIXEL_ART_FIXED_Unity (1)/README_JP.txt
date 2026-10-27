VOICE ESCAPE / Phase 10 ドット絵素材 修正版
======================================

■ 修正した問題
1. 待機画像：左右の足をそろえた静止姿勢を使用。
2. 歩行01 / 02：元の同じ脚を繰り返すのではなく、左足前・右足前の2状態になるよう脚の位置を変更。
   横向きでは脚の前後関係を逆にし、キャラクターの向きは変えない。
3. 透過：キャラの外側だけ透過。キラーの仮面の黒い目と発電機の機体内部には不透明な暗色の裏地を入れた。
4. 画像サイズ：プレイヤー・キラーいずれも128×144px、共通キャンバス。
5. ステージ床は不透明な全面画像、2700×1900px。

■ Unityへの導入
 - Characters/Player : Player_Idle_Down.pngなどの待機4枚＋各方向Walk_01/02
 - Characters/Killer : 同上。Killer_Walk_Left_01.pngなど。
 - AnimationsPreview : 方向ごとに01/02が交互に動く確認用GIF計8本。Unityへのインポート不要。
 - CharacterSheets : 4列×3行のスプライトシート。
 - Floor/Walls/Obstacles/Generators/Lockers/Doors/UI : 機能別の個別PNG。
 - Unity設定：Sprite (2D and UI) / Single / Point (no filter) / Compression None。
 - 床PPU 100、Transform Scale(1,1,1)で約27×19ワールドユニット。
 - ゲームの当たり判定（Rigidbody2D/Collider2D）には触らず、VisualのSpriteRendererを置き換えること。

■ 注意
このパックは元のドット絵キャラの頭・胴体・服装のデザインを残し、脚部分と透過処理を改修した版です。
画像生成による素材をベースにしているため、ゲームで大きく拡大した際は小さな輪郭の個体差が残る場合があります。
