#!/usr/bin/env python3
import sqlite3
import json
import sys

db='python/tools/ws_cards_test.db'
conn=sqlite3.connect(db)
cur=conn.cursor()
try:
    cur.execute("SELECT count(*) FROM cards")
    total=cur.fetchone()[0]
    print('TOTAL_ROWS:', total)
    cur.execute("PRAGMA index_list('cards')")
    idxs=cur.fetchall()
    print('\nINDEX_LIST:')
    for i in idxs:
        print(' ', i)
    print('\nSAMPLE_ROWS:')
    cur.execute("SELECT card_no,name,work_id,side,color,image_url FROM cards ORDER BY id LIMIT 5")
    rows=cur.fetchall()
    for r in rows:
        print(json.dumps({'card_no':r[0],'name':r[1],'work_id':r[2],'side':r[3],'color':r[4],'image_url':r[5]}, ensure_ascii=False))
finally:
    conn.close()
