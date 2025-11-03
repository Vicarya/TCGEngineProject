using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using UnityEditor;
using UnityEngine;

// Simple SQLite importer for Unity
// - Uses Mono.Data.Sqlite which is commonly available in Unity's runtime on Windows.
// - Loads rows from the `cards` table and maps to a lightweight CardRecord class.
// - Adapt `ApplyToYourCardClass` to map fields into your project's existing card classes or ScriptableObjects.

[Serializable]
public class CardRecord
{
    public string card_no;
    public string name;
    public string work_id;
    public string detail_page_url;
    public string image_url;
    public string side;
    public string color;
    public string type;
    public int? level;
    public int? power;
    public int? cost;
    public string rarity;
    public string trigger;
    public string flavor_text;
    public string abilities_json;
    public string traits_json;
    public string metadata_json;
    public string visual_local_path;
    public int visual_fetch_status;
    public string created_at;
    public string updated_at;
}

public static class SQLiteCardImporter
{
    // Load all cards from a SQLite DB file (path can be absolute or relative). Returns list of CardRecord.
    public static List<CardRecord> LoadAll(string dbPath)
    {
        var abs = MakeAbsolutePath(dbPath);
        if (!File.Exists(abs))
            throw new FileNotFoundException("SQLite DB not found:", abs);

        var list = new List<CardRecord>();
        var connString = $"URI=file:{abs}";
        using (var conn = new SqliteConnection(connString))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM cards ORDER BY id";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var r = new CardRecord();
                        r.card_no = SafeGetString(reader, "card_no");
                        r.name = SafeGetString(reader, "name");
                        r.work_id = SafeGetString(reader, "work_id");
                        r.detail_page_url = SafeGetString(reader, "detail_page_url");
                        r.image_url = SafeGetString(reader, "image_url");
                        r.side = SafeGetString(reader, "side");
                        r.color = SafeGetString(reader, "color");
                        r.type = SafeGetString(reader, "type");
                        r.level = SafeGetNullableInt(reader, "level");
                        r.power = SafeGetNullableInt(reader, "power");
                        r.cost = SafeGetNullableInt(reader, "cost");
                        r.rarity = SafeGetString(reader, "rarity");
                        r.trigger = SafeGetString(reader, "trigger");
                        r.flavor_text = SafeGetString(reader, "flavor_text");
                        r.abilities_json = SafeGetString(reader, "abilities_json");
                        r.traits_json = SafeGetString(reader, "traits_json");
                        r.metadata_json = SafeGetString(reader, "metadata");
                        r.visual_local_path = SafeGetString(reader, "visual_local_path");
                        r.visual_fetch_status = SafeGetInt(reader, "visual_fetch_status");
                        r.created_at = SafeGetString(reader, "created_at");
                        r.updated_at = SafeGetString(reader, "updated_at");
                        list.Add(r);
                    }
                }
            }
            conn.Close();
        }

        return list;
    }

    static string MakeAbsolutePath(string dbPath)
    {
        if (Path.IsPathRooted(dbPath)) return dbPath;
        // Common: project-relative paths like "python/tools/ws_cards_test.db"
        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return Path.GetFullPath(Path.Combine(projectRoot, dbPath));
    }

    static string SafeGetString(IDataReader r, string col)
    {
        try
        {
            var idx = r.GetOrdinal(col);
            if (r.IsDBNull(idx)) return null;
            return r.GetString(idx);
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }

    static int SafeGetInt(IDataReader r, string col)
    {
        var v = SafeGetString(r, col);
        if (string.IsNullOrEmpty(v)) return 0;
        if (int.TryParse(v, out var x)) return x;
        return 0;
    }

    static int? SafeGetNullableInt(IDataReader r, string col)
    {
        try
        {
            var idx = r.GetOrdinal(col);
            if (r.IsDBNull(idx)) return null;
            // Some DBs return ints directly
            var val = r.GetValue(idx);
            if (val is int) return (int)val;
            if (val is long) return (int)(long)val;
            var s = val.ToString();
            if (int.TryParse(s, out var x)) return x;
        }
        catch (IndexOutOfRangeException)
        {
        }
        return null;
    }

    // Example adapter: customize this to map CardRecord into your project's card class or ScriptableObject.
    // Replace `YourCardClass` and the mapping below with your actual class and properties.
    public static void ApplyToYourCardClass(CardRecord rec)
    {
        // Example (pseudo):
        // var card = new YourCardClass();
        // card.CardNo = rec.card_no;
        // card.Name = rec.name;
        // card.ImageUrl = rec.image_url;
        // card.Side = rec.side;
        // ... then add to a manager or create an asset.
        Debug.Log($"Map card {rec.card_no} -> YourCardClass (implement mapping)");
    }

    // Editor convenience: menu item to load and log first N cards
    [MenuItem("Tools/Import/Load cards from SQLite (Log 10)")]
    public static void EditorLoadAndLog()
    {
        try
        {
            var db = "python/tools/ws_cards_test.db"; // change if needed
            var cards = LoadAll(db);
            Debug.Log($"Loaded {cards.Count} cards from DB");
            for (int i = 0; i < Math.Min(cards.Count, 10); i++)
            {
                var c = cards[i];
                Debug.Log($"{i+1}: {c.card_no} - {c.name} - side={c.side} image={c.image_url}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("SQLite import failed: " + ex.Message + "\n" + ex.StackTrace);
        }
    }
}
