namespace Photino.NET;

partial class Platform
{
    public static class Linux
    {
        /// <summary>
        /// Linux-only native hit-test settings for chromeless windows.
        /// </summary>
        public readonly record struct ChromelessSettings
        {
            /// <summary>
            /// Initializes Linux-only native hit-test settings for chromeless windows.
            /// </summary>
            /// <param name="dragRegionHeight">
            /// Height, in logical pixels, of the native drag region measured from the WebView top edge.
            /// Set to 0 to disable native Linux chromeless drag.
            /// </param>
            /// <param name="dragRegionLeftInset">
            /// Left inset, in logical pixels, excluded from the native drag region.
            /// </param>
            /// <param name="dragRegionRightInset">
            /// Right inset, in logical pixels, excluded from the native drag region.
            /// Use this to exclude custom title bar buttons from native drag.
            /// </param>
            /// <param name="resizeBorderThickness">
            /// Thickness, in logical pixels, of the native resize border measured from the WebView edges.
            /// Set to 0 to disable native Linux chromeless resize borders.
            /// </param>
            public ChromelessSettings(
                int dragRegionHeight = 0,
                int dragRegionLeftInset = 0,
                int dragRegionRightInset = 0,
                int resizeBorderThickness = 8)
            {
                DragRegionHeight = dragRegionHeight;
                DragRegionLeftInset = dragRegionLeftInset;
                DragRegionRightInset = dragRegionRightInset;
                ResizeBorderThickness = resizeBorderThickness;
            }

            /// <summary>
            /// Height, in logical pixels, of the native drag region measured from the WebView top edge.
            /// Set to 0 to disable native Linux chromeless drag.
            /// </summary>
            public int DragRegionHeight { get; init; }

            /// <summary>
            /// Left inset, in logical pixels, excluded from the native drag region.
            /// </summary>
            public int DragRegionLeftInset { get; init; }

            /// <summary>
            /// Right inset, in logical pixels, excluded from the native drag region.
            /// Use this to exclude custom title bar buttons from native drag.
            /// </summary>
            public int DragRegionRightInset { get; init; }

            /// <summary>
            /// Thickness, in logical pixels, of the native resize border measured from the WebView edges.
            /// Set to 0 to disable native Linux chromeless resize borders.
            /// </summary>
            public int ResizeBorderThickness { get; init; }
        }
    }
}