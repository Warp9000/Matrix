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
        // If the index is negative, loop back to the end of the array
        if (index < 0)
        {
            return array[array.Length + index];
        }
        // If the index is greater than the length of the array, loop back to the start of the array
        if (index >= array.Length)
        {
            return array[index - array.Length];
        }

        return array[index];
    }
}