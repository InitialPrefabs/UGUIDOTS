using UnityEngine;
using Unity.Entities;
using System;

    public struct TextureWrapper {
        public Texture Value;

        public static implicit operator Texture(TextureWrapper value) => value.Value;
        public static implicit operator TextureWrapper(Texture value) => new TextureWrapper { Value = value };
    }

    public struct TextureArrayPtr {
        public BlobArray<TextureWrapper> Ptr;
    }

    public struct TextureCollectionBlob : ISystemStateComponentData, IDisposable {

        public BlobAssetReference<TextureArrayPtr> BlobAsset;

        public void Dispose() {
            if (BlobAsset.IsCreated) {
                BlobAsset.Dispose();
            }
        }
    }

    public static class TextureCollectionBlobExt {

        /// <summary>
        /// Returns the reference of a texture given its index.
        /// </summary>
        public static Texture At(this ref TextureCollectionBlob blob, int index) {
            return blob.BlobAsset.Value.Ptr[index].Value;
        }

        public static int Length(this ref TextureCollectionBlob blob) {
            return blob.BlobAsset.Value.Ptr.Length;
        }
    }
