using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Controls.Systems {

    public class MobileMouseCollisionSystemTests : TestFixture {

        private MobileMouseCollisionSystem mouseCollisionSystem;

        public override void SetUp() {
            base.SetUp();

            mouseCollisionSystem = world.CreateSystem<MobileMouseCollisionSystem>();

            CreateTouchInputs();
            CreateMockButton();
        }

        [Test]
        public void ClickHovers() {
            Entities.ForEach((ref ButtonClickType c0) => {
                c0.Value = ClickType.PressDown | ClickType.ReleaseUp;
            });
            Entities.ForEach((DynamicBuffer<TouchElement> b0) => {
                b0[0] = new TouchElement {
                    Phase = TouchPhase.Canceled
                };
            });

            mouseCollisionSystem.Update();

            Entities.ForEach((ref ClickState c0, ref ButtonVisual c1) => {
                Assert.AreEqual(false, c0.Value);
                Assert.AreEqual(ButtonVisualState.Hover, c1.Value);
            });
        }

        [Test]
        public void ClickResets() {
            Entities.ForEach((ref ButtonClickType c0) => {
                c0.Value = ClickType.PressDown | ClickType.ReleaseUp;
            });
            Entities.ForEach((DynamicBuffer<TouchElement> b0) => {
                for (int i = 0; i < b0.Length; i++) {
                    b0[i] = new TouchElement {
                        Phase = TouchPhase.Canceled,
                        Position = new float2(1920, 1080)
                    };
                }
            });

            mouseCollisionSystem.Update();

            Entities.ForEach((ref ClickState c0, ref ButtonVisual c1) => {
                Assert.AreEqual(false, c0.Value);
                Assert.AreEqual(ButtonVisualState.None, c1.Value);
            });
        }

        [Test]
        public void ClicksOnRelease() {
            Entities.ForEach((ref ButtonClickType c0) => {
                c0.Value = ClickType.ReleaseUp;
            });
            Entities.ForEach((DynamicBuffer<TouchElement> b0) => {
                for (int i = 0; i < b0.Length; i++) {
                    b0[i] = new TouchElement {
                        Phase = TouchPhase.Ended,
                    };
                }
            });

            mouseCollisionSystem.Update();

            Entities.ForEach((ref ClickState c0, ref ButtonVisual c1) => {
                Assert.AreEqual(true, c0.Value);
                Assert.AreEqual(ButtonVisualState.Pressed, c1.Value);
            });
        }

        [Test]
        public void ClicksOnPress() {
            Entities.ForEach((ref ButtonClickType c0) => {
                c0.Value = ClickType.PressDown;
            });
            Entities.ForEach((DynamicBuffer<TouchElement> b0) => {
                for (int i = 0; i < b0.Length; i++) {
                    b0[i] = new TouchElement {
                        Phase    = TouchPhase.Began,
                        Position = new float2()
                    };
                }
            });

            mouseCollisionSystem.Update();

            Entities.ForEach((ref ClickState c0, ref ButtonVisual c1) => {
                Assert.AreEqual(true, c0.Value);
                Assert.AreEqual(ButtonVisualState.Pressed, c1.Value);
            });
        }

        [Test]
        public void ClicksOnHold() {
            Entities.ForEach((ref ButtonClickType c0) => {
                c0.Value = ClickType.Held;
            });
            Entities.ForEach((DynamicBuffer<TouchElement> b0) => {
                for (int i = 0; i < b0.Length; i++) {
                    b0[i] = new TouchElement {
                        Phase    = TouchPhase.Began | TouchPhase.Moved | TouchPhase.Stationary,
                        Position = new float2()
                    };
                }
            });

            mouseCollisionSystem.Update();

            Entities.ForEach((ref ClickState c0, ref ButtonVisual c1) => {
                Assert.AreEqual(true, c0.Value);
                Assert.AreEqual(ButtonVisualState.Pressed, c1.Value);
            });
        }

        private unsafe void CreateTouchInputs() {
            var entity = manager.CreateEntity();
            var buffer = manager.AddBuffer<TouchElement>(entity);
            buffer.ResizeUninitialized(10);

            UnsafeUtility.MemSet(buffer.GetUnsafePtr(), default, UnsafeUtility.AlignOf<TouchElement>() * 10);
        }

        private void CreateMockButton() {
            var entity = manager.CreateEntity();
            manager.AddComponentData(entity, new ClickState { });
            manager.AddComponentData(entity, ButtonVisual.Default());
            manager.AddComponentData(entity, new Dimensions {
                Value = new int2(150, 150)
            });
            manager.AddComponentData(entity, new LocalToWorld {
                Value = float4x4.TRS(default, quaternion.identity, new float3(1))
            });
            manager.AddComponentData(entity, new ButtonClickType { });
        }
    }
}
