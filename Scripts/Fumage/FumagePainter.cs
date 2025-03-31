// FumagePainter.cs
// GAM713 Prototype 1
//
// Created by Avi Virendra Parmar
// On 1st February 2025
//


using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FumagePainter : MonoBehaviour
{

    #region Variables
    CanvasGyro canvasGyro;

    [Header("Sequence settings")]
    [SerializeField] private float fumageTimer = 10f;
    [SerializeField] private string nextScene_proceduralTerrainGeneration;

    [Header("Brush Settings")]
    [SerializeField][Range(0f, 0.9f)] private float brushPressure = 0.1f;
    [SerializeField][Range(0.1f, 5f)] private float brushStrokeSpeed = 0.2f;

    [Header("Texture links")]
    [SerializeField] private RenderTexture canvasRendureTexture;
    [SerializeField] private Material canvasMaterial;
    [SerializeField] private Transform target;
    [SerializeField] private Texture2D brushTexture;
    [SerializeField] private string savePNGTexturePath = "/HeightMap.png";
    private Texture2D writableRenderTexture;
    #endregion


    #region Start, Update, CoRoutines
    private void Start() {
        //Create a writable texture based on render texture
        writableRenderTexture = new Texture2D(canvasRendureTexture.width, canvasRendureTexture.height, TextureFormat.RGBA32, true);

        //make canvas black
        ClearSoot();

        //Start Fumage coroutine
        StartCoroutine(FumageCoroutine());
    }

    private void Update() {
    }
    IEnumerator PaintCoroutine(float strokeSpeed) {

        while (true) {
            Debug.Log("called paint coroutine");
            Vector2 textureCoord;

            if (GetTextureCoord(out textureCoord)) {
                Paint(textureCoord);
            }
    
            //Delay to reduce the brush speed
            yield return new WaitForSeconds(strokeSpeed);
        }
    }
    
    IEnumerator FumageCoroutine() {

        Debug.Log("Welcome to fumage");

        yield return new WaitForSeconds(1f);

        Debug.Log("hold your phone in your palm, your phone is now a giant canvas over a candle.");

        yield return new WaitForSeconds(1f);

        Debug.Log("responding to the ambience, move your hand however you please to create a pattern on the canvas");

        yield return new WaitForSeconds(1f);

        StartCoroutine(PaintCoroutine(1/brushStrokeSpeed));

        yield return new WaitForSeconds(fumageTimer);
        StopCoroutine(PaintCoroutine(1/brushStrokeSpeed));

        yield return new WaitForSeconds(2f);
        SaveTextureAsPNG(ConvertRenderTextureToTexture2D(canvasRendureTexture));
        SceneManager.LoadScene(nextScene_proceduralTerrainGeneration);
        

    }
    #endregion

    #region Functions
    void Paint(Vector2 uv) {
        int sootTextureUV_x = (int)(uv.x * writableRenderTexture.width);
        int sootTextureUV_y = (int)(uv.y * writableRenderTexture.height);

        //Brush pressure
        float pressure = brushPressure; // Normalize speed to range 0-1

        // Generate a random rotation angle
        float randomRotationAngle = Random.Range(0f, 360f);

        // Rotate the brush texture
        Color[] rotatedBrushPixels = RotateBrushTexture(brushTexture, randomRotationAngle);

        // Color[] brushPixels = brushTexture.GetPixels();

        //Get Soot texture pixels
        Color[] sootPixels = writableRenderTexture.GetPixels(sootTextureUV_x, sootTextureUV_y, brushTexture.width, brushTexture.height);

        for (int i = 0; i < rotatedBrushPixels.Length; i++) {
            float alpha = rotatedBrushPixels[i].a; // Get Brush Alpha Value

            Color brushPixel = rotatedBrushPixels[i];

            brushPixel.a *= pressure;
            
            sootPixels[i] = Color.Lerp(sootPixels[i], brushPixel, brushPixel.a); // blend using Alpha
        }

        //Apply Blended Pixels
        writableRenderTexture.SetPixels(sootTextureUV_x, sootTextureUV_y, brushTexture.width, brushTexture.height, sootPixels);
        writableRenderTexture.Apply();

        //Update RenderTexture
        Graphics.Blit(writableRenderTexture, canvasRendureTexture);
    }

    Color[] RotateBrushTexture(Texture2D texture, float angle) {
        int width = texture.width;
        int height = texture.height;

        Color[] originalPixels = texture.GetPixels();
        Color[] rotatedPixels = new Color[originalPixels.Length];

        int centerX = width/2;
        int centerY = height/2;

        float radAngle = angle * Mathf.Deg2Rad;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x<width; x++) {
                //Rotate coordinates around the center
                int newX = Mathf.RoundToInt((x - centerX) * Mathf.Cos(radAngle) - (y-centerY) * Mathf.Sin(radAngle) + centerX);
                int newY = Mathf.RoundToInt((x - centerX) * Mathf.Sin(radAngle) + (y - centerY) * Mathf.Cos(radAngle) + centerY);

                //Check if within bounds
                 if (newX >= 0 && newX < width && newY >= 0 && newY < height) {
                    rotatedPixels[y * width + x] = originalPixels[newY * width + newX];
                } 
                else {
                    rotatedPixels[y * width + x] = new Color(0, 0, 0, 0); // Transparent outside bounds
                }
            }
        }

        return rotatedPixels;
    }

    void ClearSoot() {
        Color[] clearPixels = new Color[writableRenderTexture.width * canvasRendureTexture.height];

        for (int i = 0; i < clearPixels.Length; i++) {
            clearPixels[i] = Color.black; // Fully covered in soot initially
        }

        writableRenderTexture.SetPixels(clearPixels);
        writableRenderTexture.Apply();

        Graphics.Blit(writableRenderTexture, canvasRendureTexture);
    }

    bool GetTextureCoord(out Vector2 coord) {

        Ray ray = new Ray(transform.position, Vector3.up);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            coord = hit.textureCoord;
            Debug.Log(hit.textureCoord);
            return true;
        }


        coord = Vector2.zero;
        return false;
    }
    
    Texture2D ConvertRenderTextureToTexture2D(RenderTexture renderTexture) {
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        RenderTexture.active = null;

        return texture;
    }

    void SaveTextureAsPNG(Texture2D texture) {
        string filePath = Application.persistentDataPath + savePNGTexturePath;
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, bytes);
        Debug.Log("Saved heightmap at:" +filePath);
    }
    #endregion
}
