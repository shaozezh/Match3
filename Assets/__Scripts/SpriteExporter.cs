using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteExporter : MonoBehaviour
{
    [MenuItem("Tools/Export Sprites")]
    public static void ExportSprites()
    {
        string path = "Assets/Window.png"; // Path to your sprite sheet
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        if (texture == null)
        {
            Debug.LogError("Texture not found at path: " + path);
            return;
        }

        if (!texture.isReadable)
        {
            Debug.LogError("Texture is not readable: " + path);
            return;
        }

        string savePath = "Assets/Sprites/";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
        foreach (Object sprite in sprites)
        {
            if (sprite is Sprite)
            {
                Sprite s = sprite as Sprite;
                Texture2D spriteTexture = new Texture2D((int)s.rect.width, (int)s.rect.height);
                Color[] pixels = texture.GetPixels((int)s.rect.x, (int)s.rect.y, (int)s.rect.width, (int)s.rect.height);
                spriteTexture.SetPixels(pixels);
                spriteTexture.Apply();

                byte[] bytes = spriteTexture.EncodeToPNG();
                File.WriteAllBytes(savePath + s.name + ".png", bytes);

                DestroyImmediate(spriteTexture);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Sprites exported to " + savePath);
    }
}
