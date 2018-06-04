using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mel.Cameras
{
    [ExecuteInEditMode]
    public class BlitScreen : MonoBehaviour
    {
        [SerializeField]
        Material mat;

        private void OnRenderImage(RenderTexture source, RenderTexture destination) {
            Graphics.Blit(source, destination, mat);
        }

    }
}
