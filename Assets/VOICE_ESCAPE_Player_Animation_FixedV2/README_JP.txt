VOICE ESCAPE / PLAYER ANIMATION / FIXED V2

修正内容:
・待機(Idle)4方向を追加。立った状態で両足を揃えたものです。
・Rightの足欠けを修正。足先が欠けていないLeftの4フレームを左右反転して作成しました。
・全20枚 / 176x196ピクセル / 透過PNG / 足元ライン統一。

Unity 6の設定:
・Sprite (2D and UI), Single, Point (no filter), Compression None
・CharacterSpriteAnimator2D の Idle Down/Up/Left/Right に各 Player_Idle_*.png を登録
・Walk Down/Up/Left/Rightの配列Sizeを各4にし、Player_Walk_方向_01～04を登録
・同じSpriteRendererをAnimatorからも制御しないでください
・キャラクターサイズはVisualのScaleから変更し、親のColliderを変更しないでください

備考:
・元画像はAI生成のピクセルアートです。右向きは左右反転によって足の欠落を防止しています。
・各方向のGIFで歩行確認できます。
