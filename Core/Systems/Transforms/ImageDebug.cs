using UGUIDOTS.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace UGUIDOTS.Render.Systems {

    public class ImageDebug : MonoBehaviour
    {

        public Canvas Canvas;
        public Image Img;

        void OnDrawGizmos() {
            var texture = Img.sprite != null ? Img.sprite.texture : Texture2D.whiteTexture;

            var res = new DefaultSpriteResolution { Value = new int2(texture.width, texture.height) };
            var spriteData = SpriteData.FromSprite(Img.sprite);
            var dimension = new Dimension { Value = Img.rectTransform.Int2Size() };

            var space = new ScreenSpace {
                Scale = (Vector2)(Img.rectTransform.localScale),
                Translation = (Vector2)(Img.rectTransform.position)
            };


            var pos = ImageUtils.CreateImagePositionData(res, spriteData, dimension, space, Canvas.transform.localScale.x);

            var bl = new float3(pos.xy, 0);
            var tl = new float3(pos.xw, 0);
            var tr = new float3(pos.zw, 0);
            var br = new float3(pos.zy, 0);

            Gizmos.color = Color.red;

            Gizmos.DrawLine(bl, tl);
            Gizmos.DrawLine(tl, tr);
            Gizmos.DrawLine(tr, br);
            Gizmos.DrawLine(br, bl);
            // Debug.Log($"BL: {bl}, TL: {tl}, TR: {tr}, BR: {br}");
            Debug.Log($"Debugged: Min Max: {pos} || Pos: {Img.transform.position}");
        }
    }
}
