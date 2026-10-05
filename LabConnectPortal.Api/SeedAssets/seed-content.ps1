$ErrorActionPreference = 'Stop'
$base = 'http://localhost:5299'
$root = 'd:\LabConnectPortal\LabConnectPortal.Api'
$seedRoot = Join-Path $root 'SeedAssets'
$dataPath = Join-Path $seedRoot 'seed-content-data.json'
$imgRoot = Join-Path $seedRoot 'content\images'
$uploadRoots = @(
    (Join-Path $root 'uploads\posts'),
    (Join-Path $root 'bin\Debug\net8.0\uploads\posts')
)

$EGroup = 'aecc303a-0a4e-4044-ab32-8cd99b7c4de4'
$ENews = 'c4f1a8e2-3b57-4d91-9e26-7a0b5c8d2f14'
$EArticles = 'd5a2b9f3-4c68-4e02-af37-8b1c6d9e3025'
$EDocuments = 'e6b3c0a4-5d79-4f13-b048-9c2d7e0f4136'

function Sql([string]$q) {
    $full = "SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; $q"
    sqlcmd -S . -d LabConnect -U sa -P '1qaz!QAZ' -f 65001 -Q $full | Out-Null
}

function SqlEsc([string]$s) {
    if ($null -eq $s) { return '' }
    return ($s -replace '''', '''''')
}

function Get-EntityForType([int]$type) {
    switch ($type) {
        1 { return $ENews }
        2 { return $EArticles }
        3 { return $EDocuments }
        default { throw "Unknown type $type" }
    }
}

function Api($method, $path, $token, $entity, $obj) {
    $headers = @{ Authorization = "Bearer $token"; 'X-System-Entity' = $entity }
    if ($null -ne $obj) {
        $headers['Content-Type'] = 'application/json'
        $json = $obj | ConvertTo-Json -Compress -Depth 16
        return Invoke-RestMethod -Uri "$base/$path" -Method $method -Headers $headers -Body ([Text.Encoding]::UTF8.GetBytes($json))
    }
    return Invoke-RestMethod -Uri "$base/$path" -Method $method -Headers $headers
}

function Login($user, $pass) {
    $body = @{ username = $user; password = $pass } | ConvertTo-Json -Compress
    $r = Invoke-RestMethod -Uri "$base/Auth/LoginWithPassword" -Method POST -ContentType 'application/json' -Body ([Text.Encoding]::UTF8.GetBytes($body))
    if (-not $r.status) { throw "Login failed $user : $($r.message)" }
    return $r.data.accessToken
}

function UploadFeatured($postId, $token, $entity, $file) {
    $raw = curl.exe -s -X POST "$base/ContentPost/UploadFeaturedImage?contentPostId=$postId" `
        -H "Authorization: Bearer $token" -H "X-System-Entity: $entity" -F "file=@$file"
    return $raw | ConvertFrom-Json
}

function Clear-ContentData {
    Write-Output '=== Clearing existing content ==='
    foreach ($dir in $uploadRoots) {
        if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }
    }
    Sql 'DELETE FROM dbo.ContentPosts; DELETE FROM dbo.ContentGroups;'
}

function Seed-ContentDirect {
    param($data)
    Write-Output '=== Seeding via SQL + file copy ==='
    $groupIds = @{}
    foreach ($g in @($data.groups)) {
        $title = SqlEsc $g.title
        $code = SqlEsc $g.code
        $sql = "SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; INSERT INTO dbo.ContentGroups (Code, Title) VALUES (N'$code', N'$title'); SELECT CAST(SCOPE_IDENTITY() AS int) AS Id;"
        $idLine = sqlcmd -S . -d LabConnect -U sa -P '1qaz!QAZ' -f 65001 -h -1 -W -Q $sql | Where-Object { $_ -match '^\d+$' } | Select-Object -First 1
        $groupIds[$g.code] = [int]$idLine.Trim()
        Write-Output "  Group $($g.title) => $($groupIds[$g.code])"
    }

    $now = Get-Date
    $count = 0
    foreach ($p in @($data.posts)) {
        $postId = [Guid]::NewGuid()
        $groupId = $groupIds[$p.groupCode]
        $published = $now.AddDays(-1 * [int]$p.daysAgo).ToString('yyyy-MM-dd HH:mm:ss')
        $created = $now.ToString('yyyy-MM-dd HH:mm:ss')

        $imgSrc = Join-Path $imgRoot $p.imageFile
        if (-not (Test-Path $imgSrc)) { throw "Missing image $($p.imageFile)" }

        $imgName = "$([Guid]::NewGuid().ToString('N')).jpg"
        $relPath = "uploads/posts/$($postId.ToString('N'))/$imgName"
        foreach ($uploadRoot in $uploadRoots) {
            $destDir = Join-Path $uploadRoot $postId.ToString('N')
            New-Item -ItemType Directory -Force -Path $destDir | Out-Null
            Copy-Item $imgSrc (Join-Path $destDir $imgName) -Force
        }

        $title = SqlEsc $p.title
        $short = SqlEsc $p.shortDescription
        $body = SqlEsc $p.fullBody
        $author = SqlEsc $p.authorName
        $showHome = if ($p.showOnHomePage) { 1 } else { 0 }
        $pathEsc = SqlEsc $relPath

        $insert = @"
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;
INSERT INTO dbo.ContentPosts (
  ContentPostId, ContentGroupId, Type, Title, ShortDescription, FullBody, FeaturedImagePath,
  IsActive, ShowOnHomePage, ShowAuthor, AuthorName, PublishedAt, ViewCount, CreatedAt, UpdatedAt
) VALUES (
  '$postId', $groupId, $($p.type), N'$title', N'$short', N'$body', N'$pathEsc',
  1, $showHome, 1, N'$author', '$published', 0, '$created', '$created'
);
"@
        Sql $insert
        $count++
    }
    Write-Output "  Inserted $count posts"
}

function Seed-ContentApi {
    param($data, $token)
    Write-Output '=== Seeding via API ==='
    $groupIds = @{}
    foreach ($g in @($data.groups)) {
        $r = Api POST 'ContentGroup/Create' $token $EGroup @{ code = $g.code; title = $g.title }
        if (-not $r.status) { throw "Group create failed: $($r.message)" }
        $groupIds[$g.code] = [int]$r.data.contentGroupId
        Write-Output "  Group $($g.title) => $($groupIds[$g.code])"
    }

    $count = 0
    foreach ($p in @($data.posts)) {
        $published = (Get-Date).AddDays(-1 * [int]$p.daysAgo)
        $cmd = @{
            contentGroupId = $groupIds[$p.groupCode]
            type = [int]$p.type
            title = $p.title
            shortDescription = $p.shortDescription
            fullBody = $p.fullBody
            isActive = $true
            showOnHomePage = [bool]$p.showOnHomePage
            showAuthor = $true
            authorName = $p.authorName
            publishedAt = $published.ToString('o')
            viewCount = 0
        }
        $entity = Get-EntityForType ([int]$p.type)
        $r = Api POST 'ContentPost/Create' $token $entity $cmd
        if (-not $r.status) { throw "Post create failed: $($r.message)" }
        $postId = [Guid]$r.data.contentPostId
        $imgSrc = Join-Path $imgRoot $p.imageFile
        $up = UploadFeatured $postId $token $entity $imgSrc
        if (-not $up.status) { throw "Image upload failed for $($p.title): $($up.message)" }
        $count++
        Write-Output "  OK [$($p.type)] $($p.title)"
    }
    Write-Output "  Created $count posts via API"
}

# --- main ---
$data = Get-Content -Raw -Encoding UTF8 $dataPath | ConvertFrom-Json
Clear-ContentData

$apiUp = $false
try {
    $code = curl.exe -s -o NUL -w '%{http_code}' --max-time 3 "$base/swagger/index.html"
    $apiUp = ($code -match '^[23]')
} catch { $apiUp = $false }

if ($apiUp) {
    $token = $null
    foreach ($pair in @(@('admin','Admin@123'), @('admin','123456'))) {
        try { $token = Login $pair[0] $pair[1]; break } catch { }
    }
    if ($token) {
        Seed-ContentApi $data $token
    } else {
        Write-Output 'API up but login failed; falling back to SQL'
        Seed-ContentDirect $data
    }
} else {
    Write-Output 'API not running; using SQL seed'
    Seed-ContentDirect $data
}

Write-Output '=== SUMMARY ==='
sqlcmd -S . -d LabConnect -U sa -P '1qaz!QAZ' -f 65001 -W -Q @"
SET NOCOUNT ON;
SELECT 'Groups' AS Item, COUNT(*) AS Cnt FROM dbo.ContentGroups
UNION ALL SELECT 'Posts', COUNT(*) FROM dbo.ContentPosts
UNION ALL SELECT 'News', COUNT(*) FROM dbo.ContentPosts WHERE Type=1
UNION ALL SELECT 'Articles', COUNT(*) FROM dbo.ContentPosts WHERE Type=2
UNION ALL SELECT 'Documents', COUNT(*) FROM dbo.ContentPosts WHERE Type=3;
SELECT g.Title AS GroupTitle, COUNT(p.ContentPostId) AS PostCount
FROM dbo.ContentGroups g LEFT JOIN dbo.ContentPosts p ON p.ContentGroupId = g.ContentGroupId
GROUP BY g.Title ORDER BY g.Title;
"@

Write-Output '=== DONE ==='
