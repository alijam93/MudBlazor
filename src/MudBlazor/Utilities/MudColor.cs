﻿//// Copyright (c) Steven Coco
//// https://stackoverflow.com/questions/4087581/creating-a-c-sharp-color-from-hsl-values/4087601#4087601
//// Stripped and adapted by Meinrad Recheis and Benjamin Kappel for MudBlazor

using System;
using System.Globalization;

namespace MudBlazor.Utilities
{
    public class MudColor : IEquatable<MudColor>
    {
        private const double EPSILON = 0.000000000000001;

        private byte[] _valuesAsByte;

        public string Value { get; init; }

        public byte R => _valuesAsByte[0];
        public byte G => _valuesAsByte[1];
        public byte B => _valuesAsByte[2];
        public byte A => _valuesAsByte[3];

        public double H { get; private set; }
        public double L { get; private set; }
        public double S { get; private set; }

        private void CalcualteHSL()
        {
            var h = 0D;
            var s = 0D;
            double l;

            // normalize red, green, blue values
            var r = R / 255D;
            var g = G / 255D;
            var b = B / 255D;

            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));

            // hue
            if (Math.Abs(max - min) < EPSILON)
                h = 0D; // undefined
            else if ((Math.Abs(max - r) < EPSILON)
                    && (g >= b))
                h = (60D * (g - b)) / (max - min);
            else if ((Math.Abs(max - r) < EPSILON)
                    && (g < b))
                h = ((60D * (g - b)) / (max - min)) + 360D;
            else if (Math.Abs(max - g) < EPSILON)
                h = ((60D * (b - r)) / (max - min)) + 120D;
            else if (Math.Abs(max - b) < EPSILON)
                h = ((60D * (r - g)) / (max - min)) + 240D;

            // luminance
            l = (max + min) / 2D;

            // saturation
            if ((Math.Abs(l) < EPSILON)
                    || (Math.Abs(max - min) < EPSILON))
                s = 0D;
            else if ((0D < l)
                    && (l <= .5D))
                s = (max - min) / (max + min);
            else if (l > .5D)
                s = (max - min) / (2D - (max + min)); //(max-min > 0)?

            H = Math.Max(0D, Math.Min(360D, h));
            S = Math.Max(0D, Math.Min(1D, s));
            L = Math.Max(0D, Math.Min(1D, l));
        }

        private MudColor(byte[] rbga)
        {
            _valuesAsByte = new byte[4];

            _valuesAsByte[0] = rbga[0];
            _valuesAsByte[1] = rbga[1];
            _valuesAsByte[2] = rbga[2];
            _valuesAsByte[3] = rbga[3];

            CalcualteHSL();
            Value = $"#{R:x2}{G:x2}{B:x2}{A:x2}";
        }

        public MudColor(double h, double s, double l, double a)
            : this(h, s, l, (byte)Math.Max(0, Math.Min(255, a * 255.0)))
        {

        }

        public MudColor(double h, double s, double l, int a)
        {
            _valuesAsByte = new byte[4];

            h = Math.Max(0D, Math.Min(360D, h));
            s = Math.Max(0D, Math.Min(1D, s));
            l = Math.Max(0D, Math.Min(1D, l));
            a = Math.Max(0, Math.Min(255, a));

            // achromatic argb (gray scale)
            if (Math.Abs(s) < EPSILON)
            {
                _valuesAsByte[0] = (byte)Math.Max(0, Math.Min(255, Convert.ToInt32(double.Parse($"{l * 255D:0.00}", CultureInfo.InvariantCulture))));
                _valuesAsByte[1] = (byte)Math.Max(0, Math.Min(255, Convert.ToInt32(double.Parse($"{l * 255D:0.00}", CultureInfo.InvariantCulture))));
                _valuesAsByte[2] = (byte)Math.Max(0, Math.Min(255, Convert.ToInt32(double.Parse($"{l * 255D:0.00}", CultureInfo.InvariantCulture))));
                _valuesAsByte[3] = (byte)a;
            }
            else
            {

                var q = l < .5D
                        ? l * (1D + s)
                        : (l + s) - (l * s);
                var p = (2D * l) - q;

                var hk = h / 360D;
                var T = new double[3];
                T[0] = hk + (1D / 3D); // Tr
                T[1] = hk; // Tb
                T[2] = hk - (1D / 3D); // Tg

                for (var i = 0; i < 3; i++)
                {
                    if (T[i] < 0D)
                        T[i] += 1D;
                    if (T[i] > 1D)
                        T[i] -= 1D;

                    if ((T[i] * 6D) < 1D)
                        T[i] = p + ((q - p) * 6D * T[i]);
                    else if ((T[i] * 2D) < 1)
                        T[i] = q;
                    else if ((T[i] * 3D) < 2)
                        T[i] = p + ((q - p) * ((2D / 3D) - T[i]) * 6D);
                    else
                        T[i] = p;
                }

                _valuesAsByte[0] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(T[0] * 255D)));
                _valuesAsByte[1] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(T[1] * 255D)));
                _valuesAsByte[2] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(T[2] * 255D)));
                _valuesAsByte[3] = (byte)a;
            }

            H = h;
            S = s;
            L = l;

            Value = $"#{R:x2}{G:x2}{B:x2}{A:x2}";
        }

        public MudColor SetH(double h) => new(h, S, L, A);
        public MudColor SetS(double s) => new(H, s, L, A);
        public MudColor SetL(double l) => new(H, S, l, A);

        public MudColor SetR(int r) => new(r, G, B, A);
        public MudColor SetG(int g) => new(R, g, B, A);
        public MudColor SetB(int b) => new(R, G, b, A);

        public MudColor SetAlpha(int a) => new(R, G, B, a);
        public MudColor SetAlpha(double a) => new(R, G, B, a);

        public MudColor(byte r, byte g, byte b, byte a)
        {
            _valuesAsByte = new byte[4];

            _valuesAsByte[0] = r;
            _valuesAsByte[1] = g;
            _valuesAsByte[2] = b;
            _valuesAsByte[3] = a;

            CalcualteHSL();
            Value = $"#{R:x2}{G:x2}{B:x2}{A:x2}";
        }

        public MudColor(int r, int g, int b, double alpha) :
         this(r, g, b, (byte)Math.Max(0, Math.Min(255, alpha * 255.0)))
        {

        }

        public MudColor(int r, int g, int b, int alpha) :
            this((byte)Math.Max(0, Math.Min(255, r)), (byte)Math.Max(0, Math.Min(255, g)), (byte)Math.Max(0, Math.Min(255, b)), (byte)Math.Max(0, Math.Min(255, alpha)))
        {

        }

        public MudColor(MudColor value, double alpha) : this((string)value, alpha)
        {
        }

        public MudColor(string value, double alpha) : this(value)
        {
            _valuesAsByte[3] = (byte)Math.Max(0, Math.Min(255, 255 * alpha));
        }

        public MudColor(string value)
        {
            value = value.Trim().ToLower();

            if (value.StartsWith("rgba") == true)
            {
                string[] parts = SplitInputIntoParts(value);
                if (parts.Length != 4)
                {
                    throw new ArgumentException("invalid color format");
                }

                _valuesAsByte = new byte[]
                {
                    byte.Parse(parts[0],CultureInfo.InvariantCulture),
                    byte.Parse(parts[1],CultureInfo.InvariantCulture),
                    byte.Parse(parts[2],CultureInfo.InvariantCulture),
                    (byte)Math.Max(0, Math.Min(255, 255 * double.Parse(parts[3],CultureInfo.InvariantCulture))),
                };

                Value = $"#{R:x2}{G:x2}{B:x2}{A:x2}";
            }
            else if (value.StartsWith("rgb") == true)
            {
                string[] parts = SplitInputIntoParts(value);
                if (parts.Length != 3)
                {
                    throw new ArgumentException("invalid color format");
                }
                _valuesAsByte = new byte[]
                {
                    byte.Parse(parts[0],CultureInfo.InvariantCulture),
                    byte.Parse(parts[1],CultureInfo.InvariantCulture),
                    byte.Parse(parts[2],CultureInfo.InvariantCulture),
                    255
                };

                Value = $"#{R:x2}{G:x2}{B:x2}{A:x2}";
            }
            else
            {

                if (value.StartsWith("#"))
                {
                    value = value.Substring(1);
                }

                switch (value.Length)
                {
                    case 3:
                        value = new string(new char[8] { value[0], value[0], value[1], value[1], value[2], value[2], 'F', 'F' });
                        break;
                    case 4:
                        value = new string(new char[8] { value[0], value[0], value[1], value[1], value[2], value[2], value[3], value[3] });
                        break;
                    case 6:
                        value += "FF";
                        break;
                    case 8:
                        break;
                    default:
                        throw new ArgumentException("not a valid color", nameof(value));
                }

                Value = value;

                _valuesAsByte = new byte[]
                {
                    GetByteFromValuePart(0),
                    GetByteFromValuePart(2),
                    GetByteFromValuePart(4),
                    GetByteFromValuePart(6),
                };

                Value = '#' + value;
                CalcualteHSL();
            }
        }

        private static string[] SplitInputIntoParts(string value)
        {
            int startIndex = value.IndexOf('(');
            int lastIndex = value.LastIndexOf(')');
            string subString = value[(startIndex + 1)..lastIndex];
            string[] parts = subString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }
            return parts;
        }

        private byte GetByteFromValuePart(int index) => byte.Parse(new string(new char[] { Value[index], Value[index + 1] }), NumberStyles.HexNumber);

        public static implicit operator MudColor(string input) => new MudColor(input);
        public static explicit operator string(MudColor color) => color == null ? string.Empty : color.Value;

        public override string ToString() => Value;
        public string ToRGB() => $"rgb({R},{G},{B})";
        public string ToRGBA() => $"rgba({R},{G},{B}, {(A / 255.0).ToString(CultureInfo.InvariantCulture)})";
        public string ToColorRgbElements() => $"{R},{G},{B}";

        public MudColor ChangeLightness(double amount) => new(H, S, Math.Max(0, Math.Min(1, L + amount)), A);

        public MudColor ColorLighten(double amount) => ChangeLightness(+amount);
        public MudColor ColorDarken(double amount) => ChangeLightness(-amount);

        public MudColor ColorRgbDarken() => ColorDarken(0.075);
        public MudColor ColorRgbLighten() => ColorLighten(0.075);

        public override bool Equals(object obj) => obj is MudColor color && Equals(color);

        public bool Equals(MudColor other)
        {
            if (object.ReferenceEquals(other, null) == true) { return false; }

            return
                _valuesAsByte[0] == other._valuesAsByte[0] &&
                _valuesAsByte[1] == other._valuesAsByte[1] &&
                _valuesAsByte[2] == other._valuesAsByte[2] &&
                _valuesAsByte[3] == other._valuesAsByte[3];
        }

        public override int GetHashCode() => _valuesAsByte[1] + _valuesAsByte[2] + _valuesAsByte[3] + _valuesAsByte[4];

        public static bool operator ==(MudColor lhs, MudColor rhs) => (!object.ReferenceEquals(null, lhs) && lhs.Equals(rhs));
        public static bool operator !=(MudColor lhs, MudColor rhs) => !(lhs == rhs);
    }
}