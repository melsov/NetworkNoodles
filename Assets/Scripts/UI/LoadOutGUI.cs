using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


public class LoadOutGUI : MonoBehaviour
{
    [SerializeField]
    RectTransform loGUI;

    [SerializeField]
    InputField nameText;

    [SerializeField]
    Button startButton;

    [SerializeField]
    bool DebugAutoRandomLoadOutData;

    public struct LoadOutData
    {
        public string displayName;
    }

    LoadOutData data;

    Action<LoadOutData> callback;

    private void Awake() {
        loGUI.gameObject.SetActive(false);
    }

    public bool GetLoadOut(Action<LoadOutData> _callback) {
        if(callback != null) {
            return false;
        }
        callback = _callback;
        if(DebugAutoRandomLoadOutData) {
            nameText.text = RandomString(3);
            OnSubmittedName();
            return true;
        }
        loGUI.gameObject.SetActive(true);
        startButton.interactable = isInputGood();
        return true;
    }

    string RandomString(int length) {
        char[] result = new char[length];
        for(int i=0; i<length; ++i) {
            result[i] = (char)UnityEngine.Random.Range(65, 90);
        }
        return new string(result);
    }

    private bool isInputGood() {
        return !string.IsNullOrEmpty(nameText.text);
    }

    public void DoneEditing() {
    }

    public void OnSubmittedName() {
        if(isInputGood()) {
            data.displayName = nameText.text;

            loGUI.gameObject.SetActive(false);
            if(callback != null) {
                callback(data);
                callback = null;
            }
        }
    }

    public void OnInputChanged() {
        startButton.interactable = isInputGood();
    }



}
