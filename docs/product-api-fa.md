# مستند API مدیریت آگهی محصول

## معرفی

این API برای ایجاد، ویرایش و مشاهده آگهی‌های محصول توسط سامانه‌های بیرونی طراحی شده است. احراز هویت تمام درخواست‌ها با `ApiKey` انجام می‌شود و هر کلید فقط به آگهی‌های مرکز صاحب همان کلید دسترسی دارد.

آدرس پایه کنترلر:

```text
https://{host}/api/ProductApi
```

تمام نام فیلدهای JSON به‌صورت `camelCase` هستند. enumها در پاسخ به‌صورت رشته برگردانده می‌شوند.

## احراز هویت

روش پیشنهادی ارسال کلید:

```http
Authorization: ApiKey {YOUR_API_KEY}
```

دو روش زیر نیز پشتیبانی می‌شوند:

```http
X-Api-Key: {YOUR_API_KEY}
```

```http
ApiKey: {YOUR_API_KEY}
```

کلید API به‌صورت hash در دیتابیس نگهداری می‌شود و مقدار کامل آن فقط هنگام ایجاد کلید نمایش داده می‌شود.

### مجوزها

| عملیات | مجوز لازم |
|---|---|
| ایجاد آگهی | `AllowAdd` |
| ویرایش آگهی | `AllowEdit` |
| دریافت فهرست آگهی‌ها | `AllowView` |
| دریافت دسته‌ها | `AllowAdd` |
| دریافت ویژگی‌ها | `AllowAdd` |
| دریافت برندها | `AllowAdd` |

اگر کلید ارسال نشود، نامعتبر باشد، مرکز یا کاربر غیرفعال باشد، یا مجوز لازم روی کلید فعال نباشد، پاسخ HTTP با وضعیت `403` و بدنه زیر برگردانده می‌شود:

```json
{
  "status": false,
  "success": false,
  "message": "شما دسترسی ندارید. برای دریافت دسترسی با شرکت تماس حاصل نمایید.",
  "errors": null
}
```

## قالب عمومی پاسخ

پاسخ موفق:

```json
{
  "status": true,
  "success": true,
  "message": null,
  "errors": null,
  "data": {}
}
```

پاسخ ناموفق اعتبارسنجی یا عملیات:

```json
{
  "status": false,
  "success": false,
  "message": "متن خطا",
  "errors": null,
  "data": null
}
```

> در خطاهای منطقی سرویس ممکن است HTTP status برابر `200` باشد و نتیجه عملیات از طریق `status` یا `success` مشخص شود. خطای دسترسی ApiKey با HTTP status برابر `403` برگردانده می‌شود.

## وضعیت‌های آگهی

| مقدار | عنوان | توضیح |
|---:|---|---|
| `0` | `Draft` | پیش‌نویس |
| `1` | `PendingApproval` | در انتظار تأیید |
| `2` | `Approved` | تأییدشده |
| `3` | `Rejected` | ردشده |
| `4` | `Unpublished` | خارج‌شده از انتشار |

در JSON بهتر است مقدار رشته‌ای مانند `"Draft"` ارسال شود.

---

## ۱. ایجاد آگهی

```http
POST /api/ProductApi/Create
Content-Type: application/json
Authorization: ApiKey {YOUR_API_KEY}
```

مجوز لازم: `AllowAdd`

### نمونه درخواست

```json
{
  "title": "دستگاه آزمایشگاهی مدل X",
  "brandId": 12,
  "warranty": "یک سال ضمانت",
  "description": "دستگاه سالم و آماده بهره‌برداری است.",
  "productCategoryGroupId": 3,
  "price": 250000000,
  "isNegotiablePrice": false,
  "isUsed": true,
  "stockQuantity": 1,
  "discountPercent": 5,
  "latitude": 35.7219,
  "longitude": 51.3347,
  "featuredImageBase64": "data:image/png;base64,iVBORw0KGgoAAA...",
  "featuredImageFileName": "device.png",
  "categoryIds": [21],
  "attributeValues": [
    {
      "productAttributeId": 105,
      "value": "2025"
    }
  ],
  "expertReviews": [
    {
      "title": "وضعیت فنی",
      "description": "سرویس دوره‌ای انجام شده است.",
      "sortOrder": 0
    }
  ]
}
```

### قواعد اعتبارسنجی

- `title` الزامی است.
- حداقل یک مقدار در `categoryIds` باید ارسال شود.
- تمام دسته‌ها باید معتبر و متعلق به `productCategoryGroupId` باشند.
- اگر دسته انتخاب‌شده نیازمند برند باشد، `brandId` الزامی است.
- هر `productAttributeId` باید متعلق به یکی از دسته‌های انتخاب‌شده باشد.
- `price` نمی‌تواند منفی باشد.
- اگر `isNegotiablePrice` برابر `false` باشد، قیمت باید بزرگ‌تر از صفر باشد.
- `stockQuantity` نمی‌تواند منفی باشد.
- `discountPercent` در صورت ارسال باید بین صفر و صد باشد.
- مقدار `createdByCenterProfileId` توسط مصرف‌کننده تعیین نمی‌شود؛ مرکز از ApiKey معتبر استخراج می‌شود.
- `featuredImageBase64` اختیاری است و تصویر شاخص محصول را دریافت می‌کند.
- مقدار `featuredImageBase64` می‌تواند Data URI مانند `data:image/png;base64,...` یا Base64 خام باشد.
- هنگام ارسال Base64 خام، مقدار `featuredImageFileName` با پسوند معتبر الزامی است؛ در Data URI این فیلد اختیاری است.
- فرمت‌های مجاز تصویر `jpg`، `jpeg`، `png` و `webp` و حداکثر اندازه فایل پس از Decode برابر ۵ مگابایت است.

### نمونه پاسخ موفق

```json
{
  "status": true,
  "success": true,
  "message": null,
  "errors": null,
  "data": {
    "productId": "6c425665-e0fc-47bf-874f-c5346a1f87db",
    "title": "دستگاه آزمایشگاهی مدل X",
    "brandId": 12,
    "brandTitle": "برند نمونه",
    "warranty": "یک سال ضمانت",
    "description": "دستگاه سالم و آماده بهره‌برداری است.",
    "productCategoryGroupId": 3,
    "productCategoryGroupName": "تجهیزات آزمایشگاهی",
    "categoryIds": [21],
    "categoryTitles": ["تجهیزات عمومی"],
    "attributeValues": [
      {
        "productAttributeId": 105,
        "attributeTitle": "سال ساخت",
        "productCategoryId": 21,
        "categoryTitle": "تجهیزات عمومی",
        "value": "2025",
        "fieldType": null
      }
    ],
    "images": [],
    "expertReviews": [
      {
        "id": "7a79848c-28e6-4cf2-92c4-24e706673403",
        "title": "وضعیت فنی",
        "description": "سرویس دوره‌ای انجام شده است.",
        "sortOrder": 0
      }
    ],
    "status": "Draft",
    "featuredImagePath": "uploads/products/6c425665-e0fc-47bf-874f-c5346a1f87db/8fe1a1e62f23442f93ab22fe34735de4.png",
    "hasFeaturedImage": true,
    "price": 250000000,
    "isNegotiablePrice": false,
    "isUsed": true,
    "stockQuantity": 1,
    "discountPercent": 5,
    "latitude": 35.7219,
    "longitude": 51.3347
  }
}
```

### نمونه cURL

```bash
curl --request POST "https://{host}/api/ProductApi/Create" \
  --header "Authorization: ApiKey {YOUR_API_KEY}" \
  --header "Content-Type: application/json" \
  --data '{
    "title": "دستگاه آزمایشگاهی مدل X",
    "brandId": 12,
    "warranty": "یک سال ضمانت",
    "description": "دستگاه سالم است.",
    "productCategoryGroupId": 3,
    "price": 250000000,
    "isNegotiablePrice": false,
    "isUsed": true,
    "stockQuantity": 1,
    "featuredImageBase64": "data:image/png;base64,iVBORw0KGgoAAA...",
    "categoryIds": [21],
    "attributeValues": []
  }'
```

---

## ۲. ویرایش آگهی

```http
POST /api/ProductApi/Update
Content-Type: application/json
Authorization: ApiKey {YOUR_API_KEY}
```

مجوز لازم: `AllowEdit`

بدنه درخواست با ایجاد آگهی یکسان است و علاوه بر آن باید `productId` ارسال شود.

### نمونه درخواست

```json
{
  "productId": "6c425665-e0fc-47bf-874f-c5346a1f87db",
  "title": "دستگاه آزمایشگاهی مدل X - ویرایش‌شده",
  "brandId": 15,
  "warranty": "دو سال ضمانت",
  "description": "توضیحات جدید آگهی",
  "productCategoryGroupId": 3,
  "price": 240000000,
  "isNegotiablePrice": false,
  "isUsed": true,
  "stockQuantity": 2,
  "discountPercent": 10,
  "categoryIds": [22],
  "attributeValues": [
    {
      "productAttributeId": 110,
      "value": "کم‌کارکرد"
    }
  ],
  "expertReviews": []
}
```

### رفتار ویرایش

- فقط آگهی متعلق به مرکز صاحب ApiKey قابل ویرایش است.
- دسته‌ها، ویژگی‌ها و بررسی‌های تخصصی قبلی با مقادیر جدید جایگزین می‌شوند؛ بنابراین لیست کامل جدید باید ارسال شود.
- آگهی با وضعیت `PendingApproval` یا `Approved` مستقیماً قابل ویرایش نیست.
- ویرایش آگهی `Rejected` وضعیت آن را به `Draft` تغییر می‌دهد.
- ویرایش آگهی `Unpublished` وضعیت آن را به `PendingApproval` تغییر می‌دهد.
- خروجی موفق از نوع کامل `ProductDto` و مشابه پاسخ ایجاد است.

---

## ۳. دریافت آگهی‌های مرکز

```http
POST /api/ProductApi/GetAll
Content-Type: application/json
Authorization: ApiKey {YOUR_API_KEY}
```

مجوز لازم: `AllowView`

صرف‌نظر از فیلترهای ورودی، فقط آگهی‌های مرکز صاحب ApiKey برگردانده می‌شوند.

### نمونه درخواست

```json
{
  "title": "دستگاه",
  "brandId": 12,
  "categoryId": 21,
  "status": "Draft",
  "pendingApprovalOnly": false,
  "acceptsResume": false,
  "updatedFrom": "2026-09-01T00:00:00",
  "updatedTo": "2026-09-30T23:59:59",
  "page": 1,
  "pageSize": 20
}
```

تمام فیلترها اختیاری هستند. حداکثر مقدار مؤثر `pageSize` برابر ۱۰۰ است.

### نمونه پاسخ

```json
{
  "status": true,
  "success": true,
  "message": null,
  "errors": null,
  "data": {
    "items": [
      {
        "productId": "6c425665-e0fc-47bf-874f-c5346a1f87db",
        "title": "دستگاه آزمایشگاهی مدل X",
        "brandTitle": "برند نمونه",
        "categoryTitles": ["تجهیزات عمومی"],
        "status": "Draft",
        "featuredImagePath": null,
        "hasFeaturedImage": false,
        "price": 250000000,
        "isNegotiablePrice": false,
        "stockQuantity": 1,
        "discountPercent": 5,
        "createdByUserId": "9a89b439-4680-4163-949f-cb4181d59fc7",
        "createdByCenterProfileId": "fcf21b0f-b6af-438f-95c8-c65cf4f6b091",
        "createdByCenterName": "مرکز نمونه",
        "createdBySiteAdmin": false,
        "acceptsResume": false,
        "createdAt": "2026-09-29T12:00:00",
        "updatedAt": "2026-09-29T12:00:00"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20
  }
}
```

---

## ۴. دریافت دسته‌ها

```http
GET /api/ProductApi/GetCategories
Authorization: ApiKey {YOUR_API_KEY}
```

مجوز لازم: `AllowAdd`

این endpoint شناسه دسته و گروه آن را برمی‌گرداند. مقدار `id` را در `categoryIds` و مقدار
`productCategoryGroupId` را در فیلد هم‌نام درخواست‌های `Create` و `Update` ارسال کنید.

### نمونه پاسخ

```json
{
  "status": true,
  "success": true,
  "message": null,
  "errors": null,
  "data": [
    {
      "id": 21,
      "title": "تجهیزات عمومی",
      "productCategoryGroupId": 3,
      "productCategoryGroupTitle": "تجهیزات آزمایشگاهی",
      "acceptsResume": false
    }
  ]
}
```

---

## ۵. دریافت ویژگی‌ها

```http
POST /api/ProductApi/GetAttributes
Content-Type: application/json
Authorization: ApiKey {YOUR_API_KEY}
```

مجوز لازم: `AllowAdd`

### نمونه درخواست

```json
{
  "categoryIds": [21, 22]
}
```

اگر `categoryIds` خالی باشد، تمام ویژگی‌ها برگردانده می‌شوند.

### نمونه پاسخ

```json
{
  "status": true,
  "success": true,
  "message": null,
  "errors": null,
  "data": [
    {
      "id": 105,
      "title": "سال ساخت"
    },
    {
      "id": 110,
      "title": "وضعیت کارکرد"
    }
  ]
}
```

---

## ۶. دریافت برندها

```http
GET /api/ProductApi/GetBrands
Authorization: ApiKey {YOUR_API_KEY}
```

مجوز لازم: `AllowAdd`

### نمونه پاسخ

```json
{
  "status": true,
  "success": true,
  "message": null,
  "errors": null,
  "data": [
    {
      "id": 12,
      "title": "برند نمونه"
    },
    {
      "id": 15,
      "title": "برند دوم"
    }
  ]
}
```

## تصاویر محصول

در عملیات `Create` می‌توان تصویر شاخص را با فیلد `featuredImageBase64` داخل JSON ارسال کرد. تصویر قبل از ایجاد محصول اعتبارسنجی، Decode و از طریق زیرساخت ذخیره‌سازی فایل روی دیسک ذخیره می‌شود. اگر Base64، فرمت یا حجم تصویر معتبر نباشد، محصول ایجاد نمی‌شود.

در عملیات `Update` هنوز تصویر Base64 پذیرفته نمی‌شود و endpoint مستقل آپلود یا حذف تصویر با ApiKey ارائه نشده است.

اگر محصول از قبل تصویر داشته باشد، مسیر نسبی آن در فیلدهای `featuredImagePath` و `images[].imagePath` برگردانده می‌شود. برای دریافت فایل تصویر می‌توان از endpoint عمومی زیر استفاده کرد:

```http
GET /Product/GetImage?path={URL_ENCODED_IMAGE_PATH}&w={OPTIONAL_WIDTH}
```

مثال:

```text
https://{host}/Product/GetImage?path=uploads%2Fproducts%2F6c425665-e0fc-47bf-874f-c5346a1f87db%2Fimage.jpg&w=800
https://{host}/Product/GetImage?path=uploads/products/4d10d58e-90d5-46da-8bab-08834f3ef80a/1e86edf746554a919dd062dde9bc5288.jpg&w=800
```

فایل فیزیکی در ساختار زیر ذخیره می‌شود و فقط مسیر نسبی آن در دیتابیس قرار می‌گیرد:

```text
uploads/products/{productId}/{randomGuid}.{extension}
```

فرمت‌های قابل قبول در زیرساخت فعلی محصول عبارت‌اند از `jpg`، `jpeg`، `png` و `webp` و حداکثر حجم مؤثر فایل ۵ مگابایت است.

## محدودیت‌های فعلی

- endpoint مستقل آپلود و حذف تصویر از طریق ApiKey هنوز ارائه نشده است؛ تصویر شاخص فقط همراه درخواست `Create` قابل ارسال است.
- endpoint مستقلی برای دریافت گروه‌های بدون دسته در `ProductApi` وجود ندارد؛ اطلاعات گروه هر دسته در خروجی `GetCategories` برگردانده می‌شود.
- endpoint خارج‌کردن آگهی از انتشار در `ProductApi` وجود ندارد؛ بنابراین آگهی `Approved` از طریق این API مستقیماً قابل ویرایش نیست.
- این API عملیات حذف، ارسال برای تأیید، انتشار مجدد و دریافت جزئیات یک آگهی را ارائه نمی‌کند.

## تست ذخیره و ویرایش

سناریوی خودکار زیر در پروژه پیاده‌سازی و اجرا شده است:

```text
Create product
    -> verify product owner and center
    -> verify brand, category and attribute in database
    -> decode Base64 featured image and verify physical file and database path
    -> reject invalid Base64 without creating a product
Update product
    -> verify changed fields
    -> verify replacement of brand, category and attribute
```

فایل تست:

```text
LabConnectPortal.Tests/Controllers/ProductApiControllerTests.cs
```

دیتابیس تست از نوع EF Core InMemory است و به دیتابیس اصلی پروژه متصل نمی‌شود.

آخرین نتیجه اجرای مجموعه تست‌ها:

```text
Passed: 37
Failed: 0
Skipped: 0
```
