<#
.SYNOPSIS
    Perintah sehari-hari TechVerse X untuk Windows.

.DESCRIPTION
    Kembaran Makefile untuk mesin tanpa `make`. Keduanya sengaja dijaga sama:
    Makefile dipakai CI dan Linux/WSL, berkas ini dipakai di Windows.

.EXAMPLE
    .\run.ps1 up
    .\run.ps1 verify
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('help', 'up', 'down', 'reset', 'migrate', 'migration', 'db-script', 'seed', 'api', 'web', 'build', 'test', 'verify')]
    [string]$Command = 'help',

    [Parameter(Position = 1)]
    [string]$Name
)

$ErrorActionPreference = 'Stop'
Set-Location -Path $PSScriptRoot

$TechnologyProject = 'services/technology/TechVerseX.TechnologyService.csproj'
$ApiProject = 'apps/api/TechVerseX.Api.csproj'

function Invoke-Step {
    param([string]$Description, [scriptblock]$Action)

    Write-Host "-> $Description" -ForegroundColor Cyan
    & $Action
    if ($LASTEXITCODE -ne 0) {
        throw "Gagal: $Description (exit $LASTEXITCODE)"
    }
}

function Test-GlobalJson {
    # `setup-dotnet` menolak `sdk.version` yang bukan versi SDK utuh begitu
    # `rollForward` disebut, tetapi `dotnet` di mesin yang SDK-nya sudah terpasang
    # menerimanya diam-diam. Lubang itu karena itu cuma kelihatan di runner bersih
    # dan pernah memerahkan CI sementara verifikasi lokal hijau (issue #24).
    $sdk = (Get-Content -Raw -Path 'global.json' | ConvertFrom-Json).sdk

    if ($sdk.rollForward -and $sdk.version -notmatch '^\d+\.\d+\.\d{3}(-[0-9A-Za-z.\-]+)?$') {
        throw "global.json: sdk.version '$($sdk.version)' bukan versi SDK utuh. Dengan rollForward '$($sdk.rollForward)' .NET menuntut pita fitur, misalnya 10.0.100 - bukan versi runtime seperti 10.0.0."
    }

    Write-Host "  global.json sdk.version = $($sdk.version)" -ForegroundColor DarkGray
}

switch ($Command) {
    'help' {
        Write-Host ''
        Write-Host 'Perintah TechVerse X' -ForegroundColor Green
        Write-Host ''
        @(
            @{ n = 'up';        d = 'Menyalakan Postgres + Redis' }
            @{ n = 'down';      d = 'Mematikan infrastruktur lokal' }
            @{ n = 'reset';     d = 'Mematikan DAN menghapus datanya' }
            @{ n = 'migrate';   d = 'Menjalankan migrasi basis data' }
            @{ n = 'migration'; d = 'Migrasi baru: .\run.ps1 migration NamaMigrasi' }
            @{ n = 'db-script'; d = 'Menulis SQL idempoten ke database/migrations/' }
            @{ n = 'seed';      d = 'Mengisi contoh lewat API (API harus jalan)' }
            @{ n = 'api';       d = 'Menjalankan API di http://localhost:5080' }
            @{ n = 'web';       d = 'Menjalankan web di http://localhost:3000' }
            @{ n = 'build';     d = 'Build solusi .NET' }
            @{ n = 'test';      d = 'Menjalankan uji .NET' }
            @{ n = 'verify';    d = 'Gerbang yang sama dengan CI' }
        ) | ForEach-Object { Write-Host ('  {0,-12} {1}' -f $_.n, $_.d) }
        Write-Host ''
    }

    'up' {
        Invoke-Step 'Menyalakan Postgres + Redis' { docker compose up -d }
        docker compose ps
    }

    'down' { Invoke-Step 'Mematikan infrastruktur' { docker compose down } }

    'reset' {
        Write-Host 'Ini menghapus SELURUH data lokal.' -ForegroundColor Yellow
        Invoke-Step 'Menghapus container dan volume' { docker compose down -v }
    }

    'migrate' {
        Invoke-Step 'Menjalankan migrasi' {
            dotnet ef database update --project $TechnologyProject --startup-project $ApiProject
        }
    }

    'migration' {
        if (-not $Name) { throw 'Pakai: .\run.ps1 migration NamaMigrasi' }
        Invoke-Step "Membuat migrasi $Name" {
            dotnet ef migrations add $Name --project $TechnologyProject --startup-project $ApiProject --output-dir Infrastructure/Persistence/Migrations
        }
    }

    'db-script' {
        Invoke-Step 'Menulis SQL migrasi' {
            dotnet ef migrations script --idempotent --project $TechnologyProject --startup-project $ApiProject --output database/migrations/technology.sql
        }
    }

    'seed' {
        $api = if ($env:API_BASE_URL) { $env:API_BASE_URL } else { 'http://localhost:5080' }
        Write-Host "Mengisi contoh ke $api ..." -ForegroundColor Cyan

        $samples = @(
            @{ name = 'AI Agents'; summary = 'Pergeseran dari AI yang menjawab ke AI yang mengerjakan.'; category = 'AI & Machine Learning' }
            @{ name = 'Edge AI'; summary = 'AI berjalan langsung di perangkat, bukan di awan.'; category = 'Belum diputuskan' }
            @{ name = 'Quantum Computing'; summary = 'Era qubit logis; keunggulan komersial belum ada.'; category = 'Quantum Computing' }
            @{ name = 'Digital Twin'; summary = 'Kembaran digital dari objek atau sistem nyata.'; category = 'IoT' }
            @{ name = 'Cybersecurity Berbasis AI'; summary = 'Ancaman otomatis, maka pertahanannya ikut otomatis.'; category = 'Cybersecurity' }
        )

        foreach ($sample in $samples) {
            try {
                Invoke-RestMethod -Uri "$api/api/v1/technologies" -Method Post -ContentType 'application/json' -Body ($sample | ConvertTo-Json) | Out-Null
                Write-Host ('  dibuat  {0}' -f $sample.name)
            }
            catch {
                $code = $_.Exception.Response.StatusCode.value__
                if ($code -eq 409) { Write-Host ('  ada     {0}' -f $sample.name) }
                else { Write-Host ('  GAGAL   {0} (HTTP {1})' -f $sample.name, $code) -ForegroundColor Red }
            }
        }
    }

    'api'   { dotnet run --project $ApiProject }
    'web'   { npm run dev:web }
    'build' { Invoke-Step 'Build' { dotnet build TechVerseX.slnx } }
    'test'  { Invoke-Step 'Uji' { dotnet test TechVerseX.slnx } }

    'verify' {
        # BUKAN lewat Invoke-Step: pemeriksa ini fungsi PowerShell murni yang tidak
        # menyentuh $LASTEXITCODE, dan $LASTEXITCODE masih kosong di langkah pertama.
        Write-Host 'Periksa global.json' -ForegroundColor Cyan
        Test-GlobalJson

        Invoke-Step 'Build ketat (Release)' { dotnet build TechVerseX.slnx --configuration Release }
        Invoke-Step 'Uji .NET' { dotnet test TechVerseX.slnx --no-build --configuration Release }
        Invoke-Step 'Lint web' { npm run lint:web }
        Invoke-Step 'Build web' { npm run build:web }
        Write-Host ''
        Write-Host 'Semua gerbang hijau.' -ForegroundColor Green
    }
}
