using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Tankito.Utils
{
    public class CustomOpenLink : MonoBehaviour
    {
        public string url;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void openWindow(string url);
#else
        private static void openWindow(string url) { Debug.Log(string.Format("openWindow:{0}", url)); }
#endif

        //private void Awake()
        //{
        //    GetComponent<Button>().onClick.AddListener(() => openWindow(url));
        //}

        public void OpenWindowButton()
        {
            openWindow(url);
        }
    }
}

