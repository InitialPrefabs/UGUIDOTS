using Unity.Entities;
using Unity.Mathematics;

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

        // Should be considered read only...use the extension functions to grab the UV coords
        public float2x4 UV;
    }

    public static class GlyphExtensions {
        public static float2 UVBottomLeft(this ref GlyphElement element) {
            return element.UV.c0;
        }

        public static float2 UVBottomRight(this ref GlyphElement element) {
            return element.UV.c3;
        }

        public static float2 UVTopLeft(this ref GlyphElement element) {
            return element.UV.c1;
        }

        public static float2 UVTopRight(this ref GlyphElement element) {
            return element.UV.c2;
        }
    }

    public struct FontInfo : IComponentData {
        public int DefaultFontSize;
        public float BaseLine;
        public float LineHeight;
    }
}
