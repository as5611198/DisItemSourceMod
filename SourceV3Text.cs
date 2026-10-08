using System.Collections.Generic;
using UnityEngine;

namespace DisplayItemSourceMod
{
    internal static class SourceV3Text
    {
        // Columns: Traditional, Simplified, English, Japanese, Korean, French, German, Portuguese, Russian, Spanish.
        private static readonly Dictionary<string, string[]> Text = new Dictionary<string, string[]>
        {
            ["source_font_size"] = new[] { "來源提示字級", "来源提示字号", "Source hint font size", "提供元ヒントの文字サイズ", "출처 힌트 글자 크기", "Taille du texte des indications", "Schriftgröße der Herkunftshinweise", "Tamanho da fonte das indicações", "Размер шрифта подсказок", "Tamaño de letra de las indicaciones" },
            ["source_detailed"] = new[] { "詳細來源提示", "详细来源提示", "Detailed source hints", "提供元の詳細を表示", "자세한 출처 표시", "Afficher les détails de provenance", "Detaillierte Herkunft anzeigen", "Mostrar origem detalhada", "Подробные сведения об источнике", "Mostrar origen detallado" },
            ["source_mod_items_only"] = new[] { "來源列只顯示模組物品", "来源栏只显示模组物品", "Source row for mod items only", "MODアイテムのみ提供元を表示", "모드 아이템의 출처만 표시", "Origine uniquement pour les objets de mods", "Herkunft nur für Mod-Gegenstände", "Origem apenas para itens de mods", "Источник только для предметов модов", "Origen solo para objetos de mods" },
            ["favorite"] = new[] { "已收藏", "已收藏", "Favorite", "お気に入り", "즐겨찾기", "Favori", "Favorit", "Favorito", "Избранное", "Favorito" },
            ["pinned"] = new[] { "已釘選產物", "已固定产物", "Pinned output", "固定した生産物", "고정된 생산품", "Produit épinglé", "Angeheftetes Produkt", "Produto fixado", "Закреплённый продукт", "Producto fijado" },
            ["material"] = new[] { "追蹤材料", "追踪材料", "Tracked material", "追跡中の素材", "추적 재료", "Matériau suivi", "Verfolgtes Material", "Material acompanhado", "Отслеживаемый материал", "Material seguido" },
            ["carried"] = new[] { "隨身：角色＋寵物", "随身：角色＋宠物", "Carried: player + pet", "携帯：プレイヤー＋ペット", "소지: 플레이어 + 반려동물", "Transporté : joueur + animal", "Mitgeführt: Spieler + Begleiter", "Carregado: jogador + animal", "При себе: игрок + питомец", "Encima: jugador + mascota" },
            ["required"] = new[] { "需求", "需求", "required", "必要", "필요", "requis", "benötigt", "necessário", "требуется", "necesario" },
            ["available"] = new[] { "已有", "已有", "available", "所持", "보유", "disponible", "vorhanden", "disponível", "имеется", "disponible" },
            ["missing"] = new[] { "缺少", "缺少", "missing", "不足", "부족", "manquant", "fehlend", "em falta", "не хватает", "falta" },
            ["unknown"] = new[] { "未知", "未知", "unknown", "不明", "알 수 없음", "inconnu", "unbekannt", "desconhecido", "неизвестно", "desconocido" },
            ["unknownInventory"] = new[] { "持有量／缺額未知", "持有量／缺额未知", "Unknown holdings / deficit", "所持数／不足数は不明", "보유량 / 부족량 알 수 없음", "Quantité disponible / déficit inconnus", "Bestand / Fehlmenge unbekannt", "Quantidade / falta desconhecidas", "Запас / нехватка неизвестны", "Existencias / déficit desconocidos" },
            ["money"] = new[] { "金錢（合併計畫）", "金钱（合并计划）", "Money (merged plan)", "資金（統合計画）", "자금 (통합 계획)", "Argent (plan combiné)", "Geld (Gesamtplan)", "Dinheiro (plano combinado)", "Деньги (общий план)", "Dinero (plan combinado)" },
            ["incomplete"] = new[] { "計畫不完整：失效釘選", "计划不完整：失效固定项", "Incomplete plan: unavailable pins", "計画は不完全：無効な固定項目", "불완전한 계획: 사용할 수 없는 고정 항목", "Plan incomplet : épingles indisponibles", "Unvollständiger Plan: ungültige Einträge", "Plano incompleto: itens fixados indisponíveis", "Неполный план: недоступные закрепления", "Plan incompleto: elementos fijados no disponibles" },
            ["planUnavailable"] = new[] { "材料計畫不可用（未知或計算失敗）", "材料计划不可用（未知或计算失败）", "Material plan unavailable (unknown or calculation failed)", "素材計画を利用できません（不明または計算失敗）", "재료 계획 사용 불가 (알 수 없음 또는 계산 실패)", "Plan de matériaux indisponible (inconnu ou échec du calcul)", "Materialplan nicht verfügbar (unbekannt oder Berechnungsfehler)", "Plano de materiais indisponível (desconhecido ou falha no cálculo)", "План материалов недоступен (неизвестно или ошибка расчёта)", "Plan de materiales no disponible (desconocido o error de cálculo)" },
            ["all"] = new[] { "綜合查詢", "综合查询", "All", "総合検索", "전체 조회", "Tout", "Gesamtsuche", "Tudo", "Общий поиск", "Todo" },
            ["recipes"] = new[] { "製作", "制作", "Recipes", "製作", "제작", "Recettes", "Herstellung", "Receitas", "Рецепты", "Recetas" },
            ["usages"] = new[] { "用途", "用途", "Usages", "用途", "용도", "Utilisations", "Verwendung", "Usos", "Применение", "Usos" },
            ["owner"] = new[] { "來源 ID", "来源 ID", "Owner ID", "提供元ID", "출처 ID", "ID de provenance", "Herkunfts-ID", "ID de origem", "ID источника", "ID de origen" },
            ["identifier"] = new[] { "資源識別", "资源标识", "Identifier", "リソース識別子", "리소스 식별자", "Identifiant", "Ressourcenkennung", "Identificador", "Идентификатор", "Identificador" }
        };
        public static string Label(string key, SystemLanguage language)
        {
            int index = language switch
            {
                SystemLanguage.ChineseTraditional => 0, SystemLanguage.ChineseSimplified => 1,
                SystemLanguage.Japanese => 3, SystemLanguage.Korean => 4, SystemLanguage.French => 5,
                SystemLanguage.German => 6, SystemLanguage.Portuguese => 7, SystemLanguage.Russian => 8,
                SystemLanguage.Spanish => 9, _ => 2
            };
            return Text.TryGetValue(key, out var values) ? values[index] : "";
        }
    }
}
