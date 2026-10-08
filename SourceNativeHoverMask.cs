using System;
using System.Collections.Generic;
using System.Reflection;
using Duckov.UI;
using UnityEngine;

namespace DisplayItemSourceMod
{
    /// <summary>Visual-only masks for the checked Duckov 2.3.30 tooltip fields.
    /// Native setup may reactivate its registered/wishlist indicators after our callback;
    /// alpha masks stay closed across that setup without changing item data or actions.</summary>
    internal sealed class SourceNativeHoverMask
    {
        private static readonly string[] Fields = { "itemName", "weightDisplay", "itemDescription", "itemID",
            "itemProperties", "bulletTypeDisplay", "usageUtilitiesDisplay", "interactionIndicatorsContainer",
            "wishlistInfoParent", "registeredIndicator" };
        private readonly List<(CanvasGroup group, float alpha, bool created, bool identifier)> masks =
            new List<(CanvasGroup, float, bool, bool)>();
        private ItemHoveringUI? owner;

        public void Apply(ItemHoveringUI ui, bool hidden)
        {
            if (owner != ui)
            {
                Dispose(); owner = ui;
                foreach (string name in Fields)
                {
                    var field = typeof(ItemHoveringUI).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field == null) throw new InvalidOperationException("Unsupported native hover field: " + name);
                    object? value = field.GetValue(ui);
                    GameObject? target = value is Component c ? c.gameObject : value as GameObject;
                    if (target == null) continue;
                    var group = target.GetComponent<CanvasGroup>();
                    bool created = group == null;
                    if (created) group = target.AddComponent<CanvasGroup>();
                    masks.Add((group!, group!.alpha, created, name == "itemID"));
                }
            }
            foreach (var mask in masks)
                if (mask.group != null) mask.group.alpha = hidden || mask.identifier ? 0f : mask.alpha;
        }

        public void Release()
        {
            foreach (var mask in masks) if (mask.group != null) mask.group.alpha = mask.alpha;
        }
        public void Close()
        {
            if (owner == null) return;
            // Close only the native tooltip, using its existing Hide lifecycle.
            // It clears DisplayingItemID as well as the fade group.
            typeof(ItemHoveringUI).GetMethod("Hide", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(owner, null);
        }
        public void Dispose()
        {
            Release();
            foreach (var mask in masks) if (mask.created && mask.group != null) UnityEngine.Object.Destroy(mask.group);
            masks.Clear(); owner = null;
        }
    }
}
