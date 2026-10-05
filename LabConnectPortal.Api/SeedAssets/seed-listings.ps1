$ErrorActionPreference = 'Stop'
$base = 'http://localhost:5299'
$EProduct = '504586dc-bd75-4f6f-972b-c7fd583a9f8f'
$EJob = 'eec0d6a3-edaf-46ba-b76a-5562b4390367'
$ELoc = 'b9246a45-0941-42a9-9fa3-5fcc08f1fbff'
$root = 'd:\LabConnectPortal\LabConnectPortal.Api\SeedAssets'
$data = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'seed-listings-data.json') | ConvertFrom-Json
$seedBrands = Join-Path $root 'brands'
$hash123 = '$2a$11$8FyW4PPG5QRfxxZkLU.KFOcUZwsqiDIutxjKSADHZMnpco06WxziW'
$adminOrig = '$2a$11$sVmKmjdsHJwoacpu.TrlReoSlWwxC7DuctadxXeLZNavbGod1Ibue'
$brandImg = @{
  11='pars-peyvand.jpg'; 12='hannan-teb-pars.jpg'; 13='ronak-teb-vida.jpg'
  14='zist-shimi.jpg'; 15='kimia-pazhouhan.jpg'; 16='merck.jpg'; 17='thermo-fisher.jpg'
  18='sysmex.jpg'; 19='mindray.jpg'; 20='roche.jpg'; 21='abbott.jpg'; 22='hitachi.jpg'
  23='siemens.jpg'; 24='beckman-coulter.jpg'; 25='erba.jpg'; 26='dirui.jpg'; 27='human.jpg'
  28='dell.jpg'; 29='hp.jpg'; 30='lenovo.jpg'
}

function Sql([string]$q) {
  sqlcmd -S . -d LabConnect -U sa -P '1qaz!QAZ' -Q $q | Out-Null
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
  $r = Invoke-RestMethod -Uri "$base/Auth/LoginWithPassword" -Method POST -ContentType 'application/json' -Body "{`"username`":`"$user`",`"password`":`"$pass`"}"
  if (-not $r.status) { throw "Login failed $user : $($r.message)" }
  return $r.data.accessToken
}

function Upload($path, $token, $entity, $file) {
  $raw = curl.exe -s -X POST "$base/$path" -H "Authorization: Bearer $token" -H "X-System-Entity: $entity" -F "file=@$file"
  return $raw | ConvertFrom-Json
}

function Get-ExtraValue($extras, $title) {
  if ($null -eq $extras) { return $null }
  $prop = $extras.PSObject.Properties | Where-Object { $_.Name -eq $title } | Select-Object -First 1
  if ($prop) { return [string]$prop.Value }
  return $null
}

function Fill-Attrs($attrs, $def) {
  $list = @()
  $brandKeys = @($data.brandAttrTitles)
  $modelKeys = @($data.modelAttrTitles)
  foreach ($a in @($attrs)) {
    $title = [string]$a.title
    $val = Get-ExtraValue $def.extras $title
    if ([string]::IsNullOrWhiteSpace($val)) {
      if ($brandKeys -contains $title) { $val = [string]$def.brandTitle }
      elseif ($modelKeys -contains $title) { $val = [string]$def.model }
      else {
        $dprop = $data.attrDefaults.PSObject.Properties[$title]
        if ($dprop) { $val = [string]$dprop.Value } else { $val = 'n/a' }
      }
    }
    if ([string]::IsNullOrWhiteSpace($val)) { $val = 'n/a' }
    $list += @{ productAttributeId = [int]$a.productAttributeId; value = $val }
  }
  return $list
}

function Publish-Product($storeToken, $adminToken, $def, $attrCache) {
  $catId = [int]$def.categoryId
  $key = [string]$catId
  if (-not $attrCache.ContainsKey($key)) {
    $ar = Api POST 'ProductAttribute/GetByCategoryIds' $storeToken $EProduct @{ categoryIds = @($catId) }
    if (-not $ar.status) { throw "attrs $($ar.message)" }
    $attrCache[$key] = @($ar.data)
  }
  $attrs = Fill-Attrs $attrCache[$key] $def
  $negotiable = [bool]$def.negotiable
  $body = @{
    title = [string]$def.title
    brandId = [int]$def.brandId
    warranty = [string]$def.warranty
    description = [string]$def.description
    productCategoryGroupId = [int]$def.groupId
    price = [decimal]$def.price
    isNegotiablePrice = $negotiable
    isUsed = [bool]$def.used
    stockQuantity = [int]$def.stock
    latitude = 35.7219
    longitude = 51.3347
    categoryIds = @($catId)
    attributeValues = $attrs
    expertReviews = @()
  }
  if ($def.discount -and -not $negotiable) { $body.discountPercent = [decimal]$def.discount }
  $created = Api POST 'Product/Create' $storeToken $EProduct $body
  if (-not $created.status) { throw "CREATE $($def.title) : $($created.message)" }
  $productId = $created.data.productId
  $imgName = $brandImg[[int]$def.brandId]
  $img = Join-Path $seedBrands $imgName
  if (-not (Test-Path $img)) { $img = Join-Path $root 'stores\office1.jpg' }
  $u1 = Upload "Product/UploadFeaturedImage?productId=$productId" $storeToken $EProduct $img
  if (-not $u1.status) { Write-Output "  WARN featured $($def.title) $($u1.message)" }
  $u2 = Upload "Product/UploadImage?productId=$productId" $storeToken $EProduct $img
  if (-not $u2.status) { Write-Output "  WARN gallery $($def.title) $($u2.message)" }
  $sub = Api POST 'Product/SubmitForApproval' $storeToken $EProduct @{ productId = $productId }
  if (-not $sub.status) { throw "SUBMIT $($def.title) : $($sub.message)" }
  $ap = Api POST 'Product/Approve' $adminToken $EProduct @{ productId = $productId }
  if (-not $ap.status) { throw "APPROVE $($def.title) : $($ap.message)" }
  $tag = if ($negotiable) { 'NEGOTIABLE' } else { "PRICE $($def.price)" }
  Write-Output "  OK [$tag] $($def.title)"
}

$ptnOrig = ((sqlcmd -S . -d LabConnect -U sa -P '1qaz!QAZ' -h -1 -W -Q "SET NOCOUNT ON; SELECT PasswordHash FROM Users WHERE Username='ptn'") | Select-Object -First 1).ToString().Trim()
Sql "UPDATE Users SET PasswordHash='$hash123' WHERE Username IN ('ptn','09169624876_3','09121111120_3','admin')"

$adminToken = Login 'admin' '123456'
$tokens = @{
  companyOne = (Login 'companyOne' '123456')
  companyTwo = (Login 'companyTwo' '123456')
  ptn = (Login 'ptn' '123456')
}
$lab1 = Login '09169624876_3' '123456'
$lab2 = Login '09121111120_3' '123456'
$attrCache = @{}

foreach ($storeName in @('companyOne','companyTwo','ptn')) {
  Write-Output "=== STORE $storeName ==="
  foreach ($p in @($data.stores.$storeName)) {
    Publish-Product $tokens[$storeName] $adminToken $p $attrCache
  }
}

function Ensure-Location($token, $name, $address, $phone) {
  $existing = Api GET 'OrganizationLocation/GetMyLocations' $token $ELoc $null
  $items = @($existing.data)
  if ($existing.status -and $items.Count -gt 0 -and $items[0].locationId) {
    return $items[0].locationId
  }
  $c = Api POST 'OrganizationLocation/Create' $token $ELoc @{
    locationName = $name
    provinceId = 8
    address = $address
    phoneNumber = $phone
  }
  if (-not $c.status) { throw "location $($c.message)" }
  return $c.data.locationId
}

function Publish-Job($token, $job, $locationId) {
  $payload = @{
    jobCategoryId = [int]$job.jobCategoryId
    locationId = $locationId
    salaryRangeId = [int]$job.salaryRangeId
    minimumWorkExperienceYears = [int]$job.minimumWorkExperienceYears
    jobDescription = [string]$job.jobDescription
    contractTypes = @($job.contractTypes)
    requiredPersonalTraits = @($job.requiredPersonalTraits)
    essentialSkillIds = @($job.essentialSkillIds)
    benefits = @($job.benefits)
    genderRequirement = [string]$job.genderRequirement
    militaryServiceRequirement = [string]$job.militaryServiceRequirement
    minimumDegreeLevel = [string]$job.minimumDegreeLevel
    additionalNotes = [string]$job.additionalNotes
  }
  $c = Api POST 'JobPosting/Create' $token $EJob $payload
  if (-not $c.status) { throw "JOB CREATE $($c.message)" }
  $jid = $c.data.jobPostingId
  $p = Api POST 'JobPosting/Publish' $token $EJob @{ jobPostingId = $jid }
  if (-not $p.status) { throw "JOB PUBLISH $($p.message)" }
  Write-Output "  JOB OK"
}

$loc1 = Ensure-Location $lab1 'HQ' 'Tehran Karagar St 210' '02166596699'
$loc2 = Ensure-Location $lab2 'Lab Sharifi' 'Tehran Valiasr' '02188776655'

Write-Output '=== LAB jobs 1 ==='
foreach ($j in @($data.jobs.lab1)) { Publish-Job $lab1 $j $loc1 }
Write-Output '=== LAB jobs 2 ==='
foreach ($j in @($data.jobs.lab2)) { Publish-Job $lab2 $j $loc2 }
Write-Output '=== LAB kits 1 ==='
foreach ($p in @($data.labKits.lab1)) { Publish-Product $lab1 $adminToken $p $attrCache }
Write-Output '=== LAB kits 2 ==='
foreach ($p in @($data.labKits.lab2)) { Publish-Product $lab2 $adminToken $p $attrCache }

Sql "UPDATE Users SET PasswordHash='$adminOrig' WHERE Username='admin'"
if ($ptnOrig -and $ptnOrig.StartsWith('$2a$')) {
  Sql "UPDATE Users SET PasswordHash='$ptnOrig' WHERE Username='ptn'"
}

Write-Output '=== DONE ==='
sqlcmd -S . -d LabConnect -U sa -P '1qaz!QAZ' -f 65001 -W -Q "SET NOCOUNT ON; SELECT Status, COUNT(*) Cnt FROM Products GROUP BY Status; SELECT COUNT(*) Jobs FROM JobPostingRequests; SELECT CAST(CreatedByUserId AS varchar(36)) U, COUNT(*) C FROM Products GROUP BY CreatedByUserId"
