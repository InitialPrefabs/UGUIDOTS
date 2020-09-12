using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    public class PerImageConversionSystem : GameObjectConversionSystem {

        private Dictionary<int, Material> uniqueMaterials;

        protected override void OnUpdate() {
            Entities.ForEach((Image image) => {
                // Generate the associated texture and material entities
                var sprite = image.sprite;

                if (sprite != null) {

                }

                // TODO: To support pure index and vertex buffers I need to generate a material that
                // TODO: specifically has the correct vertex/index buffers.
                // TODO: Test out writing a shader that is a complete rewrite of unity's default UI shader.
                var material = image.material;
                if (material != null) {
                } else {
                }
            });
        }
    }
}
