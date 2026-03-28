#!/usr/bin/env python3
"""
fix_ws_cards.py

Convert raw Weiss Schwarz scraping output into the normalized card JSON
shape used by the Unity runtime model (`WeissCardData`).

Usage:
  python fix_ws_cards.py [input.json] [output.json]

Default input:
  python/tools/weiss_schwarz_cards.json

Default output:
  Assets/StreamingAssets/WeissSchwarz/cards.json
"""
from __future__ import annotations

import json
import os
import re
import sys
from urllib.parse import urlparse


COLOR_MAP = {
    "red": "Red",
    "blue": "Blue",
    "yellow": "Yellow",
    "green": "Green",
    "purple": "Purple",
    "white": "White",
    "black": "Black",
    "赤": "Red",
    "青": "Blue",
    "黄": "Yellow",
    "緑": "Green",
    "紫": "Purple",
    "白": "White",
    "黒": "Black",
}

SIDE_MAP = {
    "W": "Weiss",
    "S": "Schwarz",
    "ヴァイス": "Weiss",
    "シュヴァルツ": "Schwarz",
    "Weiss": "Weiss",
    "Schwarz": "Schwarz",
}

CARD_TYPE_MAP = {
    "キャラ": "Character",
    "キャラクター": "Character",
    "Character": "Character",
    "イベント": "Event",
    "Event": "Event",
    "クライマックス": "Climax",
    "Climax": "Climax",
}

TRIGGER_MAP = {
    "": "None",
    "-": "None",
    "－": "None",
    "なし": "None",
    "None": "None",
}

TRIGGER_ICON_MAP = {
    "soul": "Soul",
    "draw": "Draw",
    "salvage": "Comeback",
    "treasure": "Treasure",
    "choice": "Choice",
    "stock": "Stock",
    "bounce": "Bounce",
    "shot": "Shot",
    "gate": "Gate",
    "standby": "Standby",
    "chance": "Chance",
    "discovery": "Discovery",
    "bushi": "Bushi",
}


def safe_int(value, default=0):
    if value is None:
        return default
    if isinstance(value, int):
        return value
    text = str(value).strip()
    if not text:
        return default
    match = re.search(r"-?\d+", text)
    if not match:
        return default
    return int(match.group(0))


def ensure_list(value):
    if value is None:
        return []
    if isinstance(value, list):
        return value
    if isinstance(value, str):
        value = value.strip()
        return [value] if value else []
    return [str(value)]


def ensure_text(value, default=""):
    if value is None:
        return default
    if isinstance(value, list):
        texts = [str(item).strip() for item in value if str(item).strip()]
        return " ".join(texts) if texts else default
    text = str(value).strip()
    return text if text else default


def infer_work_id(card_code: str) -> str:
    if not card_code:
        return ""
    separator_index = card_code.find("/")
    return card_code[:separator_index] if separator_index > 0 else card_code


def infer_side_from_card_code(card_code: str) -> str:
    if not card_code:
        return ""
    match = re.search(r"/([A-Z])", card_code)
    if not match:
        return ""
    return SIDE_MAP.get(match.group(1), "")


def infer_from_image_filename(url: str) -> str:
    if not url:
        return ""
    parsed = urlparse(url)
    filename = os.path.basename(parsed.path).lower()
    return os.path.splitext(filename)[0]


def image_list(card: dict, plural_key: str, singular_key: str) -> list[str]:
    plural_value = card.get(plural_key)
    if isinstance(plural_value, list):
        return [str(item).strip() for item in plural_value if str(item).strip()]
    if isinstance(plural_value, str) and plural_value.strip():
        return [plural_value.strip()]

    singular_value = card.get(singular_key)
    if isinstance(singular_value, list):
        return [str(item).strip() for item in singular_value if str(item).strip()]
    if isinstance(singular_value, str) and singular_value.strip():
        return [singular_value.strip()]

    return []


def normalize_color(card: dict) -> str:
    raw_color = card.get("color") or card.get("色")
    if raw_color:
        normalized_color = ensure_text(raw_color)
        return COLOR_MAP.get(normalized_color, normalized_color)

    image_hint = infer_from_image_filename(card.get("色_img", ""))
    return COLOR_MAP.get(image_hint, "")


def normalize_side(card: dict) -> str:
    raw_side = card.get("side") or card.get("サイド")
    if raw_side:
        normalized_side = ensure_text(raw_side)
        return SIDE_MAP.get(normalized_side, normalized_side)

    image_hint = infer_from_image_filename(card.get("サイド_img", "")).upper()
    if image_hint in SIDE_MAP:
        return SIDE_MAP[image_hint]

    return infer_side_from_card_code(card.get("cardCode") or card.get("card_no") or "")


def normalize_card_type(card: dict) -> str:
    raw_type = card.get("cardType") or card.get("type") or card.get("種類")
    if not raw_type:
        return ""
    raw_type = ensure_text(raw_type)
    return CARD_TYPE_MAP.get(raw_type, raw_type)


def normalize_trigger(card: dict) -> str:
    trigger_icons = []
    for url in image_list(card, "トリガー_imgs", "トリガー_img"):
        icon_name = TRIGGER_ICON_MAP.get(infer_from_image_filename(url))
        if icon_name:
            trigger_icons.append(icon_name)

    if trigger_icons:
        return " ".join(trigger_icons)

    raw_trigger = card.get("trigger") or card.get("トリガー") or ""
    raw_trigger = ensure_text(raw_trigger)
    return TRIGGER_MAP.get(raw_trigger, raw_trigger)


def normalize_soul(card: dict) -> int:
    raw_soul = card.get("soul")
    if raw_soul is None:
        raw_soul = card.get("ソウル")

    normalized_soul = safe_int(raw_soul, default=-1)
    if normalized_soul >= 0:
        return normalized_soul

    soul_icons = image_list(card, "ソウル_imgs", "ソウル_img")
    if soul_icons:
        return len(soul_icons)

    return 0


def normalize_traits(card: dict) -> list[str]:
    raw_traits = card.get("traits")
    if raw_traits is None:
        raw_traits = card.get("特徴")

    traits = []
    for value in ensure_list(raw_traits):
        text = str(value).strip()
        if not text or text == "特徴なし":
            continue
        traits.append(text)
    return traits


def normalize_abilities(card: dict) -> list[str]:
    raw_abilities = card.get("abilities")
    if raw_abilities is None:
        raw_abilities = card.get("text")

    abilities = []
    for value in ensure_list(raw_abilities):
        text = str(value).strip()
        if text:
            abilities.append(text)
    return abilities


def build_model_card(raw_card: dict) -> dict:
    card_code = ensure_text(raw_card.get("cardCode") or raw_card.get("card_no"))

    return {
        "cardCode": card_code,
        "name": ensure_text(raw_card.get("name")),
        "workId": ensure_text(raw_card.get("workId") or infer_work_id(card_code)),
        "detailPageUrl": ensure_text(raw_card.get("detailPageUrl") or raw_card.get("detail_page_url")),
        "imageUrl": ensure_text(raw_card.get("imageUrl") or raw_card.get("image_url")),
        "side": normalize_side(raw_card),
        "cardType": normalize_card_type(raw_card),
        "level": safe_int(raw_card.get("level") or raw_card.get("レベル")),
        "cost": safe_int(raw_card.get("cost") or raw_card.get("コスト")),
        "power": safe_int(raw_card.get("power") or raw_card.get("パワー")),
        "soul": normalize_soul(raw_card),
        "color": normalize_color(raw_card),
        "rarity": ensure_text(raw_card.get("rarity") or raw_card.get("レアリティ")),
        "trigger": normalize_trigger(raw_card),
        "flavorText": ensure_text(
            raw_card.get("flavorText")
            or raw_card.get("flavor_text")
            or raw_card.get("フレーバー")
        ),
        "traits": normalize_traits(raw_card),
        "abilities": normalize_abilities(raw_card),
    }


def process_cards(cards: list[dict]) -> list[dict]:
    return [build_model_card(card) for card in cards]


def main(argv: list[str] | None = None) -> int:
    argv = argv if argv is not None else sys.argv[1:]
    script_dir = os.path.dirname(os.path.abspath(__file__))
    project_root = os.path.abspath(os.path.join(script_dir, "..", ".."))

    if len(argv) >= 1:
        input_path = argv[0]
    else:
        input_path = os.path.join(script_dir, "weiss_schwarz_cards.json")

    if len(argv) >= 2:
        output_path = argv[1]
    else:
        output_path = os.path.join(project_root, "Assets", "StreamingAssets", "WeissSchwarz", "cards.json")

    if not os.path.exists(input_path):
        print(f"Input file not found: {input_path}")
        return 2

    with open(input_path, "r", encoding="utf-8") as f:
        cards = json.load(f)

    normalized_cards = process_cards(cards)

    output_dir = os.path.dirname(output_path)
    if output_dir:
        os.makedirs(output_dir, exist_ok=True)

    with open(output_path, "w", encoding="utf-8") as f:
        json.dump(normalized_cards, f, ensure_ascii=False, indent=2)

    print(f"Wrote normalized runtime cards JSON to: {output_path}")
    print(f"Card count: {len(normalized_cards)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
