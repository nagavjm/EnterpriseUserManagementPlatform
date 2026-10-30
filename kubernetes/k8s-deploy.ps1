# Kubernetes Deployment & Lifecycle Helper Script
# Usage:
#   .\kubernetes\k8s-deploy.ps1 -Action Apply    # Deploys all manifests to the eump namespace
#   .\kubernetes\k8s-deploy.ps1 -Action Status   # Checks pod, service, and PVC status
#   .\kubernetes\k8s-deploy.ps1 -Action Delete   # Tears down all resources in the eump namespace

param (
    [ValidateSet("Apply", "Status", "Delete")]
    [string]$Action = "Apply"
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# 1. Verify kubectl connectivity
Write-Host "Verifying Kubernetes cluster connectivity..." -ForegroundColor Cyan
try {
    $clusterCheck = kubectl cluster-info 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Cannot connect to a Kubernetes cluster." -ForegroundColor Red
        Write-Host "If using Docker Desktop, open Docker Desktop Settings -> Kubernetes -> Check 'Enable Kubernetes' -> Apply & Restart." -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Host "ERROR: kubectl is not installed or not in PATH." -ForegroundColor Red
    exit 1
}

Write-Host "Connected to Kubernetes cluster successfully." -ForegroundColor Green

if ($Action -eq "Apply") {
    Write-Host "`nDeploying Enterprise User Management Platform to Kubernetes..." -ForegroundColor Cyan
    
    # Apply manifests in numbered sequence
    $manifests = Get-ChildItem -Path $scriptDir -Filter "*.yaml" | Sort-Object Name
    foreach ($manifest in $manifests) {
        Write-Host "Applying $($manifest.Name)..." -ForegroundColor Gray
        kubectl apply -f $manifest.FullName
    }

    Write-Host "`nWaiting for deployments to roll out successfully..." -ForegroundColor Cyan
    kubectl rollout status deployment/backend-deployment -n eump --timeout=120s
    kubectl rollout status deployment/frontend-deployment -n eump --timeout=120s

    Write-Host "`nAll deployments are ready!" -ForegroundColor Green
    Write-Host "`nCurrent Cluster Status in namespace 'eump':" -ForegroundColor Cyan
    kubectl get all,pvc,ingress -n eump
}
elseif ($Action -eq "Status") {
    Write-Host "`nCurrent Status in namespace 'eump':" -ForegroundColor Cyan
    kubectl get all,pvc,ingress -n eump -o wide
}
elseif ($Action -eq "Delete") {
    Write-Host "`nTearing down Enterprise User Management Platform from Kubernetes..." -ForegroundColor Yellow
    $manifests = Get-ChildItem -Path $scriptDir -Filter "*.yaml" | Sort-Object Name -Descending
    foreach ($manifest in $manifests) {
        Write-Host "Deleting $($manifest.Name)..." -ForegroundColor Gray
        kubectl delete -f $manifest.FullName --ignore-not-found=true
    }
    Write-Host "Cleanup completed." -ForegroundColor Green
}

