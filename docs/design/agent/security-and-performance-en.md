# DeepFlow Agent Security and Performance Protective Measures

As a data collection component deployed on hosts and container nodes, DeepFlow Agent has been designed and implemented with full consideration for host security and performance impact, adopting multiple protective measures.

## I. Security Protective Measures

### 1.1 Permission Management and Principle of Least Privilege

DeepFlow Agent supports fine-grained permission control based on Linux Capabilities, avoiding the need for full root privileges:

- **Required Core Capabilities**:
  - `CAP_NET_ADMIN`: Network management capability for network data collection
  - `CAP_NET_RAW`: Raw socket capability for packet capture
  - `CAP_NET_BIND_SERVICE`: Capability to bind privileged ports
  - `CAP_SYS_ADMIN`: System administration capability for eBPF program loading
  - `CAP_IPC_LOCK`: Memory locking capability to prevent sensitive data from being swapped to disk

- **Permission Verification Mechanism**:
  - Provides `--check-privileges` parameter for verifying Pod privilege configuration in Kubernetes environments
  - Supports `--add-cap` parameter to dynamically grant required Linux Capabilities

- **Memory Security Protection**:
  - Uses `mlockall()` system call to lock memory, combined with `CAP_IPC_LOCK` capability
  - Prevents sensitive memory containing eBPF collected data from being swapped to disk, avoiding potential data leakage

### 1.2 Namespace Isolation

- **Sidecar Mode**:
  - Supports `--sidecar` mode for isolated Pod-level monitoring
  - In Sidecar mode, does not access host resources, only monitors applications within the same Pod
  - Suitable for scenarios with high security isolation requirements

- **eBPF Uprobe Filtering**:
  - Filters eBPF uprobes for unprivileged containers
  - Performs socket and process tracking based on namespaces, avoiding cross-namespace data leakage

### 1.3 eBPF Kernel Safety

- **eBPF JIT Compilation**:
  - Enables eBPF JIT compilation (`bpf_jit_enable`) to improve the security of eBPF bytecode execution
  - JIT-compiled code executes in kernel space after strict verification

- **BTF Type Information Validation**:
  - Uses BTF (BPF Type Format) for kernel structure offset validation
  - Ensures eBPF program access to kernel data structures is type-safe

- **Kernel Version Compatibility Check**:
  - Supports Linux 4.12 and above
  - Checks kernel version and feature support at startup to ensure compatibility

## II. Performance Protective Measures

### 2.1 Resource Limiting via Cgroups

DeepFlow Agent implements strict resource limits through Cgroups v1 and v2:

- **Memory Limits**:
  - Default memory limit: 768MB (configurable via `max_memory`)
  - Hard limits enforced through Cgroups memory controller
  - Prevents Agent memory usage from affecting the host

- **CPU Limits**:
  - CPU usage ceiling configured via `max_millicpus`
  - Uses CFS (Completely Fair Scheduler) quota mechanism, with each millicpu unit corresponding to 100μs
  - Example: 1000 millicpus = 1 CPU core

- **Automatic Cgroups Hierarchy Detection**:
  - Automatically detects the Cgroups version used by the system (v1 or v2)
  - Automatically adapts to the corresponding controller based on system configuration

- **Resource Monitoring Fallback**:
  - Supports `--cgroups-disabled` parameter to disable Cgroups (for environments that don't support Cgroups)
  - In fallback mode, Agent checks CPU and memory usage every 10 seconds
  - When resource usage exceeds thresholds, actively reduces collection frequency or pauses some functions

### 2.2 Memory Management Optimization

- **Memory Reclaim Mechanism**:
  - Uses `malloc_trim()` to actively reclaim unused memory blocks
  - Configures memory trimming strategy based on system type
  - Periodically triggers memory trimming to avoid memory fragmentation accumulation

- **RssFile Memory Optimization**:
  - RssFile memory optimization for AF_PACKET operations
  - Reduces page cache usage

- **Page Cache Reclaim Control**:
  - Controls page cache reclaim strategy, balancing memory usage and performance

### 2.3 Traffic and Load Management

- **System Load Circuit Breaker**:
  - Automatically disables some Agent functions when system load exceeds threshold
  - Auto-recovers after load is normal during safety period (default 300 seconds)
  - Prevents Agent from exacerbating performance issues during high system load

- **Socket Connection Limits**:
  - Configurable maximum socket connection limits
  - Sets tolerance intervals to avoid frequent triggering of limits in short periods

- **IO Event Filtering**:
  - Configurable minimum IO event duration threshold
  - Filters out IO events with too short duration, reducing data processing overhead

- **Disk Space Monitoring**:
  - Monitors disk space usage
  - Automatically cleans up old log files when disk space is insufficient
  - Sets disk space thresholds to prevent logs from filling up the disk

### 2.4 Thread and Resource Isolation

- **Process Threshold Limits**:
  - Configures process count thresholds to avoid performance issues from tracking too many processes

- **Thread Count Monitoring**:
  - Monitors the Agent's own thread count
  - Detects abnormal thread creation behavior

- **Watchdog Monitoring Thread**:
  - Independent Watchdog thread monitors for stuck operations
  - Promptly detects and handles abnormal situations

### 2.5 Cgroups Controller Real-time Monitoring

- **Real-time Resource Monitoring**:
  - Cgroups controller checks resource limits every second
  - Dynamically updates resource usage status
  - Triggers alerts or takes protective measures when approaching limits

## III. Configuration and Tuning

### 3.1 Configuration-Driven Design

- All security and performance parameters can be adjusted via YAML configuration files
- Provides sensible defaults optimized for out-of-the-box experience
- Default configuration is already optimized for minimal performance overhead

### 3.2 Key Configuration File Locations

- Cgroups resource limiting: `/agent/src/utils/cgroups/linux.rs`
- System resource monitoring and protection: `/agent/src/utils/guard.rs`
- Memory locking and initialization: `/agent/src/config/handler.rs`
- eBPF architecture and security design: `/agent/src/ebpf/README.md`

## IV. Performance Overhead Assessment

Typical performance overhead of DeepFlow Agent in production environments:

- **CPU Usage**: < 1% (single core, depends on traffic and configuration)
- **Memory Footprint**: Usually < 500MB (default limit 768MB)
- **Network Overhead**: eBPF zero-copy technology, minimal impact on application network performance
- **Continuous Profiling Overhead**: < 1% (performance profiling via eBPF)

## V. Summary

DeepFlow Agent ensures host security and minimal performance impact through the following core mechanisms:

1. **Principle of Least Privilege**: Fine-grained permission control based on Linux Capabilities
2. **Strong Isolation**: Namespace isolation and Sidecar mode support
3. **Strict Resource Limits**: CPU and memory hard limits via Cgroups
4. **Intelligent Circuit Breaker**: Automatic degradation protection during high system load
5. **Memory Safety**: Memory locking and active reclaim mechanisms
6. **eBPF Technology Advantages**: Efficient collection in kernel space with zero intrusion to applications

These measures enable DeepFlow Agent to minimize its impact on production environments while ensuring observability.
