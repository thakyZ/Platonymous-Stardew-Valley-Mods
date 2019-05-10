﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Types;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;

namespace PyTK.Extensions
{

    public static class PyTextures
    {
        /* Basics */

        public static Texture2D clone(this Texture2D t)
        {
            return t.getArea(new Rectangle(0, 0, t.Width, t.Height));
        }

        public static Texture2D loadTextureData(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                var width = reader.ReadInt32();
                var height = reader.ReadInt32();
                var length = reader.ReadInt32();
                var data = new Color[length];

                for (int i = 0; i < data.Length; i++)
                {
                    var r = reader.ReadByte();
                    var g = reader.ReadByte();
                    var b = reader.ReadByte();
                    var a = reader.ReadByte();
                    data[i] = new Color(r, g, b, a);
                }

                var texture = new Texture2D(Game1.graphics.GraphicsDevice, width, height);
                texture.SetData(data, 0, data.Length);
                return texture;
            }
        }

        public static Color clone(this Color t)
        {
            return new Color(t.ToVector4());
        }

        public static int getDistanceTo(this Color current, Color match)
        {
            int redDifference;
            int greenDifference;
            int blueDifference;

            redDifference = current.R - match.R;
            greenDifference = current.G - match.G;
            blueDifference = current.B - match.B;

            return redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference;
        }


        /* Manipulation */

        public static Texture2D getArea(this Texture2D t, Rectangle area)
        {
            Color[] data = new Color[t.Width * t.Height];
            t.GetData(data);
            int w = area.Width;
            int h = area.Height;
            Color[] data2 = new Color[w * h];

            int x2 = area.X;
            int y2 = area.Y;

            for (int x = x2; x < w + x2; x++)
                for (int y = y2; y < h + y2; y++)
                    data2[(y - y2) * w + (x - x2)] = data[y * t.Width + x];

            Texture2D result = new Texture2D(t.GraphicsDevice, w, h);
            result.SetData(data2);
            return result;
        }

        public static Texture2D getTile(this Texture2D t, int index, int tileWidth = 16, int tileHeight = 16)
        {
            if (t == null)
                return null;

            Rectangle sourceRectangle = Game1.getSourceRectForStandardTileSheet(t, index, tileWidth, tileHeight);
            return t.getArea(sourceRectangle);
        }


        public static Texture2D changeColor(this Texture2D t, ColorManipulation manipulation)
        {
            Color[] colorData = new Color[t.Width * t.Height];
            t.GetData(colorData);
            Texture2D newTexture = new Texture2D(t.GraphicsDevice, t.Width, t.Height);

            for (int x = 0; x < t.Width; x++)
                for (int y = 0; y < t.Height; y++)
                    colorData[x * t.Height + y] = changeColor(colorData[x * t.Height + y], manipulation);

            t.SetData(colorData);

            return t;
        }

        public static Texture2D applyPalette(this Texture2D t, List<Color> palette)
        {
            ColorManipulation manipulation = new ColorManipulation(palette);
            return t.changeColor(manipulation);
        }

        public static Texture2D setSaturation(this Texture2D t, float saturation)
        {
            ColorManipulation manipulation = new ColorManipulation(saturation);
            return t.changeColor(manipulation);
        }

        public static Texture2D setLight(this Texture2D t, float light)
        {
            ColorManipulation manipulation = new ColorManipulation(100, light);
            return t.changeColor(manipulation);
        }

        public static Color changeColor(this Color t, ColorManipulation manipulation)
        {
            t = t.setLight(manipulation.light);
            t = t.setSaturation(manipulation.saturation);
            if (manipulation.palette.Count > 0)
                t = t.applyPalette(manipulation.palette);
            return t;
        }

        public static Color multiplyWith(this Color color1, Color color2)
        {
            color1.R = (byte)MathHelper.Min(((color1.R * color2.R) / 255), 255);
            color1.G = (byte)MathHelper.Min(((color1.G * color2.G) / 255), 255);
            color1.B = (byte)MathHelper.Min(((color1.B * color2.B) / 255), 255);
            color1.A = color1.A;

            return color1;
        }

        public static Color setSaturation(this Color t, float saturation, Vector3? saturationMultiplier = null)
        {
            Vector3 m = saturationMultiplier.HasValue ? saturationMultiplier.Value : new Vector3(0.2125f, 0.7154f, 0.0721f);
            float l = m.X * t.R + m.Y * t.G + m.Z * t.B;
            float s = 1f - (saturation / 100);

            float newR = t.R;
            float newG = t.G;
            float newB = t.B;

            if (s != 0)
            {
                newR = newR + s * (l - newR);
                newG = newG + s * (l - newG);
                newB = newB + s * (l - newB);
            }

            t.R = (byte)MathHelper.Min(newR, 255);
            t.G = (byte)MathHelper.Min(newG, 255);
            t.B = (byte)MathHelper.Min(newB, 255);

            return t;
        }

        public static Color setLight(this Color t, float light)
        {
            float l = light / 100;
            t.R = (byte)Math.Min(t.R * l, 255);
            t.G = (byte)Math.Min(t.G * l, 255);
            t.B = (byte)Math.Min(t.B * l, 255);

            return t;
        }

        public static Color applyPalette(this Color current, List<Color> palette)
        {
            int index = -1;
            int shortestDistance = int.MaxValue;

            for (int i = 0; i < palette.Count; i++)
            {
                int distance = current.getDistanceTo(palette[i]);
                if (distance < shortestDistance)
                {
                    index = i;
                    shortestDistance = distance;
                }
            }

            return palette[index];
        }

    }
}

