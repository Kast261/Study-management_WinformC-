# Run in Windows PowerShell from the downloaded repository. No password goes in arguments.
[CmdletBinding()]
param(
    [ValidateRange(1, 65535)][int]$Port = 5432,
    [string]$PsqlPath,
    [string]$ConfigPath = (Join-Path $PSScriptRoot '..\..\App.config'),
    [switch]$SkipDemoData
)

$ErrorActionPreference = 'Stop'
if (-not $PsqlPath) {
    $installed = Get-ChildItem (Join-Path $env:ProgramFiles 'PostgreSQL') -Directory |
        Where-Object { $_.Name -match '^\d+$' } |
        Sort-Object { [int]$_.Name } -Descending
    foreach ($version in $installed) {
        $candidate = Join-Path $version.FullName 'bin\psql.exe'
        if (Test-Path -LiteralPath $candidate) { $PsqlPath = $candidate; break }
    }
}
if (-not $PsqlPath -or -not (Test-Path -LiteralPath $PsqlPath -PathType Leaf)) {
    throw 'psql.exe not found. Install PostgreSQL command line tools or specify -PsqlPath.'
}
$ConfigPath = (Resolve-Path -LiteralPath $ConfigPath).Path
$config = New-Object System.Xml.XmlDocument
$config.PreserveWhitespace = $true
$config.Load($ConfigPath)
$connection = $config.SelectSingleNode('/configuration/connectionStrings/add[@name="ChuoiKetNoi"]')
if (-not $connection) { throw 'Config must contain connectionStrings/add named ChuoiKetNoi.' }

function Invoke-LocalPsql {
    param([string]$User, [string]$Database, [string]$Sql, [string]$File)
    $psqlArguments = @('-X', '-w', '-q', '-A', '-t', '--host', 'localhost', '--port', "$Port",
        '--username', $User, '--dbname', $Database, '-v', 'ON_ERROR_STOP=1')
    if ($File) { & $PsqlPath @psqlArguments --file $File }
    else { $Sql | & $PsqlPath @psqlArguments }
    if ($LASTEXITCODE -ne 0) { throw "PostgreSQL operation failed for $User/$Database. Configuration was not saved." }
}

$oldPassword = $env:PGPASSWORD
$oldEncoding = $env:PGCLIENTENCODING
$oldOutputEncoding = $OutputEncoding
try {
    $OutputEncoding = New-Object System.Text.UTF8Encoding($false)
    $env:PGCLIENTENCODING = 'UTF8'
    Write-Host 'Target: localhost, database btl_ql_hoc_tap, database login study_app.'
    Write-Host 'The study_app database password will be set to the password you enter below.'
    $adminSecret = Read-Host 'PostgreSQL administrator postgres password (from installation)' -AsSecureString
    $env:PGPASSWORD = (New-Object System.Management.Automation.PSCredential('postgres', $adminSecret)).GetNetworkCredential().Password
    if ([string]::IsNullOrEmpty($env:PGPASSWORD)) { throw 'Administrator password cannot be empty.' }
    $owner = (@(Invoke-LocalPsql -User postgres -Database postgres -Sql "SELECT pg_get_userbyid(datdba) FROM pg_database WHERE datname='btl_ql_hoc_tap';") -join '').Trim()
    if ($owner -and $owner -ne 'study_app') {
        throw "Database already exists with owner $owner. No ownership changes were made. See README.md for existing databases."
    }
    $appSecret = Read-Host 'Choose the PostgreSQL study_app password (not the app admin password)' -AsSecureString
    $appPassword = (New-Object System.Management.Automation.PSCredential('study_app', $appSecret)).GetNetworkCredential().Password
    if ([string]::IsNullOrEmpty($appPassword)) { throw 'study_app password cannot be empty.' }
    $roleSql = @'
SET standard_conforming_strings = on;
DO $setup$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname='study_app') THEN
        CREATE ROLE study_app LOGIN;
    END IF;
END;
$setup$;
'@
    $roleSql += "`nALTER ROLE study_app WITH LOGIN PASSWORD '" + $appPassword.Replace("'", "''") + "';"
    Invoke-LocalPsql -User postgres -Database postgres -Sql $roleSql
    if (-not $owner) {
        Invoke-LocalPsql -User postgres -Database postgres -Sql "CREATE DATABASE btl_ql_hoc_tap OWNER study_app ENCODING 'UTF8' TEMPLATE template0;"
    }

    $env:PGPASSWORD = $appPassword
    Invoke-LocalPsql -User study_app -Database btl_ql_hoc_tap -File (Join-Path $PSScriptRoot '01_Schema.sql')
    if (-not $SkipDemoData) {
        Invoke-LocalPsql -User study_app -Database btl_ql_hoc_tap -File (Join-Path $PSScriptRoot '02_DemoData.sql')
    }
    Invoke-LocalPsql -User study_app -Database btl_ql_hoc_tap -Sql 'SELECT current_user, current_database();'

    # The builder quotes semicolons/quotes; XmlDocument escapes XML characters in passwords.
    $builder = New-Object System.Data.Common.DbConnectionStringBuilder
    $builder['Host'] = 'localhost'
    $builder['Port'] = $Port
    $builder['Database'] = 'btl_ql_hoc_tap'
    $builder['Username'] = 'study_app'
    $builder['Password'] = $appPassword
    $backup = $ConfigPath + '.before-local-' + [guid]::NewGuid().ToString('N') + '.user'
    Copy-Item -LiteralPath $ConfigPath -Destination $backup
    $connection.SetAttribute('connectionString', $builder.ConnectionString)
    $connection.SetAttribute('providerName', 'Npgsql')
    $config.Save($ConfigPath)
    Write-Host "SUCCESS: database initialized; connection updated in $ConfigPath"
    Write-Host "Previous configuration is backed up in $backup (contains private connection settings)."
    Write-Host 'Rebuild the project, then run it. CreateDataBase.Init() can remain commented out.'
    if (-not $SkipDemoData) { Write-Host 'A fresh demo database has app accounts admin, giaovien1, hocsinh1; password 123456. Change demo passwords before real use.' }
}
finally {
    $env:PGPASSWORD = $oldPassword
    $env:PGCLIENTENCODING = $oldEncoding
    $OutputEncoding = $oldOutputEncoding
    $appPassword = $null
    $roleSql = $null
    $builder = $null
}
