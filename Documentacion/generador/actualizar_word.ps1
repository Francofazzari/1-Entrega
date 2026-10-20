# Actualiza los campos del documento (indice, indice de figuras, numeros de figura y referencias) y exporta el PDF.
# Usa una instancia nueva y oculta de Word: no toca los documentos que el usuario tenga abiertos.
# Uso: powershell -File actualizar_word.ps1 -docx <ruta.docx> [-pdf <ruta.pdf>]
param([Parameter(Mandatory = $true)][string]$docx, [string]$pdf)

$w = New-Object -ComObject Word.Application
$w.Visible = $false
$w.DisplayAlerts = 0
$w.ScreenUpdating = $false
try {
    $d = $w.Documents.Open($docx)
    $d.Fields.Update() | Out-Null
    foreach ($t in $d.TablesOfContents) { $t.Update() | Out-Null }
    foreach ($t in $d.TablesOfFigures) { $t.Update() | Out-Null }
    $d.Fields.Update() | Out-Null
    foreach ($t in $d.TablesOfContents) { $t.UpdatePageNumbers() | Out-Null }
    foreach ($t in $d.TablesOfFigures) { $t.UpdatePageNumbers() | Out-Null }
    $paginas = $d.ComputeStatistics(2)
    $d.Save()
    if ($pdf) { $d.ExportAsFixedFormat($pdf, 17) }
    $d.Close()
    Write-Output "paginas: $paginas"
}
finally {
    $w.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($w) | Out-Null
}
