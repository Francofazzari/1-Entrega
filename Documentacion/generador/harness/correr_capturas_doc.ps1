# Recrea y carga la base descartable, compila el programa de capturas de la documentacion contra la UI
# compilada y deja las imagenes + capturas.json en scratchpad\doc\capturas.
$H = $PSScriptRoot
$S = Split-Path $H
$R = "C:\Users\franco\source\repos\Entrega1software"
$csc = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"

& "$H\correr_prueba.ps1" -SoloResumen | Select-String "RESULTADO|FALLA" | ForEach-Object { $_.Line }

Copy-Item "$R\Entrega1software\bin\Debug\Entrega1software.exe" $H
Copy-Item "$H\PruebaBLL.exe.config" "$H\CapturasDoc.exe.config"
& $csc /nologo /target:exe /out:"$H\CapturasDoc.exe" /r:"$H\BE.dll" /r:"$H\DAL.dll" /r:"$H\BLL.dll" /r:"$H\Entrega1software.exe" `
    /r:System.Configuration.dll /r:System.Data.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll "$H\CapturasDoc.cs"
"compilacion: $LASTEXITCODE"

$out = "$S\doc\capturas"
New-Item -ItemType Directory -Force $out | Out-Null
Get-ChildItem $out -Filter *.png | ForEach-Object { $_.Delete() }
& "$H\CapturasDoc.exe" $out
