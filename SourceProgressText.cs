using UnityEngine;

namespace DisplayItemSourceMod
{
    internal static class SourceProgressText
    {
        // Player labels only; identities, paths, profile errors are never interpolated.
        private static readonly string[][] texts =
        {
            new[] { "JEI需求", "隨身", "製作範圍", "需", "缺額", "持有量未知", "計畫記錄不可用", "計畫不可用", "不完整", "資料等待中", "原生", "手動", "任務", "建築", "未知", "探索", "已記錄", "暫時辨識", "尚未記錄", "記錄不可用", "保存失敗" },
            new[] { "JEI需求", "随身", "制作范围", "需", "缺额", "持有量未知", "计划记录不可用", "计划不可用", "不完整", "数据等待中", "原生", "手动", "任务", "建筑", "未知", "探索", "已记录", "暂时识别", "尚未记录", "记录不可用", "保存失败" },
            new[] { "JEI needs", "Carried", "Crafting inventory", "need", "missing", "inventory unknown", "plan profile unavailable", "plan unavailable", "partial", "waiting for data", "Native", "manual", "quest", "building", "unknown", "Discovery", "recorded", "temporary", "not recorded", "record unavailable", "save failed" },
            new[] { "JEI必要数", "携帯", "製作範囲", "必要", "不足", "所持数不明", "計画記録が利用不可", "計画利用不可", "一部のみ", "データ待機中", "ゲーム", "手動", "クエスト", "建築", "不明", "探索", "記録済み", "一時識別", "未記録", "記録利用不可", "保存失敗" },
            new[] { "JEI 필요", "소지", "제작 범위", "필요", "부족", "보유량 알 수 없음", "계획 기록 사용 불가", "계획 사용 불가", "일부", "데이터 대기 중", "게임", "수동", "퀘스트", "건축", "알 수 없음", "탐색", "기록됨", "임시 식별", "미기록", "기록 사용 불가", "저장 실패" },
            new[] { "Besoins JEI", "Sur soi", "Stock de fabrication", "besoin", "manque", "inventaire inconnu", "profil de plan indisponible", "plan indisponible", "partiel", "données en attente", "Jeu", "manuel", "quête", "construction", "inconnu", "Découverte", "enregistré", "temporaire", "non enregistré", "registre indisponible", "échec de sauvegarde" },
            new[] { "JEI-Bedarf", "Mitgeführt", "Fertigungsvorrat", "Bedarf", "fehlend", "Bestand unbekannt", "Planprofil nicht verfügbar", "Plan nicht verfügbar", "teilweise", "warte auf Daten", "Spiel", "manuell", "Quest", "Bau", "unbekannt", "Entdeckung", "gespeichert", "temporär", "noch nicht erfasst", "Aufzeichnung nicht verfügbar", "Speichern fehlgeschlagen" },
            new[] { "Necessidade JEI", "Carregado", "Estoque de fabricação", "necessário", "falta", "inventário desconhecido", "perfil do plano indisponível", "plano indisponível", "parcial", "aguardando dados", "Jogo", "manual", "missão", "construção", "desconhecido", "Descoberta", "registrado", "temporário", "ainda não anotado", "registro indisponível", "falha ao salvar" },
            new[] { "Потребности JEI", "При себе", "Запасы для создания", "нужно", "не хватает", "запасы неизвестны", "профиль плана недоступен", "план недоступен", "частично", "ожидание данных", "Игра", "вручную", "задание", "стройка", "неизвестно", "Исследование", "записано", "временно", "ещё не отмечено", "записи недоступны", "ошибка сохранения" },
            new[] { "Necesidades JEI", "Llevado", "Inventario de fabricación", "necesita", "falta", "inventario desconocido", "perfil del plan no disponible", "plan no disponible", "parcial", "datos en espera", "Juego", "manual", "misión", "construcción", "desconocido", "Descubrimiento", "registrado", "temporal", "aún sin anotar", "registro no disponible", "error al guardar" }
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
