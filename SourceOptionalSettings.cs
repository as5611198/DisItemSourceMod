using System;
using System.Linq;
using System.Reflection;
using Duckov.Modding;

namespace DisplayItemSourceMod
{
    // Only the three required public operations are reflected; no mandatory ModSetting assembly reference.
    internal sealed class SourceOptionalSettings
    {
        private readonly Func<ModInfo> info;
        private readonly Func<Type?> find;
        private Type? provider;
        private MethodInfo? slider, toggle, saved, remove;
        public SourceOptionalSettings(Func<ModInfo> info, Func<Type?>? find = null)
        {
            this.info = info;
            this.find = find ?? (() => AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("ModSetting.ModBehaviour")).FirstOrDefault(t => t != null));
        }
        public bool Ready
        {
            get
            {
                try
                {
                    var identity = info();
                    if (string.IsNullOrWhiteSpace(identity.name) || string.IsNullOrWhiteSpace(identity.displayName)) return false;
                    var type = find();
                    if (type == null || !(type.GetProperty("Enable", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) is true) ||
                        !(type.GetProperty("IsReady", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) is true) ||
                        !(type.GetField("VERSION", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) is Version version) || version < new Version(0, 5, 0))
                        return false;
                    if (provider != type)
                    {
                        slider = type.GetMethod("AddSlider", new[] { typeof(ModInfo), typeof(string), typeof(string), typeof(int), typeof(int), typeof(int), typeof(Action<int>), typeof(int) });
                        toggle = type.GetMethod("AddToggle", new[] { typeof(ModInfo), typeof(string), typeof(string), typeof(bool), typeof(Action<bool>) });
                        remove = type.GetMethod("RemoveUI", new[] { typeof(ModInfo), typeof(string), typeof(Action<bool>) });
                        saved = type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(m => m.Name == "GetSavedValue" &&
                            m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 3 &&
                            m.GetParameters()[0].ParameterType == typeof(ModInfo) && m.GetParameters()[1].ParameterType == typeof(string) && m.GetParameters()[2].IsOut);
                        provider = type;
                    }
                    return slider != null && toggle != null && remove != null && saved != null;
                }
                catch { return false; }
            }
        }
        private static bool OwnKey(string key) => key == SourceV3Preferences.FontKey || key == SourceV3Preferences.DetailKey || key == SourceV3Preferences.ModOnlyKey;
        public (bool Found, object? Value) GetSaved(string key, Type valueType)
        {
            if (!OwnKey(key) || !Ready) return (false, null);
            try
            {
                object?[] args = { info(), key, null };
                bool found = saved!.MakeGenericMethod(valueType).Invoke(null, args) is true;
                return (found, found ? args[2] : null);
            }
            catch { return (false, null); } // Malformed/old values retain defaults for this key.
        }
        public bool Add(string key, string description, object value, Action<object> changed)
        {
            if (!OwnKey(key) || !Ready) return false;
            try
            {
                if (key == SourceV3Preferences.FontKey && value is int size)
                    slider!.Invoke(null, new object[] { info(), key, description, size, 14, 32, new Action<int>(v => changed(v)), 5 });
                else if (key != SourceV3Preferences.FontKey && value is bool enabled)
                    toggle!.Invoke(null, new object[] { info(), key, description, enabled, new Action<bool>(v => changed(v)) });
                else return false;
                return true;
            }
            catch { return false; }
        }
        public bool Remove(string key, Action<bool> completed)
        {
            if (!OwnKey(key) || !Ready) return false;
            try { remove!.Invoke(null, new object[] { info(), key, completed }); return true; }
            catch { return false; }
        }
    }
}
