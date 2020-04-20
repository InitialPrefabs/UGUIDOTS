namespace UGUIDots.Render {

    /// <summary>
    /// Fill types only support x/y axis based fills. Radial support will be coming 
    /// in later.
    /// </summary>
    public enum FillType : int {
        RightToLeft = 0,
        LeftToRight = 1,
        BottomToTop = 2,
        TopToBottom = 3,
    }
}
