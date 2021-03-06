﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace CM3D2.YATranslator.Hook
{
    public class TranslationHooks
    {
        private static readonly byte[] EmptyTextureData = new byte[0];

        public static event EventHandler<TextureTranslationEventArgs> ArcTextureLoad;
        public static event EventHandler<TextureTranslationEventArgs> ArcTextureLoaded;
        public static event EventHandler<TextureTranslationEventArgs> AssetTextureLoad;
        public static event EventHandler<GraphicTranslationEventArgs> TranslateGraphic;
        public static event EventHandler<TextureTranslationEventArgs> SpriteLoad;
        public static event EventHandler<StringTranslationEventArgs> TranslateText;
        public static event EventHandler<StringTranslationEventArgs> GetOppositePair;
        public static event EventHandler<StringTranslationEventArgs> GetOriginalText;
        public static event EventHandler<StringTranslationEventArgs> YotogiKagSubtitleCaptured;
        public static event EventHandler<SoundEventArgs> PlaySound;


        public static bool OnGetSeedButton(out Button result, ref Button[] buttons, string str)
        {
            StringTranslationEventArgs args = new StringTranslationEventArgs
            {
                Text = str,
                Type = StringType.Unknown
            };

            GetOppositePair?.Invoke(null, args);

            string other = args.Translation;

            foreach (Button button in buttons)
            {
                Text text = button.GetComponentInChildren<Text>();
                if (text.text != str && (other == null || text.text != other))
                    continue;
                result = button;
                return true;
            }
            result = null;
            return false;
        }

        public static void OnGetSeedType(ref string str)
        {
            StringTranslationEventArgs args = new StringTranslationEventArgs
            {
                Text = str,
                Type = StringType.Unknown
            };

            GetOriginalText?.Invoke(null, args);

            if (!string.IsNullOrEmpty(args.Translation))
                str = args.Translation;
        }

        public static void OnTranslateGraphic(MaskableGraphic graphic)
        {
            GraphicTranslationEventArgs args = new GraphicTranslationEventArgs
            {
                Graphic = graphic
            };

            TranslateGraphic?.Invoke(null, args);
        }

        public static void OnPlaySound(AudioSourceMgr mgr)
        {
            SoundEventArgs args = new SoundEventArgs
            {
                AudioSourceMgr = mgr
            };

            PlaySound?.Invoke(null, args);
        }

        public static void OnTranslateSprite(ref Sprite sprite)
        {
            if (sprite == null)
                return;

            string spriteName = sprite.name;
            bool previouslyTranslated;
            if (string.IsNullOrEmpty(spriteName) || (previouslyTranslated = spriteName.StartsWith("!")))
                return;

            TextureTranslationEventArgs args = new TextureTranslationEventArgs(sprite.name, "SPRITE")
            {
                OriginalTexture = sprite.texture
            };

            SpriteLoad?.Invoke(null, args);

            if (args.Data == null)
                return;

            Sprite newSprite = Sprite.Create(args.Data.CreateTexture2D(), sprite.rect, sprite.pivot);
            newSprite.name = previouslyTranslated ? spriteName : "!" + spriteName;
            sprite = newSprite;
        }

        public static void OnTranslateInfoText(int tag, ref int nightWorkId, ref string info)
        {
            OnTranslateConstText(tag, ref info);
        }

        public static TextureResource OnArcTextureLoaded(TextureResource resource, string name)
        {
            TextureTranslationEventArgs args = new TextureTranslationEventArgs(name, "ARC")
            {
                Data = resource
            };

            ArcTextureLoaded?.Invoke(null, args);

            return resource;
        }

        public static void OnTranslateUiText(int tag, Text uiText, ref string text)
        {
            StringTranslationEventArgs args = new StringTranslationEventArgs
            {
                Text = text,
                TextContainer = uiText,
                Type = (StringType) tag
            };

            TranslateText?.Invoke(null, args);

            if (!string.IsNullOrEmpty(args.Translation))
                text = args.Translation;
        }

        public static void OnTranslateConstText(int tag, ref string text)
        {
            StringTranslationEventArgs args = new StringTranslationEventArgs
            {
                Text = text,
                Type = (StringType) tag
            };

            TranslateText?.Invoke(null, args);

            if (!string.IsNullOrEmpty(args.Translation))
                text = args.Translation;
        }

        public static bool OnArcTextureLoad(out TextureResource result, string name)
        {
            name = name.Replace(".tex", "");

            TextureTranslationEventArgs args = new TextureTranslationEventArgs(name, "ARC");

            ArcTextureLoad?.Invoke(null, args);

            result = args.Data;
            return args.Data != null;
        }

        public static bool OnArcTextureLoadEx(out TextureResource result, AFileSystemBase fileSystem, string name) =>
                OnArcTextureLoad(out result, name);

        public static void OnAssetTextureLoad(int forceTag, UIWidget im)
        {
            bool force = forceTag != 0;
            Texture2D texture2D;
            if ((texture2D = im.material?.mainTexture as Texture2D) == null)
                return;

            string textureName = texture2D.name;
            if (string.IsNullOrEmpty(textureName))
                return;
            if (textureName.StartsWith("!"))
            {
                if (!force)
                    return;
                texture2D.name = texture2D.name.Substring(1);
            }

            TextureTranslationEventArgs args = new TextureTranslationEventArgs(textureName, im.name)
            {
                OriginalTexture = texture2D
            };

            AssetTextureLoad?.Invoke(im, args);

            if (args.Data == null)
                return;

            texture2D.name = "!" + textureName;
            texture2D.LoadImage(EmptyTextureData);
            texture2D.LoadImage(args.Data.data);
        }

        public static void OnTranslateText(int tag, UILabel label, ref string text)
        {
            StringTranslationEventArgs args = new StringTranslationEventArgs
            {
                Text = label.text,
                TextContainer = label,
                Type = (StringType) tag
            };

            TranslateText?.Invoke(null, args);

            if (string.IsNullOrEmpty(args.Translation))
                return;
            text = args.Translation;
            label.useFloatSpacing = false;
            label.spacingX = -1;
        }

        public static void OnYotogiKagHitRet(YotogiKagManager manager)
        {
            StringTranslationEventArgs args = new StringTranslationEventArgs
            {
                Text = manager.kag.GetText()
            };
            
            YotogiKagSubtitleCaptured?.Invoke(null, args);
        }
    }
}