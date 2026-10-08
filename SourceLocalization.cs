using UnityEngine;

namespace DisplayItemSourceMod
{
    internal static class SourceLocalization
    {
        // All ten game languages. Owner names are data and never translated here.
        // Unrecognized languages retain the English fallback.
        public static string Format(SourceHintKind kind, string owner, SystemLanguage language)
        {
            var texts = kind switch
            {
                SourceHintKind.Scanning => ("來源：等待資料…", "来源：等待数据…", "Source: waiting…"),
                SourceHintKind.ScanTimedOut => ("來源：等待資料…", "来源：等待数据…", "Source: waiting…"),
                SourceHintKind.Native => ("來源：遊戲本體", "来源：游戏本体", "Source: base game"),
                SourceHintKind.Inferred => ($"來源：{owner}", $"来源：{owner}", $"Source: {owner}"),
                SourceHintKind.Registered => ($"來源：{owner}", $"来源：{owner}", $"Source: {owner}"),
                SourceHintKind.Unknown => ("來源：未知", "来源：未知", "Source: unknown"),
                SourceHintKind.Failed => ("來源：查詢失敗", "来源：查询失败", "Source: lookup failed"),
                _ => ("", "", "")
            };
            return language switch
            {
                SystemLanguage.Japanese => kind switch
                {
                    SourceHintKind.Scanning => "提供元：データ待機中…",
                    SourceHintKind.ScanTimedOut => "提供元：データ待機中…",
                    SourceHintKind.Native => "提供元：ゲーム本体",
                    SourceHintKind.Inferred => $"提供元：{owner}",
                    SourceHintKind.Registered => $"提供元：{owner}",
                    SourceHintKind.Unknown => "提供元：不明",
                    SourceHintKind.Failed => "提供元：照会に失敗",
                    _ => ""
                },
                SystemLanguage.Korean => kind switch
                {
                    SourceHintKind.Scanning => "출처: 데이터 대기 중…",
                    SourceHintKind.ScanTimedOut => "출처: 데이터 대기 중…",
                    SourceHintKind.Native => "출처: 기본 게임",
                    SourceHintKind.Inferred => $"출처: {owner}",
                    SourceHintKind.Registered => $"출처: {owner}",
                    SourceHintKind.Unknown => "출처: 알 수 없음",
                    SourceHintKind.Failed => "출처: 조회 실패",
                    _ => ""
                },
                SystemLanguage.French => kind switch
                {
                    SourceHintKind.Scanning => "Origine : données en attente…",
                    SourceHintKind.ScanTimedOut => "Origine : données en attente…",
                    SourceHintKind.Native => "Origine : jeu de base",
                    SourceHintKind.Inferred => $"Origine : {owner}",
                    SourceHintKind.Registered => $"Origine : {owner}",
                    SourceHintKind.Unknown => "Origine : inconnue",
                    SourceHintKind.Failed => "Origine : échec de la recherche",
                    _ => ""
                },
                SystemLanguage.German => kind switch
                {
                    SourceHintKind.Scanning => "Herkunft: warte auf Daten…",
                    SourceHintKind.ScanTimedOut => "Herkunft: warte auf Daten…",
                    SourceHintKind.Native => "Herkunft: Basisspiel",
                    SourceHintKind.Inferred => $"Herkunft: {owner}",
                    SourceHintKind.Registered => $"Herkunft: {owner}",
                    SourceHintKind.Unknown => "Herkunft: unbekannt",
                    SourceHintKind.Failed => "Herkunft: Abfrage fehlgeschlagen",
                    _ => ""
                },
                SystemLanguage.Portuguese => kind switch
                {
                    SourceHintKind.Scanning => "Origem: aguardando dados…",
                    SourceHintKind.ScanTimedOut => "Origem: aguardando dados…",
                    SourceHintKind.Native => "Origem: jogo base",
                    SourceHintKind.Inferred => $"Origem: {owner}",
                    SourceHintKind.Registered => $"Origem: {owner}",
                    SourceHintKind.Unknown => "Origem: desconhecida",
                    SourceHintKind.Failed => "Origem: falha na consulta",
                    _ => ""
                },
                SystemLanguage.Russian => kind switch
                {
                    SourceHintKind.Scanning => "Источник: ожидание данных…",
                    SourceHintKind.ScanTimedOut => "Источник: ожидание данных…",
                    SourceHintKind.Native => "Источник: базовая игра",
                    SourceHintKind.Inferred => $"Источник: {owner}",
                    SourceHintKind.Registered => $"Источник: {owner}",
                    SourceHintKind.Unknown => "Источник: неизвестен",
                    SourceHintKind.Failed => "Источник: ошибка запроса",
                    _ => ""
                },
                SystemLanguage.Spanish => kind switch
                {
                    SourceHintKind.Scanning => "Origen: datos en espera…",
                    SourceHintKind.ScanTimedOut => "Origen: datos en espera…",
                    SourceHintKind.Native => "Origen: juego base",
                    SourceHintKind.Inferred => $"Origen: {owner}",
                    SourceHintKind.Registered => $"Origen: {owner}",
                    SourceHintKind.Unknown => "Origen: desconocido",
                    SourceHintKind.Failed => "Origen: error de consulta",
                    _ => ""
                },
                SystemLanguage.ChineseTraditional => texts.Item1,
                SystemLanguage.ChineseSimplified => texts.Item2,
                _ => texts.Item3
            };
        }
    }
}
