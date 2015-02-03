using System;
using System.Collections.Generic;

/// <summary>
/// A generic IComparer, primarily for sorting mesh segments by z value, to avoid overlap and Z issues.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Ferr_LambdaComparer<T> : IComparer<T> {
    private readonly Func<T, T, int> func;
    public Ferr_LambdaComparer(Func<T, T, int> comparerFunc) {
        this.func = comparerFunc;
    }

    public int Compare(T x, T y) {
        return this.func(x, y);
    }
}