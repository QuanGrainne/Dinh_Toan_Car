param(
    [string]$ConnectionString = "Server=NAT\SQLEXPRESS;Database=CarShowroomDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;",
    [switch]$Apply
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$markers = @(
    [string][char]0x00C3, # Ã
    [string][char]0x00C2, # Â
    [string][char]0x00C4, # Ä
    [string][char]0x00E1, # á
    [string][char]0x00EF  # ï
)

$latin1 = [System.Text.Encoding]::GetEncoding("ISO-8859-1")
$utf8 = [System.Text.UTF8Encoding]::new($false)

function Test-HasMojibake {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $false
    }

    foreach ($marker in $markers) {
        if ($Value.Contains($marker)) {
            return $true
        }
    }

    return $false
}

function Repair-Text {
    param([string]$Value)

    if (-not (Test-HasMojibake -Value $Value)) {
        return $Value
    }

    $current = $Value
    for ($i = 0; $i -lt 3; $i++) {
        try {
            $bytes = $latin1.GetBytes($current)
            $fixed = $utf8.GetString($bytes)
            if ($fixed -eq $current) {
                break
            }

            $current = $fixed
        }
        catch {
            break
        }
    }

    return $current
}

function New-SqlConnection {
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $connection.Open()
    return $connection
}

function Add-Parameter {
    param(
        [System.Data.SqlClient.SqlCommand]$Command,
        [string]$Name,
        $Value
    )

    $parameter = $Command.Parameters.Add("@$Name", [System.Data.SqlDbType]::NVarChar, -1)
    if ($null -eq $Value) {
        $parameter.Value = [DBNull]::Value
    }
    else {
        $parameter.Value = $Value
    }
}

function Invoke-NonQuery {
    param(
        [System.Data.SqlClient.SqlConnection]$Connection,
        [System.Data.SqlClient.SqlTransaction]$Transaction,
        [string]$CommandText,
        [hashtable]$Parameters
    )

    $command = $Connection.CreateCommand()
    $command.Transaction = $Transaction
    $command.CommandText = $CommandText

    if ($Parameters) {
        foreach ($name in $Parameters.Keys) {
            Add-Parameter -Command $command -Name $name -Value $Parameters[$name]
        }
    }

    [void]$command.ExecuteNonQuery()
}

$targets = @(
    @{ Table = "Cars"; Key = "CarId"; Columns = @("CarName", "Model", "Color", "FuelType", "Transmission", "Description", "ImageUrl", "Status") },
    @{ Table = "CarBrands"; Key = "BrandId"; Columns = @("BrandName", "Country", "Description") },
    @{ Table = "Parts"; Key = "PartId"; Columns = @("PartName", "PartCode", "Brand", "Description", "ImageUrl", "Status") },
    @{ Table = "PartCategories"; Key = "CategoryId"; Columns = @("CategoryName", "Description") },
    @{ Table = "MaintenancePackages"; Key = "PackageId"; Columns = @("PackageName", "Description", "Status") },
    @{ Table = "MaintenanceAppointments"; Key = "AppointmentId"; Columns = @("CustomerName", "CustomerPhone", "CustomerEmail", "LicensePlate", "CarName", "Note", "Status") },
    @{ Table = "PurchaseRequests"; Key = "RequestId"; Columns = @("CustomerName", "CustomerPhone", "CustomerEmail", "Message", "Status", "CaptchaCode") },
    @{ Table = "AppUsers"; Key = "UserId"; Columns = @("FullName", "Email", "PhoneNumber", "Address") },
    @{ Table = "ComboOrders"; Key = "ComboOrderId"; Columns = @("CustomerName", "CustomerPhone", "CustomerEmail", "ShippingAddress", "Note", "Source", "ChatSessionId", "PurchaseType", "Status", "CaptchaCode", "FinalCaptchaCode") },
    @{ Table = "ComboOrderItems"; Key = "ItemId"; Columns = @("ItemType", "ItemName") }
)

$connection = New-SqlConnection
$transaction = $connection.BeginTransaction()
$runId = [Guid]::NewGuid()

try {
    Invoke-NonQuery -Connection $connection -Transaction $transaction -CommandText @"
IF OBJECT_ID(N'dbo.MojibakeRepairBackup', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MojibakeRepairBackup (
        BackupId BIGINT IDENTITY(1,1) PRIMARY KEY,
        RunId UNIQUEIDENTIFIER NOT NULL,
        TableName NVARCHAR(128) NOT NULL,
        RecordId NVARCHAR(128) NOT NULL,
        ColumnName NVARCHAR(128) NOT NULL,
        OriginalValue NVARCHAR(MAX) NULL,
        FixedValue NVARCHAR(MAX) NULL,
        CapturedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
"@ -Parameters @{}

    $changes = New-Object System.Collections.Generic.List[object]

    foreach ($target in $targets) {
        $table = $target.Table
        $key = $target.Key
        $columns = $target.Columns
        $columnList = ($columns | ForEach-Object { "[$_]" }) -join ", "

        $command = $connection.CreateCommand()
        $command.Transaction = $transaction
        $command.CommandText = "SELECT [$key], $columnList FROM [$table]"
        $reader = $command.ExecuteReader()

        while ($reader.Read()) {
            $recordId = [string]$reader[$key]
            $changedColumns = New-Object System.Collections.Generic.List[object]

            foreach ($column in $columns) {
                if ($reader.IsDBNull($reader.GetOrdinal($column))) {
                    continue
                }

                $original = [string]$reader[$column]
                $fixed = Repair-Text -Value $original

                if ($fixed -ne $original) {
                    $changedColumns.Add([pscustomobject]@{
                        Column = $column
                        Original = $original
                        Fixed = $fixed
                    })
                }
            }

            if ($changedColumns.Count -gt 0) {
                $changes.Add([pscustomobject]@{
                    Table = $table
                    Key = $key
                    RecordId = $recordId
                    Columns = $changedColumns
                })
            }
        }

        $reader.Close()
    }

    if ($changes.Count -eq 0) {
        Write-Output "No mojibake rows detected."
        $transaction.Rollback()
        return
    }

    Write-Output "Detected rows requiring repair:"
    $changes |
        Group-Object Table |
        ForEach-Object { Write-Output (" - {0}: {1} row(s)" -f $_.Name, $_.Count) }

    if (-not $Apply) {
        Write-Output "Preview only. Re-run with -Apply to persist fixes."
        $transaction.Rollback()
        return
    }

    foreach ($change in $changes) {
        $setClauses = @()
        $parameters = @{ RecordId = $change.RecordId }

        foreach ($columnChange in $change.Columns) {
            $column = $columnChange.Column
            $setClauses += "[$column] = @$column"
            $parameters[$column] = $columnChange.Fixed

            Invoke-NonQuery -Connection $connection -Transaction $transaction -CommandText @"
INSERT INTO dbo.MojibakeRepairBackup (RunId, TableName, RecordId, ColumnName, OriginalValue, FixedValue)
VALUES (@RunId, @TableName, @RecordId, @ColumnName, @OriginalValue, @FixedValue)
"@ -Parameters @{
                RunId = $runId.ToString()
                TableName = $change.Table
                RecordId = $change.RecordId
                ColumnName = $column
                OriginalValue = $columnChange.Original
                FixedValue = $columnChange.Fixed
            }
        }

        $updateSql = "UPDATE [{0}] SET {1} WHERE [{2}] = @RecordId" -f $change.Table, ($setClauses -join ", "), $change.Key
        Invoke-NonQuery -Connection $connection -Transaction $transaction -CommandText $updateSql -Parameters $parameters
    }

    $transaction.Commit()
    Write-Output "Applied mojibake repair successfully. RunId: $runId"
}
catch {
    $transaction.Rollback()
    throw
}
finally {
    $connection.Dispose()
}
