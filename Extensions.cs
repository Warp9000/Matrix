using System;

namespace Matrix;

public static class Extensions
{
    public static object? Copy(this object obj)
    {
        Type type = obj.GetType();
        var properties = type.GetProperties();
        var newObj = Activator.CreateInstance(type);
        foreach (var property in properties)
        {
            property.SetValue(newObj, property.GetValue(obj));
        }
        return newObj;
    }

    public static T? MemberClone<T>(this T obj)
    {
        return (T?)obj?.Copy();
    }

    public static T LoopedIndex<T>(this T[] array, int index)
    {
        // Calculate the effective index by taking the modulo of the input index with the array length
        int effectiveIndex = index % array.Length;

        // If the effective index is negative, add the array length to it
        if (effectiveIndex < 0)
        {
            effectiveIndex += array.Length;
        }

        return array[effectiveIndex];
    }
}