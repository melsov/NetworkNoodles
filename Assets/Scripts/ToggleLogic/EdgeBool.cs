using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public struct EdgeBool
{
    private bool _Value;
    public bool Value {
        get { return _Value; }
        set {
            if(_Value != value) {
                _Value = value;
                eitherEdgeCallback(value);
            }
        }
    }

    public Action<bool> eitherEdgeCallback;


    public static implicit operator bool(EdgeBool eb) { return eb.Value; }
}

