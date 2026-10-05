using System.ComponentModel;
using System.Globalization;

namespace LabConnectPortal.Api.Infrastructure.Utils
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

        public static string GetShamsiDatePart(this string shamsiDateTime)
        {
            if (string.IsNullOrWhiteSpace(shamsiDateTime) || shamsiDateTime.Length < 10)
                return string.Empty;

            return shamsiDateTime[..10];
        }

        public static string GetShamsiTimePart(this string shamsiDateTime)
        {
            if (string.IsNullOrWhiteSpace(shamsiDateTime) || shamsiDateTime.Length <= 10)
                return string.Empty;

            return shamsiDateTime[10..];
        }

        public static (string From, string To) GetShamsiDayRange(DateTime date)
        {
            var shamsiDate = date.MiladiDateToStrShamsi();
            return (shamsiDate + "00:00", shamsiDate + "23:59");
        }

        public static (string From, string To) GetShamsiMonthRange(DateTime date)
        {
            var pc = new PersianCalendar();
            var year = pc.GetYear(date);
            var month = pc.GetMonth(date);
            var lastDay = pc.GetDaysInMonth(year, month);
            var monthPrefix = $"{year:0000}/{month:00}/";
            return (monthPrefix + "01" + "00:00", monthPrefix + lastDay.ToString("00") + "23:59");
        }

        public static (string From, string To) GetShamsiPastYearRange(DateTime date)
        {
            var toShamsi = date.MiladiDateToStrShamsi();
            var fromShamsi = date.AddYears(-1).MiladiDateToStrShamsi();
            return (fromShamsi + "00:00", toShamsi + "23:59");
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
