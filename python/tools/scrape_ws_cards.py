import json
import time
import os
import re
from selenium import webdriver
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import TimeoutException
from bs4 import BeautifulSoup
from urllib.parse import urljoin

# --- 定数 ---
SEARCH_PAGE_URL = 'https://ws-tcg.com/cardlist/search'
BASE_URL = 'https://ws-tcg.com/'

# --- セレクタ ---
CARD_TABLE_BODY_SELECTOR = 'table.search-result-table > tbody'

def parse_card_row(row_soup):
    """
    検索結果テーブルの単一の行(<tr>)からカード情報を抽出する
    """
    card_data = {}

    th_tag = row_soup.find('th')
    if th_tag and th_tag.find('a'):
        card_data['detail_page_url'] = urljoin(BASE_URL, th_tag.find('a')['href'])
        img_tag = th_tag.find('img')
        if img_tag:
            card_data['image_url'] = urljoin(BASE_URL, img_tag['src'])

    td_tag = row_soup.find('td')
    if not td_tag:
        return None

    h4_tag = td_tag.find('h4')
    if h4_tag and h4_tag.find('a'):
        anchor = h4_tag.find('a')
        # Prefer structured spans (サイトの構造に依存):
        # <a>...<span class="highlight_target">名前</span>(<span class="highlight_target">DC/W01-016</span>)</a>
        spans = anchor.find_all('span', class_='highlight_target')
        if len(spans) >= 2:
            # first highlight is name, second is card_no
            card_data['name'] = spans[0].text.strip()
            card_data['card_no'] = spans[1].text.strip()
        else:
            # fallback to plain-text parsing
            full_title = anchor.text.strip()
            match = re.search(r'(.+)\(([^)]+)\)', full_title)
            if match:
                card_data['name'] = match.group(1).strip()
                card_data['card_no'] = match.group(2).strip()
            else:
                card_data['name'] = full_title

    # helpers for mapping file names to JP names
    COLOR_MAP_JP = {
        'red': '赤', 'blue': '青', 'yellow': '黄', 'green': '緑',
        'purple': '紫', 'white': '白', 'black': '黒'
    }
    SIDE_FILE_MAP = {'w': 'ヴァイス', 's': 'シュヴァルツ'}

    unit_spans = td_tag.find_all('span', class_='unit')
    for span in unit_spans:
        # If the span contains an <img>, prefer extracting the image src and mapping from filename
        img = span.find('img')
        text = span.text.strip()
        parts = text.split('：', 1)
        key = parts[0] if parts else ''

        if img:
            img_src = img.get('src')
            img_url = urljoin(BASE_URL, img_src)
            # determine basename without extension
            fname = os.path.basename(img_src).lower()
            name_no_ext = os.path.splitext(fname)[0]

            if key == 'サイド':
                card_data['サイド_img'] = img_url
                # try mapping single-letter filenames like 'w'->ヴァイス
                mapped = None
                if name_no_ext in SIDE_FILE_MAP:
                    mapped = SIDE_FILE_MAP[name_no_ext]
                # sometimes filename could be 'wa' or other; fallback to letter
                if not mapped and len(name_no_ext) == 1 and name_no_ext in SIDE_FILE_MAP:
                    mapped = SIDE_FILE_MAP[name_no_ext]
                if mapped:
                    card_data['サイド'] = mapped
                else:
                    # still write a candidate
                    card_data.setdefault('サイド_img_candidates', []).append(img_url)

            elif key == '色':
                card_data['色_img'] = img_url
                mapped = COLOR_MAP_JP.get(name_no_ext)
                if mapped:
                    card_data['色'] = mapped
                else:
                    card_data.setdefault('色_img_candidates', []).append(img_url)

            elif key == 'ソウル':
                card_data['ソウル_img'] = img_url
            elif key == 'トリガー':
                card_data['トリガー_img'] = img_url
            else:
                # fallback: store the raw pair if text part exists
                if len(parts) == 2 and parts[1].strip():
                    card_data[key] = parts[1].strip()
                else:
                    card_data[key] = img_url
        else:
            # no image; use text parsing as before
            if len(parts) == 2:
                if key == '特徴':
                    traits = [t.text.strip() for t in span.find_all('span')]
                    card_data[key] = [t for t in traits if t]
                else:
                    card_data[key] = parts[1].strip()

    flavor_span = next((s for s in unit_spans if s.text.strip().startswith('フレーバー')), None)
    if flavor_span:
        card_data['flavor_text'] = flavor_span.text.strip().replace('フレーバー：', '', 1)

    for br in td_tag.find_all('br'):
        br.replace_with('\n')
    
    full_text = td_tag.text.strip()
    
    # FINAL BUG FIX: Corrected the regex for ability text extraction.
    ability_pattern = r'(【自】|【起】|【永】|【他】.+?)(?=\n【|\Z)'
    ability_texts = re.findall(ability_pattern, full_text, re.DOTALL)
    card_data['abilities'] = [ability.strip() for ability in ability_texts]

    return card_data

def main():
    """
    カードデータをスクレイピングするメイン処理
    """
    all_cards_data = []
    driver = None
    script_dir = os.path.dirname(os.path.abspath(__file__))
    output_filename = os.path.join(script_dir, 'weiss_schwarz_cards.json')

    try:
        chromedriver_path = os.path.join(script_dir, 'chromedriver.exe')
        if not os.path.exists(chromedriver_path):
            print(f"エラー: chromedriver.exe が見つかりません: {chromedriver_path}")
            return

        service = Service(executable_path=chromedriver_path)
        options = webdriver.ChromeOptions()
        driver = webdriver.Chrome(service=service, options=options)
        wait = WebDriverWait(driver, 15)

        driver.get(SEARCH_PAGE_URL)
        print("\n" + "="*60)
        print("ユーザー操作が必要です:")
        print("1. ブラウザで検索を実行してください（検索条件や絞り込みを設定）。")
        input("2. 検索結果が表示されたら、このコンソールでEnterキーを押してください...")
        print("="*60 + "\n")

        print("検索結果のページネーションを辿ってカードデータを抽出します...")
        try:
            wait.until(EC.presence_of_element_located((By.CSS_SELECTOR, CARD_TABLE_BODY_SELECTOR)))

            # BFS-like traversal of pagination links starting from current page
            visited = set()
            to_visit = [driver.current_url]

            while to_visit:
                cur = to_visit.pop(0)
                if cur in visited:
                    continue
                driver.get(cur)
                try:
                    wait.until(EC.presence_of_element_located((By.CSS_SELECTOR, CARD_TABLE_BODY_SELECTOR)))
                except TimeoutException:
                    print(f"タイムアウト: テーブルが見つからないページをスキップします: {cur}")
                    visited.add(cur)
                    continue

                page_source = driver.page_source
                soup = BeautifulSoup(page_source, 'html.parser')
                table_body = soup.select_one(CARD_TABLE_BODY_SELECTOR)

                if not table_body:
                    print(f"カード情報テーブルが見つかりませんでした（{cur}）。")
                    visited.add(cur)
                    continue

                rows = table_body.find_all('tr')
                print(f"{cur} -> {len(rows)} 件のカードを検出。抽出中...")
                for row in rows:
                    card_data = parse_card_row(row)
                    if card_data:
                        all_cards_data.append(card_data)

                # collect pagination-like links from this page
                for a in soup.find_all('a', href=True):
                    href = a['href']
                    # skip detail links that contain cardno=
                    if 'cardno=' in href:
                        continue
                    if href.startswith('#'):
                        continue
                    # heuristic: pagination URLs often contain page=, p=, /page/, pg=, paged=
                    if re.search(r'(page=|p=|/page/|pg=|paged=)', href) or a.get('rel') == ['next'] or 'search' in href:
                        full = urljoin(BASE_URL, href)
                        if full not in visited and full not in to_visit:
                            to_visit.append(full)

                visited.add(cur)
                # be polite
                time.sleep(1)

            print(f"合計 {len(all_cards_data)} 件のカードデータを抽出しました。")

        except TimeoutException:
            print(f"タイムアウトエラー: カード情報テーブル({CARD_TABLE_BODY_SELECTOR})が見つかりませんでした。")
            debug_filename = os.path.join(script_dir, 'debug_page_source.html')
            print(f"デバッグ用にHTMLを {debug_filename} に保存します。")
            if driver:
                with open(debug_filename, 'w', encoding='utf-8') as f:
                    f.write(driver.page_source)

    except Exception as e:
        print(f"予期せぬエラーが発生しました: {e}")
    finally:
        if driver:
            driver.quit()
            print("\nブラウザを終了しました。")

        if all_cards_data:
            print(f"データを {output_filename} に保存します。")
            with open(output_filename, 'w', encoding='utf-8') as f:
                json.dump(all_cards_data, f, indent=2, ensure_ascii=False)
            print("保存が完了しました。")
        else:
            print("\nカードデータは抽出されませんでした。")

if __name__ == '__main__':
    main()