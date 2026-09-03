param([string]$Base = "http://localhost:5099")
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Write-Output "=== GamerBacklog smoke test ($Base) ==="

# 1. GET / (landing)
$home1 = Invoke-WebRequest "$Base/" -UseBasicParsing
Write-Output ("GET / -> " + $home1.StatusCode)

# 2. GET /Games
$games = Invoke-WebRequest "$Base/Games" -UseBasicParsing
Write-Output ("GET /Games -> " + $games.StatusCode)

# 3. GET /Games/1
$g1 = Invoke-WebRequest "$Base/Games/1" -UseBasicParsing
Write-Output ("GET /Games/1 -> " + $g1.StatusCode)

# 4. GET /u/demo (perfil publico)
$prof = Invoke-WebRequest "$Base/u/demo" -UseBasicParsing
Write-Output ("GET /u/demo -> " + $prof.StatusCode)

# 4b. capa SVG
$cover = Invoke-WebRequest "$Base/covers/Elden%20Ring" -UseBasicParsing
Write-Output ("GET /covers/Elden%20Ring -> " + $cover.StatusCode + " (svg: " + ($cover.Content -match "<svg") + ")")

# 5. GET login + antiforgery + POST login
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$loginPage = Invoke-WebRequest "$Base/Identity/Account/Login" -WebSession $session -UseBasicParsing
Write-Output ("GET /Identity/Account/Login -> " + $loginPage.StatusCode)

$token = ($loginPage.InputFields | Where-Object { $_.name -eq '__RequestVerificationToken' } | Select-Object -First 1).value
if (-not $token) { throw "antiforgery token nao encontrado" }

$post = Invoke-WebRequest "$Base/Identity/Account/Login" -Method Post -WebSession $session -Body @{
    Login = "demo@GamerBacklog.dev"
    Password = "Demo123!"
    RememberMe = "false"
    __RequestVerificationToken = $token
} -UseBasicParsing -MaximumRedirection 5
Write-Output ("POST login -> " + $post.StatusCode)

$home2 = Invoke-WebRequest "$Base/" -WebSession $session -UseBasicParsing
$loggedOk = ($home2.Content -match "Sair") -and ($home2.Content -match "Continuar Jogando")
Write-Output ("GET / (logado) -> " + $home2.StatusCode + " | contem 'Sair' e 'Continuar Jogando': " + $loggedOk)
if (-not $loggedOk) { throw "login nao persistiu" }

# 6. POST /Games/1/SetStatus (Jogado = 3)
$g1b = Invoke-WebRequest "$Base/Games/1" -WebSession $session -UseBasicParsing
$token2 = [regex]::Match($g1b.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
if (-not $token2) { throw "antiforgery do /Games/1 nao encontrado" }
$statusPost = Invoke-WebRequest "$Base/Games/1/SetStatus" -Method Post -WebSession $session -Body @{
    status = "3"
    returnUrl = "/Games/1"
    __RequestVerificationToken = $token2
} -UseBasicParsing -MaximumRedirection 5
Write-Output ("POST /Games/1/SetStatus -> " + $statusPost.StatusCode)

# 7. Biblioteca reflete a mudanca
$lib = Invoke-WebRequest "$Base/Library?status=Played" -WebSession $session -UseBasicParsing
$libOk = $lib.Content -match "Elden Ring"
Write-Output ("GET /Library?status=Played -> " + $lib.StatusCode + " | contem 'Elden Ring': " + $libOk)
if (-not $libOk) { throw "status nao apareceu na biblioteca" }

# 8. Settings e notificacoes (logado)
$settings = Invoke-WebRequest "$Base/Settings/Integrations" -WebSession $session -UseBasicParsing
Write-Output ("GET /Settings/Integrations -> " + $settings.StatusCode)
$notifs = Invoke-WebRequest "$Base/Notifications" -WebSession $session -UseBasicParsing
Write-Output ("GET /Notifications -> " + $notifs.StatusCode)

# 9. Perfil proprio e busca no catalogo
$me = Invoke-WebRequest "$Base/u/demo" -WebSession $session -UseBasicParsing
Write-Output ("GET /u/demo (logado) -> " + $me.StatusCode)
$search = Invoke-WebRequest "$Base/Games?q=elden" -UseBasicParsing
$searchOk = $search.Content -match "Elden Ring"
Write-Output ("GET /Games?q=elden -> " + $search.StatusCode + " | achou 'Elden Ring': " + $searchOk)

Write-Output "=== SMOKE OK ==="
