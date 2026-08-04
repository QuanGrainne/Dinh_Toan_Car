Add-Type -AssemblyName System.IO.Compression.FileSystem

$files = Get-ChildItem -Path "d:\semester 8\Prn232\project\prn232-be\docs\*.docx"
foreach ($file in $files) {
    Write-Host "--- $($file.Name) ---"
    $zip = [System.IO.Compression.ZipFile]::OpenRead($file.FullName)
    $entry = $zip.GetEntry('word/document.xml')
    $reader = New-Object System.IO.StreamReader($entry.Open())
    $xmlStr = $reader.ReadToEnd()
    $reader.Close()
    $zip.Dispose()
    
    $xml = [xml]$xmlStr
    $ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
    $ns.AddNamespace('w', 'http://schemas.openxmlformats.org/wordprocessingml/2006/main')
    $nodes = $xml.SelectNodes('//w:p', $ns)
    foreach ($node in $nodes) {
        $text = ''
        foreach ($t in $node.SelectNodes('.//w:t', $ns)) {
            $text += $t.InnerText
        }
        if ($text -ne '') { Write-Host $text }
    }
}
