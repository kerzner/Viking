﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;
using System.Threading.Tasks;

namespace VikingXNAGraphics
{
    public enum LineStyle
    {
        Standard,
        AlphaGradient,
        NoBlur,
        AnimatedLinear,
        AnimatedBidirectional,
        AnimatedRadial,
        Ladder,
        Tubular,
        HalfTube,
        Glow,
        Textured
    }

    public static class FloatExtensions
    {
        /// <summary>
        /// Return the power of two less than or equal to the passed value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double FloorToPowerOfTwo(this double value)
        {
            return Math.Pow(Math.Floor(Math.Log(value, 2)), 2);
        }
    }

    public static class MouseButtonExtensions
    {
        public static bool Left(this System.Windows.Forms.MouseButtons buttons)
        {
            return (int)(buttons & System.Windows.Forms.MouseButtons.Left) == (int)System.Windows.Forms.MouseButtons.Left;
        }

        public static bool LeftOnly(this System.Windows.Forms.MouseButtons buttons)
        {
            return buttons == System.Windows.Forms.MouseButtons.Left;
        }

        public static bool Right(this System.Windows.Forms.MouseButtons buttons)
        {
            return (int)(buttons & System.Windows.Forms.MouseButtons.Right) == (int)System.Windows.Forms.MouseButtons.Right;
        }

        public static bool RightOnly(this System.Windows.Forms.MouseButtons buttons)
        {
            return buttons == System.Windows.Forms.MouseButtons.Right;
        }

        public static bool Middle(this System.Windows.Forms.MouseButtons buttons)
        {
            return (int)(buttons & System.Windows.Forms.MouseButtons.Middle) == (int)System.Windows.Forms.MouseButtons.Middle;
        }

        public static bool MiddleOnly(this System.Windows.Forms.MouseButtons buttons)
        {
            return buttons == System.Windows.Forms.MouseButtons.Middle;
        }

        public static bool X1(this System.Windows.Forms.MouseButtons buttons)
        {
            return (int)(buttons & System.Windows.Forms.MouseButtons.XButton1) == (int)System.Windows.Forms.MouseButtons.XButton1;
        }

        public static bool X1Only(this System.Windows.Forms.MouseButtons buttons)
        {
            return buttons == System.Windows.Forms.MouseButtons.XButton1;
        }

        public static bool X2(this System.Windows.Forms.MouseButtons buttons)
        {
            return (int)(buttons & System.Windows.Forms.MouseButtons.XButton2) == (int)System.Windows.Forms.MouseButtons.XButton2;
        }

        public static bool X2Only(this System.Windows.Forms.MouseButtons buttons)
        {
            return buttons == System.Windows.Forms.MouseButtons.XButton2;
        }

        public static bool None(this System.Windows.Forms.MouseButtons buttons)
        {
            return buttons == System.Windows.Forms.MouseButtons.None;
        }
    }

    public static class LineManagerExtensions
    {
        public static string ToString(this LineStyle style)
        {
            switch (style)
            {
                case LineStyle.Standard:
                    return "Standard";
                case LineStyle.AlphaGradient:
                    return "AlphaGradient";
                case LineStyle.NoBlur:
                    return "NoBlur";
                case LineStyle.AnimatedLinear:
                    return "AnimatedLinear";
                case LineStyle.AnimatedBidirectional:
                    return "AnimatedBidirectional";
                case LineStyle.AnimatedRadial:
                    return "AnimatedRadial";
                case LineStyle.Ladder:
                    return "Ladder";
                case LineStyle.Tubular:
                    return "Tubular";
                case LineStyle.HalfTube:
                    return "HalfTube";
                case LineStyle.Glow:
                    return "Glow";
                case LineStyle.Textured:
                    return "Textured";
                default:
                    throw new ArgumentException("Unknown line style " + style.ToString());
            }
        }
    }

    public static class VectorExtensions
    {
        public static Microsoft.Xna.Framework.Vector2 ToVector2(this Geometry.GridVector2 vec)
        {
            return new Vector2((float)vec.X, (float)vec.Y);
        }

        public static Geometry.GridVector2 ToGridVector(this Vector2 vec)
        {
            return new Geometry.GridVector2(vec.X, vec.Y);
        }
    }

    public static class ColorExtensions
    {
        /// <summary>
        /// Return alpha 0 to 1
        /// </summary>
        /// <param name="color"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static float GetAlpha(this Microsoft.Xna.Framework.Color color)
        {
            return (float)color.A / 255.0f;
        }

        public static Microsoft.Xna.Framework.Color SetAlpha(this Microsoft.Xna.Framework.Color color, float alpha)
        {
            if (alpha > 1.0f || alpha < 0f)
            {
                throw new ArgumentOutOfRangeException("Alpha value must be between 0 to 1.0");
            }

            Vector3 colorVector = color.ToVector3();
            return new Microsoft.Xna.Framework.Color(colorVector.X, colorVector.Y, colorVector.Z, alpha);
        }

        public static System.Drawing.Color SetAlpha(this System.Drawing.Color WinColor, float alpha)
        {
            return System.Drawing.Color.FromArgb((int)(alpha * 255.0), WinColor);
        }

        public static Microsoft.Xna.Framework.Color ToXNAColor(this System.Drawing.Color color)
        {
            return new Microsoft.Xna.Framework.Color((int)color.R,
                                                    (int)color.G,
                                                    (int)color.B,
                                                    (int)color.A);
        }

        public static Microsoft.Xna.Framework.Color ToXNAColor(this System.Drawing.Color color, float alpha)
        {
            return new Microsoft.Xna.Framework.Color((int)color.R,
                                                    (int)color.G,
                                                    (int)color.B,
                                                    (int)(alpha * 255.0f));
        }

        public static Microsoft.Xna.Framework.Color ToXNAColor(this int color, float alpha)
        {
            System.Drawing.Color sysColor = System.Drawing.Color.FromArgb(color);
            return sysColor.ToXNAColor(alpha);
        }

        public static Microsoft.Xna.Framework.Color ToXNAColor(this int color)
        {
            System.Drawing.Color sysColor = System.Drawing.Color.FromArgb(color);
            return sysColor.ToXNAColor();
        }

        public static Microsoft.Xna.Framework.Color ConvertToHSL(this Microsoft.Xna.Framework.Color color, float alpha)
        {
            System.Drawing.Color WinColor = System.Drawing.Color.FromArgb(color.R, color.G, color.B);

            Microsoft.Xna.Framework.Color HSLColor = new Microsoft.Xna.Framework.Color();
            HSLColor.R = (byte)(255.0 * (WinColor.GetHue() / 360.0));
            HSLColor.G = (byte)(255.0 * WinColor.GetSaturation());
            HSLColor.B = (byte)((color.R * 0.3) + (color.G * 0.59) + (color.B * 0.11));
            HSLColor.A = (byte)(alpha * 255f);

            return HSLColor;
        }

        public static Microsoft.Xna.Framework.Color ConvertToHSL(this Microsoft.Xna.Framework.Color color)
        {
            return color.ConvertToHSL((float)color.A / 255f);
        }

        public static Microsoft.Xna.Framework.Color ConvertToHSL(this System.Drawing.Color WinColor, float alpha)
        {
            Microsoft.Xna.Framework.Color HSLColor = new Microsoft.Xna.Framework.Color();
            HSLColor.R = (byte)(255.0 * (WinColor.GetHue() / 360.0));
            HSLColor.G = (byte)(255.0 * WinColor.GetSaturation());
            HSLColor.B = (byte)(((float)WinColor.R * 0.3) + ((float)WinColor.G * 0.59) + ((float)WinColor.B * 0.11));
            HSLColor.A = (byte)(alpha * 255f);

            return HSLColor;
        }

        public static Microsoft.Xna.Framework.Color ConvertToHSL(this System.Drawing.Color WinColor)
        {
            return WinColor.ConvertToHSL((float)WinColor.A / 255f);
        }

        public static Vector2[] MeasureStrings(this SpriteFont font, string[] lines)
        {
            return lines.Select(line => font.MeasureString(line)).ToArray();
        }
    }

    public static class BasicEffectExtensions
    { 
        public static void SetScene(this BasicEffect basicEffect, VikingXNA.Scene scene)
        {
            basicEffect.Projection = scene.Projection;
            basicEffect.View = scene.Camera.View;
            basicEffect.World = scene.World;
        }
    }

    public static class RenderTarget2DExtensions
    {
        public static byte[] GetData(this RenderTarget2D renderTarget)
        {
            Microsoft.Xna.Framework.Graphics.PackedVector.Byte4[] Data = new Microsoft.Xna.Framework.Graphics.PackedVector.Byte4[renderTarget.Bounds.Width * renderTarget.Bounds.Height];

            renderTarget.GetData<Microsoft.Xna.Framework.Graphics.PackedVector.Byte4>(Data);

            byte[] byteArray = new Byte[Data.Length * 4];
            int iByte = 0;
            for (int iData = 0; iData < Data.Length; iData++, iByte += 4)
            {
                byteArray[iByte] = (Byte)(Data[iData].PackedValue >> 16);
                byteArray[iByte + 1] = (Byte)(Data[iData].PackedValue >> 8);
                byteArray[iByte + 2] = (Byte)(Data[iData].PackedValue >> 0);
                byteArray[iByte + 3] = (Byte)(Data[iData].PackedValue >> 24);
            }

            return byteArray;
        }
    }
}

