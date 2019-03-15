using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace phirSOFT.SettingsService.IniSettingsService
{
    public interface IIniAdapter
    {
        object DeserializeString(string value, Type type);

        string SerializeObject(object value, Type type);
    }

    public class IniAdapterAttribute : Attribute
    {
        public IniAdapterAttribute(Type supportedType)
        {
            SupportedType = supportedType;
        }

        public Type SupportedType { get; }
    }

    public static class IniAdapterExtensions
    {
        public static IEnumerable<Type> GetSupportesTypes(this IIniAdapter adapter)
        {
            return adapter.GetType().GetTypeInfo().GetCustomAttributes<IniAdapterAttribute>()
                .Select(att => att.SupportedType).Distinct();
        }
    }
}