using Unity.Entities;
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
        public float Scale;
        public GlyphRect GlyphRect;
        public GlyphMetrics Metrics;
    }
}
