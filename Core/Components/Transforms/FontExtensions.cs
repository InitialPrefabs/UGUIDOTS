using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore;

namespace UGUIDots {

    /// <summary>
    /// Convenient extensions as getters.
    /// </summary>
    public static class FontExtensions {
        public static float BearingY(this in CharacterInfo info, in float baseline) {
            return info.maxY - baseline;
        }

        public static float BearingX(this in CharacterInfo info) {
            return info.minX;
        }

        public static float Height(this in CharacterInfo info) {
            return info.glyphHeight;
        }

        public static float Width(this in CharacterInfo info) {
            return info.glyphWidth;
        }

        public static float2 Min(this in CharacterInfo info) {
            return new float2(info.minX, info.minY);
        }

        public static float2 Max(this in CharacterInfo info) {
            return new float2(info.maxX, info.maxY);
        }

        public static int Advance(this in CharacterInfo info) {
            return info.advance;
        }

        public static FontFaceInfo ToFontFaceInfo(this in FaceInfo info, float2 normalStyle, float2 boldStyle, 
            int2 atlasSize) {

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
                UnderlineOffset        = info.underlineOffset,
                LineHeight             = info.lineHeight,
                NormalStyle            = normalStyle,
                BoldStyle              = boldStyle,
                AtlasSize              = atlasSize
            };
        }
    }
}
