#!/usr/bin/env python3
"""
import_to_sqlite.py

Stream-import Weiss Schwarz card JSON into a local SQLite database.

Features:
- Streaming parse large JSON arrays using ijson (no full memory load)
- Batch INSERT with ON CONFLICT upsert on card_no
- Basic normalization (level/power/cost -> ints or NULL)
- Infer work_id from card_no
- Create indexes after import (configurable)

Usage:
  python import_to_sqlite.py --input weiss_schwarz_cards.fixed.json --db ws_cards.db

If ijson is not installed the script will fall back to a memory-load (not recommended for very large files).
"""
from __future__ import annotations
import argparse
import json
import os
import re
import sqlite3
import sys
from datetime import datetime

try:
    import ijson
except Exception:
    ijson = None


CREATE_TABLE_SQL = """
CREATE TABLE IF NOT EXISTS cards(
  id INTEGER PRIMARY KEY,
  card_no TEXT UNIQUE,
  name TEXT,
  work_id TEXT,
  detail_page_url TEXT,
  image_url TEXT,
  side TEXT,
  color TEXT,
  type TEXT,
  level INTEGER,
  power INTEGER,
  cost INTEGER,
  rarity TEXT,
  trigger TEXT,
  flavor_text TEXT,
  abilities_json TEXT,
  traits_json TEXT,
  metadata TEXT,
  visual_local_path TEXT,
  visual_fetch_status INTEGER DEFAULT 0,
  created_at TEXT DEFAULT CURRENT_TIMESTAMP,
  updated_at TEXT
);
"""

INSERT_SQL = """
INSERT INTO cards(
  card_no,name,work_id,detail_page_url,image_url,side,color,type,level,power,cost,rarity,trigger,flavor_text,abilities_json,traits_json,metadata,updated_at
)
VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)
ON CONFLICT(card_no) DO UPDATE SET
  name=excluded.name,
  work_id=excluded.work_id,
  detail_page_url=excluded.detail_page_url,
  image_url=excluded.image_url,
  side=excluded.side,
  color=excluded.color,
  type=excluded.type,
  level=excluded.level,
  power=excluded.power,
  cost=excluded.cost,
  rarity=excluded.rarity,
  trigger=excluded.trigger,
  flavor_text=excluded.flavor_text,
  abilities_json=excluded.abilities_json,
  traits_json=excluded.traits_json,
  metadata=excluded.metadata,
  updated_at=excluded.updated_at;
"""


def create_schema(conn: sqlite3.Connection) -> None:
    cur = conn.cursor()
    cur.executescript(CREATE_TABLE_SQL)
    conn.commit()


def infer_work_id(card_no: str) -> str | None:
    # Example: DC/W01-016 -> DC/W01 as work/set id
    if not card_no:
        return None
    m = re.match(r'^([^\-]+)', card_no)
    if m:
        return m.group(1)
    return card_no


def safe_int(v):
    if v is None:
        return None
    s = str(v).strip()
    if s == '' or s == '-':
        return None
    # remove non-digit chars
    s2 = re.sub(r'[^0-9-]', '', s)
    try:
        return int(s2)
    except Exception:
        return None


def normalize_card(card: dict) -> tuple:
    card_no = card.get('card_no') or card.get('cardNo') or card.get('cardNo'.upper())
    name = card.get('name')
    work_id = infer_work_id(card_no) if card_no else None
    detail = card.get('detail_page_url')
    image = card.get('image_url')
    side = card.get('サイド') or card.get('side') or ''
    color = card.get('色') or card.get('color') or ''
    type_ = card.get('種類') or card.get('type')
    level = safe_int(card.get('レベル') or card.get('level'))
    power = safe_int(card.get('パワー') or card.get('power'))
    cost = safe_int(card.get('コスト') or card.get('cost'))
    rarity = card.get('レアリティ') or card.get('rarity')
    trigger = card.get('トリガー') or card.get('trigger')
    flavor = card.get('flavor_text') or card.get('フレーバー') or card.get('flavor')
    abilities = card.get('abilities') or []
    traits = card.get('特徴') or []
    metadata = card
    updated_at = datetime.utcnow().isoformat()

    return (
        card_no, name, work_id, detail, image, side, color, type_, level, power, cost,
        rarity, trigger, flavor, json.dumps(abilities, ensure_ascii=False), json.dumps(traits, ensure_ascii=False), json.dumps(metadata, ensure_ascii=False), updated_at
    )


def import_stream(input_path: str, db_path: str, batch_size: int = 1000, create_indexes: bool = True, max_rows: int | None = None):
    if not os.path.exists(input_path):
        print('Input file not found:', input_path)
        return 2

    conn = sqlite3.connect(db_path)
    conn.execute('PRAGMA journal_mode=WAL;')
    conn.execute('PRAGMA synchronous=NORMAL;')
    create_schema(conn)

    cur = conn.cursor()

    batch = []
    total = 0

    def flush_batch():
        nonlocal batch, total
        if not batch:
            return
        cur.executemany(INSERT_SQL, batch)
        conn.commit()
        total += len(batch)
        print(f'Imported {total} rows...')
        batch = []

    if ijson:
        with open(input_path, 'rb') as f:
            items = ijson.items(f, 'item')
            for i, card in enumerate(items, start=1):
                batch.append(normalize_card(card))
                if len(batch) >= batch_size:
                    flush_batch()
                if max_rows and i >= max_rows:
                    break
    else:
        # fallback (not streaming) - careful with large files
        print('ijson not available: falling back to full JSON load (may use lots of memory)')
        with open(input_path, 'r', encoding='utf-8') as f:
            cards = json.load(f)
            for i, card in enumerate(cards, start=1):
                batch.append(normalize_card(card))
                if len(batch) >= batch_size:
                    flush_batch()
                if max_rows and i >= max_rows:
                    break

    flush_batch()

    if create_indexes:
        print('Creating indexes...')
        cur.executescript("""
        CREATE INDEX IF NOT EXISTS idx_cards_work_id ON cards(work_id);
        CREATE INDEX IF NOT EXISTS idx_cards_side ON cards(side);
        CREATE INDEX IF NOT EXISTS idx_cards_color ON cards(color);
        CREATE INDEX IF NOT EXISTS idx_cards_type_level ON cards(type, level);
        CREATE INDEX IF NOT EXISTS idx_cards_cardno ON cards(card_no);
        """)
        conn.commit()

    conn.close()
    print('Import finished. Total imported:', total)
    return 0


def main(argv=None):
    p = argparse.ArgumentParser(description='Stream-import Weiss Schwarz JSON to SQLite')
    p.add_argument('--input', '-i', required=True, help='Input JSON file (array of cards)')
    p.add_argument('--db', '-d', default='ws_cards.db', help='SQLite DB path')
    p.add_argument('--batch', type=int, default=1000, help='Batch size for inserts')
    p.add_argument('--no-index', dest='create_indexes', action='store_false', help='Do not create indexes after import')
    p.add_argument('--max', type=int, default=None, help='Max rows to import (for testing)')
    args = p.parse_args(argv)

    if args.max:
        print('Running in test mode: max rows =', args.max)

    if ijson is None:
        print('Warning: ijson not installed. For large files install ijson (`pip install ijson`)')

    return import_stream(args.input, args.db, batch_size=args.batch, create_indexes=args.create_indexes, max_rows=args.max)


if __name__ == '__main__':
    raise SystemExit(main())
