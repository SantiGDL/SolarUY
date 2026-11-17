param([switch]$SinPausa)

$ErrorActionPreference = 'Stop'
$keycloakVerificado = $false
$actualizacionIntentada = $false

function Ejecutar-Docker([string[]]$Argumentos) {
    $resultado = & docker @Argumentos 2>$null
    if ($LASTEXITCODE -ne 0) { throw 'Fallo Docker. Comproba Docker Desktop, Compose y los servicios del entorno habitual.' }
    return ($resultado -join "`n")
}

function Consultar-Keycloak([string]$Metodo, [string]$Ruta, $Cuerpo, [string]$Token) {
    $opciones = @{
        Uri = 'http://127.0.0.1:8080' + $Ruta
        Method = $Metodo
        TimeoutSec = 15
        UseBasicParsing = $true
        ErrorAction = 'Stop'
    }
    if ($Token) { $opciones.Headers = @{ Authorization = 'Bearer ' + $Token } }
    if ($null -ne $Cuerpo) {
        if ($Token) {
            $opciones.ContentType = 'application/json; charset=utf-8'
            $opciones.Body = [System.Text.Encoding]::UTF8.GetBytes(($Cuerpo | ConvertTo-Json -Depth 50 -Compress))
        } else {
            $opciones.ContentType = 'application/x-www-form-urlencoded'
            $opciones.Body = $Cuerpo
        }
    }
    try { return Invoke-RestMethod @opciones } catch {
        throw 'No se pudo consultar o actualizar Keycloak. Comproba que este iniciado, el acceso administrativo y el realm/cliente solaruy. No se restablecieron credenciales.'
    }
}

function Preparar-Env([string]$Ruta, [string]$Subred) {
    $bytes = [System.IO.File]::ReadAllBytes($Ruta)
    $encoding = New-Object System.Text.UTF8Encoding($false, $true)
    $bom = 0
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191) { $bom = 3 }
    elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 255 -and $bytes[1] -eq 254) { $encoding = [System.Text.Encoding]::Unicode; $bom = 2 }
    elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 254 -and $bytes[1] -eq 255) { $encoding = [System.Text.Encoding]::BigEndianUnicode; $bom = 2 }
    try { $texto = $encoding.GetString($bytes, $bom, $bytes.Length - $bom) } catch {
        throw '.env debe tener codificacion UTF-8 o UTF-16. No se modifico.'
    }
    $patron = '(?m)^(?<prefijo>[ \t]*HA_PROXY_NETWORK[ \t]*=[ \t]*)(?<valor>[^\r\n]*)'
    $asignaciones = [regex]::Matches($texto, $patron)
    if ($asignaciones.Count -gt 1) { throw '.env contiene HA_PROXY_NETWORK duplicado. Deja una sola asignacion y reintenta.' }
    $nuevo = $texto
    if ($asignaciones.Count -eq 1) {
        $asignacion = $asignaciones[0]
        $valor = $asignacion.Groups['valor'].Value
        $partes = [regex]::Match($valor, '^(?<dato>"[^"\r\n]*"|''[^''\r\n]*''|[^#\s"'']*)(?<resto>[ \t]*(?:#.*)?)$')
        if (-not $partes.Success) { throw 'Formato ambiguo de HA_PROXY_NETWORK en .env. Corregi esa asignacion y reintenta.' }
        $dato = $partes.Groups['dato'].Value.Trim('"', "'")
        if ($dato -ne $Subred) {
            $inicio = $asignacion.Groups['valor'].Index
            $nuevo = $texto.Substring(0, $inicio) + $Subred + $partes.Groups['resto'].Value + $texto.Substring($inicio + $valor.Length)
        }
    } else {
        $salto = "`n"
        if ($texto.Contains("`r`n")) { $salto = "`r`n" }
        if ($texto.Length -gt 0 -and -not $texto.EndsWith("`n")) { $nuevo += $salto }
        $nuevo += 'HA_PROXY_NETWORK=' + $Subred + $salto
    }
    $salida = $bytes
    if ($nuevo -cne $texto) {
        $prefijo = [byte[]]@()
        if ($bom -gt 0) { $prefijo = $bytes[0..($bom - 1)] }
        $salida = [byte[]]($prefijo + $encoding.GetBytes($nuevo))
    }
    return @{ Original = $bytes; Nuevo = $salida; Cambio = ($nuevo -cne $texto) }
}

function Preparar-Cliente($Cliente) {
    $redirect = @($Cliente.redirectUris | Where-Object { $null -ne $_ })
    $origins = @($Cliente.webOrigins | Where-Object { $null -ne $_ })
    $atributos = @{}
    if ($null -ne $Cliente.attributes) {
        foreach ($propiedad in $Cliente.attributes.PSObject.Properties) { $atributos[$propiedad.Name] = $propiedad.Value }
    }
    $cambio = $false
    if ($redirect -cnotcontains 'http://localhost:7000/signin-oidc') { $redirect += 'http://localhost:7000/signin-oidc'; $cambio = $true }
    if ($origins -cnotcontains 'http://localhost:7000') { $origins += 'http://localhost:7000'; $cambio = $true }
    $logout = @($atributos['post.logout.redirect.uris'] -split '##' | Where-Object { $_ -ne '' })
    if ($logout -cnotcontains 'http://localhost:7000/*') {
        $anterior = [string]$atributos['post.logout.redirect.uris']
        if ($anterior -and -not $anterior.EndsWith('##')) { $anterior += '##' }
        $atributos['post.logout.redirect.uris'] = $anterior + 'http://localhost:7000/*'
        $cambio = $true
    }
    return @{ Cambio = $cambio; Cuerpo = @{ redirectUris = $redirect; webOrigins = $origins; attributes = $atributos } }
}

try {
    $raiz = Split-Path -Parent $PSScriptRoot
    $rutaEnv = Join-Path $raiz '.env'
    if (-not (Test-Path -LiteralPath $rutaEnv -PathType Leaf)) { throw 'Falta .env. Ejecuta primero Configurar-EntornoLocal.cmd y prepara el entorno habitual.' }
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw 'Falta Docker. Instala e inicia Docker Desktop con contenedores Linux.' }
    Push-Location $raiz
    try {
        $null = Ejecutar-Docker @('info', '--format', '{{.OSType}}')
        $null = Ejecutar-Docker @('compose', 'version')
        $contenedores = @()
        foreach ($servicio in @('mysql', 'keycloak')) {
            $ids = @( (Ejecutar-Docker @('compose', '-f', 'compose.yaml', 'ps', '-a', '-q', $servicio)) -split "`n" | Where-Object { $_ } )
            if ($ids.Count -ne 1) { throw 'No se encontro un unico MySQL/Keycloak del proyecto. Inicia el entorno habitual con su mismo proyecto Compose.' }
            $contenedores += @( (Ejecutar-Docker @('inspect', $ids[0]) | ConvertFrom-Json) )[0]
        }
        $proyecto = $contenedores[0].Config.Labels.'com.docker.compose.project'
        if (-not $proyecto -or $contenedores[1].Config.Labels.'com.docker.compose.project' -cne $proyecto) { throw 'Los contenedores no pertenecen a un unico proyecto Compose.' }
        $redes = @()
        foreach ($red in $contenedores[0].NetworkSettings.Networks.PSObject.Properties) {
            if ($contenedores[1].NetworkSettings.Networks.PSObject.Properties.Name -ccontains $red.Name) {
                $detalle = @( (Ejecutar-Docker @('network', 'inspect', $red.Name) | ConvertFrom-Json) )[0]
                if ($detalle.Labels.'com.docker.compose.project' -ceq $proyecto -and $detalle.Labels.'com.docker.compose.network' -ceq 'default') { $redes += $detalle }
            }
        }
        if ($redes.Count -ne 1) { throw 'No se detecto una unica red default compartida del proyecto. Revisa la configuracion Compose; no se crearon redes.' }
        $subredes = @($redes[0].IPAM.Config | Where-Object { $_.Subnet -match '^\d{1,3}(\.\d{1,3}){3}/\d{1,2}$' } | ForEach-Object { $_.Subnet })
        if ($subredes.Count -ne 1) { throw 'La red no tiene una unica subred IPv4. No se eligio una subred arbitraria.' }
        $subred = $subredes[0]
        $ip = $null
        if (-not [System.Net.IPAddress]::TryParse(($subred -split '/')[0], [ref]$ip) -or [int](($subred -split '/')[1]) -gt 32) { throw 'Subred IPv4 invalida.' }
        $planEnv = Preparar-Env $rutaEnv $subred
        if (-not $contenedores[1].State.Running) { throw 'Keycloak esta detenido. Inicia el entorno habitual antes de ejecutar este configurador.' }
        $credenciales = @{}
        foreach ($variable in $contenedores[1].Config.Env) {
            if ($variable -match '^(KC_BOOTSTRAP_ADMIN_USERNAME|KC_BOOTSTRAP_ADMIN_PASSWORD)=(.*)$') { $credenciales[$matches[1]] = $matches[2] }
        }
        if (-not $credenciales['KC_BOOTSTRAP_ADMIN_USERNAME'] -or -not $credenciales['KC_BOOTSTRAP_ADMIN_PASSWORD']) { throw 'No se encontraron credenciales administrativas locales en Keycloak. Revisa la configuracion del entorno habitual.' }
        $null = Consultar-Keycloak 'GET' '/realms/solaruy/.well-known/openid-configuration' $null $null
        $autenticacion = Consultar-Keycloak 'POST' '/realms/master/protocol/openid-connect/token' @{
            grant_type = 'password'; client_id = 'admin-cli'
            username = $credenciales['KC_BOOTSTRAP_ADMIN_USERNAME']; password = $credenciales['KC_BOOTSTRAP_ADMIN_PASSWORD']
        } $null
        $token = $autenticacion.access_token
        if (-not $token) { throw 'Keycloak no entrego un token administrativo. Revisa el acceso administrativo local.' }
        $null = Consultar-Keycloak 'GET' '/admin/realms/solaruy' $null $token
        $clientes = @(Consultar-Keycloak 'GET' '/admin/realms/solaruy/clients?clientId=solaruy' $null $token | Where-Object { $_.clientId -ceq 'solaruy' })
        if ($clientes.Count -ne 1) { throw 'No existe un unico cliente solaruy en el realm solaruy. Completa su preparacion habitual.' }
        $rutaCliente = '/admin/realms/solaruy/clients/' + [uri]::EscapeDataString($clientes[0].id)
        $cliente = Consultar-Keycloak 'GET' $rutaCliente $null $token
        $planCliente = Preparar-Cliente $cliente
        # Verificar nuevamente .env antes de cualquier escritura remota.
        if ([Convert]::ToBase64String([IO.File]::ReadAllBytes($rutaEnv)) -cne [Convert]::ToBase64String($planEnv.Original)) { throw '.env cambio durante la ejecucion. Reintenta sin editarlo simultaneamente.' }
        if ($planCliente.Cambio) {
            $actualizacionIntentada = $true
            $null = Consultar-Keycloak 'PUT' $rutaCliente $planCliente.Cuerpo $token
            $verificado = Consultar-Keycloak 'GET' $rutaCliente $null $token
            $comprobacion = Preparar-Cliente $verificado
            if ($comprobacion.Cambio) { throw 'No se pudo confirmar la configuracion de Keycloak. .env no se modifico.' }
            foreach ($nombre in $planCliente.Cuerpo.attributes.Keys) {
                if ($verificado.attributes.$nombre -cne $planCliente.Cuerpo.attributes[$nombre]) { throw 'No se pudo confirmar la conservacion de atributos. .env no se modifico.' }
            }
            foreach ($entrada in $planCliente.Cuerpo.redirectUris) { if (@($verificado.redirectUris) -cnotcontains $entrada) { throw 'No se pudo confirmar una redirect URI anterior.' } }
            foreach ($entrada in $planCliente.Cuerpo.webOrigins) { if (@($verificado.webOrigins) -cnotcontains $entrada) { throw 'No se pudo confirmar un web origin anterior.' } }
            Write-Host 'Keycloak: entradas faltantes agregadas y verificadas.'
        } else { Write-Host 'Keycloak: las tres entradas ya existen; sin cambios.' }
        $keycloakVerificado = $true
        if ($planEnv.Cambio) {
            if ([Convert]::ToBase64String([IO.File]::ReadAllBytes($rutaEnv)) -cne [Convert]::ToBase64String($planEnv.Original)) { throw '.env cambio durante la ejecucion. Reintenta sin editarlo simultaneamente.' }
            [IO.File]::WriteAllBytes($rutaEnv, $planEnv.Nuevo)
            Write-Host 'HA_PROXY_NETWORK actualizado en .env; las demas variables se conservaron.'
        } else { Write-Host 'HA_PROXY_NETWORK ya coincide con la red existente; .env sin cambios.' }
        Write-Host 'Preparacion terminada. Podes iniciar HA con compose.yaml y compose.ha.yaml. No se reiniciaron servicios.'
    } finally { Pop-Location }
} catch {
    Write-Host ('ERROR: ' + $_.Exception.Message)
    if ($keycloakVerificado) { Write-Host 'Keycloak quedo verificado; falta completar la escritura de .env. Corregi el error y reejecuta.' }
    elseif ($actualizacionIntentada) { Write-Host 'Se intento actualizar Keycloak, pero no se confirmo el resultado completo. .env no se modifico; revisa el error y reejecuta para verificar las entradas.' }
    exit 1
} finally {
    $token = $null; $autenticacion = $null; $credenciales = $null; $contenedores = $null
    if (-not $SinPausa) { $null = Read-Host 'Presiona Enter para cerrar' }
}
