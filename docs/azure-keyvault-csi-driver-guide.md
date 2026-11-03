# Azure Key Vault + Secrets Store CSI Driver Guide
## Enterprise Secret Management for Azure Kubernetes Service (AKS)

This guide documents the enterprise-grade integration of **Azure Key Vault (AKV)** with **Azure Kubernetes Service (AKS)** using the official **Secrets Store CSI Driver** (`secrets-store.csi.k8s.io`).

---

## 1. Architecture Overview

```
 ┌──────────────────────┐
 │   Azure Key Vault    │
 │ (SqlConnectionString)│
 └──────────┬───────────┘
            │ 1. Secure fetch via AKS Kubelet Managed Identity (Zero passwords in Git)
            ▼
 ┌─────────────────────────────────────────────────────────────┐
 │ Azure Kubernetes Service (AKS)                              │
 │                                                             │
 │  ┌───────────────────────────────────────────────────────┐  │
 │  │ SecretProviderClass (azure-kv-eump-secret-provider)   │  │
 │  └──────────────────────────┬────────────────────────────┘  │
 │                             │ 2. Syncs to K8s Secret        │
 │                             ▼                               │
 │             ┌────────────────────────────────┐              │
 │             │ K8s Secret: eump-secret        │              │
 │             │ (ConnectionStrings__Default..) │              │
 │             └───────────────┬────────────────┘              │
 │                             │ 3. Injected via envFrom       │
 │                             ▼                               │
 │             ┌────────────────────────────────┐              │
 │             │ Backend Pod (.NET 9 Web API)   │              │
 │             └────────────────────────────────┘              │
 └─────────────────────────────────────────────────────────────┘
```

### Key Security Benefits:
1. **Zero Credentials in Git**: Neither `appsettings.json`, nor Kubernetes manifests, nor repository history contains raw passwords.
2. **Centralized Lifecycle Management**: Password rotation, access revocation, and expiration happen inside Azure Key Vault without re-deploying code.
3. **Audit Trails**: Azure Key Vault logs every access request with caller IP, timestamp, and identity.
4. **Hardware-Backed Protection**: Secrets are encrypted at rest using Azure FIPS 140 validated HSMs.

---

## 2. Step-by-Step Setup in Azure

### Step 2.1: Create Azure Key Vault (if not already existing)
```bash
# Variables
RESOURCE_GROUP="rg-eump"
LOCATION="eastus"
KEYVAULT_NAME="kv-eump-1001" # Must be globally unique

# Create Key Vault with Azure RBAC enabled
az keyvault create \
  --name $KEYVAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --enable-rbac-authorization true
```

### Step 2.2: Add the SQL Connection String Secret
Add your Azure SQL connection string to the Key Vault under secret name `SqlConnectionString`:
```bash
az keyvault secret set \
  --vault-name $KEYVAULT_NAME \
  --name "SqlConnectionString" \
  --value "Server=tcp:eump-sql-nagarajan.database.windows.net,1433;Initial Catalog=enterprise-user-db;Persist Security Info=False;User ID=sqladmin;Password=YourActualPassword!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### Step 2.3: Enable Secrets Store CSI Driver on AKS
Enable the official Microsoft Azure Key Vault Secrets Provider add-on on your AKS cluster:
```bash
az aks enable-addons \
  --addons azure-keyvault-secrets-provider \
  --name "aks-eump" \
  --resource-group $RESOURCE_GROUP
```

### Step 2.4: Grant AKS Kubelet Identity Access to Key Vault
The CSI driver uses the AKS cluster's Kubelet identity to authenticate against Azure Key Vault:

```bash
# 1. Retrieve the Kubelet Managed Identity Client ID
KUBELET_CLIENT_ID=$(az aks show \
  --resource-group $RESOURCE_GROUP \
  --name "aks-eump" \
  --query "identityProfile.kubeletidentity.clientId" \
  -o tsv)

# 2. Retrieve Key Vault Resource ID
KV_RESOURCE_ID=$(az keyvault show \
  --name $KEYVAULT_NAME \
  --query id \
  -o tsv)

# 3. Assign "Key Vault Secrets User" RBAC role to Kubelet Identity
az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee $KUBELET_CLIENT_ID \
  --scope $KV_RESOURCE_ID
```

*(Note: If your Key Vault uses Vault Access Policies instead of Azure RBAC, run this instead:)*
```bash
az keyvault set-policy \
  --name $KEYVAULT_NAME \
  --spn $KUBELET_CLIENT_ID \
  --secret-permissions get
```

---

## 3. Kubernetes Manifests Architecture

### 1. `kubernetes/03-secret-provider-class.yaml`
Defines the `SecretProviderClass` referencing Key Vault:
- Connects using `useVMManagedIdentity: "true"`.
- Requests the `SqlConnectionString` secret.
- **`secretObjects` block**: Automatically syncs the retrieved secret into a native Kubernetes Secret named `eump-secret` under the key `ConnectionStrings__DefaultConnection`.

### 2. `kubernetes/04-backend-deployment.yaml`
- Mounts the CSI volume:
  ```yaml
  volumeMounts:
    - name: secrets-store-inline
      mountPath: "/mnt/secrets-store"
      readOnly: true
  volumes:
    - name: secrets-store-inline
      csi:
        driver: secrets-store.csi.k8s.io
        readOnly: true
        volumeAttributes:
          secretProviderClass: "azure-kv-eump-secret-provider"
  ```
- Reads the synced secret automatically via:
  ```yaml
  envFrom:
    - secretRef:
        name: eump-secret
  ```

---

## 4. Verification & Troubleshooting

### Check CSI Driver Pods
```bash
kubectl get pods -n kube-system -l 'app in (secrets-store-csi-driver, secrets-store-provider-azure)'
```

### Check Secret Synchronization
```bash
# Verify the SecretProviderClass exists
kubectl get secretproviderclass -n eump-prod

# Verify eump-secret was populated by CSI driver
kubectl get secret eump-secret -n eump-prod -o yaml

# Inspect backend pod logs to confirm successful Azure SQL connectivity
kubectl logs deployment/backend-deployment -n eump-prod -c backend
```

