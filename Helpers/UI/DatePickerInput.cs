using System;
using System.Globalization;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap.Helpers
{
    public static class DatePickerInput
    {
        public static void Configure(DateTimePicker picker)
        {
            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = "dd/MM/yyyy";
            picker.ShowCheckBox = true;
            picker.Checked = false;
        }

        public static void SetValue(DateTimePicker picker, object value)
        {
            if (value == null || value == DBNull.Value)
            {
                picker.Checked = false;
                return;
            }
            picker.Value = Convert.ToDateTime(value, CultureInfo.InvariantCulture).Date;
            picker.Checked = true;
        }

        public static string ReadText(DateTimePicker picker)
        {
            return picker.Checked ? picker.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "";
        }
    }
}
