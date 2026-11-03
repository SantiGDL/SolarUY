$ErrorActionPreference = 'Stop'

function Preguntar-SiNo([string]$pregunta) {
    while ($true) {
        $respuesta = (Read-Host "$pregunta [s/n]").Trim().ToLowerInvariant()
        if ($respuesta -eq 's' -or $respuesta -eq 'si' -or $respuesta -eq 'sí') { return $true }
        if ($respuesta -eq 'n' -or $respuesta -eq 'no') { return $false }
        Write-Host 'Responde s o n.'
    }
}

function Pedir-Usuario([string]$nombre, [string]$predeterminado) {
    Write-Host "$nombre predeterminado: $predeterminado"
    if (-not (Preguntar-SiNo '¿Querés cambiarlo?')) { return $predeterminado }
    while ($true) {
        $valor = Read-Host "Nuevo $nombre"
        if ($valor -cmatch '^[A-Za-z][A-Za-z0-9_]{0,31}$' -and -not ($nombre -eq 'MYSQL_USER' -and $valor -ieq 'root')) { return $valor }
        Write-Host 'Usá entre 1 y 32 caracteres: letra inicial, letras, números o guion bajo. MYSQL_USER no puede ser root.'
    }
}

function Pedir-Clave([string]$nombre) {
    while ($true) {
        $valor = Read-Host "$nombre (visible)"
        $confirmacion = Read-Host "Confirmar $nombre (visible)"
        if ([string]::IsNullOrEmpty($valor) -or $valor -match '[\x00-\x1f\x7f]') {
            Write-Host 'La contraseña no puede estar vacía ni contener caracteres de control.'
            continue
        }
        if ($valor -cne $confirmacion) { Write-Host 'Las contraseñas no coinciden. Intentá nuevamente.'; continue }
        return $valor
    }
}

function Formato-Env([string]$valor) {
    return "'" + $valor.Replace("'", "\'").Replace('$', '$$') + "'"
}

try {
    $raiz = Split-Path -Parent $PSScriptRoot
    $envPath = Join-Path $raiz '.env'
    $ejemploPath = Join-Path $raiz '.env.example'
    $proyectoPath = Join-Path $raiz 'SolarUy/SolarUy.csproj'

    if (Test-Path -LiteralPath $envPath) {
        Write-Host 'Ya existe .env. Por seguridad no se puede continuar y no se modificó User Secrets.'
        Write-Host 'Eliminá .env manualmente y volvé a ejecutar el script, o completá la configuración manual siguiendo Readme.md.'
        exit 1
    }
    if (-not (Test-Path -LiteralPath $ejemploPath) -or -not (Test-Path -LiteralPath $proyectoPath)) { throw 'Falta .env.example o SolarUy/SolarUy.csproj.' }
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Falta dotnet.' }
    if (-not ((& dotnet --list-sdks) | Where-Object { $_ -match '^10\.' })) { throw 'Se requiere el SDK .NET 10.' }
    if (-not (Select-String -LiteralPath $proyectoPath -Pattern '<UserSecretsId>[^<]+</UserSecretsId>' -Quiet)) { throw 'Falta UserSecretsId en el proyecto.' }
    $ejemplo = @{}
    Get-Content -LiteralPath $ejemploPath | ForEach-Object {
        if ($_ -match '^([A-Z_]+)=(.*)$') { $ejemplo[$matches[1]] = $matches[2] }
    }
    if (-not $ejemplo['MYSQL_USER'] -or -not $ejemplo['KEYCLOAK_BOOTSTRAP_ADMIN']) { throw 'Faltan nombres de usuario en .env.example.' }

    Write-Host 'Configuración local de Docker Compose y SolarUy. Las contraseñas se verán mientras las escribís.'
    Write-Host 'Cambiar .env no actualiza las credenciales de una base MySQL ya inicializada.'
    $usuarioMysql = Pedir-Usuario 'MYSQL_USER' $ejemplo['MYSQL_USER']
    $usuarioKeycloak = Pedir-Usuario 'KEYCLOAK_BOOTSTRAP_ADMIN' $ejemplo['KEYCLOAK_BOOTSTRAP_ADMIN']
    $claveRoot = Pedir-Clave 'MYSQL_ROOT_PASSWORD'
    $claveMysql = Pedir-Clave 'MYSQL_PASSWORD'
    $claveKeycloak = Pedir-Clave 'KEYCLOAK_BOOTSTRAP_PASSWORD'
    if (-not (Preguntar-SiNo '¿Crear .env y configurar ConnectionStrings:SolarUy en User Secrets? Se reemplazará el valor existente de esa clave')) {
        Write-Host 'Cancelado. No se modificó la configuración.'
        exit 0
    }

    $lineas = @(
        'MYSQL_ROOT_PASSWORD=' + (Formato-Env $claveRoot),
        'MYSQL_USER=' + (Formato-Env $usuarioMysql),
        'MYSQL_PASSWORD=' + (Formato-Env $claveMysql),
        'KEYCLOAK_BOOTSTRAP_ADMIN=' + (Formato-Env $usuarioKeycloak),
        'KEYCLOAK_BOOTSTRAP_PASSWORD=' + (Formato-Env $claveKeycloak)
    )
    $conexion = 'Server=localhost;Port=3306;Database=solaruy;User="' + $usuarioMysql.Replace('"', '""') + '";Password="' + $claveMysql.Replace('"', '""') + '";'
    $json = @{ 'ConnectionStrings:SolarUy' = $conexion } | ConvertTo-Json -Compress
    $archivoCreado = $false
    try {
        $archivo = [System.IO.File]::Open($envPath, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write)
        $archivoCreado = $true
        try {
            $contenido = [System.Text.Encoding]::UTF8.GetBytes(($lineas -join "`n") + "`n")
            $archivo.Write($contenido, 0, $contenido.Length)
        } finally { $archivo.Dispose() }
        $inicio = New-Object System.Diagnostics.ProcessStartInfo
        $inicio.FileName = (Get-Command dotnet).Source
        $inicio.Arguments = 'user-secrets set --project "' + $proyectoPath + '"'
        $inicio.UseShellExecute = $false
        $inicio.RedirectStandardInput = $true
        $inicio.RedirectStandardOutput = $true
        $inicio.RedirectStandardError = $true
        $proceso = [System.Diagnostics.Process]::Start($inicio)
        try {
            $proceso.StandardInput.Write($json)
            $proceso.StandardInput.Close()
            $null = $proceso.StandardOutput.ReadToEnd()
            $null = $proceso.StandardError.ReadToEnd()
            $proceso.WaitForExit()
            if ($proceso.ExitCode -ne 0) { throw 'Falló dotnet user-secrets.' }
        } finally { $proceso.Dispose() }
        Write-Host '.env creado y ConnectionStrings:SolarUy configurada en User Secrets.'
    } catch {
        if ($archivoCreado) {
            Write-Host 'Se creó .env, pero no se pudo confirmar User Secrets. Completá ese paso manualmente siguiendo Readme.md.'
        } else {
            Write-Host 'No se pudo crear .env. User Secrets no se modificó.'
        }
        exit 1
    }
} catch {
    Write-Error -ErrorRecord $_ -ErrorAction Continue
    exit 1
} finally {
    $null = Read-Host 'Presioná Enter para cerrar'
}
