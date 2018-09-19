Function Use-Invocation
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true)]
        [scriptblock] $Invocation,
        [Parameter(Mandatory=$true)]
        [string] $Output,
        [string] $OperationText = $null,
        [string] $OnSuccessText = $null,
        [string] $OnFailureText = $null,
        [Parameter(Mandatory=$true, ParameterSetName='PassThru')]
        [switch] $PassThru,
        [Parameter(Mandatory=$true, ParameterSetName='AsBool')]
        [switch] $AsBool
    )
    $consoleWidth = $Host.UI.RawUI.WindowSize.Width
    $consoleCursorPosition = 0

    if ($OperationText -ne $null)
    {
        $statusWidth = 0
        if ($OnSuccessText -eq $null)
        {
            $OnSuccessText = '[O]'
        }
        if ($OnFailureText -eq $null)
        {
            $OnFailureText = '[F]'
        }
        if ($OnFailureText.Length -gt $OnSuccessText.Length)
        {
            $statusWidth = $OnFailureText.Length
            $OnSuccessText = "{0}{1,$($OnFailureText.Length)}" -f '', $OnSuccessText
        }
        else 
        {
            $statusWidth = $OnSuccessText.Length
            $OnFailureText = "{0}{1,$($OnSuccessText.Length)}" -f '', $OnFailureText
        }
        $operationWidth = $consoleWidth - $statusWidth - 2
        if ($operationWidth -gt 0)
        {
            $width = $operationWidth
            if ($OperationText.Length -lt $operationWidth)
            {
                Write-Host $OperationText -NoNewline

                $width -= $OperationText.Length
            }
            else
            {
                $OperationText -split ' ' `
                    | ForEach-Object `
                    {
                        if ($width -lt ($_.Length + 1))
                        {
                            Write-Host
                            $width = $operationWidth
                        }

                        Write-Host "$_ " -NoNewline

                        $width -= $_.Length + 1
                    }
            }

            Write-Host $("{0, $width}" -f '') -NoNewline

            $consoleCursorPosition = $host.UI.RawUI.CursorPosition

            Write-Host
        }
    }

    $MyInvocation | Out-File $Output -Append

    try 
    {
        try
        {
            $result = Invoke-Command -ScriptBlock $Invocation -NoNewScope -ErrorAction Stop
        }
        finally
        {
            $consoleCurrentCursorPosition = $host.UI.RawUI.CursorPosition
            $host.UI.RawUI.CursorPosition = $consoleCursorPosition
        }
    }
    catch 
    {
        if ($OperationText -ne $null)
        {
            Write-Host $OnFailureText -ForegroundColor Red
            
            $host.UI.RawUI.CursorPosition = $consoleCurrentCursorPosition
        }


        $_ | Out-File $Output -Append

        if ($AsBool)
        {
            return $false
        }

        throw
    }

    $failure = -not $?
    
    $result | Out-File $Output -Append

    if ($failure)
    {
        if ($OperationText -ne $null)
        {
            Write-Host $OnFailureText -ForegroundColor Red

            $host.UI.RawUI.CursorPosition = $consoleCurrentCursorPosition
        }

        Write-Error "Cannot execute command. Please see '$Output' for details."

        return
    }
    if ($PassThru)
    {
        if ($OperationText -ne $null)
        {
            Write-Host $OnSuccessText -ForegroundColor Green

            $host.UI.RawUI.CursorPosition = $consoleCurrentCursorPosition
        }

        return $result
    }
    if ($AsBool)
    {
        if (($null -eq $result) -or (([string]$result).ToLower() -eq 'false'))
        {
            if ($OperationText -ne $null)
            {
                Write-Host $OnFailureText -ForegroundColor Red

                $host.UI.RawUI.CursorPosition = $consoleCurrentCursorPosition
            }

            return $false
        }
    }

    if ($OperationText -ne $null)
    {
        Write-Host $OnSuccessText -ForegroundColor Green

        $host.UI.RawUI.CursorPosition = $consoleCurrentCursorPosition
    }

    return $true
}

if (-not (Get-Command 'dotnet' -ErrorAction SilentlyContinue))
{
    Write-Error 'Could not find dotnet-cli'
    return
}

$root = $PSScriptRoot

$build_log = Join-Path -Path $root -ChildPath "build.log" -ErrorAction Stop
$database_log = Join-Path -Path $root -ChildPath "database.log" -ErrorAction Stop
$package_log = Join-Path -Path $root -ChildPath "package.log" -ErrorAction Stop
$powershell_log = Join-Path -Path $root -ChildPath "powershell.log" -ErrorAction Stop

if (Test-Path -Path $build_log)
{
    Remove-Item -Path $build_log -Force -ErrorAction Stop
}

if (Test-Path -Path $package_log)
{
    Remove-Item -Path $package_log -Force -ErrorAction Stop
}

if (Test-Path -Path $powershell_log)
{
    Remove-Item -Path $powershell_log -Force -ErrorAction Stop
}

$configuration = 'Debug'
$platform = 'x64'

$applicationName = 'JokesApp'
$applicationTypeName = 'JokesAppType'
$applicationTypeVersiion = '1.0.0'

$result = Use-Invocation `
    -OperationText 'Building application' `
    -OnSuccessText '[COMPLETED]' `
    -OnFailureText '[FAILED]' `
    -Invocation `
        { 
            Push-Location $root

            & dotnet msbuild `
                "src/all-jokes.sln" `
                "/t:restore;build" `
                /p:Configuration="$configuration" `
                /p:Platform="$platform" `
                /nologo `
                /m:1 `
                /v:m `
                /nr:false | Out-File -FilePath $build_log

            if (-not $?)
            {
                return $false
            }

            Pop-Location

            return $true
        } `
    -Output $powershell_log `
    -AsBool

if (-not $result)
{
    Write-Error "The solution build ended with errors. Please see '$build_log' for details."
    return
}

$result = Use-Invocation `
    -OperationText 'Packaging application' `
    -OnSuccessText '[COMPLETED]' `
    -OnFailureText '[FAILED]' `
    -Invocation `
        { 
            Push-Location $root

            & dotnet msbuild `
                "src/JokesApp/JokesApp.sfproj" `
                "/t:package" `
                /p:Configuration="$configuration" `
                /p:Platform="$platform" `
                /nologo `
                /m:1 `
                /v:m `
                /nr:false | Out-File -FilePath $build_log

            if (-not $?)
            {
                return $false
            }

            Pop-Location

            return $true
        } `
    -Output $powershell_log `
    -AsBool

if (-not $result)
{
    Write-Error "Service Fabric application packaging ended with errors. Please see '$build_log' for details."
    return
}

$result = Use-Invocation `
    -OperationText 'Connecting to Local Cluster' `
    -OnSuccessText '[CONNECTED]' `
    -OnFailureText '[FAILED]' `
    -Invocation `
        { 
            Connect-ServiceFabricCluster

            $Global:ClusterConnection = $ClusterConnection

            return $ClusterConnection
        } `
    -Output $powershell_log `
    -AsBool

if (-not $result)
{
    Write-Error "Cannot connect to Local Cluster. Please see '$powershell_log' for details."
    return
}

$result = Use-Invocation `
    -OperationText 'Checking whether application already exists' `
    -OnSuccessText '[DONE]' `
    -OnFailureText '[FAILED]' `
    -Invocation { Get-ServiceFabricApplication -ApplicationName "fabric:/$applicationName" } `
    -Output $powershell_log `
    -PassThru

if ($null -ne $result)
{
    $result = Use-Invocation `
        -OperationText 'Removing existing application' `
        -OnSuccessText '[COMPLETED]' `
        -OnFailureText '[FAILED]' `
        -Invocation `
            { 
                Remove-ServiceFabricApplication `
                    -ApplicationName 'fabric:/JokesApp' `
                    -Force `
                    -ForceRemove
            } `
        -Output $powershell_log `
        -AsBool

    if (-not $result)
    {
        Write-Error "Cannot remove existing application. Please see '$powershell_log' for details."
        return
    }
}

$result = Use-Invocation `
    -OperationText 'Checking whether application type was already registered' `
    -OnSuccessText '[DONE]' `
    -OnFailureText '[FAILED]' `
    -Invocation `
        { 
            Get-ServiceFabricApplicationType `
                -ApplicationTypeName $applicationTypeName `
                -ApplicationTypeVersion $applicationTypeVersiion
        } `
    -Output $powershell_log `
    -PassThru

if ($null -ne $result)
{
    $result = Use-Invocation `
        -OperationText 'Removing existing application type' `
        -OnSuccessText '[COMPLETED]' `
        -OnFailureText '[FAILED]' `
        -Invocation `
            { 
                Unregister-ServiceFabricApplicationType `
                    -ApplicationTypeName $applicationTypeName `
                    -ApplicationTypeVersion $applicationTypeVersiion `
                    -Force
            } `
        -Output $powershell_log `
        -AsBool

    if (-not $result)
    {
        Write-Error "Cannot remove existing application type. Please see '$powershell_log' for details."
        return
    }
}

$applicationPackage_path = Join-Path -Path $root -ChildPath 'src/JokesApp/pkg/Debug' -ErrorAction Stop

$result = Use-Invocation `
    -OperationText 'Copying application package' `
    -OnSuccessText '[COMPLETED]' `
    -OnFailureText '[FAILED]' `
    -Invocation `
        { 
            Copy-ServiceFabricApplicationPackage `
                -ApplicationPackagePath $applicationPackage_path `
                -ApplicationPackagePathInImageStore 'JokesAppImage'
        } `
    -Output $powershell_log `
    -AsBool

if (-not $result)
{
    Write-Error "Cannot copy application package to store. Please see '$powershell_log' for details."
    return
} 

$result = Use-Invocation `
    -OperationText 'Registering application type' `
    -OnSuccessText '[COMPLETED]' `
    -OnFailureText '[FAILED]' `
    -Invocation `
        { 
            Register-ServiceFabricApplicationType `
                -ApplicationPathInImageStore 'JokesAppImage' `
                -ErrorAction Stop
        } `
    -Output $powershell_log `
    -AsBool

if (-not $result)
{
    Write-Error "Cannot register application type. Please see '$powershell_log' for details."
    return
}

$result = Use-Invocation `
    -OperationText 'Removing used application package' `
    -OnSuccessText '[COMPLETED]' `
    -OnFailureText '[FAILED]' `
    -Invocation { Remove-ServiceFabricApplicationPackage -ApplicationPackagePathInImageStore 'JokesAppImage' } `
    -Output $powershell_log `
    -AsBool

if (-not $result)
{
    Write-Error "Cannot remove application package. Please see '$powershell_log' for details."
    return
}

$result = Use-Invocation `
    -OperationText 'Creating application' `
    -OnSuccessText '[COMPLETED]' `
    -OnFailureText '[FAILED]' `
    -Invocation `
        { 
            New-ServiceFabricApplication `
                -ApplicationName "fabric:/$applicationName" `
                -ApplicationTypeName $applicationTypeName `
                -ApplicationTypeVersion $applicationTypeVersiion 
        } `
    -Output $powershell_log `
    -AsBool

if (-not $result)
{
    Write-Error "Cannot create application. Please see '$powershell_log' for details."
    return
}