using Unity.Mathematics;
using UnityEngine.TextCore;

namespace UGUIDOTS {

    /// <summary>
    /// Convenient extensions as getters.
    /// </summary>
    public static class FontExtensions {

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
