param(
    [Parameter(Mandatory = $true)][string]$Email,
    [Parameter(Mandatory = $true)][string]$Password,
    [Parameter(Mandatory = $true)][int]$UserId,
    [Parameter(Mandatory = $true)][int]$HouseId,
    [string]$BaseUrl = 'http://localhost:5118'
)

$ErrorActionPreference = 'Stop'
$BaseUrl = $BaseUrl.TrimEnd('/')

function Assert-Request {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Path,
        [int[]]$Expected,
        [hashtable]$Headers = @{},
        [object]$Body = $null
    )

    $status = 0
    try {
        $params = @{
            Method = $Method
            Uri = "$BaseUrl$Path"
            Headers = $Headers
            UseBasicParsing = $true
        }
        if ($null -ne $Body) {
            $params.ContentType = 'application/json; charset=utf-8'
            $json = $Body | ConvertTo-Json -Depth 8
            $params.Body = [System.Text.Encoding]::UTF8.GetBytes($json)
        }
        $response = Invoke-WebRequest @params
        $status = [int]$response.StatusCode
    }
    catch {
        if ($_.Exception.Response) {
            $status = [int]$_.Exception.Response.StatusCode
        }
        else {
            throw
        }
    }

    if ($Expected -notcontains $status) {
        throw "$Name failed: expected $($Expected -join ', '), received $status."
    }
    Write-Host "[OK] $Name ($status)"
}

$loginJson = @{ email = $Email; password = $Password } | ConvertTo-Json
$login = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/Auth/Login" -ContentType 'application/json; charset=utf-8' -Body ([System.Text.Encoding]::UTF8.GetBytes($loginJson))
if ([string]::IsNullOrWhiteSpace($login.token)) {
    throw 'Login succeeded without an access token.'
}
$auth = @{ Authorization = "Bearer $($login.token)" }

Assert-Request -Name 'Privacy page' -Method Get -Path '/privacy.html' -Expected 200
Assert-Request -Name 'Account deletion page' -Method Get -Path '/account-deletion.html' -Expected 200
Assert-Request -Name 'Unauthenticated finance access' -Method Get -Path "/api/Expenses/GetExpenses/$HouseId" -Expected 401
Assert-Request -Name 'User houses' -Method Get -Path "/api/Houses/GetUserHouses/$UserId" -Expected 200 -Headers $auth
Assert-Request -Name 'House members' -Method Get -Path "/api/Houses/$HouseId/members" -Expected 200 -Headers $auth
Assert-Request -Name 'House expenses' -Method Get -Path "/api/Expenses/GetExpenses/$HouseId" -Expected 200 -Headers $auth
Assert-Request -Name 'House receipts' -Method Get -Path "/api/Receipts/ByHouse/$HouseId" -Expected 200 -Headers $auth
Assert-Request -Name 'Deletion request privacy response' -Method Post -Path '/api/account-deletion/request' -Expected 202 -Body @{ email = "roomora-smoke-$([guid]::NewGuid().ToString('N'))@example.com" }

Write-Host 'Roomora API smoke test completed successfully.'
