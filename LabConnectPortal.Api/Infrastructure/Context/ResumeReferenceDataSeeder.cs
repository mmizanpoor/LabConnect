using LabConnectPortal.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Context;

public static class ResumeReferenceDataSeeder
{
    public static async Task SeedAsync(LabConnectDbContext context)
    {
        if (!await context.Provinces.AnyAsync())
        {
            var provinces = new[]
            {
                "آذربایجان شرقی", "آذربایجان غربی", "اردبیل", "اصفهان", "البرز",
                "ایلام", "بوشهر", "تهران", "چهارمحال و بختیاری", "خراسان جنوبی",
                "خراسان رضوی", "خراسان شمالی", "خوزستان", "زنجان", "سمنان",
                "سیستان و بلوچستان", "فارس", "قزوین", "قم", "کردستان",
                "کرمان", "کرمانشاه", "کهگیلویه و بویراحمد", "گلستان", "گیلان",
                "لرستان", "مازندران", "مرکزی", "هرمزگان", "همدان", "یزد",
            };

            foreach (var provinceName in provinces)
            {
                context.Provinces.Add(new Province { ProvinceName = provinceName });
            }
        }

        if (!await context.JobCategories.AnyAsync())
        {
            var categories = new[]
            {
                "فناوری اطلاعات", "مالی و حسابداری", "فروش و بازاریابی", "مهندسی",
                "پزشکی و سلامت", "آموزش", "حقوق", "مدیریت", "هنر و رسانه", "علوم",
            };

            foreach (var categoryName in categories)
            {
                context.JobCategories.Add(new JobCategory { CategoryName = categoryName });
            }
        }

        if (!await context.SalaryRanges.AnyAsync())
        {
            var ranges = new[]
            {
                "توافقی",
                "کمتر از ۱۰ میلیون",
                "۱۰ تا ۲۰ میلیون",
                "۲۰ تا ۳۰ میلیون",
                "۳۰ تا ۵۰ میلیون",
                "۵۰ تا ۸۰ میلیون",
                "بیش از ۸۰ میلیون",
            };

            foreach (var range in ranges)
            {
                context.SalaryRanges.Add(new SalaryRange { SalaryRangeDescription = range });
            }
        }

        if (!await context.LanguageNames.AnyAsync())
        {
            var languages = new[]
            {
                "فارسی", "English", "العربية", "Français", "Deutsch",
                "Español", "Italiano", "Türkçe", "Русский", "中文",
            };

            foreach (var language in languages)
            {
                context.LanguageNames.Add(new LanguageName { Name = language });
            }
        }

        if (!await context.Skills.AnyAsync())
        {
            var skills = new[]
            {
                "برنامه‌نویسی C#", "Python Programming", "JavaScript", "SQL Server",
                "Project Management", "Microsoft Excel", "AutoCAD", "Photoshop",
                "Digital Marketing", "English Communication",
            };

            foreach (var skillName in skills)
            {
                context.Skills.Add(new Skill { SkillName = skillName });
            }
        }

        await context.SaveChangesAsync();
    }
}
