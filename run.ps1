<#
.SYNOPSIS
    Perintah sehari-hari TechVerse X untuk Windows.

.DESCRIPTION
    Kembaran Makefile untuk mesin tanpa `make`. Keduanya sengaja dijaga sama:
    Makefile untuk Linux/WSL/macOS, berkas ini untuk Windows. CI tidak memanggil
    keduanya - ci.yml menjalankan perintahnya sendiri.

.EXAMPLE
    .\run.ps1 up
    .\run.ps1 verify
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('help', 'up', 'down', 'reset', 'migrate', 'migration', 'db-script', 'seed', 'api', 'web', 'build', 'test', 'tautan', 'halaman', 'verify', 'ci')]
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
            @{ n = 'test';      d = 'Menjalankan uji .NET (uji integrasi butuh up + migrate)' }
            @{ n = 'tautan';    d = 'Memeriksa tautan di berkas Markdown (tanpa jaringan)' }
            @{ n = 'halaman';   d = 'Gerbang ADR-024 di halaman jadinya (butuh web + API hidup)' }
            @{ n = 'verify';    d = 'Gerbang yang sama dengan CI (butuh up + migrate)' }
            @{ n = 'ci';        d = 'CI mandiri: verify + pindai rahasia + job citra (butuh Docker)' }
        ) | ForEach-Object { Write-Host ('  {0,-12} {1}' -f $_.n, $_.d) }
        Write-Host ''
        Write-Host '  Uji integrasi MENULIS BARIS ke PostgreSQL sungguhan, jadi jalankan' -ForegroundColor DarkGray
        Write-Host '  .\run.ps1 up LALU .\run.ps1 migrate sebelum test maupun verify.' -ForegroundColor DarkGray
        Write-Host '  Tanpa migrate, galatnya: relation "technology.technologies" does not exist.' -ForegroundColor DarkGray
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
        # Satu implementasi untuk Windows DAN Linux/WSL/macOS: database/seeds/seed.mjs.
        # Sampai 2026-09-17 blok ini punya kembaran sendiri di seed.sh, dan
        # kembaran itu menyimpang tanpa ada yang merah - dijalankan ke basis data
        # kosong, ia mencetak 'ada' untuk tiga contoh yang tidak pernah dibuat.
        # Seluruh alasan yang dulu tinggal di blok ini pindah ke kepala berkas itu.
        # API_BASE_URL diwariskan ke node apa adanya.
        node database/seeds/seed.mjs
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }

    'api'   { dotnet run --project $ApiProject }
    'web'   { npm run dev:web }
    'build' { Invoke-Step 'Build' { dotnet build TechVerseX.slnx } }
    'test'  { Invoke-Step 'Uji' { dotnet test TechVerseX.slnx } }

    'tautan' {
        # Satu implementasi untuk Windows DAN Linux/WSL/macOS, alasan yang sama
        # dengan `seed`: berkas .mjs yang sama dipanggil run.ps1, Makefile, dan
        # ci.yml, jadi ketiganya tidak bisa menyimpang diam-diam.
        node .github/scripts/cek-tautan-markdown.mjs
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }

    'halaman' {
        # SENGAJA di luar `verify`: ia menuntut web yang sudah dibangun DAN API
        # yang hidup, sama seperti `seed`. Di CI ia TETAP jalan - job `citra`
        # menyalakan docker-compose.prod.yml lalu memanggil pemindai yang sama,
        # jadi bentuk produksinya dijaga otomatis; perintah di bawah untuk bentuk
        # PENGEMBANGAN, yang punya topik contoh dan karena itu punya relasi.
        # Urutan yang dituntutnya:
        #   .\run.ps1 up  ->  .\run.ps1 migrate  ->  .\run.ps1 api
        #   lalu, di jendela lain: npm run build:web
        #   lalu: cd apps/web; $env:API_BASE_URL='http://localhost:5080'
        #         npx next start -p 3310
        # Bentuk PRODUKSI yang diukur, bukan `next dev` - aturan teks pembaca
        # ADR-024 memang membedakan keduanya (lihat apps/web/src/lib/lingkungan.ts).
        if (-not $env:WEB_BASE_URL) { $env:WEB_BASE_URL = 'http://localhost:3310' }
        if (-not $env:API_BASE_URL) { $env:API_BASE_URL = 'http://localhost:5080' }
        node .github/scripts/periksa-halaman-web.mjs
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }

    'verify' {
        # BUKAN lewat Invoke-Step: pemeriksa ini fungsi PowerShell murni yang tidak
        # menyentuh $LASTEXITCODE, dan $LASTEXITCODE masih kosong di langkah pertama.
        Write-Host 'Periksa global.json' -ForegroundColor Cyan
        Test-GlobalJson

        Invoke-Step 'Cek tautan Markdown' { node .github/scripts/cek-tautan-markdown.mjs }
        Invoke-Step 'Build ketat (Release)' { dotnet build TechVerseX.slnx --configuration Release }
        Invoke-Step 'Uji .NET' { dotnet test TechVerseX.slnx --no-build --configuration Release }
        Invoke-Step 'Lint web' { npm run lint:web }
        Invoke-Step 'Build web' { npm run build:web }
        Write-Host ''
        Write-Host 'Semua gerbang hijau.' -ForegroundColor Green
    }

    'ci' {
        # CI MANDIRI (ADR-026): seluruh gerbang ci.yml di mesin sendiri, nol menit
        # Actions. `verify` menanggung job backend dan frontend; skrip bash-nya
        # menanggung pindai rahasia dan job `citra`. Satu skrip untuk Windows
        # (Git Bash) DAN Linux/WSL/macOS, alasan yang sama dengan `seed`.
        & $PSCommandPath verify
        if (-not $?) { exit 1 }

        # GIT BASH, bukan `bash`. Di Windows `bash` di PATH adalah
        # C:\Windows\System32\bash.exe - bash WSL - dan distro WSL belum tentu
        # punya `node`: diukur 2026-09-28, di mesin ini tidak ada, jadi pemindai
        # halaman pasti gagal di sana. Git Bash selalu ada di samping git.
        $gitRoot = Split-Path (Split-Path (Get-Command git).Source)
        $gitBash = @("$gitRoot\bin\bash.exe", "$gitRoot\usr\bin\bash.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1
        if (-not $gitBash) { throw "Git Bash tidak ditemukan di $gitRoot" }

        # gitleaks dan docker menulis log ke stderr. PowerShell 5.1 mengubah
        # setiap baris stderr program native jadi galat begitu keluarannya
        # dialihkan, dan dengan 'Stop' baris INF pertama gitleaks menghentikan
        # gerbang yang sebenarnya lulus - terjadi 2026-09-28. Yang menentukan
        # tetap exit code skripnya.
        Write-Host '-> CI mandiri: pindai rahasia + job citra' -ForegroundColor Cyan
        $ErrorActionPreference = 'Continue'
        & $gitBash .github/scripts/ci-mandiri.sh
        $kode = $LASTEXITCODE
        $ErrorActionPreference = 'Stop'
        if ($kode -ne 0) { throw "Gagal: CI mandiri (exit $kode)" }
    }
}
