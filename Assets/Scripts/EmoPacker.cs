using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using TMPro;
using System;


public class EmoPacker
{
    static string SOURCE_IMAGE_PATH = "SourceImages/Emoji/";
    static string OUTPUT_ATLAS_PATH = "Assets/FGUI/Resources/Sprites/Emoji";
    static float SCALE = 64f / 512f; //our source assets are 512px

    static int MAX_ATLAS_SIZE = 4096;
    static bool SHOULD_ADD_SUBFOLDERS = true;

    [MenuItem("FutureGrind/Pack Emojis")]
    static public void PackEmojis()
    {
        //first we have to pack all the textures into the atlas (scaling them by the SCALE value)
        //then we have to create the correct sprite importer settings and add the spritesheet slices to that
        //then we have to set up the settings in the TMP_SpriteAsset

        SearchOption searchOption = SHOULD_ADD_SUBFOLDERS ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        string[] filePaths = new List<string>(Directory.GetFiles(SOURCE_IMAGE_PATH, "*.png", searchOption)).FindAll(s => !s.Contains("e1-png")).ToArray(); //don't return files in the e1-png folder!

        var elements = new List<EmoElement>();

        for (int p = 0; p < filePaths.Length; p++)
        {
            var element = new EmoElement();
            elements.Add(element);

            element.path = filePaths[p];
            element.name = Path.GetFileNameWithoutExtension(element.path);

            element.sourceTexture = new Texture2D(0, 0, TextureFormat.ARGB32, false, false);
            element.sourceTexture.LoadImage(File.ReadAllBytes(element.path));
            element.sourceTexture.wrapMode = TextureWrapMode.Clamp; //so we don't get pixels from the other edge when scaling
            element.sourceTexture.filterMode = FilterMode.Bilinear;

            //scale the input texture
            element.outputTexture = ResizeTexture(element.sourceTexture, Mathf.RoundToInt((float)element.sourceTexture.width * SCALE), Mathf.RoundToInt((float)element.sourceTexture.height * SCALE));
        }

        //pack the textures into the atlas

        var textures = elements.ConvertAll<Texture2D>(e => e.outputTexture).ToArray();

        var atlasTexture = new Texture2D(0, 0, TextureFormat.ARGB32, false, false);
        atlasTexture.filterMode = FilterMode.Bilinear;
        var rects = atlasTexture.PackTextures(textures, 2, MAX_ATLAS_SIZE, false);

        float scaleW = (float)atlasTexture.width;
        float scaleH = (float)atlasTexture.height;

        for (int e = 0; e < elements.Count; e++)
        {
            var element = elements[e];
            var rect = rects[e];
            element.rect = rect;

            var pixelRect = new Rect(rect.x * scaleW, rect.y * scaleH, rect.width * scaleW, rect.height * scaleH); //metadata needs pixel rects;

            //https://docs.unity3d.com/ScriptReference/SpriteMetaData.html
            element.meta.name = element.name;
            element.meta.rect = pixelRect;
            element.meta.pivot = new Vector2(0.5f, 0.5f);
            element.meta.border = new Vector4(0, 0, 0, 0);
            element.meta.alignment = 0;

            element.tmpSprite = new TMP_Sprite();
            element.tmpSprite.name = element.name;
            element.tmpSprite.x = pixelRect.x;
            element.tmpSprite.y = pixelRect.y;
            element.tmpSprite.width = pixelRect.width;
            element.tmpSprite.height = pixelRect.height;
            element.tmpSprite.xAdvance = pixelRect.width;
            element.tmpSprite.xOffset = -2f;
            element.tmpSprite.yOffset = pixelRect.height * 0.8f;
            element.tmpSprite.scale = 2.5f;
            element.tmpSprite.id = e;
            element.tmpSprite.hashCode = TMP_TextUtilities.GetSimpleHashCode(element.tmpSprite.name);
        }

        var spriteMetaDatas = elements.ConvertAll<SpriteMetaData>(e => e.meta).ToArray();

        atlasTexture.Apply(false, false);

        File.WriteAllBytes(OUTPUT_ATLAS_PATH + ".png", atlasTexture.EncodeToPNG());

        //set up the sprite importer settings

        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(OUTPUT_ATLAS_PATH + ".png");

        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.textureType = TextureImporterType.Sprite;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Bilinear;
        importer.maxTextureSize = 4096;
        importer.spritesheet = spriteMetaDatas;

        AssetDatabase.ImportAsset(OUTPUT_ATLAS_PATH + ".png", ImportAssetOptions.ForceUpdate);


        //cleanup textures
        foreach (var element in elements)
        {
            Texture2D.DestroyImmediate(element.sourceTexture);
            Texture2D.DestroyImmediate(element.outputTexture);
        }


        //NEXT: add all the sprite metadata to the sprite asset

        var tmpSprites = elements.ConvertAll<TMP_Sprite>(e => e.tmpSprite);

        var spriteAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(OUTPUT_ATLAS_PATH + ".asset");

        Debug.Log("got sprite asset! " + spriteAsset);

        spriteAsset.spriteInfoList.Clear();

        spriteAsset.spriteInfoList.AddRange(tmpSprites);

        //I think this forces TMP reload the sprite asset after we've generated it?
        //... but it has been removed, and I don't think it's needed now that TMP seems to work differently
        //spriteAsset.LoadSprites();

        //adding this so it updates its name and unicode table
        spriteAsset.UpdateLookupTables();


        EditorUtility.SetDirty(spriteAsset);

        AssetDatabase.SaveAssets();

        AssetDatabase.Refresh();
        Debug.Log("Finished packing emojis into " + OUTPUT_ATLAS_PATH + ".png");

    }

    public class EmoElement
    {
        public string path;
        public string name;

        public Texture2D sourceTexture;
        public Texture2D outputTexture;

        public Rect rect;
        public SpriteMetaData meta;
        public TMP_Sprite tmpSprite;
    }

    //see: http://blog.collectivemass.com/2014/03/resizing-textures-in-unity/
    //we kind of do a kernel average pixel thing
    //this still gets really bad alpha premultiplied fringing though :/
    static public Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false, false);
        result.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[targetWidth * targetHeight];

        float smudge = 0.5f / (float)targetWidth;

        for (int p = 0; p < pixels.Length; p++)
        {
            int c = p % targetWidth;
            int r = p / targetWidth;

            float sourceX = (float)c / (float)targetWidth;
            float sourceY = (float)r / (float)targetHeight;

            var sampleWeights = new float[] { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f };
            var samples = new Color[5];

            samples[0] = source.GetPixelBilinear(sourceX, sourceY);
            samples[1] = source.GetPixelBilinear(sourceX - smudge, sourceY);
            samples[2] = source.GetPixelBilinear(sourceX + smudge, sourceY);
            samples[3] = source.GetPixelBilinear(sourceX, sourceY - smudge);
            samples[4] = source.GetPixelBilinear(sourceX, sourceY + smudge);

            var finalColor = new Color(0, 0, 0, 0);

            for (int s = 0; s < samples.Length; s++)
            {
                finalColor += samples[s] * sampleWeights[s];
            }

            pixels[p] = finalColor;
        }

        result.SetPixels(pixels, 0);
        result.Apply();

        return result;
    }

}