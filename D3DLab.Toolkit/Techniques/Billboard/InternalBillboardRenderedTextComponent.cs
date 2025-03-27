using D3DLab.ECS;

using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace D3DLab.Toolkit.Techniques.Billboard {
    /// <summary>
    /// MUST NOT BE A PUBLIC
    /// </summary>
    class InternalBillboardRenderedTextComponent : GraphicComponent {
        static readonly Typeface typeface = new Typeface("Segoe UI");
        ElementTag renderedTextCompTag;

        public Bitmap RenderedBitmapText { get; private set; }
        /// <summary>
        /// Calculate scale because image is created with big FontSize to have proper quality
        /// scale invert image size to user defined value
        /// </summary>
        public float Scale { get; private set; }

       

        public bool IsRendered(BillboardTextComponent text) => renderedTextCompTag == text.Tag;

        public void Render(BillboardTextComponent text) {
            var color = new SolidColorBrush(text.TextColor.ToColor());
            var formattedText = new FormattedText(text.Text, CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, typeface, text.FontSize, color);
            formattedText.SetFontSize(100, 0, text.Text.Length);

            formattedText.PixelsPerDip = 50;

            var renderTargetBitmapWidth = (int)(formattedText.Width / 2);
            var renderTargetBitmapHeight = (int)(formattedText.Height / 2);

            var visual = new DrawingVisual();
            using (DrawingContext drawingContext = visual.RenderOpen()) {
                drawingContext.DrawText(formattedText, new System.Windows.Point(0, 0));
            }
            var source = new RenderTargetBitmap(renderTargetBitmapWidth, renderTargetBitmapHeight, 50, 50, PixelFormats.Pbgra32);
            source.Clear();
            source.Render(visual);
            source.Freeze();

            var bitmap = new Bitmap(source.PixelWidth, source.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var data = bitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bitmap.UnlockBits(data);

            RenderedBitmapText = bitmap;

            renderedTextCompTag = text.Tag;
            Scale = (float)text.FontSize / 100f;
        }
    }
}
