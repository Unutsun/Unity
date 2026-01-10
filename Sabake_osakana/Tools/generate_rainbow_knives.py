#!/usr/bin/env python3
"""
虹色の包丁画像を生成するスクリプト
元のknife.pngを読み込んで、6色のバリエーションを作成
"""

from PIL import Image
import os

# 虹色の定義（RGB）
RAINBOW_COLORS = {
    "red": (230, 50, 50),
    "blue": (50, 130, 230),
    "yellow": (230, 200, 50),
    "green": (50, 200, 80),
    "purple": (150, 50, 200),
    "pink": (230, 100, 180),
}

def colorize_image(image, target_color):
    """
    画像を指定色で色調統一
    元の明度を維持しつつ、色相を変更
    """
    img = image.convert("RGBA")
    pixels = img.load()
    width, height = img.size

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]

            if a == 0:
                continue  # 透明ピクセルはスキップ

            # 元のピクセルの明度を計算（グレースケール値）
            luminance = (r * 0.299 + g * 0.587 + b * 0.114) / 255.0

            # ターゲット色に明度を適用
            new_r = int(target_color[0] * luminance)
            new_g = int(target_color[1] * luminance)
            new_b = int(target_color[2] * luminance)

            # 少し明るさを調整（暗くなりすぎないように）
            brightness_boost = 0.3
            new_r = min(255, int(new_r + target_color[0] * brightness_boost))
            new_g = min(255, int(new_g + target_color[1] * brightness_boost))
            new_b = min(255, int(new_b + target_color[2] * brightness_boost))

            pixels[x, y] = (new_r, new_g, new_b, a)

    return img

def main():
    # 元の包丁画像を読み込み
    input_path = "../Assets/Resources/Sprites/knife.png"
    output_dir = "../Assets/Resources/Sprites/Knives"

    if not os.path.exists(input_path):
        print(f"Error: {input_path} not found!")
        return

    # 出力ディレクトリ作成
    os.makedirs(output_dir, exist_ok=True)

    # 元画像を読み込み
    original = Image.open(input_path)
    print(f"Loaded: {input_path} ({original.size[0]}x{original.size[1]})")

    # 各色で生成
    for color_name, rgb in RAINBOW_COLORS.items():
        colored = colorize_image(original.copy(), rgb)
        output_path = os.path.join(output_dir, f"knife_{color_name}.png")
        colored.save(output_path, "PNG")
        print(f"Generated: {output_path}")

    print(f"\nDone! Generated {len(RAINBOW_COLORS)} colored knife images.")

if __name__ == "__main__":
    main()
