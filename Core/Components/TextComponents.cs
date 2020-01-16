using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.TextCore;

namespace UGUIDots {

    // TODO: Represent chars as ushort instead?
    public struct TextElement : IBufferElementData {
        public char Value;

        public static implicit operator TextElement(char value) => new TextElement { Value = value };
        public static implicit operator char(TextElement value) => value.Value;
    }

    public struct GlyphElement : IBufferElementData {
        public ushort Char;

        public float Advance;
        public float2 Bearings;
        public float2 Size;

        /// <summary>
        /// Should be considered read only...use the extension functions to grab the UV coords
        /// </summary>
        public float2x4 UV;
    }

    public static class GlyphExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 UVBottomLeft(this in GlyphElement element) {
            return element.UV.c0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 UVBottomRight(this in GlyphElement element) {
            return element.UV.c3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 UVTopLeft(this in GlyphElement element) {
            return element.UV.c1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 UVTopRight(this in GlyphElement element) {
            return element.UV.c2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetGlyphOf(this in DynamicBuffer<GlyphElement> glyphs, in char c, out GlyphElement glyph) {
            var glyphArray = glyphs.AsNativeArray();
            return GetGlyphOf(in glyphArray, in c, out glyph);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetGlyphOf(this in NativeArray<GlyphElement> glyphs, in char c, out GlyphElement glyph) {
            for (int i = 0; i < glyphs.Length; i++) {
                var current = glyphs[i];

                if (current.Char == (ushort)c) {
                    glyph = current;
                    return true;
                }
            }

            glyph = default;
            return false;
        }
    }

    public struct FontFaceInfo : IComponentData {

        public int DefaultFontSize;
        public float AscentLine;
        public float BaseLine;
        public float CapLine;
        public float DescentLine;
        public FixedString32 FamilyName;
        public float LineHeight;
        public float MeanLine;
        public float PointSize;
        public float Scale;
        public float StrikeThroughOffset;
        public float StrikeThroughThickness;
        public float SubscriptSize;
        public float SubscriptOffset;
        public float SuperscriptSize;
        public float SuperscriptOffset;
        public float TabWidth;
        public float UnderlineOffset;
        public float UnderlineThickness;

        public static implicit operator FontFaceInfo(in FaceInfo info) {
            return new FontFaceInfo {
                AscentLine             = info.ascentLine,
                BaseLine               = info.baseline,
                CapLine                = info.capLine,
                DescentLine            = info.descentLine,
                FamilyName             = info.familyName,
                MeanLine               = info.meanLine,
                PointSize              = info.pointSize,
                Scale                  = info.scale,
                StrikeThroughThickness = info.strikethroughThickness,
                StrikeThroughOffset    = info.strikethroughThickness,
                SubscriptSize          = info.subscriptSize,
                SubscriptOffset        = info.subscriptOffset,
                SuperscriptSize        = info.superscriptSize,
                SuperscriptOffset      = info.superscriptOffset,
                TabWidth               = info.tabWidth,
                UnderlineOffset        = info.underlineOffset
            };
        }
    }
}
