using UnityEngine;
using UnityEngine.U2D;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class OldTelevision : MonoBehaviour
{
    public Shader shader;
    public bool pixelize = true;

    [Header("___________________ Shader Properties ___________________")]
    [Range(0, 20)] public int density = 17;
    [Range(0, 0.5f)] public float distortion = 0.02f;
    [Range(1, 4)] public float brightness = 3f;
    [Range(0, 32)] public float interval = 0.6f;
    [Range(-1, 1)] public float speed = -0.02f;
    public Vector4 RGBOffset = (Vector3.right + Vector3.back) * 0.003f;

    [Header("___________________________ Warp ___________________________")]
    public bool warp = true;
    public bool warpVertical = false;
    public Texture2D[] warpTexOne;
    public Texture2D[] warpTexTwo;

    private Material Mat
    {
        get
        {
            if (null == mat)
            {
                mat = new Material(shader);
                TempWarning.InvokeDebug(() =>
                {
                    propertyIDs = new int[11];
                    propertyIDs[0] = Shader.PropertyToID("_Distortion");
                    propertyIDs[1] = Shader.PropertyToID("_Brightness");
                    propertyIDs[2] = Shader.PropertyToID("_Density");
                    propertyIDs[3] = Shader.PropertyToID("_Interval");
                    propertyIDs[4] = Shader.PropertyToID("_Speed");
                    propertyIDs[5] = Shader.PropertyToID("_RGBOffset");
                    propertyIDs[6] = Shader.PropertyToID("_ScreenHeight");
                    propertyIDs[7] = Shader.PropertyToID("_WarpTexH");
                    propertyIDs[8] = Shader.PropertyToID("_WarpIntenH");
                    propertyIDs[9] = Shader.PropertyToID("_WarpTexV");
                    propertyIDs[10] = Shader.PropertyToID("_WarpIntenV");
                });
#if !DEBUG
                mat.SetFloat("_Distortion", distortion);
                mat.SetFloat("_Brightness", brightness);
                mat.SetFloat("_Density", density);
                mat.SetFloat("_Interval", interval);
                mat.SetFloat("_Speed", speed);
                mat.SetVector("_RGBOffset", RGBOffset);
                propertyIDs = new int[5];
                propertyIDs[0] = Shader.PropertyToID("_ScreenHeight");
                propertyIDs[1] = Shader.PropertyToID("_WarpTexH");
                propertyIDs[2] = Shader.PropertyToID("_WarpIntenH");
                propertyIDs[3] = Shader.PropertyToID("_WarpTexV");
                propertyIDs[4] = Shader.PropertyToID("_WarpIntenV");
#endif
                //pixelCam = GetComponent<PixelPerfectCamera>();
                Distortion = distortion;
            }
            return mat;
        }
    }

    static public float stabilityOne = 6f;
    static public float stabilityTwo = 50f;
    static public float Distortion;
    static private int[] propertyIDs;
    static public Material mat = null; //TODO: temp code
    //static private Material mat = null;
    //static private PixelPerfectCamera pixelCam = null;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
#if !DEBUG
        SetMatProperties();
#endif
        TempWarning.InvokeDebug(() =>
        {
            Mat.SetFloat(propertyIDs[0], distortion);
            Mat.SetFloat(propertyIDs[1], brightness);
            Mat.SetFloat(propertyIDs[2], density);
            Mat.SetFloat(propertyIDs[3], interval);
            Mat.SetFloat(propertyIDs[4], speed);
            Mat.SetVector(propertyIDs[5], RGBOffset);
            Mat.SetFloat(propertyIDs[6], Screen.height);
            
            if (warp)
            {
                float w = Random.Range(0f, stabilityOne);
                if (w >= 0.02f && w <= 0.06f)
                {
                    Mat.SetTexture(propertyIDs[7], warpTexOne[Random.Range(0, warpTexOne.Length)]);
                    Mat.SetFloat(propertyIDs[8], w);
                    w = Random.Range(0f, stabilityTwo);
                    if (warpVertical && w >= 0.38f && w <= 1f)
                    {
                        Mat.SetTexture(propertyIDs[9], warpTexTwo[Random.Range(0, warpTexTwo.Length)]);
                        Mat.SetFloat(propertyIDs[10], w);
                    }
                    else
                        Mat.SetFloat(propertyIDs[10], 0);
                }
                else
                    Mat.SetFloat(propertyIDs[8], 0);
            }
            else
            {
                Mat.SetFloat(propertyIDs[8], 0);
                Mat.SetFloat(propertyIDs[10], 0);
            }
/*#if DEBUG
            pixelCam.runInEditMode = true;
#endif*/
        });

        //pixelCam.enabled = pixelize;
        Graphics.Blit(src, dest, Mat);
    }

    private void SetMatProperties()
    {
        Mat.SetFloat(propertyIDs[0], Screen.height);

        float w = Random.Range(0f, stabilityOne);
        if (w >= 0.02f && w <= 0.06f)
        {
            Mat.SetTexture(propertyIDs[1], warpTexOne[Random.Range(0, warpTexOne.Length)]);
            Mat.SetFloat(propertyIDs[2], w);
            w = Random.Range(0f, stabilityTwo);
            if (warpVertical && w >= 0.38f && w <= 1f)
            {
                Mat.SetTexture(propertyIDs[3], warpTexTwo[Random.Range(0, warpTexTwo.Length)]);
                Mat.SetFloat(propertyIDs[4], w);
            }
            else
                Mat.SetFloat(propertyIDs[4], 0);
        }
        else
            Mat.SetFloat(propertyIDs[2], 0);
    }
}