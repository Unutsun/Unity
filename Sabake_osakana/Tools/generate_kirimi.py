#!/usr/bin/env python3
"""
魚の切り身画像を生成するスクリプト
サイズ: 1155x1155 (sakana_normal.pngと同じ)
"""

from PIL import Image, ImageDraw
import math

def generate_kirimi(output_path, size=1155):
    """サーモン風の切り身画像を生成"""

    # 透明な背景
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # 切り身の色
    salmon_color = (255, 140, 105, 255)  # サーモンピンク
    fat_color = (255, 245, 238, 255)     # 白い脂
    skin_color = (180, 180, 180, 255)    # 皮（グレー）

    # 中心と大きさ
    cx, cy = size // 2, size // 2
    scale = size / 1155  # スケール係数

    # 切り身の形状（楕円を基本に）
    # メインの身
    body_width = int(800 * scale)
    body_height = int(500 * scale)

    # 切り身の輪郭を描く（少し斜めの楕円形）
    points = []
    for i in range(360):
        angle = math.radians(i)
        # 楕円 + 少し変形
        x = cx + body_width * 0.5 * math.cos(angle)
        y = cy + body_height * 0.5 * math.sin(angle)
        # 上部を少し平らに、下部を丸く
        if math.sin(angle) < 0:
            y += 30 * scale * math.sin(angle)
        points.append((x, y))

    # メインの身を描画
    draw.polygon(points, fill=salmon_color)

    # 白い脂の筋を描く
    for i in range(6):
        stripe_y = cy - body_height * 0.3 + i * (body_height * 0.1)
        stripe_width = body_width * (0.7 - abs(i - 2.5) * 0.1)
        stripe_x1 = cx - stripe_width * 0.4
        stripe_x2 = cx + stripe_width * 0.4

        # 波打つ脂の筋
        wave_points = []
        for j in range(int(stripe_x1), int(stripe_x2), 5):
            wave_y = stripe_y + math.sin((j - stripe_x1) * 0.05) * 8 * scale
            wave_points.append((j, wave_y))

        if len(wave_points) > 1:
            draw.line(wave_points, fill=fat_color, width=int(12 * scale))

    # 皮の部分（下端）
    skin_points = []
    for i in range(180, 360):
        angle = math.radians(i)
        x = cx + body_width * 0.5 * math.cos(angle)
        y = cy + body_height * 0.5 * math.sin(angle) + 20 * scale
        skin_points.append((x, y))

    if len(skin_points) > 1:
        draw.line(skin_points, fill=skin_color, width=int(25 * scale))

    # アンチエイリアス処理（4倍で描画して縮小）
    img_large = Image.new('RGBA', (size * 2, size * 2), (0, 0, 0, 0))
    draw_large = ImageDraw.Draw(img_large)

    # 大きいサイズで再描画
    cx2, cy2 = size, size
    scale2 = scale * 2

    # 切り身の輪郭
    points2 = []
    for i in range(360):
        angle = math.radians(i)
        x = cx2 + body_width * math.cos(angle)
        y = cy2 + body_height * math.sin(angle)
        if math.sin(angle) < 0:
            y += 60 * scale * math.sin(angle)
        points2.append((x, y))

    draw_large.polygon(points2, fill=salmon_color)

    # 白い脂の筋（大きいサイズ）
    for i in range(7):
        stripe_y = cy2 - body_height * 0.35 + i * (body_height * 0.12)
        stripe_width = body_width * (0.8 - abs(i - 3) * 0.08)
        stripe_x1 = cx2 - stripe_width * 0.45
        stripe_x2 = cx2 + stripe_width * 0.45

        wave_points = []
        for j in range(int(stripe_x1), int(stripe_x2), 8):
            wave_y = stripe_y + math.sin((j - stripe_x1) * 0.03) * 15 * scale
            wave_points.append((j, wave_y))

        if len(wave_points) > 1:
            draw_large.line(wave_points, fill=fat_color, width=int(20 * scale2))

    # 皮の部分（大きいサイズ）
    skin_points2 = []
    for i in range(170, 370):
        angle = math.radians(i)
        x = cx2 + body_width * math.cos(angle)
        y = cy2 + body_height * math.sin(angle) + 35 * scale2
        skin_points2.append((x, y))

    if len(skin_points2) > 1:
        draw_large.line(skin_points2, fill=skin_color, width=int(40 * scale2))

    # 縮小してアンチエイリアス
    img_final = img_large.resize((size, size), Image.LANCZOS)

    # 保存
    img_final.save(output_path, 'PNG')
    print(f"切り身画像を生成しました: {output_path}")
    print(f"サイズ: {size}x{size}")

if __name__ == "__main__":
    output = "../Assets/Resources/Sprites/kirimi.png"
    generate_kirimi(output)
