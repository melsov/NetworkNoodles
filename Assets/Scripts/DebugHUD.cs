using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


public class DebugHUD : MonoBehaviour
{
    private static DebugHUD _debugHUD;

    public static DebugHUD debugHUD {
        get {
            if(!_debugHUD) {
                _debugHUD = FindObjectOfType<DebugHUD>();
            }
            return _debugHUD;
        }
    }

    public static void Debugg(string s) {
        debugHUD.debug(s);
    }

    Text _text;

    Text text {
        get {
            if(!_text) {
                _text = GetComponent<Text>();
            }
            return _text;
        }
    }

    const int maxLines = 12;
    RingBuffer<string> lines = new RingBuffer<string>(maxLines);
    int totalLineCount;

    public void debug(string s) {
        Debug.Log(s);
        lines.push(s);
        totalLineCount++;
        string next = "";
        int i = 0;
        foreach(string ss in lines.getValues()) {
            next = string.Format("{0} \n [{1}] {2}", next, (totalLineCount - i) % 100, ss);
            i++;
        }
        text.text = next;

    }

}
