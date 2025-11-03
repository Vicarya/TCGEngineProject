using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json; // Added for JSON deserialization
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
        d.WorkId = rec.work_id; // Renamed from Set
        d.Rarity = rec.rarity;
        d.ImagePath = rec.image_url; // Assuming image_url is the path to be used in Unity
        d.Illustration = rec.detail_page_url; // Assuming detail_page_url is for illustration

        // Map WeissCardData specific fields
        d.Level = rec.level ?? 0;
        d.Cost = rec.cost ?? 0;
        d.Power = rec.power ?? 0;
        d.Soul = 0; // Default to 0, as no direct DB column. Could parse from metadata_json if needed.
        d.Side = rec.side;
        d.Color = rec.color;
        d.CardType = rec.type;
        d.TriggerIcon = rec.trigger;
        d.FlavorText = rec.flavor_text; // Direct mapping

        // Deserialize JSON fields for Abilities and Traits
        if (!string.IsNullOrEmpty(rec.abilities_json))
        {
            d.Abilities = JsonConvert.DeserializeObject<List<string>>(rec.abilities_json);
        }
        else
        {
            d.Abilities = new List<string>();
        }

        if (!string.IsNullOrEmpty(rec.traits_json))
        {
            d.Traits = JsonConvert.DeserializeObject<List<string>>(rec.traits_json);
        }
        else
        {
            d.Traits = new List<string>();
        }

        // Metadata: store raw JSON under key "raw"
        d.Metadata = d.Metadata ?? new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(rec.metadata_json))
        {
            d.Metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(rec.metadata_json);
        }
        else
        {
            d.Metadata = new Dictionary<string, object>();
        }

        EditorUtility.SetDirty(asset);
    }
}
