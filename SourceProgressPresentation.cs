using System.Collections.Generic;
using System.Globalization;
using DuckovCoreAPI;
using UnityEngine;

namespace DisplayItemSourceMod
{
    internal static class SourceProgressPresentation
    {
        private static bool Matches(WishlistItemResult? value, int id, InventoryScope scope) => value != null && value.TypeID == id && value.Scope == scope &&
            (scope == InventoryScope.Carried || scope == InventoryScope.CraftingAvailable);
        public static bool ReplacesTracking(WishlistItemResult? value, int id, InventoryScope scope, long planningRevision) =>
            Matches(value, id, scope) && value!.Status == WishlistQueryStatus.Success && value.PinsReady && value.PlanStatus == PlanStatus.Success && value.PlanningRevision == planningRevision && value.Required > 0;

        public static string Format(WishlistItemResult? wishlist, DiscoveryItemResult? discovery, int typeID, InventoryScope scope, long planningRevision, SystemLanguage language)
        {
            if (typeID < 0) return "";
            if (!Matches(wishlist, typeID, scope)) wishlist = null;
            if (discovery?.TypeID != typeID) discovery = null;
            if (wishlist?.Status == WishlistQueryStatus.ContextChanged || discovery?.Status == DiscoveryQueryStatus.ContextChanged) return "";
            var t = SourceProgressText.For(language);
            string Number(long value) => value.ToString(CultureInfo.InvariantCulture);
            var lines = new List<string>(); var flags = new List<string>();
            if (wishlist != null)
            {
                string prefix = t[0] + " (" + t[scope == InventoryScope.Carried ? 1 : 2] + "): ";
                string needs = "";
                if (wishlist.Status == WishlistQueryStatus.NotReady) needs = t[9];
                else if (wishlist.Status == WishlistQueryStatus.Success)
                {
                    if (!wishlist.PinsReady) needs = t[6];
                    else if (wishlist.PlanningRevision != planningRevision) needs = t[9];
                    else if (wishlist.PlanStatus != PlanStatus.Success) needs = t[7];
                    else
                    {
                        if (wishlist.Required > 0) needs = t[3] + " " + Number(wishlist.Required);
                        if (!wishlist.InventoryKnown) needs += (needs.Length > 0 ? " · " : "") + t[5];
                        else if (wishlist.Required > 0) needs += " · " + t[4] + " " + Number(wishlist.Missing);
                    }
                    if (wishlist.UnavailablePinCount > 0) needs += (needs.Length > 0 ? " · " : "") + t[8];
                    if (!wishlist.NativeKnown) flags.Add(t[10] + ": " + t[14]);
                    else
                    {
                        var native = new List<string>();
                        if (wishlist.IsManuallyWishlisted) native.Add(t[11]);
                        if (wishlist.IsQuestRequired) native.Add(t[12]);
                        if (wishlist.IsBuildingRequired) native.Add(t[13]);
                        if (native.Count > 0) flags.Add(t[10] + ": " + string.Join("/", native));
                    }
                }
                if (needs.Length > 0) lines.Add(prefix + needs);
            }
            if (discovery != null)
            {
                string state = discovery.Status switch
                {
                    DiscoveryQueryStatus.NotReady => t[9],
                    DiscoveryQueryStatus.ProfileUnavailable => t[14] + " (" + t[19] + ")",
                    DiscoveryQueryStatus.WriteFailed => t[14] + " (" + t[20] + ")",
                    DiscoveryQueryStatus.Success => discovery.State switch
                    {
                        DiscoveryState.Recorded => t[16], DiscoveryState.Temporary => t[17],
                        DiscoveryState.Undiscovered => t[18], _ => t[14]
                    },
                    _ => ""
                };
                if (state.Length > 0) flags.Add(t[15] + ": " + state);
            }
            if (flags.Count > 0) lines.Add(string.Join(" · ", flags));
            return string.Join("\n", lines);
        }

        public static string FormatPlayer(WishlistItemResult? wishlist, DiscoveryItemResult? discovery, int typeID, InventoryScope scope, long planningRevision, SystemLanguage language)
        {
            if (!Matches(wishlist, typeID, scope) || wishlist!.Status != WishlistQueryStatus.Success ||
                discovery?.Status == DiscoveryQueryStatus.ContextChanged) return "";
            var t = SourceProgressText.For(language);
            var parts = new List<string>();
            if (wishlist.PinsReady && wishlist.PlanStatus == PlanStatus.Success && wishlist.PlanningRevision == planningRevision && wishlist.Required > 0)
            {
                parts.Add(t[3] + " " + wishlist.Required.ToString(CultureInfo.InvariantCulture));
                if (!wishlist.InventoryKnown) parts.Add(t[5]);
                else if (wishlist.Missing > 0) parts.Add(t[4] + " " + wishlist.Missing.ToString(CultureInfo.InvariantCulture));
            }
            if (wishlist.NativeKnown)
            {
                if (wishlist.IsManuallyWishlisted) parts.Add(t[11]);
                if (wishlist.IsQuestRequired) parts.Add(t[12]);
                if (wishlist.IsBuildingRequired) parts.Add(t[13]);
            }
            return string.Join(" · ", parts);
        }
    }
}
