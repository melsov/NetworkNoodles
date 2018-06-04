using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class MColorSets : MonoBehaviour
{
    [SerializeField]
    List<Color> playerColors;

    int lastClaimed;

    private void Awake() {
        lastClaimed = UnityEngine.Random.Range(0, 20);
    }

    public Color nextPlayerColor() {
        Color result = playerColors[lastClaimed % playerColors.Count];
        lastClaimed = (lastClaimed + 1) % playerColors.Count;
        return result;

    }
}
