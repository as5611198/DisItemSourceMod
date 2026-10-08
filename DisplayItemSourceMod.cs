using System;
using System.Collections;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using DuckovCoreAPI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Globalization;

namespace DisplayItemSourceMod
{
    /// <summary>Compact source attribution for the currently displayed TypeID.</summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private readonly SourceHintState hint = new SourceHintState();
        private TextMeshProUGUI? text;
        private TextMeshProUGUI? planningText;
        private TextMeshProUGUI? hotkeyText;
        private TextMeshProUGUI? acquisitionText;
        private readonly SourceAcquisitionState acquisition = new SourceAcquisitionState();
        private SourceAcquisitionSubscription? acquisitionSubscription;
        private readonly SourceExternalState external = new SourceExternalState();
        private readonly SourceExternalRefreshQueue externalRefresh = new SourceExternalRefreshQueue();
        private SourceExternalSubscription? externalSubscription;
        private readonly SourceNativeAcquisitionState nativeAcquisition = new SourceNativeAcquisitionState();
        private SourceNativeAcquisitionSubscription? nativeAcquisitionSubscription;
        private TextMeshProUGUI? diagnosticText;
        private readonly SourceDiagnosticsState diagnostics = new SourceDiagnosticsState();
        private SourceDiagnosticsSubscription? diagnosticsSubscription;
        private TextMeshProUGUI? progressText;
        private readonly SourceProgressState progress = new SourceProgressState();
        private SourceProgressSubscription? progressSubscription;
        // Lazy initialization keeps cleanup safe in the metadata fixture, which
        // intentionally bypasses Unity's MonoBehaviour constructor.
        private SourceTrackingSummaryCache? trackingSummary = new SourceTrackingSummaryCache();
        private SourceLayoutCache? layoutCache = new SourceLayoutCache();
        private InventoryScope currentMarkerScope;
        private long currentPlanningRevision;
        private readonly SourceEnableGuard callbacks = new SourceEnableGuard();
        private Action<ItemHoveringUI, Item>? setupItemCallback;
        private Action<ItemHoveringUI, ItemMetaData>? setupMetaCallback;
        private Action<Duckov.Modding.ModInfo, Duckov.Modding.ModBehaviour>? providerCallback;
        private UnityAction<Scene, Scene>? sceneCallback;
        private UnityAction<Scene>? unloadCallback;
        private int hoverSceneHandle;
        private readonly SourceV3Preferences preferences = new SourceV3Preferences();
        private SourceOptionalSettings? optionalSettings;
        private SourceSettingsRegistration? settingsRegistration;
        private SourceV3Connection? v3Connection;
        private SystemLanguage language = SystemLanguage.English;
        private float nextSettingsCheck;
        private bool providerWasReady;
        private ItemHoveringUI? hoverUI;
        private Coroutine? activeLookup;
        private SourceDataSubscription? dataSubscription;
        private SourceLanguageSubscription? languageSubscription;
        private long hoverToken;
        private bool failureLogged;
        private SourceExplorationSubscription? explorationSubscription;
        private readonly SourceNativeHoverMask nativeHoverMask = new SourceNativeHoverMask();

        private void Awake() => Log("Loaded v1.10.2 (DuckovCoreAPI JEI-V15-1)");

        private void OnEnable()
        {
            long owner = callbacks.Enable();
            externalRefresh.Enable();
            explorationSubscription ??= new SourceExplorationSubscription(OnExplorationChanged);
            explorationSubscription.Enable();
            optionalSettings ??= new SourceOptionalSettings(() => info);
            settingsRegistration = new SourceSettingsRegistration(preferences, () => language,
                optionalSettings.GetSaved, optionalSettings.Add, optionalSettings.Remove, RedrawCurrent);
            providerWasReady = false;
            nextSettingsCheck = 0;
            v3Connection ??= new SourceV3Connection(OnV3Changed);
            v3Connection.Enable();
            acquisitionSubscription ??= new SourceAcquisitionSubscription(OnAcquisitionChanged);
            acquisitionSubscription.Enable();
            externalSubscription ??= new SourceExternalSubscription(OnExternalAcquisitionChanged);
            externalSubscription.Enable();
            nativeAcquisitionSubscription ??= new SourceNativeAcquisitionSubscription(OnNativeAcquisitionChanged);
            nativeAcquisitionSubscription.Enable();
            diagnosticsSubscription ??= new SourceDiagnosticsSubscription(OnDiagnosticsChanged);
            diagnosticsSubscription.Enable();
            progressSubscription ??= new SourceProgressSubscription(OnProgressChanged);
            progressSubscription.Enable();
            DetachSceneEvents();
            sceneCallback = (previous, current) => { if (callbacks.IsCurrent(owner)) OnSceneChanged(previous, current); };
            unloadCallback = unloaded => { if (callbacks.IsCurrent(owner)) OnSceneUnloaded(unloaded); };
            SceneManager.activeSceneChanged += sceneCallback;
            SceneManager.sceneUnloaded += unloadCallback;
            if (providerCallback != null)
            {
                Duckov.Modding.ModManager.OnModActivated -= providerCallback;
                Duckov.Modding.ModManager.OnModWillBeDeactivated -= providerCallback;
            }
            if (setupItemCallback != null) ItemHoveringUI.onSetupItem -= setupItemCallback;
            if (setupMetaCallback != null) ItemHoveringUI.onSetupMeta -= setupMetaCallback;
            providerCallback = (changed, behaviour) => { if (callbacks.IsCurrent(owner)) OnOptionalProviderLifecycle(changed, behaviour); };
            setupItemCallback = (ui, item) => { if (callbacks.IsCurrent(owner)) OnSetupItem(ui, item); };
            setupMetaCallback = (ui, data) => { if (callbacks.IsCurrent(owner)) OnSetupMeta(ui, data); };
            Duckov.Modding.ModManager.OnModActivated += providerCallback;
            Duckov.Modding.ModManager.OnModWillBeDeactivated += providerCallback;
            ItemHoveringUI.onSetupItem += setupItemCallback;
            ItemHoveringUI.onSetupMeta += setupMetaCallback;
            dataSubscription ??= new SourceDataSubscription(OnDataChanged);
            dataSubscription.Enable();
            languageSubscription ??= new SourceLanguageSubscription(OnLanguageChanged);
            languageSubscription.Enable();
            // Enabling while already hovering does not emit another setup event.
            if (ItemHoveringUI.Shown && ItemHoveringUI.Instance != null)
                BeginHover(ItemHoveringUI.Instance, ItemHoveringUI.DisplayingItemID);
        }

        private void OnDisable()
        {
            callbacks.Disable();
            nativeHoverMask.Close();
            explorationSubscription?.Disable();
            externalRefresh.Disable();
            v3Connection?.Disable();
            acquisitionSubscription?.Disable();
            externalSubscription?.Disable();
            nativeAcquisitionSubscription?.Disable();
            diagnosticsSubscription?.Disable();
            progressSubscription?.Disable();
            DetachSceneEvents();
            settingsRegistration?.Deactivate();
            Duckov.Modding.ModManager.OnModActivated -= providerCallback;
            Duckov.Modding.ModManager.OnModWillBeDeactivated -= providerCallback;
            ItemHoveringUI.onSetupItem -= setupItemCallback;
            ItemHoveringUI.onSetupMeta -= setupMetaCallback;
            dataSubscription?.Disable();
            languageSubscription?.Disable();
            ClearHover();
        }

        private void OnDestroy()
        {
            callbacks.Disable();
            nativeHoverMask.Close();
            explorationSubscription?.Dispose();
            externalRefresh.Disable();
            v3Connection?.Dispose();
            acquisitionSubscription?.Dispose();
            externalSubscription?.Dispose();
            nativeAcquisitionSubscription?.Dispose();
            diagnosticsSubscription?.Dispose();
            progressSubscription?.Dispose();
            DetachSceneEvents();
            Duckov.Modding.ModManager.OnModActivated -= providerCallback;
            Duckov.Modding.ModManager.OnModWillBeDeactivated -= providerCallback;
            ItemHoveringUI.onSetupItem -= setupItemCallback;
            ItemHoveringUI.onSetupMeta -= setupMetaCallback;
            settingsRegistration?.Deactivate();
            dataSubscription?.Dispose();
            languageSubscription?.Dispose();
            ClearHover();
            nativeHoverMask.Dispose();
            if (text != null) Destroy(text.gameObject);
            if (planningText != null) Destroy(planningText.gameObject);
            if (hotkeyText != null) Destroy(hotkeyText.gameObject);
            if (acquisitionText != null) Destroy(acquisitionText.gameObject);
            if (diagnosticText != null) Destroy(diagnosticText.gameObject);
            if (progressText != null) Destroy(progressText.gameObject);
            text = null;
            planningText = hotkeyText = null;
            acquisitionText = null;
            diagnosticText = null;
            progressText = null;
        }

        private void OnSetupItem(ItemHoveringUI ui, Item item)
        {
            if (item == null) ClearHover();
            else BeginHover(ui, item.TypeID);
        }

        private void OnSetupMeta(ItemHoveringUI ui, ItemMetaData data)
        {
            // Duckov 2.3.30: metadata hover, NOT a pointer-exit notification.
            BeginHover(ui, data.id);
        }

        private void BeginHover(ItemHoveringUI ui, int typeID)
        {
            ClearHover();
            if (!isActiveAndEnabled || ui == null || ui.LayoutParent == null || typeID < 0) return;

            if (!EnsureText(ui)) { v3Connection?.SetCanPresent(false); return; }
            // Register can synchronously dispatch Changed. Do it before beginning
            // the hover, because native setup events precede fadeGroup.Show().
            // Diagnostics permits native ID 0; navigation/hotkeys still require a positive ID.
            v3Connection?.SetCanPresent(typeID > 0 && ExplorationApi.CanReveal(typeID));

            hoverUI = ui;
            hoverSceneHandle = SceneManager.GetActiveScene().handle;
            long token = hint.Begin(typeID, Time.realtimeSinceStartupAsDouble);
            hoverToken = token;
            acquisition.Begin(typeID, token);
            external.Begin(typeID, token, hoverSceneHandle);
            nativeAcquisition.Begin(typeID, token, hoverSceneHandle);
            diagnostics.Begin(typeID, token, hoverSceneHandle);
            progress.Begin(typeID, token, hoverSceneHandle);
            failureLogged = false;
            RenderHint();
            activeLookup = StartCoroutine(LookupWhileHovered(token, typeID));
        }

        private void LateUpdate()
        {
            if (Time.unscaledTime >= nextSettingsCheck)
            {
                nextSettingsCheck = Time.unscaledTime + 2f;
                TrySettings();
            }
            if (text == null || planningText == null || hotkeyText == null || acquisitionText == null)
                v3Connection?.SetCanPresent(false);
            if (!hint.IsActive) return;
            hint.ObserveHover(IsHoverVisible(), ItemHoveringUI.DisplayingItemID);
            if (!hint.IsActive) ClearHover();
            if (hint.IsActive)
            {
                int refresh = externalRefresh.Take();
                if (refresh == 2) RefreshHint(hoverToken);
                else if (refresh == 1) RedrawCurrent();
            }
        }

        private bool IsHoverVisible() => isActiveAndEnabled && hoverUI != null &&
            hoverUI == ItemHoveringUI.Instance && hoverUI.isActiveAndEnabled && hoverUI.LayoutParent != null &&
            ItemHoveringUI.Shown && text != null && planningText != null && hotkeyText != null && acquisitionText != null &&
            SceneManager.GetActiveScene().handle == hoverSceneHandle;

        private void TrySettings()
        {
            bool ready = optionalSettings?.Ready == true;
            if (!ready && providerWasReady) settingsRegistration?.ProviderLost();
            providerWasReady = ready;
            if (ready) settingsRegistration?.TryRegister();
        }

        private void OnOptionalProviderLifecycle(Duckov.Modding.ModInfo changed, Duckov.Modding.ModBehaviour behaviour)
        {
            if (changed.name != "ModSetting") return;
            settingsRegistration?.ProviderLost();
            providerWasReady = false;
            nextSettingsCheck = 0;
        }

        private void OnV3Changed(long revision)
        {
            trackingSummary?.Clear();
            RedrawCurrent();
        }
        private void OnExplorationChanged(long revision)
        {
            RedactHover();
            externalRefresh.MarkQuery();
        }
        private bool CanRevealHover()
        {
            try { return hint.IsActive && ExplorationApi.GetSettings().IsReady && ExplorationApi.CanReveal(hint.TypeID); }
            catch { return false; }
        }

        private void RedactHover()
        {
            trackingSummary ??= new SourceTrackingSummaryCache();
            layoutCache ??= new SourceLayoutCache();
            trackingSummary.Clear();
            hint.Redact(hoverToken);
            acquisition.Begin(hint.TypeID, hoverToken);
            external.Begin(hint.TypeID, hoverToken, hoverSceneHandle);
            nativeAcquisition?.Begin(hint.TypeID, hoverToken, hoverSceneHandle);
            diagnostics.Clear();
            progress.Begin(hint.TypeID, hoverToken, hoverSceneHandle);
            foreach (var row in new[] { acquisitionText, planningText, hotkeyText, diagnosticText, progressText })
                if (row != null) { row.text = ""; row.gameObject.SetActive(false); }
            if (text != null)
            {
                text.text = hint.IsActive ? "？？？" : "";
                text.color = new Color32(128, 128, 128, 255);
                text.fontSize = preferences.FontSize;
                text.gameObject.SetActive(hint.IsActive);
            }
            MarkLayoutIfChanged();
            if (hoverUI != null) nativeHoverMask?.Apply(hoverUI, true);
            // Data and visible rows are gone before a synchronous presenter notification.
            v3Connection?.SetCanPresent(false);
        }
        private void OnAcquisitionChanged(long revision)
        {
            acquisition.Fail(hoverToken);
            external.Invalidate(hoverToken, hoverSceneHandle);
            nativeAcquisition?.Invalidate(hoverToken, hoverSceneHandle);
            PurgeAcquisitionPresentation();
            externalRefresh.MarkQuery();
        }
        private void OnDiagnosticsChanged(long revision)
        {
            if (revision == DiagnosticsApi.Revision) RedrawCurrent();
        }
        private void OnProgressChanged(long revision)
        {
            RedactHover();
            externalRefresh.MarkQuery();
        }
        private void OnExternalAcquisitionChanged(long revision)
        {
            external.Invalidate(hoverToken, hoverSceneHandle);
            externalRefresh.MarkExternal();
        }

        private void OnNativeAcquisitionChanged(long revision)
        {
            nativeAcquisition?.Invalidate(hoverToken, hoverSceneHandle);
            external.Invalidate(hoverToken, hoverSceneHandle);
            PurgeAcquisitionPresentation();
            externalRefresh.MarkQuery();
        }

        private void PurgeAcquisitionPresentation()
        {
            if (acquisitionText == null) return;
            acquisitionText.text = "";
            acquisitionText.gameObject.SetActive(false);
        }

        private void OnSceneChanged(Scene previous, Scene current)
        {
            nativeHoverMask?.Close();
            ClearHover();
        }
        private void OnSceneUnloaded(Scene unloaded)
        {
            if (unloaded.handle == hoverSceneHandle) { nativeHoverMask.Close(); ClearHover(); }
        }
        private void DetachSceneEvents()
        {
            SceneManager.activeSceneChanged -= sceneCallback;
            SceneManager.sceneUnloaded -= unloadCallback;
        }

        private void RedrawCurrent()
        {
            if (!hint.IsActive || !isActiveAndEnabled) return;
            hint.ObserveHover(IsHoverVisible(), ItemHoveringUI.DisplayingItemID);
            if (!hint.IsActive) ClearHover();
            else RenderHint();
        }

        private void OnDataChanged(long revision)
        {
            RedactHover();
            external.Invalidate(hoverToken, hoverSceneHandle);
            nativeAcquisition?.Invalidate(hoverToken, hoverSceneHandle);
            externalRefresh.MarkQuery();
        }

        private void OnLanguageChanged(SystemLanguage language)
        {
            if (!isActiveAndEnabled) return;
            this.language = language;
            hint.SetLanguage(language);
            if (optionalSettings?.Ready == true) settingsRegistration?.RefreshLanguage();
            if (!hint.IsActive) return;
            hint.ObserveHover(IsHoverVisible(), ItemHoveringUI.DisplayingItemID);
            if (!hint.IsActive) ClearHover();
            else RenderHint();
        }

        private void RefreshHint(long token)
        {
            if (!CanRevealHover()) { RedactHover(); return; }
            try
            {
                hint.Refresh(token, DuckovCoreAPI.ModBehaviour.IsDatabaseReady(), Time.realtimeSinceStartupAsDouble);
            }
            catch (Exception ex)
            {
                hint.Fail(token);
                if (!failureLogged)
                {
                    Log($"來源查詢失敗 (TypeID {hint.TypeID}): {ex}");
                    failureLogged = true;
                }
            }
            RenderHint();
        }

        private IEnumerator LookupWhileHovered(long token, int typeID)
        {
            // Setup events precede fadeGroup.Show(). Allow StartCoroutine to
            // return its handle before any completion or cleanup, too.
            yield return null;
            var interval = new WaitForSecondsRealtime(0.5f);
            while (hint.IsCurrent(token))
            {
                if (!IsHoverVisible() || ItemHoveringUI.DisplayingItemID != typeID)
                {
                    ClearHover();
                    yield break;
                }
                // Ownership conflict recovery shares this existing hover poll.
                v3Connection?.SetCanPresent(typeID > 0 && CanRevealHover());
                RefreshHint(token);
                // Query/External notifications coalesce in LateUpdate; low-frequency checks also
                // recover if readiness changes without another notification.
                yield return interval;
            }
        }

        private void RenderHint()
        {
            if (!CanRevealHover()) { RedactHover(); return; }
            if (text == null) return;
            if (hoverUI != null) nativeHoverMask.Apply(hoverUI, false);
            RenderAcquisition();
            RenderProgress();
            string sourceText = SourceV3Presentation.Source(hint, preferences.Detailed, preferences.ModOnly, language);
            if (text.text != sourceText) text.text = sourceText;
            text.color = hint.Kind switch
            {
                SourceHintKind.Native or SourceHintKind.Scanning => new Color32(128, 128, 128, 255),
                SourceHintKind.Inferred or SourceHintKind.Registered => new Color32(128, 224, 255, 255),
                _ => new Color32(255, 96, 96, 255)
            };
            text.fontSize = preferences.FontSize;
            text.gameObject.SetActive(hint.IsActive && text.text.Length > 0);
            if (planningText != null && hotkeyText != null)
            {
                string nextPlanning = "";
                string nextHotkeys = "";
                try
                {
                    trackingSummary ??= new SourceTrackingSummaryCache();
                    if (hint.IsActive && PlanningApi.IsProfileReady)
                    {
                        long planningRevision = PlanningApi.Revision;
                        if (!trackingSummary.TryGet(planningRevision, InventoryScope.Carried, out TrackingSummary summary))
                        {
                            summary = PlanningApi.GetTrackingSummary(InventoryScope.Carried);
                            trackingSummary.Store(planningRevision, InventoryScope.Carried, summary);
                        }
                        bool replaceTracking = progressText != null && progressText.text.Length > 0 &&
                            SourceProgressPresentation.ReplacesTracking(progress.GetWishlist(hoverToken, hoverSceneHandle, currentMarkerScope), hint.TypeID, currentMarkerScope, currentPlanningRevision);
                        bool favorite = PlanningApi.IsFavorite(hint.TypeID), pinned = PlanningApi.IsPinnedOutput(hint.TypeID);
                        nextPlanning = replaceTracking
                            ? SourceV3Presentation.PlanningWithoutMaterials(hint.TypeID, true, favorite, pinned, summary, language)
                            : SourceV3Presentation.Planning(hint.TypeID, true, favorite, pinned, summary, language);
                    }
                    nextHotkeys = hint.IsActive ? SourceV3Presentation.Hotkeys(v3Connection?.OwnsPresenter == true,
                        HotkeyApi.GetBindings(), language) : "";
                }
                catch (Exception ex)
                {
                    nextPlanning = hint.IsActive ? SourceV3Text.Label("planUnavailable", language) : "";
                    nextHotkeys = "";
                    if (!failureLogged) { Log($"V3 提示讀取失敗: {ex}"); failureLogged = true; }
                }
                if (planningText.text != nextPlanning) planningText.text = nextPlanning;
                if (hotkeyText.text != nextHotkeys) hotkeyText.text = nextHotkeys;
                planningText.fontSize = hotkeyText.fontSize = preferences.FontSize;
                planningText.gameObject.SetActive(hint.IsActive && planningText.text.Length > 0);
                hotkeyText.gameObject.SetActive(hint.IsActive && hotkeyText.text.Length > 0);
            }
            MarkLayoutIfChanged();
        }

        private void MarkLayoutIfChanged()
        {
            string[] current = { text?.text ?? "", planningText?.text ?? "", hotkeyText?.text ?? "",
                acquisitionText?.text ?? "", progressText?.text ?? "" };
            layoutCache ??= new SourceLayoutCache();
            bool[] active = { text?.gameObject.activeSelf == true, planningText?.gameObject.activeSelf == true,
                hotkeyText?.gameObject.activeSelf == true, acquisitionText?.gameObject.activeSelf == true,
                progressText?.gameObject.activeSelf == true };
            int fontSize = preferences?.FontSize ?? 18;
            if (layoutCache.Changed(fontSize, active, current) && hoverUI != null && hoverUI.LayoutParent != null)
            {
                // ItemHoveringUI.RefreshPosition is private and runs in the
                // native Update loop. Complete the native ContentSizeFitter
                // pass now, so that its next screen clamp sees Source rows.
                RectTransform layoutParent = hoverUI.LayoutParent;
                LayoutRebuilder.MarkLayoutForRebuild(layoutParent);
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutParent);
                if (layoutParent.parent is RectTransform contents)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(contents);
            }
        }

        private bool EnsureText(ItemHoveringUI ui)
        {
            var template = GameplayDataSettings.UIStyle != null ? GameplayDataSettings.UIStyle.TemplateTextUGUI : null;
            if ((text == null || planningText == null || hotkeyText == null || acquisitionText == null) && template == null) return false;
            if (text == null) text = CreateText(template!, "Source attribution");
            if (planningText == null) planningText = CreateText(template!, "Source planning");
            if (hotkeyText == null) hotkeyText = CreateText(template!, "Source hotkeys");
            if (acquisitionText == null)
            {
                acquisitionText = CreateText(template!, "Source acquisition definitions");
                acquisitionText.enableWordWrapping = false;
                acquisitionText.maxVisibleLines = 2;
                acquisitionText.overflowMode = TextOverflowModes.Ellipsis;
                acquisitionText.color = new Color32(160, 208, 192, 255);
            }
            // An optional badge cannot make existing rows lose their presenter.
            if (diagnosticText == null && template != null)
            {
                diagnosticText = CreateText(template, "Source diagnostics");
                diagnosticText.enableWordWrapping = false;
                diagnosticText.maxVisibleLines = 1;
                diagnosticText.overflowMode = TextOverflowModes.Ellipsis;
                diagnosticText.color = new Color32(192, 176, 224, 255);
            }
            if (progressText == null && template != null)
            {
                progressText = CreateText(template, "Source demand and discovery");
                progressText.enableWordWrapping = false;
                progressText.maxVisibleLines = 2;
                progressText.overflowMode = TextOverflowModes.Ellipsis;
                progressText.color = new Color32(192, 208, 144, 255);
            }
            planningText.color = new Color32(224, 196, 128, 255);
            hotkeyText.color = new Color32(221, 187, 112, 255);
            foreach (var row in new[] { text, acquisitionText, planningText, hotkeyText, diagnosticText, progressText })
            {
                if (row == null) continue;
                row.transform.SetParent(ui.LayoutParent, false);
                row.transform.localScale = Vector3.one;
                row.transform.SetAsLastSibling();
            }
            return true;
        }

        private TextMeshProUGUI CreateText(TextMeshProUGUI template, string name)
        {
            var row = Instantiate(template);
            row.name = name;
            row.fontStyle = FontStyles.Normal;
            row.alignment = TextAlignmentOptions.Left;
            // Owner / identifier values remain literal and never execute TMP markup.
            row.richText = false;
            row.raycastTarget = false;
            row.enableAutoSizing = false;
            row.enableWordWrapping = true;
            row.overflowMode = TextOverflowModes.Overflow;
            var layout = row.GetComponent<LayoutElement>();
            if (layout == null) layout = row.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = 0;
            layout.preferredWidth = 360;
            layout.flexibleWidth = 0;
            layout.minHeight = 0;
            layout.preferredHeight = -1;
            layout.flexibleHeight = 0;
            layout.ignoreLayout = false;
            row.gameObject.SetActive(false);
            return row;
        }

        private void ClearHover()
        {
            trackingSummary?.Clear();
            layoutCache?.Clear();
            hint.Clear();
            acquisition.Clear();
            external?.Clear();
            externalRefresh?.Clear();
            nativeAcquisition?.Clear();
            diagnostics.Clear();
            progress.Clear();
            nativeHoverMask?.Release();
            if (activeLookup != null) StopCoroutine(activeLookup);
            activeLookup = null;
            hoverUI = null;
            hoverToken = 0;
            hoverSceneHandle = 0;
            currentMarkerScope = InventoryScope.Carried;
            currentPlanningRevision = -1;
            // Cleanup must never instantiate a new text object.
            if (text != null)
            {
                text.text = "";
                text.gameObject.SetActive(false);
            }
            if (planningText != null) { planningText.text = ""; planningText.gameObject.SetActive(false); }
            if (hotkeyText != null) { hotkeyText.text = ""; hotkeyText.gameObject.SetActive(false); }
            if (acquisitionText != null) { acquisitionText.text = ""; acquisitionText.gameObject.SetActive(false); }
            if (diagnosticText != null) { diagnosticText.text = ""; diagnosticText.gameObject.SetActive(false); }
            if (progressText != null) { progressText.text = ""; progressText.gameObject.SetActive(false); }
            // Capability belongs to the current valid hover. Release after clearing
            // its state so synchronous Changed can safely restore Recipes fallback.
            // BeginHover calls this before every validation return, then reacquires
            // only after the next UI and our text rows are usable.
            v3Connection?.SetCanPresent(false);
        }

        private void RenderAcquisition()
        {
            if (!CanRevealHover()) { RedactHover(); return; }
            if (acquisitionText == null) return;
            string native = "";
            try
            {
                if (hint.IsActive && acquisition.NeedsRefresh(AcquisitionApi.Revision))
                {
                    // Query even when IsReady is false: ContextChanged must clear
                    // this hover immediately rather than retaining a prior world.
                    acquisition.Accept(hoverToken, AcquisitionApi.GetItem(hint.TypeID));
                }
                    native = hint.IsActive ? RenderNativeAcquisitionText(acquisition.Result) : "";
            }
            catch (Exception ex)
            {
                acquisition.Fail(hoverToken);
                if (!failureLogged) { Log($"取得定義查詢失敗: {ex}"); failureLogged = true; }
            }
            var extra = RenderExternalAcquisition();
            RenderNativeAcquisition();
            string acquisitionValue = hint.IsActive ? SourceExternalPresentation.FormatWithNativeClues(native, extra,
                nativeAcquisition.Loot, nativeAcquisition.Merchants, nativeAcquisition.Details,
                nativeAcquisition.Settings, language) : "";
            if (acquisitionText.text != acquisitionValue) acquisitionText.text = acquisitionValue;
            acquisitionText.fontSize = preferences.FontSize;
            acquisitionText.gameObject.SetActive(hint.IsActive && acquisitionText.text.Length > 0);
        }

        private string RenderNativeAcquisitionText(AcquisitionItemResult? result)
        {
            var presentation = AcquisitionPresentationApi.GetSettings();
            return SourceAcquisitionPresentation.FormatWithShopDetail(result, language,
                presentation.IsReady && presentation.DetailedShops);
        }

        private ExternalAcquisitionItemResult? RenderExternalAcquisition()
        {
            if (!CanRevealHover()) { RedactHover(); return null; }
            if (!hint.IsActive || hint.TypeID <= 0) return null;
            int scene = SceneManager.GetActiveScene().handle;
            try
            {
                var context = ExternalAcquisitionApi.GetContext();
                long revision = ExternalAcquisitionApi.Revision, catalog = QueryApi.DataRevision;
                if (external.NeedsRefresh(hoverToken, scene, revision, context.Epoch, catalog))
                {
                    var value = ExternalAcquisitionApi.GetItem(hint.TypeID);
                    string name = "";
                    ItemSourceInfo source = default;
                    if (value.Status == ExternalAcquisitionStatus.Success && value.Entries.Count > 0)
                    {
                        QueryApi.TryGetItemSource(hint.TypeID, out source);
                        foreach (var item in QueryApi.SearchItems(hint.TypeID.ToString(CultureInfo.InvariantCulture)))
                            if (item.TypeID == hint.TypeID) { name = item.ItemNameKey; break; }
                    }
                    var latest = ExternalAcquisitionApi.GetContext();
                    external.Accept(hoverToken, scene, ExternalAcquisitionApi.Revision, latest.Epoch, QueryApi.DataRevision, name, source, value);
                }
                var current = ExternalAcquisitionApi.GetContext();
                return external.Get(hoverToken, scene, ExternalAcquisitionApi.Revision, current.Epoch, QueryApi.DataRevision);
            }
            catch (Exception ex)
            {
                external.Fail(hoverToken, scene);
                if (!failureLogged) { Log($"外部取得查詢失敗: {ex}"); failureLogged = true; }
                return null;
            }
        }

        private void RenderNativeAcquisition()
        {
            if (!CanRevealHover()) { RedactHover(); return; }
            if (!hint.IsActive || hint.TypeID <= 0) return;
            int scene = SceneManager.GetActiveScene().handle;
            try
            {
                var context = NativeLootApi.GetContext();
                var presentation = AcquisitionPresentationApi.GetSettings();
                bool detailedLoot = presentation.IsReady && presentation.DetailedLoot;
                bool detailedShops = presentation.IsReady && presentation.DetailedShops;
                long lootRevision = NativeLootApi.Revision;
                long acquisitionRevision = AcquisitionApi.Revision;
                if (nativeAcquisition.NeedsRefresh(hoverToken, scene, lootRevision, acquisitionRevision,
                    presentation.Revision, context.Epoch, QueryApi.DataRevision, detailedLoot, detailedShops))
                {
                    var loot = NativeLootApi.GetItem(hint.TypeID);
                    var merchants = MerchantClueApi.GetItem(hint.TypeID);
                    var details = detailedLoot ? NativeLootApi.GetDetails(hint.TypeID) : null;
                    var latestContext = NativeLootApi.GetContext();
                    var latestPresentation = AcquisitionPresentationApi.GetSettings();
                    nativeAcquisition.Accept(hoverToken, scene, NativeLootApi.Revision, AcquisitionApi.Revision,
                        latestPresentation.Revision, latestContext.Epoch, QueryApi.DataRevision,
                        latestPresentation.IsReady && latestPresentation.DetailedLoot,
                        latestPresentation.IsReady && latestPresentation.DetailedShops,
                        loot, merchants, details, latestPresentation);
                }
            }
            catch (Exception ex)
            {
                nativeAcquisition.Fail(hoverToken, scene);
                if (!failureLogged) { Log($"原生取得線索查詢失敗: {ex}"); failureLogged = true; }
            }
        }

        private void RenderDiagnostics()
        {
            if (!CanRevealHover()) { RedactHover(); return; }
            if (diagnosticText == null) return;
            int scene = SceneManager.GetActiveScene().handle;
            try
            {
                if (hint.IsActive && diagnostics.NeedsRefresh(hoverToken, scene, DiagnosticsApi.Revision))
                    diagnostics.Accept(hoverToken, scene, DiagnosticsApi.GetItem(hint.TypeID));
                string diagnostic = hint.IsActive ? SourceDiagnosticsPresentation.Format(diagnostics.GetResult(hoverToken, scene), hint.TypeID, language) : "";
                if (diagnosticText.text != diagnostic) diagnosticText.text = diagnostic;
            }
            catch (Exception ex)
            {
                diagnostics.Fail(hoverToken, scene);
                if (diagnosticText.text.Length > 0) diagnosticText.text = "";
                if (!failureLogged) { Log($"診斷查詢失敗: {ex}"); failureLogged = true; }
            }
            diagnosticText.fontSize = preferences.FontSize;
            diagnosticText.gameObject.SetActive(hint.IsActive && diagnosticText.text.Length > 0);
        }

        private void RenderProgress()
        {
            if (!CanRevealHover()) { RedactHover(); return; }
            if (progressText == null) return;
            int scene = SceneManager.GetActiveScene().handle;
            currentMarkerScope = WishlistApi.MarkerScope;
            currentPlanningRevision = PlanningApi.Revision;
            try
            {
                if (hint.IsActive && progress.NeedsWishlistRefresh(hoverToken, scene, currentMarkerScope, WishlistApi.Revision, currentPlanningRevision))
                    progress.AcceptWishlist(hoverToken, scene, currentMarkerScope, currentPlanningRevision, WishlistApi.GetItem(hint.TypeID, currentMarkerScope));
            }
            catch (Exception ex)
            {
                progress.FailWishlist(hoverToken, scene);
                if (!failureLogged) { Log($"JEI需求查詢失敗: {ex}"); failureLogged = true; }
            }
            try
            {
                long catalog = QueryApi.DataRevision;
                if (hint.IsActive && progress.NeedsDiscoveryRefresh(hoverToken, scene, DiscoveryApi.Revision, catalog))
                    progress.AcceptDiscovery(hoverToken, scene, catalog, DiscoveryApi.GetItem(hint.TypeID));
            }
            catch (Exception ex)
            {
                progress.FailDiscovery(hoverToken, scene);
                if (!failureLogged) { Log($"探索查詢失敗: {ex}"); failureLogged = true; }
            }
            string progressValue = hint.IsActive ? SourceProgressPresentation.FormatPlayer(progress.GetWishlist(hoverToken, scene, currentMarkerScope),
                progress.GetDiscovery(hoverToken, scene), hint.TypeID, currentMarkerScope, currentPlanningRevision, language) : "";
            if (progressText.text != progressValue) progressText.text = progressValue;
            progressText.fontSize = preferences.FontSize;
            progressText.gameObject.SetActive(hint.IsActive && progressText.text.Length > 0);
        }

        public static void Log(string message) => Debug.Log($"[DisplayItemSourceMod] {message}");
    }
}
