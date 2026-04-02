# FAQs

## DeepFlow Agent 相关问题

### Q: DeepFlow Agent 如何保障主机安全和最小化性能影响？

DeepFlow Agent 采取了多项保障性措施：

**安全方面**：
- 基于 Linux Capabilities 的最小权限原则，避免使用完整 root 权限
- 支持命名空间隔离和 Sidecar 部署模式
- eBPF 内核安全验证机制
- 内存锁定防止敏感数据泄露

**性能方面**：
- 通过 Cgroups 实现 CPU 和内存严格限制（默认 768MB 内存限制）
- 系统负载熔断机制，高负载时自动降级保护
- 智能的内存管理和回收机制
- 典型 CPU 开销 < 1%，持续性能剖析开销 < 1%

详细信息请参考：[安全与性能保障措施文档](../design/agent/security-and-performance.md)

### Q: DeepFlow Agent 需要哪些权限？

DeepFlow Agent 需要以下 Linux Capabilities：
- `CAP_NET_ADMIN`：网络管理
- `CAP_NET_RAW`：原始套接字
- `CAP_NET_BIND_SERVICE`：绑定特权端口
- `CAP_SYS_ADMIN`：eBPF 程序加载
- `CAP_IPC_LOCK`：内存锁定

可以使用 `--check-privileges` 参数检查权限配置是否正确。

### Q: DeepFlow Agent 的资源限制是多少？

默认资源限制：
- **内存**：768MB（可通过 `max_memory` 配置）
- **CPU**：可通过 `max_millicpus` 配置

这些限制通过 Cgroups 强制执行，确保 Agent 不会影响主机和其他应用。