#!/bin/bash
# Build script for pseudo-deepflow-agent
# This script generates protobuf files and builds the binary

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
REPO_ROOT="$SCRIPT_DIR/../.."

echo "==> Generating protobuf files..."
cd "$REPO_ROOT/message/common"
go generate

cd "$REPO_ROOT/message/agent"
go generate

echo "==> Building pseudo-deepflow-agent..."
cd "$SCRIPT_DIR"
go mod tidy
go build -o pseudo-deepflow-agent .

echo "==> Build complete: $SCRIPT_DIR/pseudo-deepflow-agent"
