using System.ComponentModel;
using System.Globalization;

namespace LabConnectPortal.Infra
{
    public static class CustomConverter
    {
        public static string MiladiDateToStrShamsi(this DateTime date)
        {
            PersianCalendar pc = new PersianCalendar();
            return pc.GetYear(date).ToString("0000") + "/" + pc.GetMonth(date).ToString("00") + "/" + pc.GetDayOfMonth(date).ToString("00");
        }


        public static DateTime ShamsiStrToMiladiDate(this string persianDate)
        {
            int year = Convert.ToInt32(persianDate.Substring(0, 4));
            int month = Convert.ToInt32(persianDate.Substring(5, 2));
            int day = Convert.ToInt32(persianDate.Substring(8, 2));
            DateTime georgianDateTime = new DateTime(year, month, day, new System.Globalization.PersianCalendar());
            return georgianDateTime;
        }

    }

    public static class EnumExtensions
    {
        /// <summary>
        /// برای دریافت توضیحات یک ویژگی از enum اگر [Description] داشته باشد از این متد استفاده می‌شود.
        /// </summary>
        /// <param name="enumValue">مقداری که قرار است توضحیات آن دریافت شود</param>
        /// <returns>متن داخل [Description] در صورتی که وجود داشته باشد و در غیراین صورت عنوان enums ارسال شده</returns>
        public static string GetEnumDescription(this Enum enumValue)
        {
            var memberInfo = enumValue.GetType().GetField(enumValue.ToString());
            var attributes = memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            var description = attributes != null ? ((DescriptionAttribute)attributes.FirstOrDefault()).Description : enumValue.ToString();
            return description;
        }
    }
}
