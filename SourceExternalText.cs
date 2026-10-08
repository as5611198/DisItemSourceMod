using UnityEngine;
namespace DisplayItemSourceMod
{
    internal static class SourceExternalText
    {
        private static readonly string[][] texts =
        {
            new[] { "原生", "外部（提供者自述）", "已知命中", "就緒", "部分", "等待", "不可用", "未涵蓋", "資料已變更", "細節已省略", "掉落", "其他", "未知", "自述定義機率（非玩家機率）", "條件" },
            new[] { "原生", "外部（提供者自述）", "已知命中", "就绪", "部分", "等待", "不可用", "未覆盖", "数据已变更", "细节已省略", "掉落", "其他", "未知", "自述定义概率（非玩家概率）", "条件" },
            new[] { "Native", "External (self-reported)", "known matches", "ready", "partial", "waiting", "unavailable", "not covered", "data changed", "details omitted", "loot", "other", "unknown", "reported definition p (not player odds)", "conditions" },
            new[] { "ゲーム", "外部（提供者の自己申告）", "既知の一致", "準備完了", "一部", "待機", "利用不可", "対象外", "データ変更", "詳細を省略", "ドロップ", "その他", "不明", "申告された定義確率（プレイヤー確率ではない）", "条件" },
            new[] { "게임", "외부 (제공자 자체 신고)", "알려진 일치", "준비됨", "일부", "대기", "사용 불가", "미지원", "데이터 변경", "세부 생략", "전리품", "기타", "알 수 없음", "신고된 정의 확률 (플레이어 확률 아님)", "조건" },
            new[] { "Jeu", "Externe (autodéclaré)", "correspondances connues", "prêt", "partiel", "en attente", "indisponible", "non couvert", "données modifiées", "détails omis", "butin", "autre", "inconnu", "probabilité définie déclarée (pas celle du joueur)", "conditions" },
            new[] { "Spiel", "Extern (selbst angegeben)", "bekannte Treffer", "bereit", "teilweise", "wartend", "nicht verfügbar", "nicht abgedeckt", "Daten geändert", "Details ausgelassen", "Beute", "andere", "unbekannt", "gemeldete Definitionschance (keine Spielerchance)", "Bedingungen" },
            new[] { "Jogo", "Externo (autodeclarado)", "correspondências conhecidas", "pronto", "parcial", "aguardando", "indisponível", "não coberto", "dados alterados", "detalhes omitidos", "saque", "outro", "desconhecido", "probabilidade definida declarada (não do jogador)", "condições" },
            new[] { "Игра", "Внешние (заявлено поставщиком)", "известные совпадения", "готово", "частично", "ожидание", "недоступно", "не охвачено", "данные изменены", "подробности пропущены", "добыча", "другое", "неизвестно", "заявленная вероятность определения (не шанс игрока)", "условия" },
            new[] { "Juego", "Externo (autodeclarado)", "coincidencias conocidas", "listo", "parcial", "en espera", "no disponible", "no cubierto", "datos cambiados", "detalles omitidos", "botín", "otro", "desconocido", "probabilidad definida declarada (no del jugador)", "condiciones" }
        };
        public static string[] For(SystemLanguage language) => texts[language switch {
            SystemLanguage.ChineseTraditional => 0, SystemLanguage.ChineseSimplified => 1, SystemLanguage.Japanese => 3,
            SystemLanguage.Korean => 4, SystemLanguage.French => 5, SystemLanguage.German => 6,
            SystemLanguage.Portuguese => 7, SystemLanguage.Russian => 8, SystemLanguage.Spanish => 9, _ => 2 }];
    }
}
