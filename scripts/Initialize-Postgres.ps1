[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Database,
    [string]$HostName = 'localhost',
    [int]$Port = 5432,
    [Parameter(Mandatory = $true)][string]$UserName
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
foreach ($script in @('01_Schema.sql', '02_Procedures.sql')) {
    & psql --host=$HostName --port=$Port --username=$UserName --dbname=$Database --set=ON_ERROR_STOP=1 --single-transaction --file=(Join-Path $repo "Database/PostgreSQL/$script")
    if ($LASTEXITCODE -ne 0) { throw "Khởi tạo thất bại tại $script (exit $LASTEXITCODE)." }
}
Write-Host 'Đã cài schema và procedure. Không seed tài khoản/mật khẩu mặc định.'
