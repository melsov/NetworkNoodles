using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LedgeRow : MonoBehaviour
{
    Text _left, _right;

    public Text left {
        get {
            if(!_left) {
                _left = transform.Find("left").GetComponent<Text>();
            }
            return _left;
        }
    }

    public Text right {
        get {
            if(!_right) {
                _right = transform.Find("right").GetComponent<Text>();
            }
            return _right;
        }
    }

    internal void setColors(Color color) {
        left.color = color;
        right.color = color;
    }
}
