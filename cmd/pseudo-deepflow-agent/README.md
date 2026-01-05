# pseudo-deepflow-agent

A standalone command-line tool that synchronizes Kubernetes resources and reports them to deepflow-server via the gRPC interface for AutoTagging functionality.

## Background

In environments where deepflow-agent cannot directly watch the K8s apiserver, pseudo-deepflow-agent can be used as an alternative to sync Kubernetes API resources to deepflow-server.

## Protocol Version

This implementation aligns with the protocol defined in `deepflowio/deepflow v6.6.5`:
- Service: `agent.Synchronizer`
- RPC: `KubernetesAPISync(KubernetesAPISyncRequest) returns (KubernetesAPISyncResponse)`
- Transport: gRPC plaintext (no TLS)

## Features

### Monitored Resources

**Required resources (always synced):**
- `*v1.Node`
- `*v1.Namespace`
- `*v1.Pod`

**Workload resources (always synced):**
- `*v1.Deployment`
- `*v1.StatefulSet`
- `*v1.DaemonSet`
- `*v1.ReplicationController`
- `*v1.ReplicaSet`

**Optional resources:**
- `*v1.Service` (enabled with `--enable-service`)

### Sync Behavior

- Uses client-go informers (List+Watch) to cache resources
- Monitors Add/Update/Delete events to detect changes
- Periodic sync (configurable interval, default 60s):
  - If resources changed: sends full snapshot with new version (Unix timestamp)
  - If no changes: sends heartbeat with previous version (no entries)
- Ensures sync happens at least once every 24 hours (configurable)

### Data Format

Each resource type is:
1. Serialized to JSON
2. Compressed with zlib
3. Sent as a `KubernetesAPIInfo` entry with `compressed_info` field

## Build

### Using the build script (recommended)

The build script will generate protobuf files and build the binary:

```bash
cd cmd/pseudo-deepflow-agent
./build.sh
```

### Manual build

If you prefer to build manually:

```bash
# First, generate protobuf files
cd message/common
go generate

cd ../agent
go generate

# Then build the binary
cd ../../cmd/pseudo-deepflow-agent
go mod tidy
go build -o pseudo-deepflow-agent .
```

Note: You need `protoc` and `protoc-gen-gofast` installed for proto generation:
```bash
# Install protoc (Ubuntu/Debian)
sudo apt-get install protobuf-compiler

# Install protoc-gen-gofast
go install github.com/gogo/protobuf/protoc-gen-gofast@latest
```

### Docker build

Build the Docker image:

```bash
# From the repository root
docker build -f cmd/pseudo-deepflow-agent/Dockerfile -t pseudo-deepflow-agent:latest .
```

## Usage

### Required Flags

- `--cluster-id`: Kubernetes cluster ID (must match the cluster_id in deepflow-agent.yaml)
- `--deepflow-server`: DeepFlow server address in `host:port` format

### Optional Flags

- `--kubeconfig`: Path to kubeconfig file (default: uses in-cluster config)
- `--source-ip`: Source IP to report (default: auto-detected first non-loopback IPv4)
- `--vtap-id`: Agent ID/Vtap ID (default: 0)
- `--team-id`: Team ID for multi-tenancy (default: "")
- `--sync-interval`: Sync interval in seconds (default: 60)
- `--enable-service`: Enable Service resource sync (default: false)
- `--max-heartbeat`: Maximum heartbeat interval in seconds (default: 86400 = 24 hours)

### Example Commands

**Basic usage with kubeconfig:**
```bash
./pseudo-deepflow-agent \
  --cluster-id=my-k8s-cluster \
  --deepflow-server=192.168.1.100:30033 \
  --kubeconfig=/path/to/kubeconfig
```

**With all options:**
```bash
./pseudo-deepflow-agent \
  --cluster-id=production-cluster \
  --deepflow-server=deepflow-server.example.com:30033 \
  --kubeconfig=$HOME/.kube/config \
  --source-ip=10.0.1.50 \
  --vtap-id=1001 \
  --team-id=team-ops \
  --sync-interval=120 \
  --enable-service
```

**In-cluster deployment (no kubeconfig needed):**
```bash
./pseudo-deepflow-agent \
  --cluster-id=my-k8s-cluster \
  --deepflow-server=deepflow-server.deepflow.svc.cluster.local:30033
```

## Deployment

### As a Kubernetes Deployment

A complete deployment manifest is provided in `deployment.yaml`. Customize the following values:

- `image`: Replace with your actual image registry and tag
- `--cluster-id`: Your Kubernetes cluster ID
- `--deepflow-server`: DeepFlow server address

Deploy with:

```bash
# Create the deepflow namespace if it doesn't exist
kubectl create namespace deepflow

# Deploy pseudo-deepflow-agent
kubectl apply -f deployment.yaml
```

The deployment includes:
- ServiceAccount with necessary RBAC permissions
- ClusterRole with read access to required K8s resources
- ClusterRoleBinding
- Deployment with resource limits

### As a Standalone Binary

Run on a host with network access to both K8s apiserver and deepflow-server:

```bash
./pseudo-deepflow-agent \
  --cluster-id=my-k8s-cluster \
  --deepflow-server=192.168.1.100:30033 \
  --kubeconfig=/etc/kubernetes/admin.conf
```

## Logging

The agent outputs logs to stdout:
- INFO: Normal operations (resource changes, sync status)
- ERROR: Errors during sync, K8s API issues, or gRPC failures
- On errors, the agent continues running and retries on the next sync interval

## Error Handling

- **K8s API errors**: Logged and reported in `error_msg` field, agent continues running
- **Build/serialization errors**: Logged and reported in `error_msg`, previous version is used
- **gRPC errors**: Logged, sync is retried on next interval
- **Informer sync failures**: Fatal, agent exits (requires restart)

## Implementation Notes

### Protocol Alignment

This tool implements the client side of the `KubernetesAPISync` RPC as defined in the message/agent.proto file, which is compatible with deepflowio/deepflow v6.6.5.

The proto definitions are generated from:
- `message/agent.proto` - Defines the `agent.Synchronizer` service
- `message/common.proto` - Defines the `KubernetesAPIInfo` message type

### Version Management

- **Dirty state**: When resources change, version = current Unix timestamp
- **Clean state**: No changes, version = previous version (heartbeat)
- **Error state**: Build/cache errors, version = previous version, error_msg is populated

### Source IP Detection

Auto-detection selects the first non-loopback, UP, IPv4 address from network interfaces.

## License

Licensed under the Apache License, Version 2.0.
