#!/usr/bin/env python3
"""
fix_ws_cards.py

Post-process a Weiss Schwarz cards JSON export to fill missing
`サイド` and `色` by inferring from card numbers and by guessing
icon URLs under the same domain (e.g. /wordpress/wp-content/images/cardlist/_partimages/*.gif).

Usage:
  python fix_ws_cards.py [input.json] [output.json] [--verify]

If --verify is passed the script will perform HTTP HEAD requests to
confirm icon URLs exist (requires `requests`). If network isn't
available or `requests` isn't installed the script will still write
guesses but won't verify them.
"""
from __future__ import annotations
import json
import os
import re
import sys
from urllib.parse import urlparse, urljoin

try:
    import requests
except Exception:
    requests = None


COLOR_MAP = {
    "red": "赤",
    "blue": "青",
    "yellow": "黄",
    "green": "緑",
    "purple": "紫",
    "white": "白",
    "black": "黒",
}

SIDE_MAP = {
    "W": "ヴァイス",
    "S": "シュヴァルツ",
}


def url_exists(url: str, timeout: float = 5.0) -> bool:
    if not requests:
        return False
    try:
        r = requests.head(url, allow_redirects=True, timeout=timeout)
        return r.status_code == 200
    except Exception:
        return False


def infer_side_from_card_no(card_no: str) -> tuple[str, str] | None:
    # Attempt to capture a capital letter immediately after the first '/'
    # e.g. DC/W01-001 -> 'W' :ヴァイス
    if not card_no:
        return None
    m = re.search(r"/([A-Z])", card_no)
    if not m:
        return None
    letter = m.group(1)
    if letter in SIDE_MAP:
        return letter, SIDE_MAP[letter]
    return None


def build_partimage_base(image_url: str) -> str | None:
    # Derive base domain for partimages from example image_url
    # input: https://ws-tcg.com/wordpress/wp-content/images/cardlist/d/dc_w01/dc_w01_001.png
    # output: https://ws-tcg.com/wordpress/wp-content/images/cardlist/_partimages/
    if not image_url:
        return None
    p = urlparse(image_url)
    if not p.scheme or not p.netloc:
        return None
    # path up to /wordpress/wp-content/images/cardlist/
    m = re.search(r"(.*/wordpress/wp-content/images/cardlist/)", p.path)
    if not m:
        # fallback to site root + expected path
        base = f"{p.scheme}://{p.netloc}/wordpress/wp-content/images/cardlist/_partimages/"
        return base
    prefix = m.group(1)
    return f"{p.scheme}://{p.netloc}{prefix}_partimages/"


def process_cards(cards: list[dict], verify: bool = False) -> list[dict]:
    for card in cards:
        # Ensure keys exist
        side = card.get("サイド") or ""
        color = card.get("色") or ""
        image_url = card.get("image_url") or card.get("detail_page_url") or ""
        card_no = card.get("card_no") or ""

        part_base = build_partimage_base(image_url)

        # サイド inference
        if not side or str(side).strip() == "":
            inferred = infer_side_from_card_no(card_no)
            if inferred:
                letter, name = inferred
                guessed_img = None
                if part_base:
                    guessed_img = urljoin(part_base, f"{letter.lower()}.gif")
                    if verify and url_exists(guessed_img):
                        card["サイド_img"] = guessed_img
                        card["サイド"] = name
                    elif verify and not url_exists(guessed_img):
                        # verified absent, still keep the textual inference
                        card["サイド"] = name
                        card.setdefault("サイド_img_candidates", []).append(guessed_img)
                    else:
                        # no verification requested or requests unavailable
                        card["サイド"] = name
                        if guessed_img:
                            card.setdefault("サイド_img_candidates", []).append(guessed_img)
                else:
                    card["サイド"] = name

        # 色 inference
        if not color or str(color).strip() == "":
            found = False
            candidates = []
            if part_base:
                for cname, jname in COLOR_MAP.items():
                    guessed = urljoin(part_base, f"{cname}.gif")
                    candidates.append((cname, jname, guessed))
                # If verify requested, check which exists
                if verify and requests:
                    for cname, jname, guessed in candidates:
                        if url_exists(guessed):
                            card["色_img"] = guessed
                            card["色"] = jname
                            found = True
                            break
                # If not verified or nothing matched, at least provide candidates
                if not found:
                    card.setdefault("色_img_candidates", [g for _, _, g in candidates])
            # else: cannot build candidates; leave blank

    return cards


def main(argv: list[str] | None = None) -> int:
    argv = argv if argv is not None else sys.argv[1:]
    if len(argv) >= 1:
        inp = argv[0]
    else:
        inp = os.path.join(os.path.dirname(__file__), "weiss_schwarz_cards.json")
    if len(argv) >= 2:
        outp = argv[1]
    else:
        outp = os.path.join(os.path.dirname(__file__), "weiss_schwarz_cards.fixed.json")
    verify = "--verify" in argv

    if verify and not requests:
        print("[warning] --verify requested but `requests` not available; continuing without verification.")
        verify = False

    if not os.path.exists(inp):
        print(f"Input file not found: {inp}")
        return 2

    with open(inp, "r", encoding="utf-8") as f:
        cards = json.load(f)

    # Backup original
    backup = inp + ".bak"
    if not os.path.exists(backup):
        with open(backup, "w", encoding="utf-8") as b:
            json.dump(cards, b, ensure_ascii=False, indent=2)

    fixed = process_cards(cards, verify=verify)

    with open(outp, "w", encoding="utf-8") as f:
        json.dump(fixed, f, ensure_ascii=False, indent=2)

    print(f"Wrote fixed file to: {outp}")
    if os.path.exists(backup):
        print(f"Backup saved at: {backup}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
