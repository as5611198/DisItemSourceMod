using UnityEngine;

namespace DisplayItemSourceMod
{
    internal static class SourceAcquisitionText
    {
        // Acquisition scope, usage scope, decomposition, shops, item rewards,
        // item turn-ins, present, absent, waiting, partial, unavailable, unsupported, pending data.
        private static readonly string[][] texts =
        {
            new[] { "取得定義（不含掉落/Feather）", "用途定義", "拆解", "商店", "物品獎勵", "物品繳交", "有", "無", "等待", "部分", "不可用", "未涵蓋", "資料等待中" },
            new[] { "获取定义（不含掉落/Feather）", "用途定义", "分解", "商店", "物品奖励", "物品提交", "有", "无", "等待", "部分", "不可用", "未覆盖", "数据等待中" },
            new[] { "Acquisition defs (no loot/Feather)", "Usage defs", "decomposition", "shops", "item rewards", "item turn-ins", "present", "absent", "waiting", "partial", "unavailable", "not covered", "waiting for data" },
            new[] { "入手定義（ドロップ/Feather対象外）", "用途定義", "分解", "店舗", "アイテム報酬", "アイテム納品", "あり", "なし", "待機", "一部", "利用不可", "対象外", "データ待機中" },
            new[] { "획득 정의 (전리품/Feather 제외)", "용도 정의", "분해", "상점", "아이템 보상", "아이템 제출", "있음", "없음", "대기", "일부", "사용 불가", "미지원", "데이터 대기 중" },
            new[] { "Déf. d’obtention (hors butin/Feather)", "Déf. d’usage", "décomposition", "boutiques", "récompenses objet", "remises objet", "présente", "absente", "en attente", "partiel", "indisponible", "non couvert", "données en attente" },
            new[] { "Bezugsdefinitionen (ohne Beute/Feather)", "Nutzungsdefinitionen", "Zerlegung", "Läden", "Item-Belohnungen", "Item-Abgaben", "vorhanden", "fehlend", "wartend", "teilweise", "nicht verfügbar", "nicht abgedeckt", "warte auf Daten" },
            new[] { "Def. de obtenção (sem saque/Feather)", "Def. de uso", "decomposição", "lojas", "recompensas de item", "entregas de item", "presente", "ausente", "aguardando", "parcial", "indisponível", "não coberto", "aguardando dados" },
            new[] { "Определения получения (без добычи/Feather)", "Определения использования", "разборка", "магазины", "награды-предметы", "сдача предметов", "есть", "нет", "ожидание", "частично", "недоступно", "не охвачено", "ожидание данных" },
            new[] { "Def. de obtención (sin botín/Feather)", "Def. de uso", "descomposición", "tiendas", "recompensas de objeto", "entregas de objeto", "presente", "ausente", "en espera", "parcial", "no disponible", "no cubierto", "datos en espera" }
        };

        public static string[] For(SystemLanguage language) => texts[language switch
        {
            SystemLanguage.ChineseTraditional => 0, SystemLanguage.ChineseSimplified => 1,
            SystemLanguage.Japanese => 3, SystemLanguage.Korean => 4, SystemLanguage.French => 5,
            SystemLanguage.German => 6, SystemLanguage.Portuguese => 7,
            SystemLanguage.Russian => 8, SystemLanguage.Spanish => 9, _ => 2
        }];
    }
}
