using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json; // Assuming Newtonsoft.Json is available or will be added.
using TCG.Weiss; // For WeissCardDataAsset and WeissCardData
using TCG.Core; // For CardData

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

    // Adapts CardRecord into WeissCardDataAsset
    public static WeissCardDataAsset CreateWeissCardDataAsset(CardRecord rec)
    {
        var asset = ScriptableObject.CreateInstance<WeissCardDataAsset>();
        var data = asset.Data;

        // Map CardData fields
        data.CardCode = rec.card_no;
        data.Name = rec.name;
        data.WorkId = rec.work_id; // Corrected from data.Set
        data.Rarity = rec.rarity;
        data.ImagePath = rec.image_url; // Assuming image_url is the path to be used in Unity

        // Map WeissCardData specific fields
        data.Level = rec.level ?? 0;
        data.Cost = rec.cost ?? 0;
        data.Power = rec.power ?? 0;
        data.Soul = 0; // Default to 0, as no direct DB column. Could parse from metadata_json if needed.
        data.Side = rec.side;
        data.Color = rec.color;
        data.CardType = rec.type;
        data.TriggerIcon = rec.trigger;
        data.FlavorText = rec.flavor_text;

        // Deserialize JSON fields
        if (!string.IsNullOrEmpty(rec.abilities_json))
        {
            data.Abilities = JsonConvert.DeserializeObject<List<string>>(rec.abilities_json);
        }
        else
        {
            data.Abilities = new List<string>();
        }

        if (!string.IsNullOrEmpty(rec.traits_json))
        {
            data.Traits = JsonConvert.DeserializeObject<List<string>>(rec.traits_json);
        }
        else
        {
            data.Traits = new List<string>();
        }

        if (!string.IsNullOrEmpty(rec.metadata_json))
        {
            // Deserialize into a Dictionary<string, object> for flexibility
            data.Metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(rec.metadata_json);
        }
        else
        {
            data.Metadata = new Dictionary<string, object>();
        }

        // Save the ScriptableObject asset
        // This part needs to be called from an Editor context.
        // Example: AssetDatabase.CreateAsset(asset, $"Assets/CardData/Weiss/{rec.card_no}.asset");
        // Debug.Log($"Created asset for {rec.card_no}");

        return asset;
    }

    // Editor convenience: menu item to load and log first N cards
    [MenuItem("Tools/Import/Load cards from SQLite (Log 10)")]
    public static void EditorLoadAndLog()
    {
        try
        {
            var db = "python/tools/ws_cards.db"; // Use the actual DB path
            var cards = LoadAll(db);
            Debug.Log($"Loaded {cards.Count} cards from DB");
            for (int i = 0; i < Math.Min(cards.Count, 10); i++)
            {
                var c = cards[i];
                // Use the new mapping function
                var asset = CreateWeissCardDataAsset(c);
                Debug.Log($"{i+1}: {asset.Data.CardCode} - {asset.Data.Name} - Side={asset.Data.Side} Color={asset.Data.Color} Type={asset.Data.CardType}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("SQLite import failed: " + ex.Message + "\n" + ex.StackTrace);
        }
    }
}
