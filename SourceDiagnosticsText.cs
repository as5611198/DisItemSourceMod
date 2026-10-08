using UnityEngine;

namespace DisplayItemSourceMod
{
    internal static class SourceDiagnosticsText
    {
        // Prefix, severity counts, historical information, catalog coverage,
        // global omission marker, query waiting. Raw identities are not UI labels.
        private static readonly string[][] texts =
        {
            new[] { "診斷", "錯誤", "警告", "資訊", "歷史資訊", "目錄不完整", "全域紀錄有省略", "資料等待中" },
            new[] { "诊断", "错误", "警告", "信息", "历史信息", "目录不完整", "全局记录有省略", "数据等待中" },
            new[] { "Diagnostics", "errors", "warnings", "info", "historical info", "catalog incomplete", "global records omitted", "waiting for data" },
            new[] { "診断", "エラー", "警告", "情報", "履歴情報", "目録が不完全", "全体記録に省略あり", "データ待機中" },
            new[] { "진단", "오류", "경고", "정보", "이력 정보", "목록 불완전", "전체 기록 일부 생략", "데이터 대기 중" },
            new[] { "Diagnostic", "erreurs", "avertissements", "infos", "infos historiques", "catalogue incomplet", "dossiers globaux omis", "données en attente" },
            new[] { "Diagnose", "Fehler", "Warnungen", "Infos", "historische Infos", "Katalog unvollständig", "globale Einträge ausgelassen", "warte auf Daten" },
            new[] { "Diagnóstico", "erros", "avisos", "informações", "informações históricas", "catálogo incompleto", "registros globais omitidos", "aguardando dados" },
            new[] { "Диагностика", "ошибки", "предупреждения", "информация", "историческая информация", "неполный каталог", "глобальные записи опущены", "ожидание данных" },
            new[] { "Diagnóstico", "errores", "avisos", "información", "información histórica", "catálogo incompleto", "registros globales omitidos", "datos en espera" }
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
