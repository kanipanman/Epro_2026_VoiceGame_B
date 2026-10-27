VOICE ESCAPE 2D pixel-art FIXED edition / Phase 10

This package replaces the previously flawed pixel pack.
KEY CHANGES:
 - Character Idle: both feet in neutral stance.
 - Walk01 / Walk02: two distinct hand-edited leg poses. Left and right foot extension genuinely alternate.
 - PNG transparent OUTSIDE sprites; opaque inside heads, killer eye sockets and machinery backing.
 - All characters consistent 128 x 144 transparent RGBA canvas, lower feet bottom-aligned.
 - Prop silhouettes and interior holes reinforced; floor fully opaque.
 - 8 animated GIF previews (per character, per direction).

Characters/Player and Characters/Killer filenames:
Player_Idle_Down.png, Player_Walk_Down_01.png, Player_Walk_Down_02.png, etc.
Walk01 means LEFT FOOT FRONT for front/back views; Walk02 means RIGHT FOOT FRONT.
Side views alternate which side leg projects forward. Pair walk01/walk02 in your Unity script.

The closed/open double-slide EXIT: Door_Locked_Red.png, Door_Unlocked_Green_Open.png.
All static props remain in familiar directories.

Unity Import: Texture Type=Sprite (2D and UI); Mode=Single; Filter=Point (no filter); Compression=None. No mipmaps.
Character: Visual child SpriteRenderer only (keep physics/Collider on parent).
Stage floor 2700 x 1900, PPU=100 gives approx. 27 x 19 world units.

Check AnimationsPreview/*.gif to assess stepping before importing. Artistic poses are hand-adjusted from the base design for clear 2-frame gait (not AI-generated arbitrary frames).
