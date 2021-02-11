using System.Collections;
using UnityEngine;
using System.IO;
using AsyncTextureImport;

/// <summary>
/// This example uses the async texture importer to import a texture file asynchronously.
/// </summary>
public class TextureLoadTest : MonoBehaviour
{
    private void OnGUI()
    {
        if(GUILayout.Button("Load texture from file."))
        {
            StartCoroutine(ImportTextureFromFile(Path.Combine(Application.streamingAssetsPath, "ghibli.jpg")));
        }
        else if (GUILayout.Button("Load texture from memory."))
        {
            StartCoroutine(ImportTextureFromMemory(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "ghibli.jpg"))));
        }
        else if (GUILayout.Button("Load texture with Texture2D.LoadImage (SLOW - for comparison)."))
        {
            StartCoroutine(ImportTextureUnity(Path.Combine(Application.streamingAssetsPath, "ghibli.jpg")));
        }
    }

    private IEnumerator ImportTextureFromFile(string texPath)
    {
        // Create texture importer
        TextureImporter importer = new TextureImporter();

        // Import texture async
        yield return importer.ImportTexture(texPath, FREE_IMAGE_FORMAT.FIF_JPEG);

        // Fetch the result
        Texture2D tex = importer.texture;
        
        // Create sprite
        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.one * 0.5f, 100.0f, 0, SpriteMeshType.FullRect);

        GameObject obj = new GameObject();
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        yield return null;
    }

    private IEnumerator ImportTextureFromMemory(byte[] bytes)
    {
        // Create texture importer
        TextureImporter importer = new TextureImporter();

        // Import texture async
        yield return importer.ImportTexture(bytes, FREE_IMAGE_FORMAT.FIF_JPEG);

        // Fetch the result
        Texture2D tex = importer.texture;

        // Create sprite
        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.one * 0.5f, 100.0f, 0, SpriteMeshType.FullRect);

        GameObject obj = new GameObject();
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        yield return null;
    }

    private IEnumerator ImportTextureUnity(string texPath)
    {
        // Fetch the result
        byte[] bytes = File.ReadAllBytes(texPath);
        Texture2D tex = new Texture2D(0, 0);
        tex.LoadImage(bytes);

        // Create sprite
        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.one * 0.5f, 100.0f, 0, SpriteMeshType.FullRect);

        GameObject obj = new GameObject();
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        yield return null;
    }
}
