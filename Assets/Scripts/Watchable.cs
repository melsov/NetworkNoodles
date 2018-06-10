using System;
using System.Collections.Generic;
using UnityEngine;

public interface HasWatchable<T>
{
    Watchable<T> getWatchable();
}

public class Watchable<T>
{
    private T t;
    public T _value {
        get {
            return t;
        }
        set {
            t = value;
            foreach (Action<T> onValueSet in onValueSets) {
                onValueSet.Invoke(t);
            }
        }
    }

    private List<Action<T>> onValueSets = new List<Action<T>>();
    public void subscribe(Action<T> onValueSet) {
        if (!onValueSets.Contains(onValueSet)) {
            onValueSets.Add(onValueSet);
        }
    }
    //public Action<T> onValueSet;

    public Watchable(T t) {
        this.t = t;
    }

    public static implicit operator T(Watchable<T> watchable) { return watchable._value; }

    public static implicit operator bool(Watchable<T> watchable) { return watchable != null; }
}
