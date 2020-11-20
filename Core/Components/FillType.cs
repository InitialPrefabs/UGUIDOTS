using Unity.Entities;
using UnityEngine.UI;

namespace UGUIDOTS {
    
    /// <summary>
    /// The axis that should be affected.
    /// </summary>
    public enum Axis {
        X = 0,
        Y = 1
    }

    /// <summary>
    /// Defines how the image is filled - used for the associative fill type in the Shader.
    /// </summary>
    public enum FillType {
        Axis   = 0,
        Radial = 1
    }

    /// <summary>
    /// Fill types only support x/y axis based fills. Radial support will be coming 
    /// in later.
    /// </summary>
    public struct FillDirection : IComponentData {

        /// <summary>
        /// Stores the enum int value for the Origin enums found in the Image class.
        /// </summary>
        public int Value;

        // TODO: Implement the other fill type constructors.
        public static implicit operator FillDirection(Image.Origin360 fillType) {
            return new FillDirection { Value = (int)fillType };
        }
    }

    /// <summary>
    /// Stores the fill amount, the axis this effects, and whether or not the direction of the 
    /// axis should be flipped.
    /// </summary>
    public struct AxisFillAmount : IComponentData {

        /// <summary>
        /// Stores a value inclusive between [0, 1].
        /// </summary>
        public float FillAmount;

        /// <summary>
        /// Stores which axis this should effect.
        /// </summary>
        public Axis Axis;

        /// <summary>
        /// Should the direction be flipped? 
        /// For the x axis by default we fill from <b>left -> right</b>, while flipping will fill 
        /// from <b>right -> left</b>.
        /// For the y axis by default we will from <b>bottom -> top</b>, while flipping will fill 
        /// from <b>top -> bottom</b>.
        /// </summary>
        public bool Flip;
    }

    /// <summary>
    /// Stores the fill amount for a radial element.
    /// </summary>
    public struct RadialFillAmount : IComponentData {
        /// <summary>
        /// Stores the offset to begin the radial fill.
        /// </summary>
        public float Angle;

        /// <summary>
        /// The fill amount on the bottom half of the image. Putting this value to 0 will mean 
        /// that there is no fill on the entire image, while 0.5f will cause this to be half filled.
        /// </summary>
        public float Arc1;

        /// <summary>
        /// This is equivalent to the Image's Radial Fill Amount.
        ///
        /// The fill amount on the top half of the image. Putting this value of 0 will mean that 
        /// there is no fill on the entire image, while 0.5f will cause this to be half filled.
        /// </summary>
        public float Arc2;
    }
}
