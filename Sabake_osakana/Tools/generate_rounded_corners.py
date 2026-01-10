#!/usr/bin/env python3
"""
角丸四角形の角部分を生成するスクリプト
Unity UI用の9-sliceスプライトとして使用可能
"""

from PIL import Image, ImageDraw
import os

def generate_rounded_corner(
    size: int = 64,
    radius: int = 16,
    color: tuple = (204, 204, 204, 255),  # ライトグレー (0.8, 0.8, 0.8)
    border_color: tuple = (153, 153, 153, 255),  # 枠線色 (0.6, 0.6, 0.6)
    border_width: int = 2,
    output_path: str = "rounded_panel.png"
):
    """
    角丸四角形を生成

    Args:
        size: 画像サイズ（正方形）
        radius: 角の半径
        color: 背景色 (R, G, B, A)
        border_color: 枠線色 (R, G, B, A)
        border_width: 枠線の太さ
        output_path: 出力ファイルパス
    """
    # アンチエイリアス用に4倍サイズで描画
    scale = 4
    large_size = size * scale
    large_radius = radius * scale
    large_border = border_width * scale

    # 透明な背景で画像作成
    img = Image.new('RGBA', (large_size, large_size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # 枠線を含む角丸四角形を描画（外側）
    if border_width > 0:
        draw.rounded_rectangle(
            [0, 0, large_size - 1, large_size - 1],
            radius=large_radius,
            fill=border_color
        )

    # 内側の角丸四角形を描画
    inner_offset = large_border
    draw.rounded_rectangle(
        [inner_offset, inner_offset, large_size - 1 - inner_offset, large_size - 1 - inner_offset],
        radius=max(0, large_radius - large_border),
        fill=color
    )

    # 元のサイズに縮小（アンチエイリアス効果）
    img = img.resize((size, size), Image.LANCZOS)

    # 保存
    os.makedirs(os.path.dirname(output_path) if os.path.dirname(output_path) else '.', exist_ok=True)
    img.save(output_path, 'PNG')
    print(f"生成完了: {output_path}")
    return img


def generate_9slice_corners(
    corner_size: int = 32,
    radius: int = 16,
    color: tuple = (204, 204, 204, 255),
    border_color: tuple = (153, 153, 153, 255),
    border_width: int = 2,
    output_dir: str = "."
):
    """
    9-slice用の角4つを個別に生成
    """
    scale = 4
    large_size = corner_size * scale
    large_radius = radius * scale
    large_border = border_width * scale

    # フル画像を作成
    full_size = large_size * 3  # 9-slice用に3x3
    img = Image.new('RGBA', (full_size, full_size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # 枠線付き角丸四角形
    if border_width > 0:
        draw.rounded_rectangle(
            [0, 0, full_size - 1, full_size - 1],
            radius=large_radius,
            fill=border_color
        )

    inner_offset = large_border
    draw.rounded_rectangle(
        [inner_offset, inner_offset, full_size - 1 - inner_offset, full_size - 1 - inner_offset],
        radius=max(0, large_radius - large_border),
        fill=color
    )

    # 縮小
    final_size = corner_size * 3
    img = img.resize((final_size, final_size), Image.LANCZOS)

    os.makedirs(output_dir, exist_ok=True)

    # 各角を切り出し
    corners = {
        'top_left': (0, 0, corner_size, corner_size),
        'top_right': (final_size - corner_size, 0, final_size, corner_size),
        'bottom_left': (0, final_size - corner_size, corner_size, final_size),
        'bottom_right': (final_size - corner_size, final_size - corner_size, final_size, final_size),
    }

    for name, box in corners.items():
        corner = img.crop(box)
        path = os.path.join(output_dir, f"corner_{name}.png")
        corner.save(path, 'PNG')
        print(f"生成完了: {path}")

    # フル画像も保存（9-slice用）
    full_path = os.path.join(output_dir, "rounded_panel_9slice.png")
    img.save(full_path, 'PNG')
    print(f"生成完了: {full_path}")

    return img


if __name__ == "__main__":
    # 出力ディレクトリ
    output_dir = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "Sprites", "UI")

    # GameColorsに合わせた色設定
    panel_color = (204, 204, 204, 255)      # 0.8 * 255 = 204 (ライトグレー)
    border_color = (153, 153, 153, 255)     # 0.6 * 255 = 153 (ダークグレー枠線)

    print("=== 角丸パネル画像を生成 ===")
    print(f"出力先: {output_dir}")
    print()

    # メインの9-slice用パネル画像（96x96、角半径24px）
    generate_rounded_corner(
        size=96,
        radius=24,
        color=panel_color,
        border_color=border_color,
        border_width=3,
        output_path=os.path.join(output_dir, "rounded_panel.png")
    )

    # 小さめのボタン用（64x64、角半径16px）
    generate_rounded_corner(
        size=64,
        radius=16,
        color=panel_color,
        border_color=border_color,
        border_width=2,
        output_path=os.path.join(output_dir, "rounded_button.png")
    )

    # 枠線なしバージョン
    generate_rounded_corner(
        size=96,
        radius=24,
        color=panel_color,
        border_color=panel_color,  # 枠線なし
        border_width=0,
        output_path=os.path.join(output_dir, "rounded_panel_no_border.png")
    )

    print()
    print("=== 完了 ===")
    print("Unity側の設定:")
    print("1. 生成された画像をインポート")
    print("2. Sprite Editorで Border を設定（例: L=24, R=24, T=24, B=24）")
    print("3. Image の Image Type を 'Sliced' に設定")
