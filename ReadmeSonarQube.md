# SolarUY

# Integración SonarQube, paso a paso

1. Levantar SonarQube:
  
   docker run -d --name sonarqube -p 9000:9000 sonarqube:lts
   

2. Entrar a `http://localhost:9000`, iniciar sesión, cambiar la contraseña
   inicial, crear un proyecto y generar un token.


3. Instalar el scanner para .NET:
   
   dotnet tool install --global dotnet-sonarscanner
   

4. Desde la raíz del repositorio, ingresar el token de forma oculta en la
   sesión actual de PowerShell.
   
   $secureToken = Read-Host "Token de SonarQube" -AsSecureString
   $env:SONAR_TOKEN = [System.Net.NetworkCredential]::new("", $secureToken).Password
   

5. Ejecutar el análisis en orden. `begin` recibe el token mediante
   `sonar.token`. Continuar con `build` y `end` solo si `begin` termina
   correctamente. Pasar el token también a `end`:
   
   dotnet sonarscanner begin /k:"SolarUY" /n:"SolarUY" /v:"1.0" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="$env:SONAR_TOKEN"
   dotnet build "SolarUy.sln"
   dotnet sonarscanner end /d:sonar.token="$env:SONAR_TOKEN"
   

6. Al terminar, eliminar el token del entorno de esta sesión:
   
   Remove-Item Env:SONAR_TOKEN
   

7. Abrir `http://localhost:9000`, entrar al proyecto y revisar el resultado
   del análisis y su Quality Gate.


