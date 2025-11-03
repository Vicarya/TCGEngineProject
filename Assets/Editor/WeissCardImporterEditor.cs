using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TCG.Weiss;

// Editor utility to import cards from the SQLite DB created by the tools and create ScriptableObject assets.
public static class WeissCardImporterEditor
{
    [MenuItem("Tools/Import/Weiss: Import cards to ScriptableObjects")] 
    public static void ImportToScriptableObjects()
    {
        try
        {
            string dbRelative = "python/tools/ws_cards_test.db";
            string outputFolder = "Assets/CardData/Weiss";
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            var records = SQLiteCardImporter.LoadAll(dbRelative);
            int total = records.Count;
            for (int i = 0; i < records.Count; i++)
            {
                var rec = records[i];
                EditorUtility.DisplayProgressBar("Weiss Import", $"Importing {i+1}/{total}: {rec.card_no}", (float)i/total);
                CreateOrUpdateAsset(rec, outputFolder);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            Debug.Log($"Imported {records.Count} Weiss cards to {outputFolder}");
        }
        catch (Exception ex)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError("Import failed: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    static void CreateOrUpdateAsset(CardRecord rec, string folder)
    {
        // asset name safe
        var safeName = rec.card_no.Replace('/', '_').Replace(' ', '_');
        var path = Path.Combine(folder, safeName + ".asset");
        WeissCardDataAsset asset = AssetDatabase.LoadAssetAtPath<WeissCardDataAsset>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<WeissCardDataAsset>();
            AssetDatabase.CreateAsset(asset, path);
        }

        // map fields
        var d = asset.Data;
        d.CardCode = rec.card_no;
        d.Name = rec.name;
        d.Set = rec.work_id;
        d.Rarity = rec.rarity;
        d.Atrribute = rec.color;
        d.ImagePath = rec.image_url;
        d.Illustration = rec.detail_page_url;
        // Description: traits + flavor
        var traits = new List<string>();
        try {
            if (!string.IsNullOrEmpty(rec.traits_json))
            {
                var parsed = JsonUtility.FromJson<SimpleStringArray>("{\"items\":" + rec.traits_json + "}");
                if (parsed != null && parsed.items != null) traits.AddRange(parsed.items);
            }
        } catch {}
        if (!string.IsNullOrEmpty(rec.flavor_text)) traits.Add(rec.flavor_text);
        d.Description = traits.ToArray();

        // Weiss-specific
        d.Level = rec.level ?? 0;
        d.Cost = rec.cost ?? 0;
        d.Power = rec.power ?? 0;
        // Soul isn't present in DB; leave default
        d.Soul = 0;
        d.CardType = rec.type;
        d.TriggerIcon = rec.trigger;

        // Metadata: store raw JSON under key "raw"
        d.Metadata = d.Metadata ?? new System.Collections.Generic.Dictionary<string, object>();
        if (!string.IsNullOrEmpty(rec.metadata_json)) d.Metadata["raw"] = rec.metadata_json;

        EditorUtility.SetDirty(asset);
    }

    // Helper type for JsonUtility to parse simple string[] payloads
    [Serializable]
    private class SimpleStringArray { public string[] items; }
}
