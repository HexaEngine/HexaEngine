namespace Hexa.NET.KittyUI.Native.Windows
{
    public struct Margins
    {
        public int CxLeftWidth;
        public int CxRightWidth;
        public int CyTopHeight;
        public int CyBottomHeight;

        public Margins(int all)
        {
            CxLeftWidth = all;
            CxRightWidth = all;
            CyTopHeight = all;
            CyBottomHeight = all;
        }

        public Margins(int cxLeftWidth, int cxRightWidth, int cyTopHeight, int cyBottomHeight)
        {
            CxLeftWidth = cxLeftWidth;
            CxRightWidth = cxRightWidth;
            CyTopHeight = cyTopHeight;
            CyBottomHeight = cyBottomHeight;
        }
    }
}