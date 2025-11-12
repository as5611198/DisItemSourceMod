/*
 * =================================================================================
 * 專案「物品來源提示模組」 v1.1.0 
 *
 * v1.1.0 更新日誌:
 * 1. [v1.1.0 核心] :
 * - Bug: v1.0.3 在 API (檔案 1) 還沒掃完時，會直接 return (放棄)，導致 "沒了w"。
 * - 修正: 重寫 OnSetupItemHoveringUI！
 * - 修正: 1. 現在它會先顯示「來源: 正在掃描...」。
 * - 修正: 2. 它會啟動一個 Coroutine (協程) 在背景「等待」API。
 * - 修正: 3. 一旦 API 掃完 (isDatabaseReady)，協程會自動更新文字。
 * - 修正: 4. 移開滑鼠 (OnSetupMeta) 會自動中止協程。
 * =================================================================================
 */

using System;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections; // [v1.1.0] 為了 Coroutine

// [v1.0.0] 引用 核心 API
using DuckovCoreAPI;

namespace DisplayItemSourceMod
{
    /// <summary>
    /// 物品來源提示模組 (v1.1.0)
    /// (依賴 DuckovCoreAPI.dll)
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // [v1.0.0] UI 文字
        TextMeshProUGUI _text = null;
        TextMeshProUGUI Text
        {
            get
            {
                if (_text == null)
                {
                    _text = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
                }
                return _text;
            }
        }

        // [v1.1.0] 一個實例來啟動 Coroutine
        private static ModBehaviour? instance;

        // [v1.1.0] 儲存目前正在跑的「查詢」
        private Coroutine? activeLookup = null;

        void Awake()
        {
            instance = this; // [v1.1.0] 儲存實例
            Log("Loaded!!! (等待 DuckovCoreAPI...)");
        }

        void OnDestroy()
        {
            if (_text != null)
                Destroy(_text.gameObject); // [v1.1.0] 修正：刪除 GameObject
            if (instance == this)
                instance = null;
        }

        void OnEnable()
        {
            // (API: 物品價格代碼.txt)
            ItemHoveringUI.onSetupItem += OnSetupItemHoveringUI;
            ItemHoveringUI.onSetupMeta += OnSetupMeta;
        }
        void OnDisable()
        {
            ItemHoveringUI.onSetupItem -= OnSetupItemHoveringUI;
            ItemHoveringUI.onSetupMeta -= OnSetupMeta;
        }

        /// <summary>
        /// 當滑鼠移開 (v1.1.0 修正)
        /// </summary>
        private void OnSetupMeta(ItemHoveringUI uI, ItemMetaData data)
        {
            // [v1.1.0] 停止任何還在跑的「等待」
            if (activeLookup != null)
            {
                StopCoroutine(activeLookup);
                activeLookup = null;
            }
            Text.gameObject.SetActive(false);
        }

        /// <summary>
        /// 當滑鼠移入 (v1.1.0 修正)
        /// </summary>
        private void OnSetupItemHoveringUI(ItemHoveringUI uiInstance, Item item)
        {
            // [v1.1.0] 停止「上一個」還在跑的「等待」
            if (activeLookup != null)
            {
                StopCoroutine(activeLookup);
                activeLookup = null;
            }

            // --- 1. [v1.0.0] 設定 UI (照舊) ---
            if (item == null)
            {
                Text.gameObject.SetActive(false);
                return;
            }

            Text.gameObject.SetActive(true);
            Text.transform.SetParent(uiInstance.LayoutParent);
            Text.transform.localScale = Vector3.one;
            Text.fontSize = 20f;
            Text.fontStyle = FontStyles.Normal; // [v1.1.0] 確保樣式重置

            // --- 2. [v1.1.0] 啟動「等待 Coroutine」 ---
            activeLookup = StartCoroutine(WaitForAPI_Coroutine(item, Text));
        }

        /// <summary>
        /// [v1.1.0 新增] 在背景等待 API 掃描完畢的協程
        /// </summary>
        private IEnumerator WaitForAPI_Coroutine(Item item, TextMeshProUGUI textInstance)
        {
            // --- 1. 檢查 API 是否好了？ ---
            if (!DuckovCoreAPI.ModBehaviour.IsDatabaseReady())
            {
                // API 還沒好，先顯示「掃描中...」
                textInstance.text = "<color=#808080>來源: 正在掃描...</color>";

                // [v1.1.0 核心] 進入「等待迴圈」
                float timer = 0f;
                while (!DuckovCoreAPI.ModBehaviour.IsDatabaseReady())
                {
                    // [v1.1.0] 每 0.5 秒檢查一次 API
                    yield return new WaitForSeconds(0.5f);

                    // [v1.1.0] 安全機制：如果 Coroutine 已經被 OnSetupMeta 停止了
                    // (activeLookup 被設為 null)，就立刻中止
                    if (activeLookup == null)
                        yield break;

                    timer += 0.5f;
                    if (timer > 120f) // [v1.1.0] 如果 API 炸了，等 2 分鐘後放棄
                    {
                        textInstance.text = "<color=#FF6060>來源: API 掃描超時！</color>";
                        yield break;
                    }
                }
            }

            // --- 2. API 100% 好了！ ---
            // (可能一開始就好了，或是等了 20 秒後好了)

            // [v1.1.0] 再次檢查 item 是否還在 (安全機制)
            if (item == null)
            {
                textInstance.gameObject.SetActive(false);
                yield break;
            }

            // --- 3. 執行 v1.0.3 的「顯示邏輯」 ---
            DuckovCoreAPI.LedgerEntry entry;
            if (DuckovCoreAPI.ModBehaviour.GetEntry(item.TypeID, out entry))
            {
                // 抓到了！
                if (entry.GoldenID == "BaseGame")
                {
                    // 遊戲本體 (灰色)
                    textInstance.text = "<color=#808080>來源: 遊戲本體</color>";
                }
                else
                {
                    // Mod 物品 (亮藍色)
                    textInstance.text = $"<color=#80E0FF>來源: {entry.BronzeID}</color>";
                }
            }
            else
            {
                // 帳本裡沒有？ (這不該發生，但還是處理一下)
                textInstance.text = "<color=#FF6060>來源: 未知 (不在帳本)</color>";
            }

            // [v1.1.0] Coroutine 任務完成
            activeLookup = null;
        }

        // [v1.0.0] Log 函數
        public static void Log(string message)
        {
            UnityEngine.Debug.Log($"[DisplayItemSourceMod] {message}");
        }
    }

}

