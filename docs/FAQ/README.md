# FAQs

## DeepFlow 为了保证高性能采集做了哪些事情？

DeepFlow 在数据采集链路的各个层面都进行了深度优化，以下是核心的高性能保障措施：

### 1. 高效的数据包捕获技术

**AF_PACKET + TPACKET v3（零拷贝环形缓冲区）**

DeepFlow Agent 在 Linux 环境下使用 `AF_PACKET` 套接字结合 `TPACKET_V3` 协议，通过 `mmap` 将内核数据包环形缓冲区直接映射到用户空间，实现**零拷贝**数据包读取，避免了内核态与用户态之间的数据复制，大幅降低 CPU 开销。
相关实现：`agent/src/dispatcher/recv_engine/af_packet/tpacket.rs`

**eBPF 内核态数据采集**

通过 eBPF 程序直接在内核态进行数据采集，减少上下文切换和数据拷贝。eBPF Collector 可采集 Socket 数据（七层协议解析）、系统调用、进程事件等，无需修改应用程序。
相关实现：`agent/src/ebpf_dispatcher.rs`

**DPDK 高速数据包处理**

对于需要极高吞吐量的场景，DeepFlow 支持 DPDK（Data Plane Development Kit）数据源，在用户空间完成高速数据包的收发处理，绕过内核网络协议栈。
相关实现：`agent/src/dispatcher/recv_engine/mod.rs`

---

### 2. 多线程并行处理

**Packet Fanout（数据包分散负载）**

利用 Linux `PACKET_FANOUT` 机制将网络数据包分散到多个 Dispatcher 线程并行处理，充分利用多核 CPU 的处理能力，降低单核软中断压力。
相关实现：`agent/src/dispatcher/recv_engine/af_packet/tpacket.rs`（`PACKET_FANOUT`）

**多级队列并发流水线**

Agent 内部采用多级异步队列（queue）将数据包捕获、协议解析、流统计、数据发送等各阶段解耦为独立线程并发处理，形成高效流水线，各阶段互不阻塞。
架构示意图见：`docs/design/data-flow.md`

---

### 3. 高效内存管理

**无锁内存池（LockFreePool）**

Server 侧基于 Go 的 `sync.Pool` 构建了 `LockFreePool`，每个 CPU 线程拥有独立的对象缓存切片，在绝大多数情况下可**无锁**地分配和回收对象，避免高并发下的锁竞争，显著减少 GC 压力。
相关实现：`server/libs/pool/pool.go`

**Agent 侧可复用内存池（MemoryPool）**

Rust Agent 中针对 `FlowNode`、`TcpPerf` 等高频创建/销毁的对象实现了本地 `MemoryPool`，对象使用完毕后执行 `reset()` 重置并放回池中，减少堆内存分配和释放的开销。
相关实现：`agent/src/flow_generator/pool.rs`

**批量缓冲区分配（BatchedBox / BatchedBuffer）**

通过 `Allocator` 预分配大块连续内存，使用 `BatchedBox<T>` 和 `BatchedBuffer<T>` 在该内存块中划分给各个对象，多个对象共享同一底层内存块的引用计数，减少碎片化的堆分配。
相关实现：`agent/crates/public/src/buffer.rs`

---

### 4. 策略查找加速

**双级策略缓存（FastPath + FirstPath）**

Policy 模块采用两级查找缓存：
- **FastPath**：以数据包的 5 元组为 key，缓存最近命中的策略结果，命中时直接返回，避免全量规则匹配。
- **FirstPath**：在 FastPath 未命中时进行完整的 IP/端口规则匹配，并将结果写入 FastPath 缓存。

这种两级架构使得绝大多数数据包在 FastPath 即可完成策略查找，大幅降低平均查找耗时。
相关实现：`agent/src/policy/fast_path.rs`、`agent/src/policy/first_path.rs`

---

### 5. 限速与背压机制

**漏桶限速（LeakyBucket）**

Agent 使用漏桶算法（`LeakyBucket`）对数据包捕获速率进行限制（`max_capture_rate`），防止突发流量压垮下游处理模块，保证系统稳定运行。
相关实现：`agent/crates/public/src/leaky_bucket.rs`

**Throttler 流日志限流**

L4/L7 流日志输出端设置 Throttler，对超出配额的流日志进行采样丢弃，在保障核心指标完整性的同时，控制存储和网络带宽的消耗。
相关实现：`agent/src/collector/flow_aggr.rs`

---

### 6. 数据写入优化

**ClickHouse 写入优化**

Server 侧通过以下方式优化 ClickHouse 的写入性能：
- 设置 `ttl_only_drop_parts`，使 TTL 过期时直接删除整个 Part，避免逐行删除的高开销。
- 物化视图本地表去除 `GROUP BY`，降低聚合写入的 CPU 消耗。
- 批量写入（CKWriter）减少写入请求次数，并降低 CKWriter 内存占用。

---

### 总结

DeepFlow 通过零拷贝数据包捕获（AF_PACKET TPACKET v3）、内核态 eBPF 采集、DPDK 高速转发、Packet Fanout 多核并行、无锁内存池、双级策略缓存、异步多级流水线队列以及 ClickHouse 批量写入等一系列技术手段，在各个层面减少不必要的拷贝、锁竞争和内存分配，从而实现高吞吐、低延迟的可观测性数据采集能力。