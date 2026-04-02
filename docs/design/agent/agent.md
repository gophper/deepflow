# Agent

DeepFlow Agent 是部署在每个 K8s 容器节点、传统服务器或云主机上的数据采集组件，负责采集所有应用进程的观测数据。

## 文档

- [安全与性能保障措施](./security-and-performance.md) - DeepFlow Agent 的安全和性能保护机制（中文）
- [Security and Performance Protective Measures](./security-and-performance-en.md) - DeepFlow Agent's security and performance protection mechanisms (English)

## 架构设计

### 核心组件

DeepFlow Agent 主要包含以下核心模块：

- **eBPF 数据采集模块**：基于 eBPF 技术实现零侵扰的数据采集
- **协议解析模块**：支持多种标准协议和可扩展的 Wasm 插件
- **数据处理模块**：对采集的数据进行聚合和处理
- **资源控制模块**：通过 Cgroups 实现资源限制和保护
- **数据传输模块**：将处理后的数据发送到 DeepFlow Server

### 部署模式

- **DaemonSet 模式**：在 Kubernetes 环境中以 DaemonSet 形式部署，每个节点运行一个 Agent
- **Sidecar 模式**：在 Pod 中以 Sidecar 形式部署，提供更强的隔离性
- **独立进程模式**：在传统服务器或云主机上以独立进程形式运行