using ReaLTaiizor.Colors;

namespace BLT_Quan_Ly_Hoc_Tap
{
    internal class ColorScheme : MaterialColorScheme
    {
        private object indigo500;
        private object indigo700;
        private object indigo100;
        private object pink200;
        private object wHITE;

        public ColorScheme(object indigo500, object indigo700, object indigo100, object pink200, object wHITE)
        {
            this.indigo500 = indigo500;
            this.indigo700 = indigo700;
            this.indigo100 = indigo100;
            this.pink200 = pink200;
            this.wHITE = wHITE;
        }
    }
}