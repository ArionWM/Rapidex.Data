using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;

internal static class ImplementationMappingHelper
{
    private static bool IsNullableProperty(PropertyInfo property)
    {
        if (!property.PropertyType.IsValueType)
            return true; // Reference types are nullable

        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
            return true; // Nullable value types

        return false; // Non-nullable value types
    }

    public static void MergeListTo(IImplementHost host, IImplementer parentImplementer, IList list, ref IList targetList)
    {
        foreach (var impElement in list)
        {
            if (impElement == null)
                continue;
            object targetElement = null;
            IImplementer implementer = impElement as IImplementer;

            if (implementer == null)
            {
                targetElement = impElement;
            }
            else
            {
                object target = null;
                implementer.Implement(host, parentImplementer, ref target);

                if (target == null)
                    continue;

                targetElement = target;
            }

            targetList.Add(targetElement);
        }

    }

    public static void MergeListToFromParent(IImplementHost host, IImplementer impParent, PropertyInfo prop, ref object targetParent)
    {
        IList listData = (IList)prop.GetValue(impParent);
        if (listData.IsNullOrEmpty())
            return;

        Type targetPType = targetParent.GetType();
        var trgProp = targetPType.GetProperty(prop.Name, BindingFlags.Instance | BindingFlags.Public);
        if (trgProp == null)
            return;

        var listFromTarget = (IList)(trgProp.GetValue(targetParent) ?? TypeHelper.CreateInstance(trgProp.PropertyType));

        MergeListTo(host, impParent, listData, ref listFromTarget);

        if (trgProp.SetMethod != null)
            trgProp.SetValue(targetParent, listFromTarget);
    }

    public static void MergeTo(this IImplementer imp, IImplementHost host, object target)
    {
        //TODO: Typemap ve daha yüksek performans
        if (imp == null)
            return;

        Type targetType = target.GetType();

        var props = imp.GetType().GetProperties();
        foreach (var impProp in props)
        {
            if (impProp.PropertyType.IsSupportTo<IList>())
            {
                MergeListToFromParent(host, imp, impProp, ref target);
                continue;
            }

            if (!IsNullableProperty(impProp))
                continue;

            var trgProp = targetType.GetProperty(impProp.Name, BindingFlags.Instance | BindingFlags.Public);
            if (trgProp == null)
                continue;

            if (!trgProp.CanWrite)
                continue;

            var trgValue = trgProp.GetValue(target);

            var impPropValue = impProp.GetValue(imp);
            if (impPropValue == null)
                continue;

            if (impProp.PropertyType.IsSupportTo<IImplementer>())
            {
                IImplementer impPropImp = (IImplementer)impPropValue;

                //DataListImplementer? Kendisi IImplementer, karşılığı ise IList<Entity?>
                //IImplementTarget impPropTarget = (IImplementTarget)trgValue;
                impPropImp.Implement(host, imp, ref trgValue);
                trgProp.SetValue(target, trgValue);
            }
            else
            {
                Type propUndType = Nullable.GetUnderlyingType(impProp.PropertyType);
                if (trgProp.PropertyType != impProp.PropertyType && trgProp.PropertyType != propUndType)
                    continue;

                trgProp.SetValue(target, impPropValue);
            }
        }

    }
}
