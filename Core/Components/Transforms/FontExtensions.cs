using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots {

    /// <summary>
    /// Convenient extensions as getters.
    /// </summary>
    public static class FontExtensions {
        public static float BearingY(this ref CharacterInfo info, in float baseline) {
            return info.maxY - baseline;
        }

        public static float BearingX(this ref CharacterInfo info) {
            return info.minX;
        }

        public static float Height(this ref CharacterInfo info) {
            return info.glyphHeight;
        }

        public static float Width(this ref CharacterInfo info) {
            return info.glyphWidth;
        }

        public static float2 Min(this ref CharacterInfo info) {
            return new float2(info.minX, info.minY);
        }

        public static float2 Max(this ref CharacterInfo info) {
            return new float2(info.maxX, info.maxY);
        }

        public static int Advance(this ref CharacterInfo info) {
            return info.advance;
        }
    }
}
