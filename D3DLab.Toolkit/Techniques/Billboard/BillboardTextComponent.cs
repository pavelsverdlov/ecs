using D3DLab.ECS;

using System.Numerics;

namespace D3DLab.Toolkit.Techniques.Billboard {
    //TODO: implement in future
    public enum BillboardTypes {
        /// <summary>
        /// All static bilbord will be united in one atlas
        /// </summary>
        Static,
        /// <summary>
        /// Updatable has own texture resource for itself
        /// </summary>
        Updatable
    }
    /// <summary>
    /// Size in 3D scene depends on the mode... make a selection of the FontSize values
    /// </summary>
    public enum BillboardSizeModes {
        /// <summary>
        /// Size of billboard will not depends on camera view, it is always the same in screen
        /// </summary>
        SceenFixed,
        /// <summary>
        /// Using original size of billboard (image) it will be updatable after camera manipulation
        /// </summary>
        SizeFixed,
    }

    public enum BillboardAttacmentPoints {
        LeftBottom,
    }
    public struct BillboardTextComponent : IGraphicComponent {
        /// <summary>
        /// NOTE: the text inside image has gap need to move attachment point backford a little bit
        /// </summary>
        /// <param name="position"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="textColor"></param>
        /// <param name="mode"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static BillboardTextComponent CreateUpdatable(Vector3 position, string text, double fontSize, 
            Vector4 textColor, BillboardSizeModes mode, BillboardAttacmentPoints point) {
            return new BillboardTextComponent {
                Tag = ElementTag.New("BILLTEX_"),
                AttacmentType = point,
                Type = BillboardTypes.Updatable,
                SizeMode = mode,
                Position = position,
                FontSize = fontSize,
                Text = text,
                TextColor = textColor,
                //OffsetToAttachmentPoint = 0.05f
            };
        }

        public Vector3 Position { get; private set; }
        public BillboardTypes Type { get; private set; }
        public BillboardSizeModes SizeMode { get; private set; }
        public BillboardAttacmentPoints AttacmentType { get; private set; }

        public double FontSize { get; private set; }
        public string Text { get; private set; }
        public Vector4 TextColor { get; private set; }
        public ElementTag Tag { get; private set; }
        public bool IsValid { get; private set; }

        public void Dispose() {
        }
    }
}
