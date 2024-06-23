using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostEffectScript : MonoBehaviour
{
    public Material mat;
    void OnRenderImage( RenderTexture src, RenderTexture dest)
    {
        // src is the full rendered scene that you would normally
        // send directly to the monitor. We are intercepting this
        // so that we can do a bit more work, before passing it on.

        Graphics.Blit(src, dest, mat);
    }
}
