using System;
using System.Collections.Generic;
using System.Linq;
using DuckovCoreAPI;
using SodaCraft.Localizations;
using UnityEngine;

namespace DisplayItemSourceMod
{
    internal static class SourceNativeText
    {
        private const int MaxCompactValues = 3;
        // Compact labels are kept here so the existing V12 external wording
        // remains stable while Drops gain the same ten-language fallback.
        private static readonly string[][] Labels =
        {
            new[] { "掉落", "商店", "地面", "物資箱", "敵人", "釣魚", "名稱未知", "地圖", "來源" },
            new[] { "掉落", "商店", "地面", "物资箱", "敌人", "钓鱼", "名称未知", "地图", "来源" },
            new[] { "Drops", "shops", "ground", "container", "enemy", "fishing", "unnamed", "Maps", "Sources" },
            new[] { "ドロップ", "商店", "地面", "コンテナ", "敵", "釣り", "名称不明", "マップ", "入手元" },
            new[] { "전리품", "상점", "지면", "상자", "적", "낚시", "이름 없음", "지도", "출처" },
            new[] { "Butin", "boutiques", "sol", "conteneur", "ennemi", "pêche", "nom inconnu", "Cartes", "Sources" },
            new[] { "Beute", "Läden", "Boden", "Behälter", "Gegner", "Angeln", "ohne Namen", "Karten", "Quellen" },
            new[] { "Saque", "lojas", "chão", "contêiner", "inimigo", "pesca", "sem nome", "Mapas", "Fontes" },
            new[] { "Добыча", "магазины", "земля", "контейнер", "враг", "рыбалка", "имя неизвестно", "Карты", "Источники" },
            new[] { "Botín", "tiendas", "suelo", "contenedor", "enemigo", "pesca", "sin nombre", "Mapas", "Fuentes" }
        };

        public static string Label(int index, SystemLanguage language)
        {
            int languageIndex = language switch
            {
                SystemLanguage.ChineseTraditional => 0, SystemLanguage.ChineseSimplified => 1,
                SystemLanguage.Japanese => 3, SystemLanguage.Korean => 4, SystemLanguage.French => 5,
                SystemLanguage.German => 6, SystemLanguage.Portuguese => 7, SystemLanguage.Russian => 8,
                SystemLanguage.Spanish => 9, _ => 2
            };
            return Labels[languageIndex][index];
        }

        public static string Category(NativeLootCategory category, SystemLanguage language) => category switch
        {
            NativeLootCategory.Ground => Label(2, language), NativeLootCategory.Container => Label(3, language),
            NativeLootCategory.Enemy => Label(4, language), NativeLootCategory.Fishing => Label(5, language),
            _ => Label(6, language)
        };

        public static string Resolve(string key, string fallback)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    string normalizedKey = key.Trim();
                    string value = LocalizationManager.GetPlainText(normalizedKey);
                    if (!string.IsNullOrWhiteSpace(value) && value != normalizedKey && value != "*" + normalizedKey + "*") return SourceV3Presentation.Safe(value);
                }
            }
            catch { }
            return SourceV3Presentation.Safe(fallback);
        }

        private static string ResolveMap(string key, string fallback)
        {
            string localized = Resolve(key, "");
            if (localized.Length > 0) return localized;

            // Core 3.15.2 supplies MapFallbackName only as a player-facing
            // fallback. Refuse scene stems, paths, IDs and other raw tokens
            // when an actual map localization is unavailable.
            string candidate = SourceV3Presentation.Safe(fallback).Trim();
            if (candidate.Length == 0 || string.Equals(candidate, key?.Trim(), StringComparison.Ordinal)) return "";
            if (candidate.IndexOfAny(new[] { '/', '\\', ':', '.', '_' }) >= 0) return "";
            if (candidate.IndexOf("scene", StringComparison.OrdinalIgnoreCase) >= 0) return "";
            if (!candidate.Any(char.IsUpper) && candidate.All(c => c <= 127)) return "";
            return candidate;
        }

        private static string JoinBounded(IEnumerable<string> values)
        {
            var unique = new List<string>();
            bool omitted = false;
            foreach (string value in values)
            {
                if (unique.Contains(value)) continue;
                if (unique.Count >= MaxCompactValues) { omitted = true; continue; }
                unique.Add(value);
            }
            if (omitted) unique.Add("…");
            return string.Join(", ", unique);
        }

        public static string Compact(NativeLootItemResult? loot, MerchantClueResult? merchants, SystemLanguage language)
        {
            var parts = new List<string>();
            if (loot?.Status == NativeLootStatus.Success)
            {
                var maps = new List<string>();
                var categories = new List<string>();
                foreach (var clue in loot.Clues)
                {
                    string map = ResolveMap(clue.MapNameKey, clue.MapFallbackName);
                    if (map.Length > 0 && !maps.Contains(map)) maps.Add(map);
                    string category = Category(clue.Category, language);
                    if (!categories.Contains(category)) categories.Add(category);
                }
                if (maps.Count > 0) parts.Add(Label(7, language) + ": " + JoinBounded(maps));
                if (categories.Count > 0) parts.Add(Label(8, language) + ": " + JoinBounded(categories));
                if (loot.OmittedClueCount > 0) parts.Add(SourceExternalText.For(language)[9]);
            }
            if (merchants?.Status == NativeLootStatus.Success)
            {
                var names = new List<string>();
                foreach (var merchant in merchants.Merchants)
                {
                    string value = Resolve(merchant.NameKey, "");
                    if (value.Length > 0 && !names.Any(name => StringComparer.Ordinal.Equals(name, value))) names.Add(value);
                }
                if (names.Count > 0) parts.Add(Label(1, language) + ": " + JoinBounded(names));
            }
            return string.Join(" · ", parts);
        }
    }
}
