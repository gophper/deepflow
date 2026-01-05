# pseudo-deepflow-agent

## 概述

在 `gophper/deepflow` 仓库中新增了一个独立的命令行工具 `pseudo-deepflow-agent`，用于在环境不允许 deepflow-agent 直接 Watch K8s apiserver 时，同步 K8s 资源信息并通过 deepflow-server 的明文 gRPC 接口上报。

## 位置

- **源代码**: `cmd/pseudo-deepflow-agent/`
- **文档**: `cmd/pseudo-deepflow-agent/README.md`

## 主要功能

1. **K8s 资源监控**
   - 必须上报：Node, Namespace, Pod
   - 工作负载：Deployment, StatefulSet, DaemonSet, ReplicationController, ReplicaSet
   - 可选：Service (通过 `--enable-service` 启用)

2. **智能同步**
   - 使用 client-go informer (List+Watch) 缓存资源
   - 资源变化时上报完整快照（版本号为 Unix 时间戳）
   - 无变化时发送心跳（保持上一版本，不携带 entries）
   - 可配置同步间隔（默认 60 秒），保证 24 小时内至少同步一次

3. **数据处理**
   - JSON 序列化
   - zlib 压缩
   - 通过 gRPC 明文连接上报

## 协议对齐

实现了 deepflowio/deepflow v6.6.5 的协议定义：
- Service: `agent.Synchronizer`
- RPC: `KubernetesAPISync(KubernetesAPISyncRequest) returns (KubernetesAPISyncResponse)`
- Proto 文件：`message/agent.proto` 和 `message/common.proto`

## 快速开始

### 编译

```bash
cd cmd/pseudo-deepflow-agent
./build.sh
```

### 运行示例

```bash
./pseudo-deepflow-agent \
  --cluster-id=my-k8s-cluster \
  --deepflow-server=192.168.1.100:30033 \
  --kubeconfig=/path/to/kubeconfig \
  --sync-interval=60
```

### Kubernetes 部署

```bash
kubectl apply -f cmd/pseudo-deepflow-agent/deployment.yaml
```

## 命令行参数

| 参数 | 必填 | 默认值 | 说明 |
|------|------|--------|------|
| `--cluster-id` | 是 | - | K8s 集群 ID（需与 deepflow-agent.yaml 一致） |
| `--deepflow-server` | 是 | - | DeepFlow server 地址 (host:port) |
| `--kubeconfig` | 否 | - | kubeconfig 文件路径（默认使用 in-cluster 配置） |
| `--source-ip` | 否 | auto | 上报的源 IP（默认自动检测） |
| `--vtap-id` | 否 | 0 | Agent ID/Vtap ID |
| `--team-id` | 否 | "" | Team ID（多租户） |
| `--sync-interval` | 否 | 60 | 同步间隔（秒） |
| `--enable-service` | 否 | false | 启用 Service 资源同步 |
| `--max-heartbeat` | 否 | 86400 | 最大心跳间隔（秒，默认 24 小时） |

## 文件清单

```
cmd/pseudo-deepflow-agent/
├── main.go              # 主程序实现
├── go.mod               # Go 模块定义
├── go.sum               # 依赖校验和
├── build.sh             # 编译脚本
├── Dockerfile           # Docker 镜像构建文件
├── deployment.yaml      # Kubernetes 部署清单
└── README.md            # 详细文档（英文）
```

## 实现细节

- **版本管理**: 资源变化时使用当前 Unix 时间戳；无变化时复用上一版本
- **错误处理**: K8s API 错误或构建失败时，通过 `error_msg` 字段上报，并复用上次的版本和 entries
- **源 IP 检测**: 自动选择第一个非 loopback、UP 状态的 IPv4 地址
- **日志输出**: 所有操作日志输出到 stdout，包括资源变化、同步状态和错误信息

## 注意事项

1. proto 文件 (`*.pb.go`) 在构建时自动生成，不提交到版本控制
2. 需要安装 `protoc` 和 `protoc-gen-gofast` 才能编译
3. gRPC 连接为明文（无 TLS），适用于内网环境
4. 建议在 K8s 集群内部署，使用 ServiceAccount 认证

## 相关文档

详细使用说明和示例请参考：[cmd/pseudo-deepflow-agent/README.md](cmd/pseudo-deepflow-agent/README.md)
