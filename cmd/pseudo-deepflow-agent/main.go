/*
 * Copyright (c) 2024 Yunshan Networks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package main

import (
	"bytes"
	"compress/zlib"
	"context"
	"encoding/json"
	"flag"
	"fmt"
	"log"
	"net"
	"os"
	"os/signal"
	"sync"
	"syscall"
	"time"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
	appsv1 "k8s.io/api/apps/v1"
	corev1 "k8s.io/api/core/v1"
	"k8s.io/client-go/informers"
	"k8s.io/client-go/kubernetes"
	"k8s.io/client-go/tools/cache"
	"k8s.io/client-go/tools/clientcmd"

	"github.com/deepflowio/deepflow/message/agent"
	"github.com/deepflowio/deepflow/message/common"
)

// CLI flags
var (
	clusterID       = flag.String("cluster-id", "", "Kubernetes cluster ID (required, must match deepflow-agent.yaml)")
	deepflowServer  = flag.String("deepflow-server", "", "DeepFlow server address host:port (required)")
	kubeconfig      = flag.String("kubeconfig", "", "Path to kubeconfig file (optional, uses in-cluster config if not set)")
	sourceIP        = flag.String("source-ip", "", "Source IP to report (optional, auto-detected if not set)")
	vtapID          = flag.Uint("vtap-id", 0, "Vtap ID (optional)")
	teamID          = flag.String("team-id", "", "Team ID (optional)")
	syncInterval    = flag.Uint("sync-interval", 60, "Sync interval in seconds (default 60)")
	enableService   = flag.Bool("enable-service", false, "Enable Service resource sync (optional)")
	maxHeartbeatSec = flag.Uint("max-heartbeat", 86400, "Maximum heartbeat interval in seconds (default 86400 = 24 hours)")
)

// Resource cache and state management
type resourceCache struct {
	mu                sync.RWMutex
	nodes             map[string]*corev1.Node
	namespaces        map[string]*corev1.Namespace
	pods              map[string]*corev1.Pod
	deployments       map[string]*appsv1.Deployment
	statefulSets      map[string]*appsv1.StatefulSet
	daemonSets        map[string]*appsv1.DaemonSet
	replicaSets       map[string]*appsv1.ReplicaSet
	replicationCtrls  map[string]*corev1.ReplicationController
	services          map[string]*corev1.Service
	dirty             bool
	lastVersion       uint64
	lastSyncTime      time.Time
}

func newResourceCache() *resourceCache {
	return &resourceCache{
		nodes:            make(map[string]*corev1.Node),
		namespaces:       make(map[string]*corev1.Namespace),
		pods:             make(map[string]*corev1.Pod),
		deployments:      make(map[string]*appsv1.Deployment),
		statefulSets:     make(map[string]*appsv1.StatefulSet),
		daemonSets:       make(map[string]*appsv1.DaemonSet),
		replicaSets:      make(map[string]*appsv1.ReplicaSet),
		replicationCtrls: make(map[string]*corev1.ReplicationController),
		services:         make(map[string]*corev1.Service),
		dirty:            true, // Initial sync should send data
		lastVersion:      0,
		lastSyncTime:     time.Now(),
	}
}

func (rc *resourceCache) markDirty() {
	rc.mu.Lock()
	defer rc.mu.Unlock()
	rc.dirty = true
}

// Auto-detect source IP
func detectSourceIP() (string, error) {
	interfaces, err := net.Interfaces()
	if err != nil {
		return "", err
	}

	for _, iface := range interfaces {
		// Skip loopback and down interfaces
		if iface.Flags&net.FlagLoopback != 0 || iface.Flags&net.FlagUp == 0 {
			continue
		}

		addrs, err := iface.Addrs()
		if err != nil {
			continue
		}

		for _, addr := range addrs {
			var ip net.IP
			switch v := addr.(type) {
			case *net.IPNet:
				ip = v.IP
			case *net.IPAddr:
				ip = v.IP
			}

			// Return first non-loopback IPv4 address
			if ip != nil && ip.To4() != nil && !ip.IsLoopback() {
				return ip.String(), nil
			}
		}
	}

	return "", fmt.Errorf("no suitable IPv4 address found")
}

// Compress data using zlib
func compressData(data []byte) ([]byte, error) {
	var buf bytes.Buffer
	w := zlib.NewWriter(&buf)
	_, err := w.Write(data)
	if err != nil {
		w.Close()
		return nil, err
	}
	w.Close()
	return buf.Bytes(), nil
}

// Build KubernetesAPIInfo entries from cache
func buildEntries(rc *resourceCache, enableSvc bool) ([]*common.KubernetesAPIInfo, error) {
	rc.mu.RLock()
	defer rc.mu.RUnlock()

	var entries []*common.KubernetesAPIInfo

	// Helper function to add resource type
	addResourceType := func(resourceType string, resources interface{}) error {
		jsonData, err := json.Marshal(resources)
		if err != nil {
			return fmt.Errorf("failed to marshal %s: %v", resourceType, err)
		}

		compressed, err := compressData(jsonData)
		if err != nil {
			return fmt.Errorf("failed to compress %s: %v", resourceType, err)
		}

		typeStr := resourceType
		entries = append(entries, &common.KubernetesAPIInfo{
			Type:           &typeStr,
			CompressedInfo: compressed,
		})
		return nil
	}

	// Convert maps to slices for JSON serialization
	// Nodes
	nodeList := make([]*corev1.Node, 0, len(rc.nodes))
	for _, node := range rc.nodes {
		nodeList = append(nodeList, node)
	}
	if err := addResourceType("*v1.Node", nodeList); err != nil {
		return nil, err
	}

	// Namespaces
	nsList := make([]*corev1.Namespace, 0, len(rc.namespaces))
	for _, ns := range rc.namespaces {
		nsList = append(nsList, ns)
	}
	if err := addResourceType("*v1.Namespace", nsList); err != nil {
		return nil, err
	}

	// Pods
	podList := make([]*corev1.Pod, 0, len(rc.pods))
	for _, pod := range rc.pods {
		podList = append(podList, pod)
	}
	if err := addResourceType("*v1.Pod", podList); err != nil {
		return nil, err
	}

	// Deployments
	deployList := make([]*appsv1.Deployment, 0, len(rc.deployments))
	for _, deploy := range rc.deployments {
		deployList = append(deployList, deploy)
	}
	if err := addResourceType("*v1.Deployment", deployList); err != nil {
		return nil, err
	}

	// StatefulSets
	stsList := make([]*appsv1.StatefulSet, 0, len(rc.statefulSets))
	for _, sts := range rc.statefulSets {
		stsList = append(stsList, sts)
	}
	if err := addResourceType("*v1.StatefulSet", stsList); err != nil {
		return nil, err
	}

	// DaemonSets
	dsList := make([]*appsv1.DaemonSet, 0, len(rc.daemonSets))
	for _, ds := range rc.daemonSets {
		dsList = append(dsList, ds)
	}
	if err := addResourceType("*v1.DaemonSet", dsList); err != nil {
		return nil, err
	}

	// ReplicationControllers
	rcList := make([]*corev1.ReplicationController, 0, len(rc.replicationCtrls))
	for _, ctrl := range rc.replicationCtrls {
		rcList = append(rcList, ctrl)
	}
	if err := addResourceType("*v1.ReplicationController", rcList); err != nil {
		return nil, err
	}

	// ReplicaSets
	rsList := make([]*appsv1.ReplicaSet, 0, len(rc.replicaSets))
	for _, rs := range rc.replicaSets {
		rsList = append(rsList, rs)
	}
	if err := addResourceType("*v1.ReplicaSet", rsList); err != nil {
		return nil, err
	}

	// Services (optional)
	if enableSvc {
		svcList := make([]*corev1.Service, 0, len(rc.services))
		for _, svc := range rc.services {
			svcList = append(svcList, svc)
		}
		if err := addResourceType("*v1.Service", svcList); err != nil {
			return nil, err
		}
	}

	return entries, nil
}

// Setup informers for K8s resources
func setupInformers(clientset *kubernetes.Clientset, rc *resourceCache, enableSvc bool) informers.SharedInformerFactory {
	factory := informers.NewSharedInformerFactory(clientset, 0)

	// Node informer
	nodeInformer := factory.Core().V1().Nodes().Informer()
	nodeInformer.AddEventHandler(cache.ResourceEventHandlerFuncs{
		AddFunc: func(obj interface{}) {
			node := obj.(*corev1.Node)
			rc.mu.Lock()
			rc.nodes[string(node.UID)] = node
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("Node added: %s", node.Name)
		},
		UpdateFunc: func(oldObj, newObj interface{}) {
			node := newObj.(*corev1.Node)
			rc.mu.Lock()
			rc.nodes[string(node.UID)] = node
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("Node updated: %s", node.Name)
		},
		DeleteFunc: func(obj interface{}) {
			node := obj.(*corev1.Node)
			rc.mu.Lock()
			delete(rc.nodes, string(node.UID))
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("Node deleted: %s", node.Name)
		},
	})

	// Namespace informer
	nsInformer := factory.Core().V1().Namespaces().Informer()
	nsInformer.AddEventHandler(cache.ResourceEventHandlerFuncs{
		AddFunc: func(obj interface{}) {
			ns := obj.(*corev1.Namespace)
			rc.mu.Lock()
			rc.namespaces[string(ns.UID)] = ns
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("Namespace added: %s", ns.Name)
		},
		UpdateFunc: func(oldObj, newObj interface{}) {
			ns := newObj.(*corev1.Namespace)
			rc.mu.Lock()
			rc.namespaces[string(ns.UID)] = ns
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("Namespace updated: %s", ns.Name)
		},
		DeleteFunc: func(obj interface{}) {
			ns := obj.(*corev1.Namespace)
			rc.mu.Lock()
			delete(rc.namespaces, string(ns.UID))
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("Namespace deleted: %s", ns.Name)
		},
	})

	// Pod informer
	podInformer := factory.Core().V1().Pods().Informer()
	podInformer.AddEventHandler(cache.ResourceEventHandlerFuncs{
		AddFunc: func(obj interface{}) {
			pod := obj.(*corev1.Pod)
			rc.mu.Lock()
			rc.pods[string(pod.UID)] = pod
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("Pod added: %s/%s", pod.Namespace, pod.Name)
		},
		UpdateFunc: func(oldObj, newObj interface{}) {
			pod := newObj.(*corev1.Pod)
			rc.mu.Lock()
			rc.pods[string(pod.UID)] = pod
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("Pod updated: %s/%s", pod.Namespace, pod.Name)
		},
		DeleteFunc: func(obj interface{}) {
			pod := obj.(*corev1.Pod)
			rc.mu.Lock()
			delete(rc.pods, string(pod.UID))
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("Pod deleted: %s/%s", pod.Namespace, pod.Name)
		},
	})

	// Deployment informer
	deployInformer := factory.Apps().V1().Deployments().Informer()
	deployInformer.AddEventHandler(cache.ResourceEventHandlerFuncs{
		AddFunc: func(obj interface{}) {
			deploy := obj.(*appsv1.Deployment)
			rc.mu.Lock()
			rc.deployments[string(deploy.UID)] = deploy
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("Deployment added: %s/%s", deploy.Namespace, deploy.Name)
		},
		UpdateFunc: func(oldObj, newObj interface{}) {
			deploy := newObj.(*appsv1.Deployment)
			rc.mu.Lock()
			rc.deployments[string(deploy.UID)] = deploy
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("Deployment updated: %s/%s", deploy.Namespace, deploy.Name)
		},
		DeleteFunc: func(obj interface{}) {
			deploy := obj.(*appsv1.Deployment)
			rc.mu.Lock()
			delete(rc.deployments, string(deploy.UID))
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("Deployment deleted: %s/%s", deploy.Namespace, deploy.Name)
		},
	})

	// StatefulSet informer
	stsInformer := factory.Apps().V1().StatefulSets().Informer()
	stsInformer.AddEventHandler(cache.ResourceEventHandlerFuncs{
		AddFunc: func(obj interface{}) {
			sts := obj.(*appsv1.StatefulSet)
			rc.mu.Lock()
			rc.statefulSets[string(sts.UID)] = sts
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("StatefulSet added: %s/%s", sts.Namespace, sts.Name)
		},
		UpdateFunc: func(oldObj, newObj interface{}) {
			sts := newObj.(*appsv1.StatefulSet)
			rc.mu.Lock()
			rc.statefulSets[string(sts.UID)] = sts
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("StatefulSet updated: %s/%s", sts.Namespace, sts.Name)
		},
		DeleteFunc: func(obj interface{}) {
			sts := obj.(*appsv1.StatefulSet)
			rc.mu.Lock()
			delete(rc.statefulSets, string(sts.UID))
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("StatefulSet deleted: %s/%s", sts.Namespace, sts.Name)
		},
	})

	// DaemonSet informer
	dsInformer := factory.Apps().V1().DaemonSets().Informer()
	dsInformer.AddEventHandler(cache.ResourceEventHandlerFuncs{
		AddFunc: func(obj interface{}) {
			ds := obj.(*appsv1.DaemonSet)
			rc.mu.Lock()
			rc.daemonSets[string(ds.UID)] = ds
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("DaemonSet added: %s/%s", ds.Namespace, ds.Name)
		},
		UpdateFunc: func(oldObj, newObj interface{}) {
			ds := newObj.(*appsv1.DaemonSet)
			rc.mu.Lock()
			rc.daemonSets[string(ds.UID)] = ds
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("DaemonSet updated: %s/%s", ds.Namespace, ds.Name)
		},
		DeleteFunc: func(obj interface{}) {
			ds := obj.(*appsv1.DaemonSet)
			rc.mu.Lock()
			delete(rc.daemonSets, string(ds.UID))
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("DaemonSet deleted: %s/%s", ds.Namespace, ds.Name)
		},
	})

	// ReplicaSet informer
	rsInformer := factory.Apps().V1().ReplicaSets().Informer()
	rsInformer.AddEventHandler(cache.ResourceEventHandlerFuncs{
		AddFunc: func(obj interface{}) {
			rs := obj.(*appsv1.ReplicaSet)
			rc.mu.Lock()
			rc.replicaSets[string(rs.UID)] = rs
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("ReplicaSet added: %s/%s", rs.Namespace, rs.Name)
		},
		UpdateFunc: func(oldObj, newObj interface{}) {
			rs := newObj.(*appsv1.ReplicaSet)
			rc.mu.Lock()
			rc.replicaSets[string(rs.UID)] = rs
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("ReplicaSet updated: %s/%s", rs.Namespace, rs.Name)
		},
		DeleteFunc: func(obj interface{}) {
			rs := obj.(*appsv1.ReplicaSet)
			rc.mu.Lock()
			delete(rc.replicaSets, string(rs.UID))
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("ReplicaSet deleted: %s/%s", rs.Namespace, rs.Name)
		},
	})

	// ReplicationController informer
	rcInformer := factory.Core().V1().ReplicationControllers().Informer()
	rcInformer.AddEventHandler(cache.ResourceEventHandlerFuncs{
		AddFunc: func(obj interface{}) {
			ctrl := obj.(*corev1.ReplicationController)
			rc.mu.Lock()
			rc.replicationCtrls[string(ctrl.UID)] = ctrl
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("ReplicationController added: %s/%s", ctrl.Namespace, ctrl.Name)
		},
		UpdateFunc: func(oldObj, newObj interface{}) {
			ctrl := newObj.(*corev1.ReplicationController)
			rc.mu.Lock()
			rc.replicationCtrls[string(ctrl.UID)] = ctrl
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("ReplicationController updated: %s/%s", ctrl.Namespace, ctrl.Name)
		},
		DeleteFunc: func(obj interface{}) {
			ctrl := obj.(*corev1.ReplicationController)
			rc.mu.Lock()
			delete(rc.replicationCtrls, string(ctrl.UID))
			rc.dirty = true
			rc.mu.Unlock()
			log.Printf("ReplicationController deleted: %s/%s", ctrl.Namespace, ctrl.Name)
		},
	})

	// Service informer (optional)
	if enableSvc {
		svcInformer := factory.Core().V1().Services().Informer()
		svcInformer.AddEventHandler(cache.ResourceEventHandlerFuncs{
			AddFunc: func(obj interface{}) {
				svc := obj.(*corev1.Service)
				rc.mu.Lock()
				rc.services[string(svc.UID)] = svc
				rc.dirty = true
				rc.mu.Unlock()
				log.Printf("Service added: %s/%s", svc.Namespace, svc.Name)
			},
			UpdateFunc: func(oldObj, newObj interface{}) {
				svc := newObj.(*corev1.Service)
				rc.mu.Lock()
				rc.services[string(svc.UID)] = svc
				rc.dirty = true
				rc.mu.Unlock()
				log.Printf("Service updated: %s/%s", svc.Namespace, svc.Name)
			},
			DeleteFunc: func(obj interface{}) {
				svc := obj.(*corev1.Service)
				rc.mu.Lock()
				delete(rc.services, string(svc.UID))
				rc.dirty = true
				rc.mu.Unlock()
				log.Printf("Service deleted: %s/%s", svc.Namespace, svc.Name)
			},
		})
	}

	return factory
}

// Sync resources with deepflow-server
func syncResources(ctx context.Context, client agent.SynchronizerClient, rc *resourceCache, config syncConfig) error {
	rc.mu.Lock()
	isDirty := rc.dirty
	lastVer := rc.lastVersion
	timeSinceLastSync := time.Since(rc.lastSyncTime)
	rc.mu.Unlock()

	// Prepare request
	req := &agent.KubernetesAPISyncRequest{
		ClusterId: &config.clusterID,
		SourceIp:  &config.sourceIP,
	}

	if config.teamID != "" {
		req.TeamId = &config.teamID
	}

	if config.vtapID > 0 {
		agentID := uint32(config.vtapID)
		req.AgentId = &agentID
	}

	var entries []*common.KubernetesAPIInfo
	var version uint64
	var errorMsg string

	if isDirty {
		// Build and send full entries
		var err error
		entries, err = buildEntries(rc, config.enableService)
		if err != nil {
			errorMsg = fmt.Sprintf("Failed to build entries: %v", err)
			log.Printf("ERROR: %s", errorMsg)
			req.ErrorMsg = &errorMsg
			// Use last version on error
			version = lastVer
		} else {
			// Generate new version on successful build
			version = uint64(time.Now().Unix())
			req.Entries = entries
			log.Printf("Sending full sync with %d resource types (version: %d)", len(entries), version)
		}
	} else if timeSinceLastSync.Seconds() >= float64(config.maxHeartbeatSec) {
		// Send heartbeat (no entries, same version)
		version = lastVer
		log.Printf("Sending heartbeat (version: %d)", version)
	} else {
		// No need to sync yet
		return nil
	}

	req.Version = &version

	// Make gRPC call
	resp, err := client.KubernetesAPISync(ctx, req)
	if err != nil {
		return fmt.Errorf("KubernetesAPISync failed: %v", err)
	}

	log.Printf("Sync successful, server version: %d", resp.GetVersion())

	// Update state
	rc.mu.Lock()
	rc.dirty = false
	rc.lastVersion = version
	rc.lastSyncTime = time.Now()
	rc.mu.Unlock()

	return nil
}

type syncConfig struct {
	clusterID       string
	sourceIP        string
	teamID          string
	vtapID          uint
	enableService   bool
	maxHeartbeatSec uint
}

func main() {
	flag.Parse()

	// Validate required flags
	if *clusterID == "" {
		log.Fatal("ERROR: --cluster-id is required")
	}
	if *deepflowServer == "" {
		log.Fatal("ERROR: --deepflow-server is required")
	}

	// Auto-detect source IP if not provided
	sourceIPStr := *sourceIP
	if sourceIPStr == "" {
		var err error
		sourceIPStr, err = detectSourceIP()
		if err != nil {
			log.Printf("WARNING: Failed to auto-detect source IP: %v, using empty string", err)
			sourceIPStr = ""
		} else {
			log.Printf("Auto-detected source IP: %s", sourceIPStr)
		}
	}

	// Build kubeconfig
	config, err := clientcmd.BuildConfigFromFlags("", *kubeconfig)
	if err != nil {
		log.Fatalf("Failed to build kubeconfig: %v", err)
	}

	// Create clientset
	clientset, err := kubernetes.NewForConfig(config)
	if err != nil {
		log.Fatalf("Failed to create Kubernetes clientset: %v", err)
	}

	// Initialize resource cache
	rc := newResourceCache()

	// Setup informers
	factory := setupInformers(clientset, rc, *enableService)

	// Start informers
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	factory.Start(ctx.Done())

	// Wait for cache sync
	log.Println("Waiting for informer caches to sync...")
	for gvr, ok := range factory.WaitForCacheSync(ctx.Done()) {
		if !ok {
			log.Fatalf("Failed to sync cache for %v", gvr)
		}
	}
	log.Println("Informer caches synced successfully")

	// Connect to deepflow-server
	conn, err := grpc.Dial(*deepflowServer, grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		log.Fatalf("Failed to connect to deepflow-server: %v", err)
	}
	defer conn.Close()

	grpcClient := agent.NewSynchronizerClient(conn)

	// Setup sync config
	syncCfg := syncConfig{
		clusterID:       *clusterID,
		sourceIP:        sourceIPStr,
		teamID:          *teamID,
		vtapID:          *vtapID,
		enableService:   *enableService,
		maxHeartbeatSec: *maxHeartbeatSec,
	}

	// Start sync ticker
	ticker := time.NewTicker(time.Duration(*syncInterval) * time.Second)
	defer ticker.Stop()

	// Handle signals
	sigCh := make(chan os.Signal, 1)
	signal.Notify(sigCh, syscall.SIGINT, syscall.SIGTERM)

	log.Printf("pseudo-deepflow-agent started (cluster-id: %s, server: %s, sync-interval: %ds)",
		*clusterID, *deepflowServer, *syncInterval)

	// Main loop
	for {
		select {
		case <-ticker.C:
			if err := syncResources(ctx, grpcClient, rc, syncCfg); err != nil {
				log.Printf("ERROR: Sync failed: %v", err)
			}
		case <-sigCh:
			log.Println("Received shutdown signal, exiting...")
			return
		case <-ctx.Done():
			log.Println("Context cancelled, exiting...")
			return
		}
	}
}
