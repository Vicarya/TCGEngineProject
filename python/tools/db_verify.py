#!/usr/bin/env python3
import sqlite3
import json
import sys
import re

db_path = 'python/tools/ws_cards.db'
conn = sqlite3.connect(db_path)
cur = conn.cursor()

try:
    cur.execute("SELECT abilities_json FROM cards")
    rows = cur.fetchall()
    
    unique_costs = set()
    
    for row in rows:
        abilities = json.loads(row[0])
        for ability_text in abilities:
            if ability_text.startswith('【起】'):
                # Extract the cost part, which is between '】' and the description starting with '「' or the end of the string
                match = re.search(r'】(.*?)(「|$)', ability_text)
                if match:
                    cost_string = match.group(1).strip()
                    if cost_string:
                        unique_costs.add(cost_string)

    print("--- Unique Activate Ability Costs ---")
    for cost in sorted(list(unique_costs)):
        print(cost)
    print("------------------------------------")

finally:
    conn.close()
