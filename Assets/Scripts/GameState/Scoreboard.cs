using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Scoreboard : MonoBehaviour
{
    public struct LedgerEntry {
        public MPlayerData playerData;
        public int score;

        public override string ToString() { 
            return string.Format("LE: {0} | score: {1}", playerData.ToString(), score);
        }
    }

    List<LedgerEntry> les = new List<LedgerEntry>();

    [SerializeField]
    RectTransform _displayPanel;
    RectTransform displayPanel {
        get {
            if(!_displayPanel) {
                _displayPanel = GameObject.Find("Scoreboard").GetComponent<RectTransform>();
                if(!_displayPanel) {
                    throw new Exception("Didn't find a 'Scoreboard' rect transform");
                }
            }
            return _displayPanel;
        }
    }

    LedgeRow[] _entries;
    LedgeRow[] entries {
        get {
            if(_entries == null) {
                var result = new List<LedgeRow>();
                foreach(Transform child in displayPanel.transform) {
                    var entry = child.GetComponent<LedgeRow>();
                    if(entry) {
                        result.Add(entry);
                    }
                }
                _entries = result.ToArray();
            }
            return _entries;
        }
    }

    void addOrUpdate(LedgerEntry le) {
        int index = -1;
        for(int i = 0; i < les.Count; ++i) {
            if(les[i].playerData.netID == le.playerData.netID) {
                index = i;
                break;
            }
        }
        if (index >= 0) {
            les[index] = le;
        } else {
            les.Add(le);
        }
    }

    public void UpdateDisplay(LedgerEntry le) {
        addOrUpdate(le);
        //TODO: les should be a dictionary
        les = les.OrderByDescending(a => a.score).ToList();

        for (int i=0; i < entries.Length; ++i) {
            string row = ""; 
            string score = "";
            if (i < les.Count) {
                LedgerEntry l = les[i];
                row = string.Format("({0}) {1}", i, l.playerData.displayName);
                score = string.Format("{0}", l.score);
                entries[i].setColors(l.playerData.color);
            }
            entries[i].left.text = row;
            entries[i].right.text = score;
        }
    }
}
